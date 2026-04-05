namespace AgendaContactosInteligente.Models;

public class Correo
{
    public int CorreoID { get; set; }
    public int ContactoID { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}