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
