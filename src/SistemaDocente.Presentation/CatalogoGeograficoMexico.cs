using System.Reflection;
using System.Text.Json;

namespace SistemaDocente.Presentation;

public static class CatalogoGeograficoMexico
{
    private const string NombreRecurso = "SistemaDocente.Presentation.Data.estados-municipios.json";
    private static readonly Lazy<IReadOnlyDictionary<string, string[]>> Datos = new(CargarDatos);

    public static IReadOnlyList<string> EntidadesFederativas { get; } =
    [
        "Aguascalientes",
        "Baja California",
        "Baja California Sur",
        "Campeche",
        "Coahuila de Zaragoza",
        "Colima",
        "Chiapas",
        "Chihuahua",
        "Ciudad de México",
        "Durango",
        "Guanajuato",
        "Guerrero",
        "Hidalgo",
        "Jalisco",
        "México",
        "Michoacán de Ocampo",
        "Morelos",
        "Nayarit",
        "Nuevo León",
        "Oaxaca",
        "Puebla",
        "Querétaro",
        "Quintana Roo",
        "San Luis Potosí",
        "Sinaloa",
        "Sonora",
        "Tabasco",
        "Tamaulipas",
        "Tlaxcala",
        "Veracruz de Ignacio de la Llave",
        "Yucatán",
        "Zacatecas",
    ];

    public static IReadOnlyList<string> ObtenerMunicipios(string? entidadFederativa)
    {
        if (string.IsNullOrWhiteSpace(entidadFederativa)) return Array.Empty<string>();
        return Datos.Value.TryGetValue(entidadFederativa.Trim(), out var municipios)
            ? municipios
            : Array.Empty<string>();
    }

    public static bool ContieneEntidad(string? entidadFederativa) =>
        !string.IsNullOrWhiteSpace(entidadFederativa)
        && Datos.Value.ContainsKey(entidadFederativa.Trim());

    public static bool ContieneMunicipio(string? entidadFederativa, string? municipio) =>
        !string.IsNullOrWhiteSpace(municipio)
        && ObtenerMunicipios(entidadFederativa).Contains(municipio.Trim(), StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string[]> CargarDatos()
    {
        var ensamblado = Assembly.GetExecutingAssembly();
        using var stream = ensamblado.GetManifestResourceStream(NombreRecurso)
            ?? throw new InvalidOperationException("No se pudo cargar el catálogo geográfico local de México.");

        var datos = JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream)
            ?? throw new InvalidOperationException("El catálogo geográfico local de México no es válido.");

        foreach (var entidad in EntidadesFederativas)
        {
            if (!datos.ContainsKey(entidad))
            {
                throw new InvalidOperationException($"El catálogo geográfico no contiene la entidad '{entidad}'.");
            }
        }

        return datos;
    }
}