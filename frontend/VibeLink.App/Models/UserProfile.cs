namespace VibeLink.App.Models;

/// <summary>
/// Representa un usuario. Coincide con el modelo User del backend.
/// No incluimos PasswordHash porque el frontend nunca lo necesita.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string? CodigoInvitacion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public bool IsPremium { get; set; }
    public int Swipes { get; set; }
}
