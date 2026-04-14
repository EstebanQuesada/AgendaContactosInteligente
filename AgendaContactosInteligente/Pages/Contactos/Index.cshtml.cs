using AgendaContactosInteligente.Models;
using AgendaContactosInteligente.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgendaContactosInteligente.Pages.Contactos;

public class IndexModel : PageModel
{
    private readonly IContactoService _contactoService;
    private readonly IEtiquetaService _etiquetaService;

    public IndexModel(IContactoService contactoService, IEtiquetaService etiquetaService)
    {
        _contactoService = contactoService ?? throw new ArgumentNullException(nameof(contactoService));
        _etiquetaService = etiquetaService ?? throw new ArgumentNullException(nameof(etiquetaService));
    }

    public IReadOnlyList<Contacto> Contactos { get; set; } = new List<Contacto>();
    public IReadOnlyList<ResultadoBusqueda> ResultadosBusqueda { get; set; } = new List<ResultadoBusqueda>();
    public IReadOnlyList<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();

    [BindProperty(SupportsGet = true)]
    public string? TextoBusqueda { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<int> EtiquetaIds { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Provincia { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? TieneCorreo { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? Favoritos { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; } = 10;
    public int TotalResultados { get; set; }
    public int TotalPaginas { get; set; }

    public int TotalAltaCoincidencia { get; set; }
    public int TotalMediaCoincidencia { get; set; }
    public int TotalBajaCoincidencia { get; set; }

    public CriteriosBusqueda CriteriosAplicados { get; set; } = CriteriosBusqueda.Vacio();
    public IReadOnlyList<Etiqueta> EtiquetasSeleccionadas { get; set; } = new List<Etiqueta>();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Etiquetas = await _etiquetaService.ListAsync();

        try
        {
            var criterios = new CriteriosBusqueda
            {
                TextoBusqueda = TextoBusqueda,
                EtiquetaIds = EtiquetaIds ?? new List<int>(),
                Provincia = Provincia,
                TieneCorreo = TieneCorreo,
                Favoritos = Favoritos
            };

            CriteriosAplicados = _contactoService.NormalizarCriteriosBusqueda(criterios);

            IReadOnlyList<ResultadoBusqueda> resultadosCompletos;

            if (CriteriosAplicados.TieneFiltrosActivos)
            {
                resultadosCompletos = await _contactoService.BuscarAsync(CriteriosAplicados);
            }
            else
            {
                Contactos = await _contactoService.ListAsync();

                var cargaEtiquetas = Contactos.Select(async contacto =>
                {
                    if (contacto.Etiquetas is null || contacto.Etiquetas.Count == 0)
                    {
                        contacto.Etiquetas = (await _etiquetaService.GetEtiquetasByContactoIdAsync(contacto.ContactoID)).ToList();
                    }

                    return contacto;
                });

                Contactos = (await Task.WhenAll(cargaEtiquetas)).ToList();

                resultadosCompletos = Contactos.Select(c => new ResultadoBusqueda
                {
                    Contacto = c,
                    PuntajeCoincidencia = 100d,
                    RankingInterno = 1000d,
                    CamposCoincidencia = new List<string>(),
                    MotivoPrincipal = "Coincidencia por filtros aplicados"
                }).ToList();
            }

            EtiquetasSeleccionadas = Etiquetas
                .Where(e => CriteriosAplicados.EtiquetaIdsNormalizados.Contains(e.EtiquetaID))
                .OrderBy(e => e.Nombre)
                .ToList();

            TextoBusqueda = CriteriosAplicados.TextoBusqueda;
            EtiquetaIds = CriteriosAplicados.EtiquetaIdsNormalizados.ToList();
            Provincia = CriteriosAplicados.Provincia;
            TieneCorreo = CriteriosAplicados.TieneCorreo;
            Favoritos = CriteriosAplicados.Favoritos;

            TotalResultados = resultadosCompletos.Count;
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(TotalResultados / (double)PageSize));

            if (PageNumber <= 0)
                PageNumber = 1;

            if (PageNumber > TotalPaginas)
                PageNumber = TotalPaginas;

            TotalAltaCoincidencia = resultadosCompletos.Count(r => r.NivelCoincidencia == "Alta");
            TotalMediaCoincidencia = resultadosCompletos.Count(r => r.NivelCoincidencia == "Media");
            TotalBajaCoincidencia = resultadosCompletos.Count(r => r.NivelCoincidencia == "Baja");

            ResultadosBusqueda = resultadosCompletos
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Contactos = ResultadosBusqueda.Select(r => r.Contacto).ToList();

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Contactos = new List<Contacto>();
            ResultadosBusqueda = new List<ResultadoBusqueda>();
            EtiquetasSeleccionadas = new List<Etiqueta>();
            CriteriosAplicados = CriteriosBusqueda.Vacio();
            TotalResultados = 0;
            TotalPaginas = 1;
            PageNumber = 1;
            TotalAltaCoincidencia = 0;
            TotalMediaCoincidencia = 0;
            TotalBajaCoincidencia = 0;
            return Page();
        }
    }

    public bool TieneFiltrosActivos()
    {
        return CriteriosAplicados.TieneFiltrosActivos;
    }

    public bool TieneBusquedaPorTexto()
    {
        return !string.IsNullOrWhiteSpace(CriteriosAplicados.TextoBusqueda);
    }

    public string ObtenerTextoTieneCorreo()
    {
        return TieneCorreo switch
        {
            true => "Con correo",
            false => "Sin correo",
            _ => string.Empty
        };
    }

    public string ObtenerTextoFavoritos()
    {
        return Favoritos switch
        {
            true => "Solo favoritos",
            false => "Solo no favoritos",
            _ => string.Empty
        };
    }

    public string ObtenerResumenResultados()
    {
        if (!TieneFiltrosActivos())
            return $"Mostrando {TotalResultados} contacto(s) registrados.";

        if (TieneBusquedaPorTexto())
            return $"Se encontraron {TotalResultados} resultado(s) ordenados por coincidencia y relevancia.";

        return $"Se encontraron {TotalResultados} resultado(s) según los filtros aplicados.";
    }

    public bool TienePaginacion()
    {
        return TotalPaginas > 1;
    }

    public int PaginaAnterior()
    {
        return Math.Max(1, PageNumber - 1);
    }

    public int PaginaSiguiente()
    {
        return Math.Min(TotalPaginas, PageNumber + 1);
    }
}