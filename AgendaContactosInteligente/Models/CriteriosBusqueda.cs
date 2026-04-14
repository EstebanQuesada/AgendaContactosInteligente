using System.ComponentModel.DataAnnotations;

namespace AgendaContactosInteligente.Models;

public sealed class CriteriosBusqueda : IValidatableObject
{
    public const int TextoBusquedaMaxLength = 200;
    public const int ProvinciaMaxLength = 100;
    public const int MaximoEtiquetasPermitidas = 50;

    [StringLength(
        TextoBusquedaMaxLength,
        ErrorMessage = "El texto de búsqueda no puede exceder los 200 caracteres.")]
    public string? TextoBusqueda { get; set; }

    public List<int> EtiquetaIds { get; set; } = new();

    [StringLength(
        ProvinciaMaxLength,
        ErrorMessage = "La provincia no puede exceder los 100 caracteres.")]
    public string? Provincia { get; set; }

    public bool? TieneCorreo { get; set; }

    public bool? Favoritos { get; set; }

    public bool TieneFiltrosActivos =>
        !string.IsNullOrWhiteSpace(TextoBusqueda) ||
        EtiquetaIdsNormalizados.Count > 0 ||
        !string.IsNullOrWhiteSpace(Provincia) ||
        TieneCorreo.HasValue ||
        Favoritos.HasValue;

    public IReadOnlyList<int> EtiquetaIdsNormalizados =>
        EtiquetaIds
            .Where(id => id > 0)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

    public bool DebeFiltrarPorEtiquetas => EtiquetaIdsNormalizados.Count > 0;

    public bool DebeFiltrarPorTexto => !string.IsNullOrWhiteSpace(TextoBusqueda);

    public bool DebeFiltrarPorProvincia => !string.IsNullOrWhiteSpace(Provincia);

    public CriteriosBusqueda ObtenerNormalizado()
    {
        return new CriteriosBusqueda
        {
            TextoBusqueda = NormalizarTexto(TextoBusqueda),
            Provincia = NormalizarTexto(Provincia),
            TieneCorreo = TieneCorreo,
            Favoritos = Favoritos,
            EtiquetaIds = EtiquetaIds
                .Where(id => id > 0)
                .Distinct()
                .OrderBy(id => id)
                .ToList()
        };
    }

    public CriteriosBusqueda ValidarYObtenerNormalizado()
    {
        var normalizado = ObtenerNormalizado();

        var context = new ValidationContext(normalizado);
        Validator.ValidateObject(normalizado, context, validateAllProperties: true);

        var errores = normalizado.Validate(context).ToList();
        if (errores.Count > 0)
        {
            throw new ValidationException(errores.First().ErrorMessage);
        }

        return normalizado;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TextoBusqueda is not null && string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            yield return new ValidationResult(
                "El texto de búsqueda no puede contener únicamente espacios.",
                new[] { nameof(TextoBusqueda) });
        }

        if (Provincia is not null && string.IsNullOrWhiteSpace(Provincia))
        {
            yield return new ValidationResult(
                "La provincia no puede contener únicamente espacios.",
                new[] { nameof(Provincia) });
        }

        if (EtiquetaIds.Count > MaximoEtiquetasPermitidas)
        {
            yield return new ValidationResult(
                $"No se permite seleccionar más de {MaximoEtiquetasPermitidas} etiquetas para una búsqueda.",
                new[] { nameof(EtiquetaIds) });
        }

        if (EtiquetaIds.Any(id => id <= 0))
        {
            yield return new ValidationResult(
                "La lista de etiquetas contiene identificadores inválidos.",
                new[] { nameof(EtiquetaIds) });
        }
    }

    public static CriteriosBusqueda Vacio() => new();

    private static string? NormalizarTexto(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.Trim();
    }
}