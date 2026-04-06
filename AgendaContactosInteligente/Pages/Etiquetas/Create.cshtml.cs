using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Etiquetas;

public class CreateModel : PageModel
{
    private readonly IEtiquetaService _etiquetaService;

    public CreateModel(IEtiquetaService etiquetaService)
    {
        _etiquetaService = etiquetaService;
    }

    [BindProperty]
    public Etiqueta Input { get; set; } = new();

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
            var etiquetaId = await _etiquetaService.CreateAsync(Input);
            SuccessMessage = "Etiqueta creada correctamente.";
            return RedirectToPage("Edit", new { id = etiquetaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, DbExceptionHelper.GetFriendlyMessage(ex));
            return Page();
        }
    }
}