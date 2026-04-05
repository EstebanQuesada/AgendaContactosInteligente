using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Services;
using AgendaContactosInteligente.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class CreateModel : PageModel
{
    private readonly IContactoService _contactoService;

    public CreateModel(IContactoService contactoService)
    {
        _contactoService = contactoService;
    }

    [BindProperty]
    public ContactoFormViewModel Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var contactoId = await _contactoService.CrearAsync(Input);
            SuccessMessage = "Contacto creado correctamente.";
            return RedirectToPage("Details", new { id = contactoId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, DbExceptionHelper.GetFriendlyMessage(ex));
            return Page();
        }
    }
}