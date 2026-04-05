namespace AgendaContactosInteligente.Models;

public class Nota
{
    public int NotaID { get; set; }
    public int ContactoID { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}