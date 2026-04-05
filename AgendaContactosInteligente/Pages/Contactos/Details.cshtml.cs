using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class DetailsModel : PageModel
{
    private readonly IContactoService _contactoService;

    public DetailsModel(IContactoService contactoService)
    {
        _contactoService = contactoService;
    }

    public Contacto? Contacto { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Contacto = await _contactoService.ObtenerPorIdAsync(id);
        if (Contacto is null)
            return NotFound();

        return Page();
    }
}