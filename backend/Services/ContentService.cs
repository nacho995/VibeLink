using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.Services;

/// <summary>
/// Servicio para obtener contenido de APIs externas.
/// TMDB: películas y series (API key simple)
/// IGDB: videojuegos (OAuth2 de Twitch)
/// </summary>
public class ContentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContentService> _logger;

    // TMDB Config
    private string TmdbApiKey => _configuration["ExternalApis:TmdbApiKey"] ?? "";
    private const string TmdbBaseUrl = "https://api.themoviedb.org/3";
    private const string TmdbImageBaseUrl = "https://image.tmdb.org/t/p/w500";

    // IGDB/Twitch Config
    private string TwitchClientId => _configuration["ExternalApis:TwitchClientId"] ?? "";
    private string TwitchClientSecret => _configuration["ExternalApis:TwitchClientSecret"] ?? "";
    private const string IgdbBaseUrl = "https://api.igdb.com/v4";
    private const string IgdbImageBaseUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/";

    // Caché del token de Twitch (expira cada ~60 días)
    private string? _twitchAccessToken;
    private DateTime _twitchTokenExpiry = DateTime.MinValue;

    public ContentService(HttpClient httpClient, IConfiguration configuration, ILogger<ContentService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    // ==================== PELÍCULAS (TMDB) ====================

    public async Task<List<ContentItem>> GetPopularMoviesAsync(int page = 1)
    {
        try
        {
            var url = $"{TmdbBaseUrl}/movie/popular?api_key={TmdbApiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error: {StatusCode}", response.StatusCode);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);

            return result?.Results?.Select(m => new ContentItem
            {
                ExternalId = $"tmdb-movie-{m.Id}",
                Title = m.Title ?? m.Name ?? "Sin título",
                Type = DiscoverContentType.Movie,
                ImageUrl = string.IsNullOrEmpty(m.PosterPath) ? null : $"{TmdbImageBaseUrl}{m.PosterPath}",
                BackdropUrl = string.IsNullOrEmpty(m.BackdropPath) ? null : $"{TmdbImageBaseUrl}{m.BackdropPath}",
                Description = m.Overview ?? "",
                Rating = m.VoteAverage,
                Year = ParseYear(m.ReleaseDate ?? m.FirstAirDate),
                Genres = []
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching popular movies");
            return [];
        }
    }

    public async Task<List<ContentItem>> GetPopularSeriesAsync(int page = 1)
    {
        try
        {
            var url = $"{TmdbBaseUrl}/tv/popular?api_key={TmdbApiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);

            return result?.Results?.Select(m => new ContentItem
            {
                ExternalId = $"tmdb-tv-{m.Id}",
                Title = m.Name ?? m.Title ?? "Sin título",
                Type = DiscoverContentType.Series,
                ImageUrl = string.IsNullOrEmpty(m.PosterPath) ? null : $"{TmdbImageBaseUrl}{m.PosterPath}",
                BackdropUrl = string.IsNullOrEmpty(m.BackdropPath) ? null : $"{TmdbImageBaseUrl}{m.BackdropPath}",
                Description = m.Overview ?? "",
                Rating = m.VoteAverage,
                Year = ParseYear(m.FirstAirDate ?? m.ReleaseDate),
                Genres = []
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching popular series");
            return [];
        }
    }

    public async Task<List<ContentItem>> SearchMoviesAndSeriesAsync(string query, int page = 1)
    {
        try
        {
            var url = $"{TmdbBaseUrl}/search/multi?api_key={TmdbApiKey}&language=es-ES&query={Uri.EscapeDataString(query)}&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);

            return result?.Results?
                .Where(m => m.MediaType == "movie" || m.MediaType == "tv")
                .Select(m => new ContentItem
                {
                    ExternalId = $"tmdb-{m.MediaType}-{m.Id}",
                    Title = m.Title ?? m.Name ?? "Sin título",
                    Type = m.MediaType == "movie" ? DiscoverContentType.Movie : DiscoverContentType.Series,
                    ImageUrl = string.IsNullOrEmpty(m.PosterPath) ? null : $"{TmdbImageBaseUrl}{m.PosterPath}",
                    BackdropUrl = string.IsNullOrEmpty(m.BackdropPath) ? null : $"{TmdbImageBaseUrl}{m.BackdropPath}",
                    Description = m.Overview ?? "",
                    Rating = m.VoteAverage,
                    Year = ParseYear(m.ReleaseDate ?? m.FirstAirDate),
                    Genres = []
                }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies/series");
            return [];
        }
    }

    // ==================== VIDEOJUEGOS (IGDB via Twitch) ====================

    /// <summary>
    /// Obtiene token OAuth2 de Twitch para usar IGDB.
    /// El token se cachea y se renueva automáticamente.
    /// </summary>
    private async Task<string?> GetTwitchTokenAsync()
    {
        if (_twitchAccessToken != null && DateTime.UtcNow < _twitchTokenExpiry)
            return _twitchAccessToken;

        try
        {
            var url = $"https://id.twitch.tv/oauth2/token?client_id={TwitchClientId}&client_secret={TwitchClientSecret}&grant_type=client_credentials";
            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twitch OAuth error: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<TwitchTokenResponse>(json);

            if (tokenData?.AccessToken != null)
            {
                _twitchAccessToken = tokenData.AccessToken;
                _twitchTokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn - 60); // 1 min margen
                return _twitchAccessToken;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Twitch token");
            return null;
        }
    }

    /// <summary>
    /// Hace una petición POST a IGDB con el query language propio (Apicalypse).
    /// </summary>
    private async Task<string?> IgdbQueryAsync(string endpoint, string body)
    {
        var token = await GetTwitchTokenAsync();
        if (token == null) return null;

        var request = new HttpRequestMessage(HttpMethod.Post, $"{IgdbBaseUrl}/{endpoint}");
        request.Headers.Add("Client-ID", TwitchClientId);
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = new StringContent(body, Encoding.UTF8, "text/plain");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("IGDB API error on {Endpoint}: {StatusCode}", endpoint, response.StatusCode);
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Obtiene juegos populares de IGDB (ordenados por rating).
    /// </summary>
    public async Task<List<ContentItem>> GetPopularGamesAsync(int page = 1)
    {
        try
        {
            int offset = (page - 1) * 20;
            var query = $"fields name,cover.image_id,rating,first_release_date,genres.name,platforms.name,summary; where rating > 70 & cover != null; sort rating desc; limit 20; offset {offset};";

            var json = await IgdbQueryAsync("games", query);
            if (json == null) return [];

            var games = JsonSerializer.Deserialize<List<IgdbGame>>(json);

            return games?.Select(g => new ContentItem
            {
                ExternalId = $"igdb-{g.Id}",
                Title = g.Name ?? "Sin título",
                Type = DiscoverContentType.Game,
                ImageUrl = g.Cover?.ImageId != null ? $"{IgdbImageBaseUrl}{g.Cover.ImageId}.jpg" : null,
                BackdropUrl = g.Cover?.ImageId != null ? $"https://images.igdb.com/igdb/image/upload/t_screenshot_big/{g.Cover.ImageId}.jpg" : null,
                Description = g.Summary ?? "",
                Rating = (g.Rating ?? 0) / 10.0, // IGDB usa 0-100, convertimos a 0-10
                Year = g.FirstReleaseDate.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(g.FirstReleaseDate.Value).Year
                    : null,
                Genres = g.Genres?.Select(ge => ge.Name ?? "").Where(n => n != "").ToList() ?? [],
                Platforms = g.Platforms?.Select(p => p.Name ?? "").Where(n => n != "").ToList()
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching popular games from IGDB");
            return [];
        }
    }

    /// <summary>
    /// Busca videojuegos en IGDB.
    /// </summary>
    public async Task<List<ContentItem>> SearchGamesAsync(string query, int page = 1)
    {
        try
        {
            int offset = (page - 1) * 20;
            var igdbQuery = $"search \"{query.Replace("\"", "\\\"")}\"; fields name,cover.image_id,rating,first_release_date,genres.name,platforms.name,summary; where cover != null; limit 20; offset {offset};";

            var json = await IgdbQueryAsync("games", igdbQuery);
            if (json == null) return [];

            var games = JsonSerializer.Deserialize<List<IgdbGame>>(json);

            return games?.Select(g => new ContentItem
            {
                ExternalId = $"igdb-{g.Id}",
                Title = g.Name ?? "Sin título",
                Type = DiscoverContentType.Game,
                ImageUrl = g.Cover?.ImageId != null ? $"{IgdbImageBaseUrl}{g.Cover.ImageId}.jpg" : null,
                BackdropUrl = g.Cover?.ImageId != null ? $"https://images.igdb.com/igdb/image/upload/t_screenshot_big/{g.Cover.ImageId}.jpg" : null,
                Description = g.Summary ?? "",
                Rating = (g.Rating ?? 0) / 10.0,
                Year = g.FirstReleaseDate.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(g.FirstReleaseDate.Value).Year
                    : null,
                Genres = g.Genres?.Select(ge => ge.Name ?? "").Where(n => n != "").ToList() ?? [],
                Platforms = g.Platforms?.Select(p => p.Name ?? "").Where(n => n != "").ToList()
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching games on IGDB");
            return [];
        }
    }

    // ==================== COMBINADO ====================

    public async Task<ContentDiscovery> GetDiscoveryContentAsync(int page = 1)
    {
        var moviesTask = GetPopularMoviesAsync(page);
        var seriesTask = GetPopularSeriesAsync(page);
        var gamesTask = GetPopularGamesAsync(page);

        await Task.WhenAll(moviesTask, seriesTask, gamesTask);

        return new ContentDiscovery
        {
            Movies = await moviesTask,
            Series = await seriesTask,
            Games = await gamesTask
        };
    }

    public async Task<List<ContentItem>> SearchAllAsync(string query, int page = 1)
    {
        var moviesSeriesTask = SearchMoviesAndSeriesAsync(query, page);
        var gamesTask = SearchGamesAsync(query, page);

        await Task.WhenAll(moviesSeriesTask, gamesTask);

        var results = new List<ContentItem>();
        results.AddRange(await moviesSeriesTask);
        results.AddRange(await gamesTask);

        return results.OrderByDescending(c => c.Rating).ToList();
    }

    // ==================== HELPERS ====================

    private static int? ParseYear(string? date)
    {
        if (string.IsNullOrEmpty(date)) return null;
        if (DateTime.TryParse(date, out var parsed))
            return parsed.Year;
        return null;
    }
}

// ==================== DTOs Comunes ====================

public enum DiscoverContentType { Movie, Series, Game }

public class ContentItem
{
    public string ExternalId { get; set; } = "";
    public string Title { get; set; } = "";
    public DiscoverContentType Type { get; set; }
    public string? ImageUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string Description { get; set; } = "";
    public double Rating { get; set; }
    public int? Year { get; set; }
    public List<string> Genres { get; set; } = [];
    public List<string>? Platforms { get; set; }
}

public class ContentDiscovery
{
    public List<ContentItem> Movies { get; set; } = [];
    public List<ContentItem> Series { get; set; } = [];
    public List<ContentItem> Games { get; set; } = [];
}

// ==================== TMDB Response Models ====================

public class TmdbResponse
{
    [JsonPropertyName("results")] public List<TmdbItem>? Results { get; set; }
}

public class TmdbItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
    [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
    [JsonPropertyName("media_type")] public string? MediaType { get; set; }
}

// ==================== IGDB Response Models ====================

public class IgdbGame
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("summary")] public string? Summary { get; set; }
    [JsonPropertyName("rating")] public double? Rating { get; set; }
    [JsonPropertyName("first_release_date")] public long? FirstReleaseDate { get; set; }
    [JsonPropertyName("cover")] public IgdbCover? Cover { get; set; }
    [JsonPropertyName("genres")] public List<IgdbNamedItem>? Genres { get; set; }
    [JsonPropertyName("platforms")] public List<IgdbNamedItem>? Platforms { get; set; }
}

public class IgdbCover
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("image_id")] public string? ImageId { get; set; }
}

public class IgdbNamedItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

// ==================== Twitch OAuth Response ====================

public class TwitchTokenResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")] public string? TokenType { get; set; }
}
