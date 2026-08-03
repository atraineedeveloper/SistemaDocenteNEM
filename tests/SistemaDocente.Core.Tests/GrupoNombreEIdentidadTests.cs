using System.Reflection;

namespace SistemaDocente.Core.Tests;

public sealed class GrupoNombreEIdentidadTests
{
    [Fact]
    public void CrearGeneraIdentidadOpacaDistintaYEstable()
    {
        var primero = Grupo.Crear("Primero A");
        var segundo = Grupo.Crear("Primero B");
        var identidadOriginal = primero.Id;

        Assert.NotEqual(default, primero.Id);
        Assert.NotEqual(primero.Id, segundo.Id);
        Assert.Equal(identidadOriginal, primero.Id);
        Assert.True(Guid.TryParse(primero.Id.ToString(), out _));
        Assert.NotEqual(typeof(GrupoId), typeof(EstudianteId));
    }

    [Fact]
    public void IdentidadesNoTienenConstructoresPublicos()
    {
        const BindingFlags constructoresPublicos =
            BindingFlags.Public | BindingFlags.Instance;

        Assert.Empty(typeof(GrupoId).GetConstructors(constructoresPublicos));
        Assert.Empty(typeof(EstudianteId).GetConstructors(constructoresPublicos));
    }

    [Fact]
    public void CrearNormalizaEspaciosDelNombreSinAlterarCaracteres()
    {
        var grupo = Grupo.Crear("  Quinto   “Á”  ");

        Assert.Equal("Quinto “Á”", grupo.NombreVisible);
    }

    [Fact]
    public void CrearAceptaNombreDeCienCaracteres()
    {
        var nombre = new string('Á', 100);

        var grupo = Grupo.Crear(nombre);

        Assert.Equal(nombre, grupo.NombreVisible);
    }

    [Fact]
    public void CrearRechazaNombreDeCientoUnCaracteres()
    {
        var nombre = new string('a', 101);

        Assert.Throws<DomainValidationException>(() => Grupo.Crear(nombre));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void CrearRechazaNombreVacioDespuesDeNormalizar(string nombre)
    {
        Assert.Throws<DomainValidationException>(() => Grupo.Crear(nombre));
    }

    [Fact]
    public void RenombrarGrupoEsAtomicoAnteNombreInvalido()
    {
        var grupo = Grupo.Crear("Nombre original");

        Assert.Throws<DomainValidationException>(() => grupo.Renombrar("   "));

        Assert.Equal("Nombre original", grupo.NombreVisible);
    }
}