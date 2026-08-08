using SistemaDocente.Application;

namespace SistemaDocente.Application.Tests;

public sealed class DiagnosticoSeguroTests
{
    [Fact]
    public void CrearEventoOmiteMensajesYConservaSoloMetadatosTecnicos()
    {
        var exception = new InvalidOperationException(
            "ALUMNA_SECRETA no debe aparecer",
            new IOException("C:\\Users\\Docente\\datos-alumno.db"));
        var fecha = new DateTimeOffset(2026, 8, 8, 19, 0, 0, TimeSpan.Zero);
        var id = Guid.Parse("4b5c9e79-30ad-4fe6-8c29-90c020e02f69");

        var evento = DiagnosticoSeguro.CrearEvento(
            exception,
            CategoriaEventoDiagnostico.FalloNoControlado,
            ModoDiagnosticoLocal.Produccion,
            fecha,
            id);

        Assert.Equal(fecha, evento.FechaHoraUtc);
        Assert.Equal(id, evento.EventoId);
        Assert.Equal(CategoriaEventoDiagnostico.FalloNoControlado, evento.Categoria);
        Assert.Equal(typeof(InvalidOperationException).FullName, evento.TipoExcepcion);
        Assert.Equal(
            [typeof(InvalidOperationException).FullName, typeof(IOException).FullName],
            evento.CadenaTiposExcepcion);
        Assert.Equal(64, evento.HuellaTecnica.Length);
        Assert.Equal(IdentidadProducto.Version, evento.VersionAplicacion);
        Assert.Equal(ModoDiagnosticoLocal.Produccion, evento.Modo);

        var representacion = string.Join('|',
            evento.TipoExcepcion,
            string.Join(',', evento.CadenaTiposExcepcion),
            evento.HuellaTecnica,
            evento.VersionAplicacion);
        Assert.DoesNotContain("ALUMNA_SECRETA", representacion, StringComparison.Ordinal);
        Assert.DoesNotContain("Docente", representacion, StringComparison.Ordinal);
        Assert.DoesNotContain("datos-alumno", representacion, StringComparison.Ordinal);
    }

    [Fact]
    public void HuellaEsEstableParaLaMismaFormaDeExcepcion()
    {
        var primera = DiagnosticoSeguro.CrearEvento(
            new InvalidOperationException("mensaje uno", new IOException("ruta uno")),
            CategoriaEventoDiagnostico.FalloNoControlado,
            ModoDiagnosticoLocal.Demostracion);
        var segunda = DiagnosticoSeguro.CrearEvento(
            new InvalidOperationException("mensaje dos", new IOException("ruta dos")),
            CategoriaEventoDiagnostico.FalloNoControlado,
            ModoDiagnosticoLocal.Demostracion);

        Assert.Equal(primera.HuellaTecnica, segunda.HuellaTecnica);
    }
}