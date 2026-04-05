using AgendaContactosInteligente.Helpers;
using AgendaContactosInteligente.Services;
using AgendaContactosInteligente.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class EditModel : PageModel
{
    private readonly IContactoService _contactoService;

    public EditModel(IContactoService contactoService)
    {
        _contactoService = contactoService;
    }

    [BindProperty]
    public ContactoFormViewModel Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var contacto = await _contactoService.ObtenerPorIdAsync(id);
        if (contacto is null)
            return NotFound();

        Input = new ContactoFormViewModel
        {
            ContactoID = contacto.ContactoID,
            Nombre = contacto.Nombre,
            Apellido = contacto.Apellido,
            EsFavorito = contacto.EsFavorito,
            TelefonoNumero = contacto.Telefonos.FirstOrDefault()?.Numero,
            TelefonoTipo = contacto.Telefonos.FirstOrDefault()?.Tipo,
            Email = contacto.Correos.FirstOrDefault()?.Email,
            Provincia = contacto.Direccion?.Provincia,
            Canton = contacto.Direccion?.Canton,
            Distrito = contacto.Direccion?.Distrito,
            DireccionExacta = contacto.Direccion?.DireccionExacta,
            NotaContenido = contacto.Notas.FirstOrDefault()?.Contenido
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _contactoService.ActualizarAsync(Input);
            SuccessMessage = "Contacto actualizado correctamente.";
            return RedirectToPage("Details", new { id = Input.ContactoID });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, DbExceptionHelper.GetFriendlyMessage(ex));
            return Page();
        }
    }
}