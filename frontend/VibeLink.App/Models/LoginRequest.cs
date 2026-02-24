namespace VibeLink.App.Models;

/// <summary>
/// Lo que enviamos al backend en POST /api/auth/login
/// Coincide con LoginDTO del backend
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
