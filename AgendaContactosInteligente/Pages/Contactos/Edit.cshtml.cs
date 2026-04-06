using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using AgendaContactosInteligente.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class EditModel : PageModel
{
    private readonly IContactoService _contactoService;
    private readonly IEtiquetaService _etiquetaService;

    public EditModel(IContactoService contactoService, IEtiquetaService etiquetaService)
    {
        _contactoService = contactoService;
        _etiquetaService = etiquetaService;
    }

    [BindProperty]
    public ContactoFormViewModel Input { get; set; } = new();

    [BindProperty]
    public List<int> SelectedEtiquetaIds { get; set; } = new();

    public IReadOnlyList<Etiqueta> EtiquetasDisponibles { get; set; } = new List<Etiqueta>();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var contacto = await _contactoService.ObtenerPorIdAsync(id);
        if (contacto is null)
            return NotFound();

        Input = new ContactoFormViewModel
        {
            ContactoID = contacto.ContactoID,
            Nombre = contacto.Nombre,
            Apellido = contacto.Apellido,
            EsFavorito = contacto.EsFavorito,
            TelefonoNumero = contacto.Telefonos.FirstOrDefault()?.Numero,
            TelefonoTipo = contacto.Telefonos.FirstOrDefault()?.Tipo,
            Email = contacto.Correos.FirstOrDefault()?.Email,
            Provincia = contacto.Direccion?.Provincia,
            Canton = contacto.Direccion?.Canton,
            Distrito = contacto.Direccion?.Distrito,
            DireccionExacta = contacto.Direccion?.DireccionExacta,
            NotaContenido = contacto.Notas.FirstOrDefault()?.Contenido
        };

        EtiquetasDisponibles = await _etiquetaService.ListAsync();
        SelectedEtiquetaIds = (await _etiquetaService.GetEtiquetasByContactoIdAsync(id))
            .Select(e => e.EtiquetaID)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadEtiquetasDisponiblesAsync();

        if (!Input.ContactoID.HasValue || Input.ContactoID.Value <= 0)
        {
            ModelState.AddModelError(string.Empty, "El identificador del contacto es inválido.");
            return Page();
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _contactoService.ActualizarAsync(Input);
            await _etiquetaService.SyncEtiquetasContactoAsync(Input.ContactoID.Value, SelectedEtiquetaIds);

            SuccessMessage = "Contacto actualizado correctamente.";
            return RedirectToPage("Details", new { id = Input.ContactoID });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, DbExceptionHelper.GetFriendlyMessage(ex));
            return Page();
        }
    }

    private async Task LoadEtiquetasDisponiblesAsync()
    {
        EtiquetasDisponibles = await _etiquetaService.ListAsync();
    }
}