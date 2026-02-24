namespace VibeLink.App.Models;

/// <summary>
/// Resultado de compatibilidad con otro usuario.
/// Respuesta del endpoint GET /api/matching/{userId}
/// </summary>
public class CompatibilityResult
{
    public UserProfile? Usuario { get; set; }
    public int Compatibilidad { get; set; }
}
