using System.ComponentModel.DataAnnotations;

namespace AgendaContactosInteligente.Models;

public sealed class ResultadoBusqueda
{
    [Required]
    public Contacto Contacto { get; set; } = new();

    [Range(0, 100)]
    public double PuntajeCoincidencia { get; set; }

    public double RankingInterno { get; set; }

    public List<string> CamposCoincidencia { get; set; } = new();

    public string MotivoPrincipal { get; set; } = string.Empty;

    public bool TieneCoincidencias => CamposCoincidencia.Count > 0;

    public string PuntajeFormateado => $"{Math.Round(PuntajeCoincidencia, 0)}%";

    public string NivelCoincidencia =>
        PuntajeCoincidencia >= 85 ? "Alta" :
        PuntajeCoincidencia >= 65 ? "Media" :
        "Baja";

    public string NivelCoincidenciaTexto =>
        PuntajeCoincidencia >= 85 ? "Alta coincidencia" :
        PuntajeCoincidencia >= 65 ? "Media coincidencia" :
        "Baja coincidencia";

    public IReadOnlyList<string> CamposCoincidenciaOrdenados =>
        CamposCoincidencia
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();
}