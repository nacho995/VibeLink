using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VibeLink.App.Services;

/// <summary>
/// Gestiona la sesión del usuario actual.
/// Decodifica el token JWT para extraer el nombre de usuario,
/// y guarda el ID del usuario para usarlo en las peticiones.
/// 
/// El JWT de tu backend contiene: ClaimTypes.Name = username
/// Pero necesitamos el ID numérico para las peticiones.
/// Por eso, tras el login, buscamos el usuario por username y guardamos su ID.
/// </summary>
public class SessionService
{
    private readonly TokenService _tokenService;
    
    // ID del usuario logueado (se setea después del login)
    public int CurrentUserId { get; set; }
    public string CurrentUsername { get; set; } = "";

    public SessionService(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Lee el token JWT guardado y extrae el username.
    /// </summary>
    public async Task<string?> GetUsernameFromTokenAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.Name || c.Type == "unique_name");
            return claim?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Cierra la sesión: borra el token y resetea los datos.
    /// </summary>
    public void Logout()
    {
        _tokenService.RemoveToken();
        CurrentUserId = 0;
        CurrentUsername = "";
    }
}
