using System.Globalization;
using System.Text;

namespace SistemaDocente.Core;

public static class CatalogoNemPrimaria
{
    private static readonly GradoPrimaria[] GradosReales =
    [
        GradoPrimaria.Primero,
        GradoPrimaria.Segundo,
        GradoPrimaria.Tercero,
        GradoPrimaria.Cuarto,
        GradoPrimaria.Quinto,
        GradoPrimaria.Sexto,
    ];

    public static IReadOnlyList<GradoPrimaria> TodosLosGrados { get; } = Array.AsReadOnly(GradosReales);

    public static FaseNem ObtenerFase(GradoPrimaria grado) => grado switch
    {
        GradoPrimaria.Primero or GradoPrimaria.Segundo => FaseNem.Fase3,
        GradoPrimaria.Tercero or GradoPrimaria.Cuarto => FaseNem.Fase4,
        GradoPrimaria.Quinto or GradoPrimaria.Sexto => FaseNem.Fase5,
        _ => FaseNem.NoEspecificada,
    };

    public static IReadOnlyList<FaseNem> ObtenerFases(IEnumerable<GradoPrimaria> grados)
    {
        ArgumentNullException.ThrowIfNull(grados);
        return grados
            .Where(EsGradoReal)
            .Select(ObtenerFase)
            .Where(fase => fase != FaseNem.NoEspecificada)
            .Distinct()
            .OrderBy(fase => (int)fase)
            .ToArray();
    }

    public static IReadOnlyList<GradoPrimaria> NormalizarGrados(IEnumerable<GradoPrimaria>? grados)
    {
        if (grados is null) return Array.Empty<GradoPrimaria>();
        var resultado = grados
            .Where(EsGradoReal)
            .Distinct()
            .OrderBy(grado => (int)grado)
            .ToArray();
        return resultado;
    }

    public static bool EsGradoReal(GradoPrimaria grado) =>
        grado is >= GradoPrimaria.Primero and <= GradoPrimaria.Sexto;

    public static string FormatearGrado(GradoPrimaria grado) => grado switch
    {
        GradoPrimaria.Primero => "1.º",
        GradoPrimaria.Segundo => "2.º",
        GradoPrimaria.Tercero => "3.º",
        GradoPrimaria.Cuarto => "4.º",
        GradoPrimaria.Quinto => "5.º",
        GradoPrimaria.Sexto => "6.º",
        _ => "No especificado",
    };

    public static string FormatearGrados(IEnumerable<GradoPrimaria> grados)
    {
        var normalizados = NormalizarGrados(grados);
        return normalizados.Count == 0
            ? string.Empty
            : string.Join(" · ", normalizados.Select(FormatearGrado));
    }

    public static string FormatearFase(FaseNem fase) => fase switch
    {
        FaseNem.Fase3 => "Fase 3",
        FaseNem.Fase4 => "Fase 4",
        FaseNem.Fase5 => "Fase 5",
        _ => "No especificada",
    };

    public static string FormatearFases(IEnumerable<GradoPrimaria> grados)
    {
        var fases = ObtenerFases(grados);
        return fases.Count == 0
            ? "Sin fase configurada"
            : string.Join(" · ", fases.Select(FormatearFase));
    }

    public static bool TryParseGradoLegacy(string? valor, out GradoPrimaria grado)
    {
        grado = GradoPrimaria.NoEspecificado;
        if (string.IsNullOrWhiteSpace(valor)) return false;

        var normalizado = QuitarDiacriticos(valor)
            .ToLowerInvariant()
            .Replace("°", string.Empty, StringComparison.Ordinal)
            .Replace("º", string.Empty, StringComparison.Ordinal)
            .Replace(".o", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Trim();

        var tokens = normalizado
            .Split([' ', '-', '_', '/', ',', ';', '·'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var candidatos = tokens
            .Select(ParseToken)
            .Where(EsGradoReal)
            .Distinct()
            .ToArray();

        if (candidatos.Length != 1) return false;
        grado = candidatos[0];
        return true;
    }

    public static IReadOnlyList<EtapaDesarrolloCognoscitivo> ObtenerReferenciaPiaget(IEnumerable<GradoPrimaria> grados)
    {
        var normalizados = NormalizarGrados(grados);
        var etapas = new HashSet<EtapaDesarrolloCognoscitivo>();

        foreach (var grado in normalizados)
        {
            if (grado == GradoPrimaria.Primero)
            {
                etapas.Add(EtapaDesarrolloCognoscitivo.Preoperacional);
                etapas.Add(EtapaDesarrolloCognoscitivo.OperacionesConcretas);
                continue;
            }

            etapas.Add(EtapaDesarrolloCognoscitivo.OperacionesConcretas);
            if (grado == GradoPrimaria.Sexto)
            {
                etapas.Add(EtapaDesarrolloCognoscitivo.OperacionesFormales);
            }
        }

        return etapas.OrderBy(etapa => (int)etapa).ToArray();
    }

    public static string DescribirReferenciaPiaget(IEnumerable<GradoPrimaria> grados)
    {
        var etapas = ObtenerReferenciaPiaget(grados);
        if (etapas.Count == 0)
        {
            return "Configura los grados atendidos para mostrar una referencia general de desarrollo.";
        }

        if (etapas.Count == 1 && etapas[0] == EtapaDesarrolloCognoscitivo.OperacionesConcretas)
        {
            return "Operaciones concretas · referencia general frecuente en estas edades; no constituye un diagnóstico individual.";
        }

        if (etapas.Contains(EtapaDesarrolloCognoscitivo.Preoperacional))
        {
            return "Transición preoperacional → operaciones concretas · referencia general; el desarrollo individual puede variar.";
        }

        return "Operaciones concretas con posible transición hacia operaciones formales · referencia general, no diagnóstica.";
    }

    private static GradoPrimaria ParseToken(string token) => token switch
    {
        "1" or "1o" or "1ro" or "primero" or "primer" => GradoPrimaria.Primero,
        "2" or "2o" or "2do" or "segundo" => GradoPrimaria.Segundo,
        "3" or "3o" or "3ro" or "tercero" or "tercer" => GradoPrimaria.Tercero,
        "4" or "4o" or "4to" or "cuarto" => GradoPrimaria.Cuarto,
        "5" or "5o" or "5to" or "quinto" => GradoPrimaria.Quinto,
        "6" or "6o" or "6to" or "sexto" => GradoPrimaria.Sexto,
        _ => GradoPrimaria.NoEspecificado,
    };

    private static string QuitarDiacriticos(string valor)
    {
        var descompuesto = valor.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(descompuesto.Length);
        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(caracter);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}