using System.Globalization;
using System.Text;
using AgendaContactosInteligente.Models;

namespace AgendaContactosInteligente.Services;

public sealed class MotorBusquedaInteligente
{
    private const double UmbralSimilitudMinimo = 0.58d;

    private static readonly Dictionary<string, double> PesosCampo = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Nombre"] = 1.00d,
        ["Apellido"] = 0.82d,
        ["Nombre completo"] = 1.12d,
        ["Nombre invertido"] = 1.04d,
        ["Teléfono"] = 0.92d,
        ["Correo"] = 0.90d,
        ["Provincia"] = 0.72d,
        ["Nota"] = 0.45d,
        ["Etiqueta"] = 0.76d,
        ["Cantón"] = 0.60d,
        ["Distrito"] = 0.56d,
        ["Dirección"] = 0.50d,
        ["Tipo de teléfono"] = 0.42d
    };

    public IReadOnlyList<ResultadoBusqueda> Buscar(IEnumerable<Contacto> contactos, CriteriosBusqueda criterios)
    {
        if (contactos is null)
            throw new InvalidOperationException("La colección de contactos es obligatoria.");

        if (criterios is null)
            throw new InvalidOperationException("Los criterios de búsqueda son obligatorios.");

        var criteriosNormalizados = criterios.ValidarYObtenerNormalizado();
        IEnumerable<Contacto> query = contactos.Where(c => c is not null);

        if (criteriosNormalizados.DebeFiltrarPorProvincia)
        {
            var provincia = criteriosNormalizados.Provincia!;
            query = query.Where(c => CoincideTextoExactoNormalizado(c.Direccion?.Provincia, provincia));
        }

        if (criteriosNormalizados.TieneCorreo.HasValue)
        {
            query = criteriosNormalizados.TieneCorreo.Value
                ? query.Where(c => c.Correos.Any())
                : query.Where(c => !c.Correos.Any());
        }

        if (criteriosNormalizados.Favoritos.HasValue)
        {
            query = criteriosNormalizados.Favoritos.Value
                ? query.Where(c => c.EsFavorito)
                : query.Where(c => !c.EsFavorito);
        }

        if (criteriosNormalizados.DebeFiltrarPorEtiquetas)
        {
            var etiquetas = criteriosNormalizados.EtiquetaIdsNormalizados.ToHashSet();

            query = query.Where(c =>
                c.Etiquetas.Any() &&
                etiquetas.All(id => c.Etiquetas.Any(et => et.EtiquetaID == id)));
        }

        if (!criteriosNormalizados.DebeFiltrarPorTexto)
        {
            return query
                .OrderByDescending(c => c.EsFavorito)
                .ThenBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .ThenByDescending(c => c.FechaCreacion)
                .Select(c => new ResultadoBusqueda
                {
                    Contacto = c,
                    PuntajeCoincidencia = 100,
                    RankingInterno = 1000,
                    CamposCoincidencia = new List<string>(),
                    MotivoPrincipal = "Coincidencia por filtros aplicados"
                })
                .ToList();
        }

        var textoBusqueda = criteriosNormalizados.TextoBusqueda!;

        return query
            .Select(contacto => ConstruirResultado(contacto, textoBusqueda))
            .Where(r => r is not null)
            .Select(r => r!)
            .OrderByDescending(r => r.RankingInterno)
            .ThenByDescending(r => r.PuntajeCoincidencia)
            .ThenByDescending(r => r.Contacto.EsFavorito)
            .ThenBy(r => r.Contacto.Nombre)
            .ThenBy(r => r.Contacto.Apellido)
            .ThenByDescending(r => r.Contacto.FechaCreacion)
            .ToList();
    }

    public double CalcularSimilitud(string? origen, string? terminoBusqueda)
    {
        var textoOrigen = NormalizarTexto(origen);
        var textoBusquedaNormalizado = NormalizarTexto(terminoBusqueda);

        if (string.IsNullOrWhiteSpace(textoOrigen) || string.IsNullOrWhiteSpace(textoBusquedaNormalizado))
            return 0d;

        if (textoOrigen == textoBusquedaNormalizado)
            return 1d;

        if (textoOrigen.StartsWith(textoBusquedaNormalizado, StringComparison.Ordinal))
            return 0.96d;

        if (textoOrigen.Contains(textoBusquedaNormalizado, StringComparison.Ordinal))
            return 0.88d;

        var tokensOrigen = Tokenizar(textoOrigen);
        var tokensBusqueda = Tokenizar(textoBusquedaNormalizado);

        var mejorToken = 0d;
        var sumaMejores = 0d;
        var coincidenciasToken = 0;

        foreach (var tokenBusqueda in tokensBusqueda)
        {
            var mejorParaToken = 0d;

            foreach (var tokenOrigen in tokensOrigen)
            {
                if (tokenOrigen == tokenBusqueda)
                {
                    mejorParaToken = Math.Max(mejorParaToken, 1d);
                    continue;
                }

                if (tokenOrigen.StartsWith(tokenBusqueda, StringComparison.Ordinal))
                {
                    mejorParaToken = Math.Max(mejorParaToken, 0.95d);
                    continue;
                }

                if (tokenOrigen.Contains(tokenBusqueda, StringComparison.Ordinal) ||
                    tokenBusqueda.Contains(tokenOrigen, StringComparison.Ordinal))
                {
                    mejorParaToken = Math.Max(mejorParaToken, 0.84d);
                    continue;
                }

                var distanciaToken = CalcularDistanciaLevenshtein(tokenOrigen, tokenBusqueda);
                var longitudMaximaToken = Math.Max(tokenOrigen.Length, tokenBusqueda.Length);
                if (longitudMaximaToken == 0)
                    continue;

                var similitudToken = 1d - ((double)distanciaToken / longitudMaximaToken);
                mejorParaToken = Math.Max(mejorParaToken, similitudToken);
            }

            if (mejorParaToken > 0)
            {
                sumaMejores += mejorParaToken;
                coincidenciasToken++;
            }

            mejorToken = Math.Max(mejorToken, mejorParaToken);
        }

        var promedioTokens = coincidenciasToken > 0 ? sumaMejores / coincidenciasToken : 0d;

        var distancia = CalcularDistanciaLevenshtein(textoOrigen, textoBusquedaNormalizado);
        var longitudMaxima = Math.Max(textoOrigen.Length, textoBusquedaNormalizado.Length);
        var similitudGlobal = longitudMaxima == 0 ? 0d : 1d - ((double)distancia / longitudMaxima);

        return Math.Max(mejorToken, Math.Max(promedioTokens, similitudGlobal));
    }

    public string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var textoNormalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(textoNormalizado.Length);

        foreach (var c in textoNormalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var sinAcentos = sb.ToString().Normalize(NormalizationForm.FormC);
        var caracteres = sinAcentos
            .Select(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ? c : ' ')
            .ToArray();

        return string.Join(
            " ",
            new string(caracteres)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private ResultadoBusqueda? ConstruirResultado(Contacto contacto, string textoBusqueda)
    {
        var candidatos = ConstruirCandidatos(contacto).ToList();
        var mejorSimilitud = 0d;
        var mejorRanking = 0d;
        var mejorCampo = string.Empty;
        var mejorTipoCoincidencia = TipoCoincidencia.Aproximada;
        var campos = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidato in candidatos)
        {
            var evaluacion = EvaluarCandidato(candidato, textoBusqueda);
            if (!evaluacion.EsValida)
                continue;

            mejorSimilitud = Math.Max(mejorSimilitud, evaluacion.Similitud);

            if (evaluacion.Ranking > mejorRanking)
            {
                mejorRanking = evaluacion.Ranking;
                mejorCampo = candidato.Campo;
                mejorTipoCoincidencia = evaluacion.TipoCoincidencia;
            }

            if (campos.TryGetValue(candidato.Campo, out var existente))
            {
                if (evaluacion.Ranking > existente)
                    campos[candidato.Campo] = evaluacion.Ranking;
            }
            else
            {
                campos[candidato.Campo] = evaluacion.Ranking;
            }
        }

        if (mejorRanking <= 0d || string.IsNullOrWhiteSpace(mejorCampo))
            return null;

        var porcentaje = ConvertirRankingAPorcentaje(
            mejorRanking,
            mejorTipoCoincidencia,
            contacto.EsFavorito,
            mejorCampo);

        return new ResultadoBusqueda
        {
            Contacto = contacto,
            PuntajeCoincidencia = porcentaje,
            RankingInterno = Math.Round(mejorRanking, 4),
            CamposCoincidencia = campos
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .ToList(),
            MotivoPrincipal = $"Coincidió en {mejorCampo}"
        };
    }

    private EvaluacionCandidato EvaluarCandidato(CandidatoBusqueda candidato, string textoBusqueda)
    {
        var valorNormalizado = NormalizarTexto(candidato.Valor);
        var busquedaNormalizada = NormalizarTexto(textoBusqueda);

        if (string.IsNullOrWhiteSpace(valorNormalizado) || string.IsNullOrWhiteSpace(busquedaNormalizada))
            return EvaluacionCandidato.Invalida();

        var similitud = CalcularSimilitud(valorNormalizado, busquedaNormalizada);
        if (similitud < UmbralSimilitudMinimo)
            return EvaluacionCandidato.Invalida();

        var tipo = DeterminarTipoCoincidencia(valorNormalizado, busquedaNormalizada);
        var pesoTipo = ObtenerPesoTipoCoincidencia(tipo);
        var pesoTokens = CalcularFactorTokens(valorNormalizado, busquedaNormalizada);

        var ranking = similitud * 100d * candidato.Peso * pesoTipo * pesoTokens;

        return new EvaluacionCandidato(true, similitud, ranking, tipo);
    }

    private IEnumerable<CandidatoBusqueda> ConstruirCandidatos(Contacto contacto)
    {
        var nombre = contacto.Nombre?.Trim() ?? string.Empty;
        var apellido = contacto.Apellido?.Trim() ?? string.Empty;
        var nombreCompleto = $"{nombre} {apellido}".Trim();
        var nombreInvertido = $"{apellido} {nombre}".Trim();

        yield return new CandidatoBusqueda("Nombre", nombre, PesosCampo["Nombre"]);
        yield return new CandidatoBusqueda("Apellido", apellido, PesosCampo["Apellido"]);
        yield return new CandidatoBusqueda("Nombre completo", nombreCompleto, PesosCampo["Nombre completo"]);
        yield return new CandidatoBusqueda("Nombre invertido", nombreInvertido, PesosCampo["Nombre invertido"]);

        foreach (var telefono in contacto.Telefonos)
        {
            yield return new CandidatoBusqueda("Teléfono", telefono.Numero, PesosCampo["Teléfono"]);
            yield return new CandidatoBusqueda("Tipo de teléfono", telefono.Tipo, PesosCampo["Tipo de teléfono"]);
        }

        foreach (var correo in contacto.Correos)
        {
            yield return new CandidatoBusqueda("Correo", correo.Email, PesosCampo["Correo"]);
        }

        if (contacto.Direccion is not null)
        {
            yield return new CandidatoBusqueda("Provincia", contacto.Direccion.Provincia, PesosCampo["Provincia"]);
            yield return new CandidatoBusqueda("Cantón", contacto.Direccion.Canton, PesosCampo["Cantón"]);
            yield return new CandidatoBusqueda("Distrito", contacto.Direccion.Distrito, PesosCampo["Distrito"]);
            yield return new CandidatoBusqueda("Dirección", contacto.Direccion.DireccionExacta, PesosCampo["Dirección"]);
        }

        foreach (var nota in contacto.Notas)
        {
            yield return new CandidatoBusqueda("Nota", nota.Contenido, PesosCampo["Nota"]);
        }

        foreach (var etiqueta in contacto.Etiquetas)
        {
            yield return new CandidatoBusqueda("Etiqueta", etiqueta.Nombre, PesosCampo["Etiqueta"]);
        }
    }

    private TipoCoincidencia DeterminarTipoCoincidencia(string valorNormalizado, string busquedaNormalizada)
    {
        if (valorNormalizado == busquedaNormalizada)
            return TipoCoincidencia.Exacta;

        if (valorNormalizado.StartsWith(busquedaNormalizada, StringComparison.Ordinal))
            return TipoCoincidencia.Prefijo;

        if (valorNormalizado.Contains(busquedaNormalizada, StringComparison.Ordinal))
            return TipoCoincidencia.Parcial;

        var tokensValor = Tokenizar(valorNormalizado);
        var tokensBusqueda = Tokenizar(busquedaNormalizada);

        if (tokensBusqueda.All(tb => tokensValor.Any(tv => tv == tb)))
            return TipoCoincidencia.Parcial;

        return TipoCoincidencia.Aproximada;
    }

    private double ObtenerPesoTipoCoincidencia(TipoCoincidencia tipo)
    {
        return tipo switch
        {
            TipoCoincidencia.Exacta => 1.35d,
            TipoCoincidencia.Prefijo => 1.18d,
            TipoCoincidencia.Parcial => 1.00d,
            _ => 0.82d
        };
    }

    private double CalcularFactorTokens(string valorNormalizado, string busquedaNormalizada)
    {
        var tokensValor = Tokenizar(valorNormalizado);
        var tokensBusqueda = Tokenizar(busquedaNormalizada);

        if (tokensBusqueda.Count == 0 || tokensValor.Count == 0)
            return 1d;

        var coincidencias = tokensBusqueda.Count(tb =>
            tokensValor.Any(tv =>
                tv == tb ||
                tv.StartsWith(tb, StringComparison.Ordinal) ||
                tb.StartsWith(tv, StringComparison.Ordinal)));

        var proporcion = (double)coincidencias / tokensBusqueda.Count;

        if (proporcion >= 1d)
            return 1.10d;

        if (proporcion >= 0.75d)
            return 1.05d;

        if (proporcion >= 0.50d)
            return 1.00d;

        return 0.92d;
    }

    private double ConvertirRankingAPorcentaje(
        double rankingInterno,
        TipoCoincidencia tipo,
        bool esFavorito,
        string mejorCampo)
    {
        double porcentaje = tipo switch
        {
            TipoCoincidencia.Exacta => mejorCampo.Equals("Nombre completo", StringComparison.OrdinalIgnoreCase) ? 100d : 98d,
            TipoCoincidencia.Prefijo => mejorCampo.Equals("Nombre completo", StringComparison.OrdinalIgnoreCase) ? 94d : 90d,
            TipoCoincidencia.Parcial => mejorCampo.Equals("Nombre completo", StringComparison.OrdinalIgnoreCase) ? 86d : 78d,
            _ => 55d
        };

        if (rankingInterno >= 140d)
            porcentaje += 3d;
        else if (rankingInterno >= 120d)
            porcentaje += 2d;
        else if (rankingInterno >= 100d)
            porcentaje += 1d;

        if (esFavorito && porcentaje < 100d)
            porcentaje += 1d;

        porcentaje = Math.Min(100d, porcentaje);
        porcentaje = Math.Max(1d, porcentaje);

        return Math.Round(porcentaje, 0, MidpointRounding.AwayFromZero);
    }

    private bool CoincideTextoExactoNormalizado(string? valorFuente, string valorEsperado)
    {
        var origen = NormalizarTexto(valorFuente);
        var esperado = NormalizarTexto(valorEsperado);

        return !string.IsNullOrWhiteSpace(origen) &&
               !string.IsNullOrWhiteSpace(esperado) &&
               origen == esperado;
    }

    private List<string> Tokenizar(string texto)
    {
        return texto
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private int CalcularDistanciaLevenshtein(string origen, string destino)
    {
        if (string.IsNullOrEmpty(origen))
            return destino.Length;

        if (string.IsNullOrEmpty(destino))
            return origen.Length;

        var matriz = new int[origen.Length + 1, destino.Length + 1];

        for (var i = 0; i <= origen.Length; i++)
            matriz[i, 0] = i;

        for (var j = 0; j <= destino.Length; j++)
            matriz[0, j] = j;

        for (var i = 1; i <= origen.Length; i++)
        {
            for (var j = 1; j <= destino.Length; j++)
            {
                var costo = origen[i - 1] == destino[j - 1] ? 0 : 1;

                matriz[i, j] = Math.Min(
                    Math.Min(matriz[i - 1, j] + 1, matriz[i, j - 1] + 1),
                    matriz[i - 1, j - 1] + costo);
            }
        }

        return matriz[origen.Length, destino.Length];
    }

    private enum TipoCoincidencia
    {
        Exacta,
        Prefijo,
        Parcial,
        Aproximada
    }

    private readonly record struct CandidatoBusqueda(string Campo, string? Valor, double Peso);

    private readonly record struct EvaluacionCandidato(
        bool EsValida,
        double Similitud,
        double Ranking,
        TipoCoincidencia TipoCoincidencia)
    {
        public static EvaluacionCandidato Invalida() =>
            new(false, 0d, 0d, Services.MotorBusquedaInteligente.TipoCoincidencia.Aproximada);
    }
}