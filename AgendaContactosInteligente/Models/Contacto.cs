using AgendaContactosInteligente.Models;

namespace AgendaContactosInteligente.Models;

public class Contacto
{
    public int ContactoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public bool EsFavorito { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Telefono> Telefonos { get; set; } = new();
    public List<Correo> Correos { get; set; } = new();
    public Direccion? Direccion { get; set; }
    public List<Nota> Notas { get; set; } = new();
}