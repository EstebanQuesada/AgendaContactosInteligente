using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class IndexModel : PageModel
{
    private readonly IContactoService _contactoService;
    private readonly IEtiquetaService _etiquetaService;

    public IndexModel(IContactoService contactoService, IEtiquetaService etiquetaService)
    {
        _contactoService = contactoService;
        _etiquetaService = etiquetaService;
    }

    public IReadOnlyList<Contacto> Contactos { get; set; } = new List<Contacto>();
    public IReadOnlyList<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();

    [BindProperty(SupportsGet = true)]
    public int? EtiquetaId { get; set; }

    public string? EtiquetaNombreSeleccionada { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Etiquetas = await _etiquetaService.ListAsync();

        if (EtiquetaId.HasValue && !Etiquetas.Any(e => e.EtiquetaID == EtiquetaId.Value))
            return NotFound();

        var contactos = EtiquetaId.HasValue
            ? await _etiquetaService.GetContactosAsync(EtiquetaId.Value)
            : await _contactoService.ListAsync();

        var cargaEtiquetas = contactos.Select(async contacto =>
        {
            contacto.Etiquetas = (await _etiquetaService.GetEtiquetasByContactoIdAsync(contacto.ContactoID)).ToList();
            return contacto;
        });

        Contactos = (await Task.WhenAll(cargaEtiquetas)).ToList();

        EtiquetaNombreSeleccionada = EtiquetaId.HasValue
            ? Etiquetas.FirstOrDefault(e => e.EtiquetaID == EtiquetaId.Value)?.Nombre
            : null;

        return Page();
    }
}