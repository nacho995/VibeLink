namespace DefaultNamespace;
public enum ConectionState
{
    Pendiente,
    Aceptado,
    Rechazado,
    Bloqueado
}
public class Conection
{
    public int Id { get; set;}
    public int UserId { get; set;}
    public int FriendId { get; set;}
    public ConectionState State { get; set;}
    public DateTime Date { get; set;}
}