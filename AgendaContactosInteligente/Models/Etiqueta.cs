using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AgendaContactosInteligente.Models;

public class Etiqueta
{
    private static readonly Regex HexColorRegex = new("^#([A-Fa-f0-9]{6})$", RegexOptions.Compiled);

    public int EtiquetaID { get; set; }

    [Required(ErrorMessage = "El nombre de la etiqueta es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre de la etiqueta no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El color de la etiqueta es obligatorio.")]
    [StringLength(7, MinimumLength = 7, ErrorMessage = "El color debe tener formato hexadecimal, por ejemplo #4F46E5.")]
    public string ColorHex { get; set; } = "#4F46E5";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CantidadContactos { get; set; }

    public bool IsColorHexValido()
    {
        return !string.IsNullOrWhiteSpace(ColorHex) && HexColorRegex.IsMatch(ColorHex.Trim());
    }
}