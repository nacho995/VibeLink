namespace VibeLink.App.Models;

/// <summary>
/// Lo que enviamos al backend en POST /api/auth/register
/// Coincide con RegisterDTO del backend
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}
