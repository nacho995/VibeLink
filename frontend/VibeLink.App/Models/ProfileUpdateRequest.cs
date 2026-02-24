namespace VibeLink.App.Models;

/// <summary>
/// Datos para actualizar el perfil.
/// Coincide con ProfileDataDTOs del backend.
/// </summary>
public class ProfileUpdateRequest
{
    public string? AvatarUrl { get; set; }
    public int Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Bio { get; set; }
}
