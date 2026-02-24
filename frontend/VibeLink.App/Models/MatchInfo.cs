namespace VibeLink.App.Models;

/// <summary>
/// Información de un match (respuesta del endpoint GET /api/matching/mymatch/{userId}).
/// Incluye datos del otro usuario con el que hiciste match.
/// </summary>
public class MatchInfo
{
    public int MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public MatchUser? AnotherUser { get; set; }
}

public class MatchUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}
