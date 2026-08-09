using System.Security.Cryptography;
using System.Text;

namespace SistemaDocente.Application;

public enum CategoriaEventoDiagnostico
{
    NoEspecificada = 0,
    FalloNoControlado = 1,
    FalloInicioAlmacenamiento = 2,
    FalloComandoTerminal = 3,
    FalloActualizacion = 4,
}

public enum ModoDiagnosticoLocal
{
    Produccion = 0,
    Demostracion = 1,
}

public sealed record EventoDiagnosticoSeguro(
    DateTimeOffset FechaHoraUtc,
    Guid EventoId,
    CategoriaEventoDiagnostico Categoria,
    string TipoExcepcion,
    IReadOnlyList<string> CadenaTiposExcepcion,
    string HuellaTecnica,
    string VersionAplicacion,
    ModoDiagnosticoLocal Modo);

public interface IRegistroDiagnosticoSeguro
{
    void Registrar(Exception exception, CategoriaEventoDiagnostico categoria);
}

public static class DiagnosticoSeguro
{
    public static EventoDiagnosticoSeguro CrearEvento(
        Exception exception,
        CategoriaEventoDiagnostico categoria,
        ModoDiagnosticoLocal modo,
        DateTimeOffset? fechaHoraUtc = null,
        Guid? eventoId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (!Enum.IsDefined(categoria))
            throw new ArgumentOutOfRangeException(nameof(categoria));
        if (!Enum.IsDefined(modo))
            throw new ArgumentOutOfRangeException(nameof(modo));

        var tipos = EnumerarTipos(exception);
        return new EventoDiagnosticoSeguro(
            fechaHoraUtc ?? DateTimeOffset.UtcNow,
            eventoId ?? Guid.NewGuid(),
            categoria,
            tipos[0],
            tipos,
            CrearHuellaTecnica(exception),
            IdentidadProducto.Version,
            modo);
    }

    private static string[] EnumerarTipos(Exception exception)
    {
        var tipos = new List<string>();
        for (Exception? actual = exception; actual is not null; actual = actual.InnerException)
        {
            tipos.Add(actual.GetType().FullName ?? actual.GetType().Name);
        }

        return tipos.ToArray();
    }

    private static string CrearHuellaTecnica(Exception exception)
    {
        var partes = new List<string>();
        for (Exception? actual = exception; actual is not null; actual = actual.InnerException)
        {
            var tipo = actual.GetType().FullName ?? actual.GetType().Name;
            var metodo = actual.TargetSite;
            var destino = metodo is null
                ? string.Empty
                : $"{metodo.DeclaringType?.FullName}.{metodo.Name}";
            partes.Add($"{tipo}|{destino}");
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", partes)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
