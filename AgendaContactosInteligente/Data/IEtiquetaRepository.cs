using AgendaContactosInteligente.Models;

namespace AgendaContactosInteligente.Data;

public interface IEtiquetaRepository
{
    Task<IReadOnlyList<Etiqueta>> ListAsync(bool soloActivas = true);
    Task<Etiqueta?> GetByIdAsync(int etiquetaId);
    Task<int> CreateAsync(Etiqueta etiqueta);
    Task UpdateAsync(Etiqueta etiqueta);
    Task DeleteAsync(int etiquetaId);

    Task AssignToContactoAsync(int contactoId, int etiquetaId);
    Task RemoveFromContactoAsync(int contactoId, int etiquetaId);

    Task<IReadOnlyList<Etiqueta>> GetEtiquetasByContactoIdAsync(int contactoId);
    Task<IReadOnlyList<Contacto>> GetContactosAsync(int? etiquetaId = null);
}