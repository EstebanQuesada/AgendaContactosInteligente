using AgendaContactosInteligente.Models;

namespace AgendaContactosInteligente.ViewModels;

public class ContactoBusquedaViewModel
{
    public CriteriosBusqueda Criterios { get; set; } = CriteriosBusqueda.Vacio();
    public IReadOnlyList<int> EtiquetasSeleccionadas => Criterios.EtiquetaIdsNormalizados;
    public IReadOnlyList<Contacto> Resultados { get; set; } = new List<Contacto>();
    public bool SeRealizoBusqueda => Criterios.TieneFiltrosActivos;
}