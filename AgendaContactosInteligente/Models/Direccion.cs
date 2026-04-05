namespace AgendaContactosInteligente.Models;

public class Direccion
{
    public int DireccionID { get; set; }
    public int ContactoID { get; set; }
    public string? Provincia { get; set; }
    public string? Canton { get; set; }
    public string? Distrito { get; set; }
    public string? DireccionExacta { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}