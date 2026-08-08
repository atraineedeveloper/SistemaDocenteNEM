namespace SistemaDocente.Application;

public interface IExportadorTabular
{
    void Exportar(
        DocumentoTabularSalida documento,
        string rutaArchivo,
        FormatoExportacionTabular formato);
}

public enum FormatoExportacionTabular
{
    Xlsx = 1,
    Csv = 2,
}

public sealed record DocumentoTabularSalida(
    IReadOnlyList<HojaTabularSalida> Hojas)
{
    public static DocumentoTabularSalida Crear(params HojaTabularSalida[] hojas) => new(hojas);
}

public sealed record HojaTabularSalida(
    string Nombre,
    IReadOnlyList<ColumnaTabularSalida> Columnas,
    IReadOnlyList<FilaTabularSalida> Filas);

public sealed record ColumnaTabularSalida(
    string Encabezado);

public sealed record FilaTabularSalida(
    IReadOnlyList<CeldaTabularSalida> Celdas)
{
    public static FilaTabularSalida Crear(params CeldaTabularSalida[] celdas) => new(celdas);
}

public enum TipoCeldaTabularSalida
{
    Vacia = 0,
    Texto = 1,
    Numero = 2,
    Fecha = 3,
    Booleano = 4,
}

public sealed record CeldaTabularSalida(
    TipoCeldaTabularSalida Tipo,
    string Texto = "",
    decimal? Numero = null,
    DateOnly? Fecha = null,
    bool? Booleano = null)
{
    public static CeldaTabularSalida Vacia { get; } = new(TipoCeldaTabularSalida.Vacia);

    public static CeldaTabularSalida DesdeTexto(string? texto) =>
        string.IsNullOrEmpty(texto)
            ? Vacia
            : new CeldaTabularSalida(TipoCeldaTabularSalida.Texto, texto);

    public static CeldaTabularSalida DesdeNumero(decimal numero) =>
        new(TipoCeldaTabularSalida.Numero, Numero: numero);

    public static CeldaTabularSalida DesdeFecha(DateOnly fecha) =>
        new(TipoCeldaTabularSalida.Fecha, Fecha: fecha);

    public static CeldaTabularSalida DesdeBooleano(bool valor) =>
        new(TipoCeldaTabularSalida.Booleano, Booleano: valor);
}

public sealed class ExportacionTabularException : Exception
{
    public ExportacionTabularException(string message)
        : base(message)
    {
    }

    public ExportacionTabularException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
