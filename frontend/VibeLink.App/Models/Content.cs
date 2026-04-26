namespace VibeLink.App.Models;

/// <summary>
/// Contenido (película, serie, videojuego).
/// Espejo del modelo Content del backend.
/// </summary>
public enum ContentType
{
    pelicula,
    serie,
    videojuego
}

public class Content
{
    public int Id { get; set; }
    public ContentType Type { get; set; }
    public int ApiId { get; set; }
    public string Titulo { get; set; } = "";
    public string? ImagenUrl { get; set; }
    public List<string>? Generos { get; set; }
    public int Año { get; set; }
}

/// <summary>
/// Tipo de contenido devuelto por el DiscoverController.
/// Debe coincidir con backend.Services.DiscoverContentType: Movie=0, Series=1, Game=2
/// </summary>
public enum DiscoverType
{
    Movie = 0,
    Series = 1,
    Game = 2
}

/// <summary>
/// Contenido devuelto por las APIs externas (TMDB/IGDB) via DiscoverController.
/// </summary>
public class DiscoverContentItem
{
    public string ExternalId { get; set; } = "";
    public string Title { get; set; } = "";
    public DiscoverType Type { get; set; }
    public string? ImageUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string Description { get; set; } = "";
    public double Rating { get; set; }
    public int? Year { get; set; }
    public List<string> Genres { get; set; } = [];
    public List<string>? Platforms { get; set; }

    // Propiedades calculadas para UI
    public string TypeLabel => Type switch
    {
        DiscoverType.Movie => "Pelicula",
        DiscoverType.Series => "Serie",
        DiscoverType.Game => "Juego",
        _ => Type.ToString()
    };

    public string TypeColor => Type switch
    {
        DiscoverType.Movie => "#e94560",   // Rojo Netflix
        DiscoverType.Series => "#00d4ff",  // Cyan
        DiscoverType.Game => "#7bed9f",    // Verde
        _ => "#a855f7"
    };

    public string TypeName => Type switch
    {
        DiscoverType.Movie => "Movie",
        DiscoverType.Series => "Series",
        DiscoverType.Game => "Game",
        _ => "Movie"
    };

    public string RatingFormatted => Rating.ToString("0.0");
    public string YearText => Year?.ToString() ?? "";
    public string GenresText => Genres.Count > 0 ? string.Join(", ", Genres.Take(3)) : "";
    
    // Imagen con fallback
    public string DisplayImage => ImageUrl ?? BackdropUrl ?? "";
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl) || !string.IsNullOrEmpty(BackdropUrl);
}

/// <summary>
/// Respuesta del endpoint /api/discover con todas las categorías.
/// </summary>
public class ContentDiscovery
{
    public List<DiscoverContentItem> Movies { get; set; } = [];
    public List<DiscoverContentItem> Series { get; set; } = [];
    public List<DiscoverContentItem> Games { get; set; } = [];
}
