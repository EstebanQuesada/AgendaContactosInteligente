using AgendaContactosInteligente.Data;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.ViewModels;

namespace AgendaContactosInteligente.Services;

public class ContactoService : IContactoService
{
    private readonly IContactoRepository _repository;

    public ContactoService(IContactoRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CrearAsync(ContactoFormViewModel model)
    {
        ValidarModelo(model);

        var contactoId = await _repository.CreateAsync(new Contacto
        {
            Nombre = model.Nombre.Trim(),
            Apellido = string.IsNullOrWhiteSpace(model.Apellido) ? null : model.Apellido.Trim(),
            EsFavorito = model.EsFavorito
        });

        await GuardarDatosRelacionadosAsync(contactoId, model, false);
        return contactoId;
    }

    public async Task ActualizarAsync(ContactoFormViewModel model)
    {
        if (!model.ContactoID.HasValue || model.ContactoID.Value <= 0)
            throw new InvalidOperationException("El identificador del contacto es inválido.");

        ValidarModelo(model);

        await _repository.UpdateAsync(new Contacto
        {
            ContactoID = model.ContactoID.Value,
            Nombre = model.Nombre.Trim(),
            Apellido = string.IsNullOrWhiteSpace(model.Apellido) ? null : model.Apellido.Trim(),
            EsFavorito = model.EsFavorito
        });

        await GuardarDatosRelacionadosAsync(model.ContactoID.Value, model, true);
    }

    public Task<Contacto?> ObtenerPorIdAsync(int contactoId)
        => _repository.GetByIdAsync(contactoId);

    public Task EliminarAsync(int contactoId)
        => _repository.DeleteAsync(contactoId);

    private async Task GuardarDatosRelacionadosAsync(int contactoId, ContactoFormViewModel model, bool isEdit)
    {
        var telefono = model.TelefonoNumero?.Trim();
        if (!string.IsNullOrWhiteSpace(telefono))
        {
            var telefonos = await _repository.GetTelefonosByContactoIdAsync(contactoId);
            var actual = telefonos.FirstOrDefault();

            if (actual is null)
            {
                await _repository.CreateTelefonoAsync(new Telefono
                {
                    ContactoID = contactoId,
                    Numero = telefono,
                    Tipo = string.IsNullOrWhiteSpace(model.TelefonoTipo) ? null : model.TelefonoTipo.Trim()
                });
            }
            else
            {
                actual.Numero = telefono;
                actual.Tipo = string.IsNullOrWhiteSpace(model.TelefonoTipo) ? null : model.TelefonoTipo.Trim();
                await _repository.UpdateTelefonoAsync(actual);
            }
        }

        var email = model.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var correos = await _repository.GetCorreosByContactoIdAsync(contactoId);
            var actual = correos.FirstOrDefault();

            if (actual is null)
            {
                await _repository.CreateCorreoAsync(new Correo
                {
                    ContactoID = contactoId,
                    Email = email
                });
            }
            else
            {
                actual.Email = email;
                await _repository.UpdateCorreoAsync(actual);
            }
        }

        var tieneDireccion =
            !string.IsNullOrWhiteSpace(model.Provincia) ||
            !string.IsNullOrWhiteSpace(model.Canton) ||
            !string.IsNullOrWhiteSpace(model.Distrito) ||
            !string.IsNullOrWhiteSpace(model.DireccionExacta);

        if (tieneDireccion)
        {
            await _repository.UpsertDireccionAsync(new Direccion
            {
                ContactoID = contactoId,
                Provincia = model.Provincia?.Trim(),
                Canton = model.Canton?.Trim(),
                Distrito = model.Distrito?.Trim(),
                DireccionExacta = model.DireccionExacta?.Trim()
            });
        }
        else if (isEdit)
        {
            await _repository.DeleteDireccionByContactoIdAsync(contactoId);
        }

        var nota = model.NotaContenido?.Trim();
        if (!string.IsNullOrWhiteSpace(nota))
        {
            var notas = await _repository.GetNotasByContactoIdAsync(contactoId);
            var actual = notas.FirstOrDefault();

            if (actual is null)
            {
                await _repository.CreateNotaAsync(new Nota
                {
                    ContactoID = contactoId,
                    Contenido = nota
                });
            }
            else
            {
                actual.Contenido = nota;
                await _repository.UpdateNotaAsync(actual);
            }
        }
    }

    private static void ValidarModelo(ContactoFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre))
            throw new InvalidOperationException("El nombre es obligatorio.");

        if (!string.IsNullOrWhiteSpace(model.TelefonoNumero))
        {
            var telefono = model.TelefonoNumero.Trim();
            if (telefono.Length < 8 || telefono.Length > 20)
                throw new InvalidOperationException("El teléfono debe tener entre 8 y 20 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(model.Email) && model.Email.Length > 150)
            throw new InvalidOperationException("El correo no puede exceder 150 caracteres.");
    }
}