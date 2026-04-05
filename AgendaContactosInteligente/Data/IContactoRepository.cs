using AgendaContactosInteligente.Models;

namespace AgendaContactosInteligente.Data;

public interface IContactoRepository
{
    Task<Contacto?> GetByIdAsync(int contactoId);
    Task<int> CreateAsync(Contacto contacto);
    Task UpdateAsync(Contacto contacto);
    Task DeleteAsync(int contactoId);

    Task<IReadOnlyList<Telefono>> GetTelefonosByContactoIdAsync(int contactoId);
    Task<int> CreateTelefonoAsync(Telefono telefono);
    Task UpdateTelefonoAsync(Telefono telefono);
    Task DeleteTelefonoAsync(int telefonoId);

    Task<IReadOnlyList<Correo>> GetCorreosByContactoIdAsync(int contactoId);
    Task<int> CreateCorreoAsync(Correo correo);
    Task UpdateCorreoAsync(Correo correo);
    Task DeleteCorreoAsync(int correoId);

    Task<Direccion?> GetDireccionByContactoIdAsync(int contactoId);
    Task UpsertDireccionAsync(Direccion direccion);
    Task DeleteDireccionByContactoIdAsync(int contactoId);

    Task<IReadOnlyList<Nota>> GetNotasByContactoIdAsync(int contactoId);
    Task<int> CreateNotaAsync(Nota nota);
    Task UpdateNotaAsync(Nota nota);
    Task DeleteNotaAsync(int notaId);
}