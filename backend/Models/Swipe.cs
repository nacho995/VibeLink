namespace DefaultNamespace;

public enum SwipeState
{
    Like,
    Dislike
}
public class Swipe
{
    public int Id { get; set;}
    public int UserId { get; set;}
    public int MatchingUserId { get; set;}
    public SwipeState State { get; set; }
}