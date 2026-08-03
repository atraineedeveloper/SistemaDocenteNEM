using System.Text;

namespace SistemaDocente.Core;

internal static class NormalizadorNombreVisible
{
    internal static string NormalizarYValidar(string? valor, int longitudMaxima, string campo)
    {
        if (valor is null)
        {
            throw new DomainValidationException($"{campo} es obligatorio.");
        }

        var resultado = new StringBuilder(valor.Length);
        var espacioPendiente = false;

        foreach (var caracter in valor)
        {
            if (char.IsWhiteSpace(caracter))
            {
                espacioPendiente = resultado.Length > 0;
                continue;
            }

            if (espacioPendiente)
            {
                resultado.Append(' ');
                espacioPendiente = false;
            }

            resultado.Append(caracter);
        }

        var normalizado = resultado.ToString();

        if (normalizado.Length == 0)
        {
            throw new DomainValidationException($"{campo} es obligatorio.");
        }

        if (normalizado.Length > longitudMaxima)
        {
            throw new DomainValidationException(
                $"{campo} no puede exceder {longitudMaxima} caracteres.");
        }

        return normalizado;
    }
}