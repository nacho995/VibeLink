using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers;

/// <summary>
/// Endpoints para descubrir contenido de TMDB y RAWG.
/// Sirve películas, series y juegos con posters reales.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscoverController : ControllerBase
{
    private readonly ContentService _contentService;

    public DiscoverController(ContentService contentService)
    {
        _contentService = contentService;
    }

    /// <summary>
    /// GET /api/discover
    /// Obtiene contenido popular de todas las categorías (para onboarding/carrusel).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ContentDiscovery>> GetDiscovery([FromQuery] int page = 1)
    {
        var content = await _contentService.GetDiscoveryContentAsync(page);
        return Ok(content);
    }

    /// <summary>
    /// GET /api/discover/movies?page=1
    /// Obtiene películas populares.
    /// </summary>
    [HttpGet("movies")]
    public async Task<ActionResult<List<ContentItem>>> GetMovies([FromQuery] int page = 1)
    {
        var movies = await _contentService.GetPopularMoviesAsync(page);
        return Ok(movies);
    }

    /// <summary>
    /// GET /api/discover/series?page=1
    /// Obtiene series populares.
    /// </summary>
    [HttpGet("series")]
    public async Task<ActionResult<List<ContentItem>>> GetSeries([FromQuery] int page = 1)
    {
        var series = await _contentService.GetPopularSeriesAsync(page);
        return Ok(series);
    }

    /// <summary>
    /// GET /api/discover/games?page=1
    /// Obtiene juegos populares.
    /// </summary>
    [HttpGet("games")]
    public async Task<ActionResult<List<ContentItem>>> GetGames([FromQuery] int page = 1)
    {
        var games = await _contentService.GetPopularGamesAsync(page);
        return Ok(games);
    }

    /// <summary>
    /// GET /api/discover/search?q=batman&page=1
    /// Busca en todas las categorías.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<ContentItem>>> Search([FromQuery] string q, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("El parámetro 'q' es obligatorio");

        var results = await _contentService.SearchAllAsync(q, page);
        return Ok(results);
    }

    /// <summary>
    /// GET /api/discover/search/movies?q=batman
    /// Busca solo películas y series.
    /// </summary>
    [HttpGet("search/movies")]
    public async Task<ActionResult<List<ContentItem>>> SearchMovies([FromQuery] string q, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("El parámetro 'q' es obligatorio");

        var results = await _contentService.SearchMoviesAndSeriesAsync(q, page);
        return Ok(results);
    }

    /// <summary>
    /// GET /api/discover/search/games?q=zelda
    /// Busca solo juegos.
    /// </summary>
    [HttpGet("search/games")]
    public async Task<ActionResult<List<ContentItem>>> SearchGames([FromQuery] string q, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("El parámetro 'q' es obligatorio");

        var results = await _contentService.SearchGamesAsync(q, page);
        return Ok(results);
    }
}
