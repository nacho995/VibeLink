namespace DefaultNamespace;

public class PeopleSwipe
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MatchingUserId { get; set; }
    public SwipeState State { get; set; }

    public PeopleSwipe(int userId, int matchingUserId, SwipeState state)
    {
        UserId = userId;
        MatchingUserId = matchingUserId;
        State = state;
    }
}
