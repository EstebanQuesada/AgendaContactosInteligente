using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Etiquetas;

public class DeleteModel : PageModel
{
    private readonly IEtiquetaService _etiquetaService;

    public DeleteModel(IEtiquetaService etiquetaService)
    {
        _etiquetaService = etiquetaService;
    }

    public Etiqueta? Etiqueta { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Etiqueta = await _etiquetaService.GetByIdAsync(id);
        if (Etiqueta is null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _etiquetaService.DeleteAsync(id);
            SuccessMessage = "Etiqueta eliminada correctamente.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = DbExceptionHelper.GetFriendlyMessage(ex);
            return RedirectToPage("Index");
        }
    }
}