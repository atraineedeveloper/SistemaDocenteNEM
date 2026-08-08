using System.Reflection;

namespace SistemaDocente.Application;

/// <summary>
/// Identidad comercial visible del producto. Los identificadores técnicos legacy
/// permanecen separados para conservar compatibilidad con datos y respaldos existentes.
/// </summary>
public static class IdentidadProducto
{
    public const string Nombre = "AulaRaíz";
    public const string NombreSeguroArchivo = "AulaRaiz";
    public const string Subtitulo = "Gestión docente para la Nueva Escuela Mexicana";

    public static string Version
    {
        get
        {
            var informacional = typeof(IdentidadProducto).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            if (!string.IsNullOrWhiteSpace(informacional))
            {
                var separadorMetadatos = informacional.IndexOf('+', StringComparison.Ordinal);
                return separadorMetadatos < 0
                    ? informacional
                    : informacional[..separadorMetadatos];
            }

            return typeof(IdentidadProducto).Assembly.GetName().Version?.ToString(3) ?? "desconocida";
        }
    }

    public static string VersionVisible => $"v{Version}";

    /// <summary>
    /// Identidad técnica histórica usada por almacenamiento y paquetes de recuperación.
    /// No debe cambiarse sin una migración explícita y compatible.
    /// </summary>
    public const string IdentificadorTecnicoLegado = "SistemaDocenteNEM";
}
