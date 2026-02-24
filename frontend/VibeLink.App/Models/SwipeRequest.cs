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
