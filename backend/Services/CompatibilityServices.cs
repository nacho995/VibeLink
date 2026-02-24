namespace DefaultNamespace;
public class CompatibilityService
{
    private readonly AppDbContext _context;
    public CompatibilityService(AppDbContext context)
    {
        _context = context;
    }
    public int CalculateCompatibility(int userId1, int userId2)
    {
        var gustosUsuario1 = _context.UserLikes.Where(ul => ul.UserId == userId1).ToList();
        var gustosUsuario2 = _context.UserLikes.Where(ul => ul.UserId == userId2).ToList();
        var ids1 = gustosUsuario1.Select(ul => ul.ContentId).ToList();
        var ids2 = gustosUsuario2.Select(ul => ul.ContentId).ToList();
        var common = ids1.Intersect(ids2).ToList();
        int menor = Math.Min(ids1.Count, ids2.Count);
        if (menor == 0) return 0;
        double porcentage = ((double)common.Count / menor) * 100;
        return (int)porcentage;
    }
}