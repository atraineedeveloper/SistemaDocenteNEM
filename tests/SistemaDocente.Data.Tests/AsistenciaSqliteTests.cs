using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class AsistenciaSqliteTests : IDisposable
{
    private static readonly DateOnly Fecha = new(2026, 8, 3);
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void AdaptadorGuardaCargaActualizaYReabreConIdentidadesEstables()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var bea = grupo.AgregarEstudiante("Bea", 2);
        _base.Persistencia.Guardar(grupo);
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Presente), new(bea.Id, EstadoAsistencia.Falta)]);

        almacenamiento.Guardar(asistencia);
        asistencia.CambiarEstado(ana.Id, EstadoAsistencia.Retardo);
        almacenamiento.Guardar(asistencia);
        SqliteConnection.ClearAllPools();
        var reabierta = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var cargada = reabierta.Cargar(grupo.Id, Fecha)!;

        Assert.IsAssignableFrom<IAlmacenamientoAsistencias>(almacenamiento);
        Assert.True(reabierta.Existe(grupo.Id, Fecha));
        Assert.False(reabierta.Existe(grupo.Id, Fecha.AddDays(1)));
        Assert.Null(reabierta.Cargar(grupo.Id, Fecha.AddDays(1)));
        Assert.Equal(grupo.Id, cargada.GrupoId);
        Assert.Equal([ana.Id, bea.Id], cargada.Registros.Select(x => x.EstudianteId));
        Assert.Equal(EstadoAsistencia.Retardo, cargada.Registros[0].Estado);
    }

    [Fact]
    public void GuardadoNoEliminaRegistroHistoricoInactivo()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var inactivo = grupo.AgregarEstudiante("Beto", 2);
        _base.Persistencia.Guardar(grupo);
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var completa = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Presente), new(inactivo.Id, EstadoAsistencia.Falta)]);
        almacenamiento.Guardar(completa);
        grupo.DesactivarEstudiante(inactivo.Id);
        _base.Persistencia.Guardar(grupo);
        completa.CambiarEstado(inactivo.Id, EstadoAsistencia.Justificada);

        almacenamiento.Guardar(completa);

        var cargada = almacenamiento.Cargar(grupo.Id, Fecha)!;
        Assert.Equal(2, cargada.Registros.Count);
        Assert.Equal(
            EstadoAsistencia.Justificada,
            cargada.Registros.Single(x => x.EstudianteId == inactivo.Id).Estado);
    }

    [Fact]
    public void TriggerAbortRevierteActualizacionParcial()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var bea = grupo.AgregarEstudiante("Bea", 2);
        _base.Persistencia.Guardar(grupo);
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Presente), new(bea.Id, EstadoAsistencia.Presente)]);
        almacenamiento.Guardar(asistencia);
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = $"""
                CREATE TRIGGER fallo_guardado
                BEFORE UPDATE OF estado ON registros_asistencia
                WHEN NEW.estudiante_id = '{bea.Id.Valor:D}'
                BEGIN
                    SELECT RAISE(ABORT, 'fallo inducido');
                END;
                """;
            comando.ExecuteNonQuery();
        }

        asistencia.CambiarEstado(ana.Id, EstadoAsistencia.Falta);
        asistencia.CambiarEstado(bea.Id, EstadoAsistencia.Retardo);
        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => almacenamiento.Guardar(asistencia));
        var cargada = almacenamiento.Cargar(grupo.Id, Fecha)!;

        Assert.IsAssignableFrom<DataAccessException>(error.InnerException);
        Assert.All(cargada.Registros, x => Assert.Equal(EstadoAsistencia.Presente, x.Estado));
    }

    [Fact]
    public void EstudianteDeOtroGrupoEsRechazadoYTraducido()
    {
        var grupo = Grupo.Crear("Primero A");
        var otro = Grupo.Crear("Segundo B");
        var ajeno = otro.AgregarEstudiante("Ajeno", 1);
        _base.Persistencia.Guardar(grupo);
        _base.Persistencia.Guardar(otro);
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(ajeno.Id, EstadoAsistencia.Presente)]);

        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => almacenamiento.Guardar(asistencia));

        Assert.IsType<DataIntegrityException>(error.InnerException);
        Assert.IsType<SqliteException>(error.InnerException!.InnerException);
    }

    [Fact]
    public void ArchivosTemporalesNoCompartenAsistencia()
    {
        using var otra = new BaseSqliteTemporal();
        var grupo = Grupo.Crear("Primero A");
        _base.Persistencia.Guardar(grupo);
        otra.Persistencia.Guardar(grupo);
        new PersistenciaAsistenciaSqlite(_base.Ruta).Guardar(
            AsistenciaDiaria.Crear(grupo.Id, Fecha, []));

        Assert.Null(new PersistenciaAsistenciaSqlite(otra.Ruta).Cargar(grupo.Id, Fecha));
    }

    [Fact]
    public void CargarIntervaloDevuelveDiasCompletosOrdenadosYSoloDelGrupo()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var otro = Grupo.Crear("Otro");
        _base.Persistencia.Guardar(grupo);
        _base.Persistencia.Guardar(otro);
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        almacenamiento.Guardar(AsistenciaDiaria.Crear(
            grupo.Id, Fecha.AddDays(2), [new(estudiante.Id, EstadoAsistencia.Falta)]));
        almacenamiento.Guardar(AsistenciaDiaria.Crear(
            grupo.Id, Fecha, [new(estudiante.Id, EstadoAsistencia.Presente)]));
        almacenamiento.Guardar(AsistenciaDiaria.Crear(otro.Id, Fecha, []));

        var dias = almacenamiento.CargarIntervalo(grupo.Id, Fecha, Fecha.AddDays(2));

        Assert.Equal([Fecha, Fecha.AddDays(2)], dias.Select(x => x.Fecha));
        Assert.All(dias, x => Assert.Equal(grupo.Id, x.GrupoId));
        Assert.Empty(almacenamiento.CargarIntervalo(
            grupo.Id, Fecha.AddMonths(1), Fecha.AddMonths(1).AddDays(5)));
    }

    [Fact]
    public void CargarIntervaloRechazaRangoInversoSinCambiarVersion()
    {
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        _base.Persistencia.Inicializar();

        Assert.Throws<ArgumentException>(() => almacenamiento.CargarIntervalo(
            GrupoId.DesdeGuid(Guid.NewGuid()), Fecha.AddDays(1), Fecha));

        using var conexion = _base.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA user_version;";
        Assert.Equal(2L, comando.ExecuteScalar());
    }

    [Fact]
    public void CargarIntervaloRecuperaMesCompletoTrasReapertura()
    {
        var grupo = Grupo.Crear("Primero A");
        _base.Persistencia.Guardar(grupo);
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var inicio = new DateOnly(2026, 8, 1);
        var fin = new DateOnly(2026, 8, 31);
        foreach (var numero in Enumerable.Range(0, 31))
        {
            almacenamiento.Guardar(AsistenciaDiaria.Crear(grupo.Id, inicio.AddDays(numero), []));
        }

        SqliteConnection.ClearAllPools();
        var reabierta = new PersistenciaAsistenciaSqlite(_base.Ruta);
        var resultado = reabierta.CargarIntervalo(grupo.Id, inicio, fin);

        Assert.Equal(31, resultado.Count);
        Assert.Equal(inicio, resultado[0].Fecha);
        Assert.Equal(fin, resultado[^1].Fecha);
    }

    [Fact]
    public void CargarIntervaloTraduceErrorTecnicoSinModificarVersion()
    {
        _base.Persistencia.Inicializar();
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = "PRAGMA user_version = 3;";
            comando.ExecuteNonQuery();
        }
        var almacenamiento = new PersistenciaAsistenciaSqlite(_base.Ruta);

        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(() =>
            almacenamiento.CargarIntervalo(GrupoId.DesdeGuid(Guid.NewGuid()), Fecha, Fecha.AddDays(1)));

        Assert.IsAssignableFrom<DataAccessException>(error.InnerException);
        using var verificacion = _base.AbrirConexion();
        using var consulta = verificacion.CreateCommand();
        consulta.CommandText = "PRAGMA user_version;";
        Assert.Equal(3L, consulta.ExecuteScalar());
    }

    public void Dispose() => _base.Dispose();
}