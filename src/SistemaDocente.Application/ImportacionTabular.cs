namespace SistemaDocente.Application;

public interface ILectorImportacionTabular
{
    DocumentoTabular Leer(string rutaArchivo);
}

public sealed record DocumentoTabular(
    string NombreArchivo,
    IReadOnlyList<HojaTabular> Hojas);

public sealed record HojaTabular(
    string Nombre,
    IReadOnlyList<CeldaTabular> Encabezados,
    IReadOnlyList<FilaTabular> Filas);

public sealed record FilaTabular(
    int NumeroOrigen,
    IReadOnlyList<CeldaTabular> Celdas);

public enum TipoCeldaTabular
{
    Vacia = 0,
    Texto = 1,
    Numero = 2,
    Fecha = 3,
    Booleano = 4,
}

public sealed record CeldaTabular(
    TipoCeldaTabular Tipo,
    string Texto,
    decimal? Numero = null,
    DateOnly? Fecha = null,
    bool? Booleano = null)
{
    public static CeldaTabular Vacia { get; } = new(TipoCeldaTabular.Vacia, string.Empty);

    public static CeldaTabular DesdeTexto(string? texto)
    {
        var normalizado = texto?.Trim() ?? string.Empty;
        return normalizado.Length == 0
            ? Vacia
            : new CeldaTabular(TipoCeldaTabular.Texto, normalizado);
    }

    public static CeldaTabular DesdeNumero(decimal numero, string texto) =>
        new(TipoCeldaTabular.Numero, texto, Numero: numero);

    public static CeldaTabular DesdeFecha(DateOnly fecha, string texto) =>
        new(TipoCeldaTabular.Fecha, texto, Fecha: fecha);

    public static CeldaTabular DesdeBooleano(bool valor, string texto) =>
        new(TipoCeldaTabular.Booleano, texto, Booleano: valor);
}

public sealed class ImportacionTabularException : Exception
{
    public ImportacionTabularException(string message)
        : base(message)
    {
    }

    public ImportacionTabularException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
