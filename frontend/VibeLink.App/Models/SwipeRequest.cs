namespace VibeLink.App.Models;

/// <summary>
/// Swipe sobre otra persona (Like/Dislike).
/// Coincide con PeopleSwipeDTOs del backend.
/// </summary>
public enum SwipeState
{
    Like,
    Dislike
}

public class PeopleSwipeRequest
{
    public int UserId { get; set; }
    public int MatchingUserId { get; set; }
    public SwipeState State { get; set; }
}

/// <summary>
/// Swipe sobre contenido (Like/Dislike película/serie/juego).
/// Coincide con SwipeDTOs del backend.
/// </summary>
public enum ContentState
{
    Liked,
    Disliked
}

public class ContentSwipeRequest
{
    public int UserId { get; set; }
    public int ContentId { get; set; }
    public ContentState State { get; set; }
    public int Punctuation { get; set; }
}

/// <summary>
/// Like/Dislike desde el onboarding usando ExternalId de APIs externas.
/// Coincide con ExternalLikeDTO del backend (POST /api/userlikes/external).
/// </summary>
public class ExternalLikeRequest
{
    public int UserId { get; set; }
    public string ExternalId { get; set; } = "";  // ej: "tmdb-movie-550", "igdb-1942"
    public string Title { get; set; } = "";
    public string? ImageUrl { get; set; }
    public ContentState State { get; set; }
}
