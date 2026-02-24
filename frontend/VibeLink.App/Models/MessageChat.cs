namespace VibeLink.App.Models;

/// <summary>
/// Mensaje de chat entre dos usuarios que tienen match.
/// </summary>
public class MessageChat
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MatchingUserId { get; set; }
    public string Message { get; set; } = "";
    public DateTime Date { get; set; }
    
    // ID del usuario actual para determinar si es mensaje propio o del otro
    public int CurrentUserId { get; set; }
    
    // Propiedades calculadas para el UI
    public bool IsFromMe => UserId == CurrentUserId;
    public bool IsFromOther => !IsFromMe;
    public bool IsRead { get; set; } = true; // Por ahora siempre true
}

/// <summary>
/// Lo que enviamos para crear un nuevo mensaje.
/// </summary>
public class SendMessageRequest
{
    public int UserId { get; set; }
    public int MatchingUserId { get; set; }
    public string Message { get; set; } = "";
}
