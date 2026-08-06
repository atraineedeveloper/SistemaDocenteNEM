using System.Text;
using System.Text.RegularExpressions;

namespace SistemaDocente.Core;

public static class ValidadorContenidoPedagogico
{
    private static readonly string[] TérminosClinicosProhibidos =
    [
        "diagnostico", "diagnosticar", "trastorno", "sindrome",
        "tdah", "autismo", "autista", "asperger",
        "depresion", "depresivo", "ansiedad", "esquizofrenia",
        "terapia medica", "receta", "medicado", "medicamento",
        "psiquiatra", "psiquiatrico", "neurologo", "neurologico",
        "clinico", "discapacidad mental", "patologia"
    ];

    public static void ValidarTextoPedagogico(string texto, string nombreCampo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(texto, nombreCampo);

        var normalizado = NormalizarTextoSinAcentos(texto.ToLowerInvariant());

        foreach (var termino in TérminosClinicosProhibidos)
        {
            var patron = $@"\b{Regex.Escape(termino)}\b";
            if (Regex.IsMatch(normalizado, patron))
            {
                throw new DomainValidationException(
                    $"El campo '{nombreCampo}' contiene términos de carácter médico o clínico ('{termino}'). " +
                    "De acuerdo con las directrices formativas de la NEM, el expediente sólo debe registrar observaciones pedagógicas, " +
                    "desempeño académico, apoyos aplicados en el aula y acuerdos formativos.");
            }
        }
    }

    private static string NormalizarTextoSinAcentos(string texto)
    {
        var stFormD = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in stFormD)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
