using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace DefaultNamespace;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<SessionMatch> SessionMatches { get; set; }
    public DbSet<Conection> Conections { get; set; }
    public DbSet<Swipe> Swipes { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Content> Content { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserLikes> UserLikes { get; set; }
    public DbSet<PeopleSwipe> PeopleSwipe { get; set; }
    public DbSet<MessageChat> MessageChats { get; set; }
}