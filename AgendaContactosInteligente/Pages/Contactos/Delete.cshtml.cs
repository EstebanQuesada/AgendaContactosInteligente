using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class DeleteModel : PageModel
{
    private readonly IContactoService _contactoService;

    public DeleteModel(IContactoService contactoService)
    {
        _contactoService = contactoService;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _contactoService.EliminarAsync(id);
            SuccessMessage = "Contacto eliminado correctamente.";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, DbExceptionHelper.GetFriendlyMessage(ex));
            Contacto = await _contactoService.ObtenerPorIdAsync(id);
            return Page();
        }
    }
}