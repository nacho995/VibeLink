namespace DefaultNamespace;

public enum Gender { Male, Female };

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string? CodigoInvitacion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public bool IsPremium { get; set; }
    public int Swipes { get; set; }
    public DateTime LastSwipeUpdate { get; set; }
}