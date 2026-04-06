using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Etiquetas;

public class IndexModel : PageModel
{
    private readonly IEtiquetaService _etiquetaService;

    public IndexModel(IEtiquetaService etiquetaService)
    {
        _etiquetaService = etiquetaService;
    }

    public IReadOnlyList<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Etiquetas = await _etiquetaService.ListAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = DbExceptionHelper.GetFriendlyMessage(ex);
            Etiquetas = new List<Etiqueta>();
        }
    }
}