using System.Net.Http.Headers;
using System.Net.Http.Json;
using VibeLink.App.Models;

namespace VibeLink.App.Services;

/// <summary>
/// Servicio central que hace TODAS las llamadas HTTP al backend.
/// Cada método corresponde a un endpoint de tu API.
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public ApiService(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Añade el token JWT a las cabeceras antes de cada petición autenticada.
    /// </summary>
    private async Task SetAuthHeader()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ==================== AUTH ====================

    public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                await _tokenService.SaveTokenAsync(token);
                return (true, "Login exitoso");
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RegisterAsync(
        string username, string email, string password, string confirmPassword)
    {
        try
        {
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

            if (response.IsSuccessStatusCode)
                return (true, "Registro exitoso");

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // ==================== USERS ====================

    /// <summary>GET /api/users — Obtener todos los usuarios</summary>
    public async Task<List<UserProfile>> GetUsersAsync()
    {
        await SetAuthHeader();
        var response = await _httpClient.GetAsync("api/users");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<UserProfile>>() ?? [];
        return [];
    }

    /// <summary>GET /api/users/{id} — Obtener un usuario por ID</summary>
    public async Task<UserProfile?> GetUserAsync(int id)
    {
        await SetAuthHeader();
        var response = await _httpClient.GetAsync($"api/users/{id}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<UserProfile>();
        return null;
    }

    /// <summary>PUT /api/users/{id} — Actualizar perfil</summary>
    public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, ProfileUpdateRequest profile)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}", profile);
            if (response.IsSuccessStatusCode)
                return (true, "Perfil actualizado");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== SWIPE PERSONAS ====================

    /// <summary>POST /api/swipe — Dar Like/Dislike a una persona</summary>
    public async Task<(bool Success, bool IsMatch, string Message)> SwipePersonAsync(PeopleSwipeRequest request)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/swipe", request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                // Si el backend devuelve un objeto Match, es porque hubo match mutuo
                bool isMatch = body.Contains("matchingUserId", StringComparison.OrdinalIgnoreCase);
                return (true, isMatch, isMatch ? "Es un Match!" : "Swipe registrado");
            }
            var error = await response.Content.ReadAsStringAsync();
            return (false, false, error);
        }
        catch (Exception ex)
        {
            return (false, false, $"Error: {ex.Message}");
        }
    }

    // ==================== SWIPE CONTENIDO ====================

    /// <summary>POST /api/userlikes — Dar Like/Dislike a contenido</summary>
    public async Task<(bool Success, string Message)> SwipeContentAsync(ContentSwipeRequest request)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/userlikes", request);
            if (response.IsSuccessStatusCode)
                return (true, "OK");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== CONTENIDO (legacy) ====================

    /// <summary>GET /api/contentscontrollers — Obtener todo el contenido local</summary>
    public async Task<List<Content>> GetContentAsync()
    {
        await SetAuthHeader();
        var response = await _httpClient.GetAsync("api/contentscontrollers");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<Content>>() ?? [];
        return [];
    }

    // ==================== DISCOVER (TMDB + RAWG) ====================

    /// <summary>GET /api/discover — Contenido popular de todas las categorías</summary>
    public async Task<ContentDiscovery> GetDiscoveryAsync(int page = 1)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/discover?page={page}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ContentDiscovery>() ?? new();
            return new();
        }
        catch { return new(); }
    }

    /// <summary>GET /api/discover/movies — Películas populares</summary>
    public async Task<List<DiscoverContentItem>> GetPopularMoviesAsync(int page = 1)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/discover/movies?page={page}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DiscoverContentItem>>() ?? [];
            return [];
        }
        catch { return []; }
    }

    /// <summary>GET /api/discover/series — Series populares</summary>
    public async Task<List<DiscoverContentItem>> GetPopularSeriesAsync(int page = 1)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/discover/series?page={page}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DiscoverContentItem>>() ?? [];
            return [];
        }
        catch { return []; }
    }

    /// <summary>GET /api/discover/games — Juegos populares</summary>
    public async Task<List<DiscoverContentItem>> GetPopularGamesAsync(int page = 1)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/discover/games?page={page}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DiscoverContentItem>>() ?? [];
            return [];
        }
        catch { return []; }
    }

    /// <summary>GET /api/discover/search?q=... — Búsqueda global</summary>
    public async Task<List<DiscoverContentItem>> SearchContentAsync(string query, int page = 1)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/discover/search?q={Uri.EscapeDataString(query)}&page={page}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DiscoverContentItem>>() ?? [];
            return [];
        }
        catch { return []; }
    }

    // ==================== MATCHING ====================

    /// <summary>GET /api/matching/{userId} — Ranking de compatibilidad</summary>
    public async Task<List<CompatibilityResult>> GetCompatibilityAsync(int userId)
    {
        await SetAuthHeader();
        var response = await _httpClient.GetAsync($"api/matching/{userId}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<CompatibilityResult>>() ?? [];
        return [];
    }

    /// <summary>GET /api/matching/mymatch/{userId} — Mis matches</summary>
    public async Task<List<MatchInfo>> GetMyMatchesAsync(int userId)
    {
        await SetAuthHeader();
        var response = await _httpClient.GetAsync($"api/matching/mymatch/{userId}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<MatchInfo>>() ?? [];
        return [];
    }

    // ==================== CHAT ====================

    /// <summary>GET /api/chat?userId=X and matchingUserId=Y — Mensajes entre dos usuarios</summary>
    public async Task<List<MessageChat>> GetMessagesAsync(int userId, int matchingUserId)
    {
        await SetAuthHeader();
        var response = await _httpClient.GetAsync($"api/chat?userId={userId}&matchingUserId={matchingUserId}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<MessageChat>>() ?? [];
        return [];
    }

    /// <summary>POST /api/chat — Enviar mensaje</summary>
    public async Task<(bool Success, string Message)> SendMessageAsync(SendMessageRequest request)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/chat", request);
            if (response.IsSuccessStatusCode)
                return (true, "Enviado");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== PAYMENT ====================

    /// <summary>POST /api/payment/create-checkout/{userId} — Crear sesión de pago Stripe</summary>
    public async Task<string?> CreateCheckoutAsync(int userId)
    {
        try
        {
            await SetAuthHeader();
            var response = await _httpClient.PostAsync($"api/payment/create-checkout/{userId}", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
                return result?.Url;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>Respuesta del endpoint de checkout de Stripe</summary>
public class CheckoutResponse
{
    public string Url { get; set; } = "";
}
