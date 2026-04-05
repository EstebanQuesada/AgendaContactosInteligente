using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class IndexModel : PageModel
{
    private readonly IContactoService _contactoService;

    public IndexModel(IContactoService contactoService)
    {
        _contactoService = contactoService;
    }

    public IReadOnlyList<Contacto> Contactos { get; set; } = new List<Contacto>();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Contactos = await _contactoService.ListAsync();
    }
}