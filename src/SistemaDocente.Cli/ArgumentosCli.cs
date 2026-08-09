namespace SistemaDocente.Cli;

public sealed class ArgumentosCli
{
    private static readonly HashSet<string> OpcionesBooleanas = new(StringComparer.OrdinalIgnoreCase)
    {
        "--json",
        "--demo",
        "--apply",
        "--include-personal-data",
        "--help",
    };

    private static readonly HashSet<string> OpcionesConValor = new(StringComparer.OrdinalIgnoreCase)
    {
        "--group",
        "--student",
        "--date",
        "--state",
    };

    private readonly Dictionary<string, string?> _opciones;

    private ArgumentosCli(IReadOnlyList<string> posicion, Dictionary<string, string?> opciones)
    {
        Posicion = posicion;
        _opciones = opciones;
    }

    public IReadOnlyList<string> Posicion { get; }

    public bool Tiene(string opcion) => _opciones.ContainsKey(opcion);

    public string? Valor(string opcion) =>
        _opciones.TryGetValue(opcion, out var valor) ? valor : null;

    public static ArgumentosCli Analizar(IReadOnlyList<string> argumentos)
    {
        ArgumentNullException.ThrowIfNull(argumentos);
        var posicion = new List<string>();
        var opciones = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var indice = 0; indice < argumentos.Count; indice++)
        {
            var actual = argumentos[indice];
            if (!actual.StartsWith("--", StringComparison.Ordinal))
            {
                posicion.Add(actual);
                continue;
            }

            if (OpcionesBooleanas.Contains(actual))
            {
                if (!opciones.TryAdd(actual, null))
                    throw new ArgumentException($"La opción '{actual}' no puede repetirse.");
                continue;
            }

            if (!OpcionesConValor.Contains(actual))
                throw new ArgumentException($"La opción '{actual}' no está soportada.");
            if (indice + 1 >= argumentos.Count || argumentos[indice + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"La opción '{actual}' requiere un valor.");
            if (!opciones.TryAdd(actual, argumentos[++indice]))
                throw new ArgumentException($"La opción '{actual}' no puede repetirse.");
        }

        return new ArgumentosCli(posicion, opciones);
    }
}