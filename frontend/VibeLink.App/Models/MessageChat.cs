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
