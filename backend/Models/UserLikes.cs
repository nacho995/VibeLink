namespace DefaultNamespace;
public enum State
{
    Liked,
    Disliked,
}
public class UserLikes
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContentId { get; set; }
    public int Punctuation { get; set; }
    public State State { get; set; } = State.Liked;
    
    public UserLikes() { }
    
    public UserLikes(int userId, int contentId)
    {
        UserId = userId;
        ContentId = contentId;
    }
}