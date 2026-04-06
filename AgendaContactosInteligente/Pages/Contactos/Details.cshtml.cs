using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class DetailsModel : PageModel
{
    private readonly IContactoService _contactoService;
    private readonly IEtiquetaService _etiquetaService;

    public DetailsModel(IContactoService contactoService, IEtiquetaService etiquetaService)
    {
        _contactoService = contactoService;
        _etiquetaService = etiquetaService;
    }

    public Contacto? Contacto { get; set; }
    public IReadOnlyList<Etiqueta> EtiquetasContacto { get; set; } = new List<Etiqueta>();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Contacto = await _contactoService.ObtenerPorIdAsync(id);
        if (Contacto is null)
            return NotFound();

        EtiquetasContacto = await _etiquetaService.GetEtiquetasByContactoIdAsync(id);

        return Page();
    }
}