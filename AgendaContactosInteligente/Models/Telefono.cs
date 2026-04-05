namespace AgendaContactosInteligente.Models;

public class Telefono
{
    public int TelefonoID { get; set; }
    public int ContactoID { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
