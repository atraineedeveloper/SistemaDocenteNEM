namespace SistemaDocente.Application.Tests;

public sealed class IdentidadProductoTests
{
    [Fact]
    public void MarcaComercialEsAulaRaizYConservaIdentidadTecnicaLegada()
    {
        Assert.Equal("AulaRaíz", IdentidadProducto.Nombre);
        Assert.Equal("AulaRaiz", IdentidadProducto.NombreSeguroArchivo);
        Assert.Equal("Gestión docente para la Nueva Escuela Mexicana", IdentidadProducto.Subtitulo);
        Assert.Equal("SistemaDocenteNEM", IdentidadProducto.IdentificadorTecnicoLegado);
    }

    [Fact]
    public void VersionVisibleProvieneDeMetadatosCompartidosDelProducto()
    {
        Assert.Equal("0.2.0", IdentidadProducto.Version);
        Assert.Equal("v0.2.0", IdentidadProducto.VersionVisible);
    }
}