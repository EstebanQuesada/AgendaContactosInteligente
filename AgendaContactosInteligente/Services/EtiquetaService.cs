using AgendaContactosInteligente.Data;
using AgendaContactosInteligente.Models;

namespace AgendaContactosInteligente.Services;

public class EtiquetaService : IEtiquetaService
{
    private readonly IEtiquetaRepository _etiquetaRepository;
    private readonly IContactoRepository _contactoRepository;

    public EtiquetaService(
        IEtiquetaRepository etiquetaRepository,
        IContactoRepository contactoRepository)
    {
        _etiquetaRepository = etiquetaRepository;
        _contactoRepository = contactoRepository;
    }

    public async Task<IReadOnlyList<Etiqueta>> ListAsync(bool soloActivas = true)
    {
        return await _etiquetaRepository.ListAsync(soloActivas);
    }

    public async Task<Etiqueta?> GetByIdAsync(int etiquetaId)
    {
        ValidateId(etiquetaId, nameof(etiquetaId));
        return await _etiquetaRepository.GetByIdAsync(etiquetaId);
    }

    public async Task<int> CreateAsync(Etiqueta etiqueta)
    {
        ValidateEtiquetaForCreateOrUpdate(etiqueta);
        etiqueta.Nombre = NormalizeNombre(etiqueta.Nombre);
        etiqueta.ColorHex = NormalizeColor(etiqueta.ColorHex);

        return await _etiquetaRepository.CreateAsync(etiqueta);
    }

    public async Task UpdateAsync(Etiqueta etiqueta)
    {
        ValidateEtiquetaForCreateOrUpdate(etiqueta);
        ValidateId(etiqueta.EtiquetaID, nameof(etiqueta.EtiquetaID));

        etiqueta.Nombre = NormalizeNombre(etiqueta.Nombre);
        etiqueta.ColorHex = NormalizeColor(etiqueta.ColorHex);

        await _etiquetaRepository.UpdateAsync(etiqueta);
    }

    public async Task DeleteAsync(int etiquetaId)
    {
        ValidateId(etiquetaId, nameof(etiquetaId));
        await _etiquetaRepository.DeleteAsync(etiquetaId);
    }

    public async Task AssignToContactoAsync(int contactoId, int etiquetaId)
    {
        ValidateId(contactoId, nameof(contactoId));
        ValidateId(etiquetaId, nameof(etiquetaId));

        await EnsureContactoExistsAsync(contactoId);
        await _etiquetaRepository.AssignToContactoAsync(contactoId, etiquetaId);
    }

    public async Task RemoveFromContactoAsync(int contactoId, int etiquetaId)
    {
        ValidateId(contactoId, nameof(contactoId));
        ValidateId(etiquetaId, nameof(etiquetaId));

        await EnsureContactoExistsAsync(contactoId);
        await _etiquetaRepository.RemoveFromContactoAsync(contactoId, etiquetaId);
    }

    public async Task<IReadOnlyList<Etiqueta>> GetEtiquetasByContactoIdAsync(int contactoId)
    {
        ValidateId(contactoId, nameof(contactoId));
        await EnsureContactoExistsAsync(contactoId);

        return await _etiquetaRepository.GetEtiquetasByContactoIdAsync(contactoId);
    }

    public async Task<IReadOnlyList<Contacto>> GetContactosAsync(int? etiquetaId = null)
    {
        if (etiquetaId.HasValue)
        {
            ValidateId(etiquetaId.Value, nameof(etiquetaId));
        }

        return await _etiquetaRepository.GetContactosAsync(etiquetaId);
    }

    public async Task SyncEtiquetasContactoAsync(int contactoId, IEnumerable<int>? etiquetaIds)
    {
        ValidateId(contactoId, nameof(contactoId));
        await EnsureContactoExistsAsync(contactoId);

        var etiquetasObjetivo = (etiquetaIds ?? Enumerable.Empty<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();

        var etiquetasActuales = await _etiquetaRepository.GetEtiquetasByContactoIdAsync(contactoId);
        var actualesIds = etiquetasActuales
            .Select(e => e.EtiquetaID)
            .ToHashSet();

        var etiquetasParaAgregar = etiquetasObjetivo.Except(actualesIds).ToList();
        var etiquetasParaRemover = actualesIds.Except(etiquetasObjetivo).ToList();

        foreach (var etiquetaId in etiquetasParaAgregar)
        {
            await _etiquetaRepository.AssignToContactoAsync(contactoId, etiquetaId);
        }

        foreach (var etiquetaId in etiquetasParaRemover)
        {
            await _etiquetaRepository.RemoveFromContactoAsync(contactoId, etiquetaId);
        }
    }

    private async Task EnsureContactoExistsAsync(int contactoId)
    {
        var contacto = await _contactoRepository.GetByIdAsync(contactoId);
        if (contacto is null)
            throw new InvalidOperationException("El contacto indicado no existe.");
    }

    private static void ValidateEtiquetaForCreateOrUpdate(Etiqueta? etiqueta)
    {
        if (etiqueta is null)
            throw new ArgumentNullException(nameof(etiqueta), "La etiqueta no puede ser nula.");

        if (string.IsNullOrWhiteSpace(etiqueta.Nombre))
            throw new ArgumentException("El nombre de la etiqueta es obligatorio.", nameof(etiqueta.Nombre));

        var nombreNormalizado = etiqueta.Nombre.Trim();

        if (nombreNormalizado.Length > 100)
            throw new ArgumentException("El nombre de la etiqueta no puede exceder los 100 caracteres.", nameof(etiqueta.Nombre));

        if (string.IsNullOrWhiteSpace(etiqueta.ColorHex))
            throw new ArgumentException("El color de la etiqueta es obligatorio.", nameof(etiqueta.ColorHex));

        var colorNormalizado = etiqueta.ColorHex.Trim().ToUpperInvariant();

        if (!System.Text.RegularExpressions.Regex.IsMatch(colorNormalizado, "^#[0-9A-F]{6}$"))
            throw new ArgumentException("El color de la etiqueta debe tener un formato hexadecimal válido.", nameof(etiqueta.ColorHex));
    }

    private static void ValidateId(int id, string paramName)
    {
        if (id <= 0)
            throw new ArgumentException("El identificador proporcionado no es válido.", paramName);
    }

    private static string NormalizeNombre(string nombre)
    {
        return nombre.Trim();
    }

    private static string NormalizeColor(string colorHex)
    {
        return colorHex.Trim().ToUpperInvariant();
    }
}