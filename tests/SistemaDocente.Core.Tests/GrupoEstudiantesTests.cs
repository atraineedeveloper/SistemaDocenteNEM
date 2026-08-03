using System.Collections;

namespace SistemaDocente.Core.Tests;

public sealed class GrupoEstudiantesTests
{
    [Fact]
    public void AgregarEstudianteGeneraIdentidadYEstadoActivo()
    {
        var grupo = Grupo.Crear("Primero A");

        var estudiante = grupo.AgregarEstudiante("Ana", 1);

        Assert.NotEqual(default, estudiante.Id);
        Assert.True(Guid.TryParse(estudiante.Id.ToString(), out _));
        Assert.True(estudiante.EstaActivo);
        Assert.Same(estudiante, Assert.Single(grupo.Estudiantes));
    }

    [Fact]
    public void AgregarYRenombrarNormalizanEspaciosYConservanCaracteres()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("  María   José  ", 1);

        grupo.RenombrarEstudiante(estudiante.Id, "  Ángel   O'Connor-López  ");

        Assert.Equal("Ángel O'Connor-López", estudiante.NombreVisible);
    }

    [Fact]
    public void NombreDeEstudianteAceptaCientoCincuentaCaracteres()
    {
        var grupo = Grupo.Crear("Primero A");
        var nombre = new string('Á', 150);

        var estudiante = grupo.AgregarEstudiante(nombre, 1);

        Assert.Equal(nombre, estudiante.NombreVisible);
    }

    [Fact]
    public void NombreDeEstudianteRechazaCientoCincuentaYUnCaracteresAtomicamente()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var nombreOriginal = estudiante.NombreVisible;

        Assert.Throws<DomainValidationException>(
            () => grupo.RenombrarEstudiante(estudiante.Id, new string('a', 151)));

        Assert.Equal(nombreOriginal, estudiante.NombreVisible);
        Assert.Single(grupo.Estudiantes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NombreDeEstudianteVacioSeRechazaSinAgregar(string nombre)
    {
        var grupo = Grupo.Crear("Primero A");

        Assert.Throws<DomainValidationException>(
            () => grupo.AgregarEstudiante(nombre, 1));

        Assert.Empty(grupo.Estudiantes);
    }

    [Fact]
    public void NombresRepetidosSeAceptanConIdentidadesDistintas()
    {
        var grupo = Grupo.Crear("Primero A");

        var primero = grupo.AgregarEstudiante("Alex", 1);
        var segundo = grupo.AgregarEstudiante("Alex", 2);

        Assert.Equal(primero.NombreVisible, segundo.NombreVisible);
        Assert.NotEqual(primero.Id, segundo.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NumeroNoPositivoSeRechazaSinAgregar(int numero)
    {
        var grupo = Grupo.Crear("Primero A");

        Assert.Throws<DomainValidationException>(
            () => grupo.AgregarEstudiante("Ana", numero));

        Assert.Empty(grupo.Estudiantes);
    }

    [Fact]
    public void NumerosPermitenHuecosYNoTienenLimiteSuperiorAdicional()
    {
        var grupo = Grupo.Crear("Primero A");

        grupo.AgregarEstudiante("Ana", 1);
        grupo.AgregarEstudiante("Luis", int.MaxValue);

        Assert.Equal([1, int.MaxValue], grupo.Estudiantes.Select(e => e.NumeroLista));
    }

    [Fact]
    public void MismoNumeroSePermiteEnGruposDiferentes()
    {
        var primero = Grupo.Crear("Primero A");
        var segundo = Grupo.Crear("Primero B");

        primero.AgregarEstudiante("Ana", 1);
        segundo.AgregarEstudiante("Luis", 1);

        Assert.Equal(1, Assert.Single(primero.Estudiantes).NumeroLista);
        Assert.Equal(1, Assert.Single(segundo.Estudiantes).NumeroLista);
    }

    [Fact]
    public void DuplicadoEntreActivosSeRechazaSinReasignarNiAgregar()
    {
        var grupo = Grupo.Crear("Primero A");
        var existente = grupo.AgregarEstudiante("Ana", 1);

        Assert.Throws<DomainConflictException>(
            () => grupo.AgregarEstudiante("Luis", 1));

        Assert.Same(existente, Assert.Single(grupo.Estudiantes));
        Assert.Equal(1, existente.NumeroLista);
    }

    [Fact]
    public void ActivoPuedeReutilizarNumeroDeInactivo()
    {
        var grupo = Grupo.Crear("Primero A");
        var inactivo = grupo.AgregarEstudiante("Ana", 1);
        grupo.DesactivarEstudiante(inactivo.Id);

        var activo = grupo.AgregarEstudiante("Luis", 1);

        Assert.Equal(1, inactivo.NumeroLista);
        Assert.False(inactivo.EstaActivo);
        Assert.True(activo.EstaActivo);
    }

    [Fact]
    public void CambiarNumeroDeActivoValidaUnicidadAtomicamente()
    {
        var grupo = Grupo.Crear("Primero A");
        var primero = grupo.AgregarEstudiante("Ana", 1);
        var segundo = grupo.AgregarEstudiante("Luis", 2);

        Assert.Throws<DomainConflictException>(
            () => grupo.CambiarNumeroLista(segundo.Id, 1));

        Assert.Equal(1, primero.NumeroLista);
        Assert.Equal(2, segundo.NumeroLista);
    }

    [Fact]
    public void CambiarNumeroDeInactivoPreparaReactivacion()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        grupo.DesactivarEstudiante(estudiante.Id);
        grupo.AgregarEstudiante("Luis", 1);

        grupo.CambiarNumeroLista(estudiante.Id, 2);

        Assert.False(estudiante.EstaActivo);
        Assert.Equal(2, estudiante.NumeroLista);
        grupo.ReactivarEstudiante(estudiante.Id);
        Assert.True(estudiante.EstaActivo);
    }

    [Fact]
    public void CambioDeNumeroInvalidoEsAtomico()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);

        Assert.Throws<DomainValidationException>(
            () => grupo.CambiarNumeroLista(estudiante.Id, 0));

        Assert.Equal(1, estudiante.NumeroLista);
    }

    [Fact]
    public void DesactivarYReactivarConservanIdentidadYDatos()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var identidad = estudiante.Id;

        grupo.DesactivarEstudiante(estudiante.Id);
        grupo.ReactivarEstudiante(estudiante.Id);

        Assert.Equal(identidad, estudiante.Id);
        Assert.Equal("Ana", estudiante.NombreVisible);
        Assert.Equal(1, estudiante.NumeroLista);
        Assert.True(estudiante.EstaActivo);
    }

    [Fact]
    public void DesactivarInactivoYReactivarActivoSonIdempotentes()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);

        grupo.ReactivarEstudiante(estudiante.Id);
        grupo.ReactivarEstudiante(estudiante.Id);
        grupo.DesactivarEstudiante(estudiante.Id);
        grupo.DesactivarEstudiante(estudiante.Id);

        Assert.False(estudiante.EstaActivo);
        Assert.Single(grupo.Estudiantes);
    }

    [Fact]
    public void ConflictoAlReactivarEsAtomico()
    {
        var grupo = Grupo.Crear("Primero A");
        var inactivo = grupo.AgregarEstudiante("Ana", 1);
        var identidad = inactivo.Id;
        grupo.DesactivarEstudiante(inactivo.Id);
        var activo = grupo.AgregarEstudiante("Luis", 1);

        Assert.Throws<DomainConflictException>(
            () => grupo.ReactivarEstudiante(inactivo.Id));

        Assert.False(inactivo.EstaActivo);
        Assert.Equal(identidad, inactivo.Id);
        Assert.Equal("Ana", inactivo.NombreVisible);
        Assert.Equal(1, inactivo.NumeroLista);
        Assert.True(activo.EstaActivo);
        Assert.Equal(2, grupo.Estudiantes.Count);
    }

    [Fact]
    public void ColeccionExpuestaEsSoloLectura()
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Ana", 1);
        var coleccion = Assert.IsAssignableFrom<IList>(grupo.Estudiantes);

        Assert.True(coleccion.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => coleccion.Clear());
        Assert.Single(grupo.Estudiantes);
    }

    [Fact]
    public void ConsultaActivosExcluyeInactivosYOrdenaDeterministicamente()
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Zoe", 10);
        var inactivo = grupo.AgregarEstudiante("Carlos", 3);
        grupo.AgregarEstudiante("Álvaro", 1);
        grupo.DesactivarEstudiante(inactivo.Id);

        var activos = grupo.EstudiantesActivos;

        Assert.Equal([1, 10], activos.Select(e => e.NumeroLista));
        Assert.Equal(
            activos.OrderBy(e => e.NumeroLista).ThenBy(e => e.NombreVisible, StringComparer.Ordinal),
            activos);
        Assert.DoesNotContain(inactivo, activos);
    }
}