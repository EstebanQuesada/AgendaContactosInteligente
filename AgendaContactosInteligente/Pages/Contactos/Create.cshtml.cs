using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using AgendaContactosInteligente.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class CreateModel : PageModel
{
    private readonly IContactoService _contactoService;
    private readonly IEtiquetaService _etiquetaService;

    public CreateModel(IContactoService contactoService, IEtiquetaService etiquetaService)
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

    public async Task OnGetAsync()
    {
        await LoadEtiquetasDisponiblesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadEtiquetasDisponiblesAsync();

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var contactoId = await _contactoService.CrearAsync(Input);
            await _etiquetaService.SyncEtiquetasContactoAsync(contactoId, SelectedEtiquetaIds);

            SuccessMessage = "Contacto creado correctamente.";
            return RedirectToPage("Details", new { id = contactoId });
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