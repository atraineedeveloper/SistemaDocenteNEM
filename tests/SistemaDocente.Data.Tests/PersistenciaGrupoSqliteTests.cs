using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class PersistenciaGrupoSqliteTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void GuardarYCargarConservaIdentidadesYEstados()
    {
        var grupo = Grupo.Crear("Primero A");
        var activo = grupo.AgregarEstudiante("Ana", 1);
        var inactivo = grupo.AgregarEstudiante("Luis", 2);
        grupo.DesactivarEstudiante(inactivo.Id);

        _base.Persistencia.Guardar(grupo);
        var cargado = _base.Persistencia.Cargar(grupo.Id);

        Assert.NotNull(cargado);
        Assert.Equal(grupo.Id, cargado.Id);
        Assert.Equal(grupo.NombreVisible, cargado.NombreVisible);
        Assert.Collection(
            cargado.Estudiantes,
            estudiante =>
            {
                Assert.Equal(activo.Id, estudiante.Id);
                Assert.True(estudiante.EstaActivo);
            },
            estudiante =>
            {
                Assert.Equal(inactivo.Id, estudiante.Id);
                Assert.False(estudiante.EstaActivo);
            });
    }

    [Fact]
    public void GuardarDeNuevoActualizaNombresYNumerosSinDuplicar()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        _base.Persistencia.Guardar(grupo);

        grupo.Renombrar("Segundo B");
        grupo.RenombrarEstudiante(estudiante.Id, "Ana María");
        grupo.CambiarNumeroLista(estudiante.Id, 20);
        _base.Persistencia.Guardar(grupo);
        var cargado = _base.Persistencia.Cargar(grupo.Id);

        Assert.NotNull(cargado);
        Assert.Equal("Segundo B", cargado.NombreVisible);
        var estudianteCargado = Assert.Single(cargado.Estudiantes);
        Assert.Equal(estudiante.Id, estudianteCargado.Id);
        Assert.Equal("Ana María", estudianteCargado.NombreVisible);
        Assert.Equal(20, estudianteCargado.NumeroLista);
    }

    [Fact]
    public void DesactivacionYReactivacionPersisten()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        grupo.DesactivarEstudiante(estudiante.Id);
        _base.Persistencia.Guardar(grupo);

        var inactivo = _base.Persistencia.Cargar(grupo.Id)!;
        Assert.False(Assert.Single(inactivo.Estudiantes).EstaActivo);

        inactivo.ReactivarEstudiante(estudiante.Id);
        _base.Persistencia.Guardar(inactivo);
        var reactivado = _base.Persistencia.Cargar(grupo.Id)!;

        Assert.True(Assert.Single(reactivado.Estudiantes).EstaActivo);
        Assert.Equal(estudiante.Id, Assert.Single(reactivado.Estudiantes).Id);
    }

    [Fact]
    public void CargarGrupoInexistenteDevuelveAusencia()
    {
        _base.Persistencia.Inicializar();

        var resultado = _base.Persistencia.Cargar(GrupoId.DesdeGuid(Guid.NewGuid()));

        Assert.Null(resultado);
    }

    [Fact]
    public void GuardarNoBorraEstudiantesAusentesDelSnapshot()
    {
        var grupo = Grupo.Crear("Primero A");
        var primero = grupo.AgregarEstudiante("Ana", 1);
        var segundo = grupo.AgregarEstudiante("Luis", 2);
        _base.Persistencia.Guardar(grupo);
        var reducido = Grupo.Rehidratar(
            grupo.Id,
            grupo.NombreVisible,
            [new(primero.Id, primero.NombreVisible, primero.NumeroLista, true)]);

        _base.Persistencia.Guardar(reducido);
        var cargado = _base.Persistencia.Cargar(grupo.Id);

        Assert.NotNull(cargado);
        Assert.Equal(2, cargado.Estudiantes.Count);
        Assert.Contains(cargado.Estudiantes, actual => actual.Id == segundo.Id);
    }

    [Fact]
    public void MoverEstudianteAOtroGrupoFallaYRevierteElGrupoNuevo()
    {
        var original = Grupo.Crear("Primero A");
        var estudiante = original.AgregarEstudiante("Ana", 1);
        _base.Persistencia.Guardar(original);
        var otroGrupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var otroGrupo = Grupo.Rehidratar(
            otroGrupoId,
            "Primero B",
            [new(estudiante.Id, estudiante.NombreVisible, 1, true)]);

        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => _base.Persistencia.Guardar(otroGrupo));
        Assert.IsType<DataIntegrityException>(error.InnerException);

        Assert.Null(_base.Persistencia.Cargar(otroGrupoId));
        var cargadoOriginal = _base.Persistencia.Cargar(original.Id)!;
        Assert.Equal(estudiante.Id, Assert.Single(cargadoOriginal.Estudiantes).Id);
    }

    [Fact]
    public void NombreManipuladoNoSeCorrigeYLaCargaFalla()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana María", 1);
        _base.Persistencia.Guardar(grupo);
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = "UPDATE estudiantes SET nombre = 'Ana  María' WHERE id = $id;";
            comando.Parameters.AddWithValue("$id", estudiante.Id.Valor.ToString("D"));
            comando.ExecuteNonQuery();
        }

        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => _base.Persistencia.Cargar(grupo.Id));
        Assert.IsType<DataIntegrityException>(error.InnerException);
    }

    [Fact]
    public void TriggerConRaiseAbortRevierteCambiosIntermedios()
    {
        var grupo = Grupo.Crear("Primero A");
        var primero = grupo.AgregarEstudiante("Ana", 1);
        var segundo = grupo.AgregarEstudiante("Luis", 2);
        _base.Persistencia.Guardar(grupo);
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                CREATE TRIGGER fallar_guardado_intermedio
                BEFORE UPDATE ON estudiantes
                WHEN NEW.nombre = 'Falla'
                BEGIN
                    SELECT RAISE(ABORT, 'fallo de prueba');
                END;
                """;
            comando.ExecuteNonQuery();
        }

        grupo.Renombrar("Nombre modificado");
        grupo.RenombrarEstudiante(primero.Id, "Actualizada");
        grupo.RenombrarEstudiante(segundo.Id, "Falla");

        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => _base.Persistencia.Guardar(grupo));
        Assert.IsType<DataIntegrityException>(error.InnerException);

        SqliteConnection.ClearAllPools();
        var cargado = new PersistenciaGrupoSqlite(_base.Ruta).Cargar(grupo.Id)!;
        Assert.Equal("Primero A", cargado.NombreVisible);
        Assert.Equal("Ana", cargado.Estudiantes[0].NombreVisible);
        Assert.Equal("Luis", cargado.Estudiantes[1].NombreVisible);
    }

    public void Dispose() => _base.Dispose();
}