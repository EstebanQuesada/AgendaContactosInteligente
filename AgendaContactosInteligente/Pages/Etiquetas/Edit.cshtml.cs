using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Etiquetas;

public class EditModel : PageModel
{
    private readonly IEtiquetaService _etiquetaService;

    public EditModel(IEtiquetaService etiquetaService)
    {
        _etiquetaService = etiquetaService;
    }

    [BindProperty]
    public Etiqueta Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var etiqueta = await _etiquetaService.GetByIdAsync(id);
        if (etiqueta is null)
            return NotFound();

        Input = etiqueta;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _etiquetaService.UpdateAsync(Input);
            SuccessMessage = "Etiqueta actualizada correctamente.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, DbExceptionHelper.GetFriendlyMessage(ex));
            return Page();
        }
    }
}