using System.ComponentModel.DataAnnotations;

namespace AgendaContactosInteligente.ViewModels;

public class ContactoFormViewModel
{
    public int? ContactoID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres.")]
    public string? Apellido { get; set; }

    public bool EsFavorito { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string? TelefonoNumero { get; set; }

    [StringLength(50, ErrorMessage = "El tipo de teléfono no puede exceder 50 caracteres.")]
    public string? TelefonoTipo { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? Provincia { get; set; }

    [StringLength(100)]
    public string? Canton { get; set; }

    [StringLength(100)]
    public string? Distrito { get; set; }

    [StringLength(300)]
    public string? DireccionExacta { get; set; }

    public string? NotaContenido { get; set; }
}