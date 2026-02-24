namespace VibeLink.App.Services;

/// <summary>
/// Guarda y recupera el token JWT de forma segura.
/// 
/// Usa SecureStorage de MAUI, que internamente:
/// - En iOS/Mac: usa el Keychain (almacenamiento seguro de Apple)
/// - En Android: usa EncryptedSharedPreferences
/// 
/// Es como una "caja fuerte" donde guardar el token.
/// Sin esto, el usuario tendría que hacer login cada vez que abre la app.
/// </summary>
public class TokenService
{
    private const string TokenKey = "auth_token";

    /// <summary>
    /// Guarda el token después de un login exitoso
    /// </summary>
    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);
    }

    /// <summary>
    /// Recupera el token guardado (para enviarlo en las peticiones)
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(TokenKey);
    }

    /// <summary>
    /// Borra el token (logout)
    /// </summary>
    public void RemoveToken()
    {
        SecureStorage.Remove(TokenKey);
    }

    /// <summary>
    /// Comprueba si hay un token guardado (para saber si el usuario ya está logueado)
    /// </summary>
    public async Task<bool> IsLoggedInAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
