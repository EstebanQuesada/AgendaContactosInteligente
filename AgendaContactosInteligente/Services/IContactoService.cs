using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.ViewModels;

namespace AgendaContactosInteligente.Services;

public interface IContactoService
{
    Task<IReadOnlyList<Contacto>> ListAsync();
    Task<IReadOnlyList<ResultadoBusqueda>> BuscarAsync(CriteriosBusqueda criterios);
    Task<int> CrearAsync(ContactoFormViewModel model);
    Task ActualizarAsync(ContactoFormViewModel model);
    Task<Contacto?> ObtenerPorIdAsync(int contactoId);
    Task EliminarAsync(int contactoId);
    CriteriosBusqueda NormalizarCriteriosBusqueda(CriteriosBusqueda criterios);
}