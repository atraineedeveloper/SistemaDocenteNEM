using Microsoft.Data.Sqlite;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class CicloVidaGrupoSqliteTests
{
    [Fact]
    public void ExtensionInicializaGruposExistentesComoActivosYPersisteArchivo()
    {
        using var db = new BaseSqliteTemporal();
        var grupo = Grupo.Crear("Quinto A");
        grupo.AgregarEstudiante("Ana López", 1);
        db.Persistencia.Guardar(grupo);

        var almacenamiento = new PersistenciaGrupoCicloVidaSqlite(db.Persistencia);
        var cargado = almacenamiento.Cargar(grupo.Id)!;

        Assert.False(cargado.EstaArchivado);
        Assert.Equal(6, LeerEntero(db, "PRAGMA user_version;"));
        Assert.Equal(1, LeerEntero(
            db,
            "SELECT version FROM esquema_extensiones WHERE nombre='group-lifecycle';"));

        cargado.Archivar();
        almacenamiento.Guardar(cargado);

        var archivado = almacenamiento.Cargar(grupo.Id)!;
        Assert.True(archivado.EstaArchivado);
        Assert.Equal(grupo.Id, archivado.Id);
        Assert.Equal(Assert.Single(grupo.Estudiantes).Id, Assert.Single(archivado.Estudiantes).Id);

        archivado.Restaurar();
        almacenamiento.Guardar(archivado);

        Assert.False(almacenamiento.Cargar(grupo.Id)!.EstaArchivado);
    }

    [Fact]
    public void ResumenDistingueGrupoVacioDeGrupoConDatos()
    {
        using var db = new BaseSqliteTemporal();
        var vacio = Grupo.Crear("Grupo vacío");
        db.Persistencia.Guardar(vacio);
        var almacenamiento = new PersistenciaGrupoCicloVidaSqlite(db.Persistencia);

        Assert.False(almacenamiento.ObtenerResumenEliminacion(vacio.Id).TieneDatos);

        var poblado = Grupo.Crear("Grupo poblado");
        poblado.AgregarEstudiante("Luis Pérez", 1);
        almacenamiento.Guardar(poblado);

        var resumen = almacenamiento.ObtenerResumenEliminacion(poblado.Id);
        Assert.True(resumen.TieneDatos);
        Assert.Equal(1, resumen.Estudiantes);
    }

    [Fact]
    public void EliminacionQuitaGrafoRelacionalCompletoSinAfectarOtroGrupo()
    {
        using var db = new BaseSqliteTemporal();
        var objetivo = Grupo.Crear("Cuarto A");
        var estudiante = objetivo.AgregarEstudiante("Ana López", 1);
        var otro = Grupo.Crear("Sexto B");
        otro.AgregarEstudiante("Mario Ruiz", 1);
        db.Persistencia.Guardar(objetivo);
        db.Persistencia.Guardar(otro);

        var almacenamiento = new PersistenciaGrupoCicloVidaSqlite(db.Persistencia);
        _ = almacenamiento.ListarTodos();

        var proyectoId = Guid.NewGuid().ToString("D");
        var actividadId = Guid.NewGuid().ToString("D");
        var grupoId = objetivo.Id.Valor.ToString("D");
        var estudianteId = estudiante.Id.Valor.ToString("D");

        using (var conexion = db.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = """
                INSERT INTO asistencias_diarias (grupo_id, fecha)
                VALUES ($grupoId, '2026-08-12');
                INSERT INTO registros_asistencia (grupo_id, fecha, estudiante_id, estado)
                VALUES ($grupoId, '2026-08-12', $estudianteId, 0);
                INSERT INTO proyectos_didacticos (
                    proyecto_id, grupo_id, nombre, descripcion, fecha_inicio,
                    fecha_termino, estado, observaciones, version)
                VALUES ($proyectoId, $grupoId, 'Proyecto', '', '2026-08-12',
                    '2026-08-13', 0, '', 1);
                INSERT INTO actividades_proyecto (
                    actividad_id, proyecto_id, grupo_id, titulo, descripcion,
                    fecha_realizacion, observaciones_generales, estado, version)
                VALUES ($actividadId, $proyectoId, $grupoId, 'Actividad', '',
                    '2026-08-12', '', 0, 1);
                INSERT INTO entregas_actividad (
                    actividad_id, estudiante_id, grupo_id, estado_entrega, observacion)
                VALUES ($actividadId, $estudianteId, $grupoId, 0, '');
                """;
            comando.Parameters.AddWithValue("$grupoId", grupoId);
            comando.Parameters.AddWithValue("$estudianteId", estudianteId);
            comando.Parameters.AddWithValue("$proyectoId", proyectoId);
            comando.Parameters.AddWithValue("$actividadId", actividadId);
            comando.ExecuteNonQuery();
        }

        var resumen = almacenamiento.ObtenerResumenEliminacion(objetivo.Id);
        Assert.Equal(1, resumen.Estudiantes);
        Assert.Equal(1, resumen.DiasAsistencia);
        Assert.Equal(1, resumen.Proyectos);
        Assert.Equal(1, resumen.Actividades);
        Assert.Equal(1, resumen.Entregas);

        almacenamiento.Eliminar(objetivo.Id);

        Assert.False(almacenamiento.Existe(objetivo.Id));
        Assert.True(almacenamiento.Existe(otro.Id));
        Assert.Equal(0, Contar(db, "estudiantes", grupoId));
        Assert.Equal(0, Contar(db, "asistencias_diarias", grupoId));
        Assert.Equal(0, Contar(db, "registros_asistencia", grupoId));
        Assert.Equal(0, Contar(db, "proyectos_didacticos", grupoId));
        Assert.Equal(0, Contar(db, "actividades_proyecto", grupoId));
        Assert.Equal(0, Contar(db, "entregas_actividad", grupoId));
        Assert.Equal(0, Contar(db, "ciclo_vida_grupo", grupoId));
        Assert.Equal(1, Contar(db, "grupos", otro.Id.Valor.ToString("D"), columna: "id"));
    }

    [Fact]
    public void DecoradorRespaldaAntesDeEliminarGrupoConDatos()
    {
        var grupo = Grupo.Crear("Quinto A");
        var inner = new AlmacenamientoFalso(grupo, new ResumenEliminacionGrupo(1, 0, 0, 0, 0, 0));
        var recuperacion = new RecuperacionFalsa();
        var directorio = Path.Combine(Path.GetTempPath(), "AulaRaiz.Tests", Guid.NewGuid().ToString("N"));

        try
        {
            var almacenamiento = new AlmacenamientoGruposConRespaldoEliminacion(
                inner,
                recuperacion,
                directorio);

            almacenamiento.Eliminar(grupo.Id);

            Assert.Equal(1, recuperacion.Creaciones);
            Assert.True(inner.Eliminado);
            Assert.True(recuperacion.CreacionFinalizadaAntesDeEliminar);
        }
        finally
        {
            if (Directory.Exists(directorio)) Directory.Delete(directorio, true);
        }
    }

    [Fact]
    public void FalloDeRespaldoImpideEliminarGrupoConDatos()
    {
        var grupo = Grupo.Crear("Quinto A");
        var inner = new AlmacenamientoFalso(grupo, new ResumenEliminacionGrupo(1, 0, 0, 0, 0, 0));
        var recuperacion = new RecuperacionFalsa { Fallar = true };
        var directorio = Path.Combine(Path.GetTempPath(), "AulaRaiz.Tests", Guid.NewGuid().ToString("N"));

        try
        {
            var almacenamiento = new AlmacenamientoGruposConRespaldoEliminacion(
                inner,
                recuperacion,
                directorio);

            Assert.Throws<RecuperacionLocalException>(() => almacenamiento.Eliminar(grupo.Id));
            Assert.False(inner.Eliminado);
        }
        finally
        {
            if (Directory.Exists(directorio)) Directory.Delete(directorio, true);
        }
    }

    [Fact]
    public void GrupoVacioSeEliminaSinCrearRespaldoAutomatico()
    {
        var grupo = Grupo.Crear("Error de captura");
        var inner = new AlmacenamientoFalso(grupo, new ResumenEliminacionGrupo(0, 0, 0, 0, 0, 0));
        var recuperacion = new RecuperacionFalsa();
        var directorio = Path.Combine(Path.GetTempPath(), "AulaRaiz.Tests", Guid.NewGuid().ToString("N"));

        var almacenamiento = new AlmacenamientoGruposConRespaldoEliminacion(
            inner,
            recuperacion,
            directorio);
        almacenamiento.Eliminar(grupo.Id);

        Assert.Equal(0, recuperacion.Creaciones);
        Assert.True(inner.Eliminado);
    }

    private static int LeerEntero(BaseSqliteTemporal db, string sql)
    {
        using var conexion = db.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static int Contar(
        BaseSqliteTemporal db,
        string tabla,
        string grupoId,
        string columna = "grupo_id")
    {
        using var conexion = db.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = $"SELECT COUNT(*) FROM {tabla} WHERE {columna} = $grupoId;";
        comando.Parameters.AddWithValue("$grupoId", grupoId);
        return Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class AlmacenamientoFalso : IAlmacenamientoGrupos
    {
        private Grupo? _grupo;
        private readonly ResumenEliminacionGrupo _resumen;

        internal AlmacenamientoFalso(Grupo grupo, ResumenEliminacionGrupo resumen)
        {
            _grupo = grupo;
            _resumen = resumen;
        }

        internal bool Eliminado { get; private set; }

        public Grupo? Cargar(GrupoId grupoId) => _grupo?.Id == grupoId ? _grupo : null;
        public bool Existe(GrupoId grupoId) => _grupo?.Id == grupoId;
        public void Guardar(Grupo grupo) => _grupo = grupo;
        public IReadOnlyList<Grupo> ListarTodos() => _grupo is null ? [] : [_grupo];
        public ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId) => _resumen;

        public void Eliminar(GrupoId grupoId)
        {
            RecuperacionFalsa.EliminarObservado = true;
            Eliminado = true;
            _grupo = null;
        }
    }

    private sealed class RecuperacionFalsa : IServicioRecuperacionLocal
    {
        internal static bool EliminarObservado { get; set; }
        internal int Creaciones { get; private set; }
        internal bool Fallar { get; init; }
        internal bool CreacionFinalizadaAntesDeEliminar { get; private set; }
        public ModoAlmacenamientoLocal ModoActual => ModoAlmacenamientoLocal.Produccion;

        public ResultadoRespaldoLocal CrearRespaldo(
            string rutaDestino,
            DateTimeOffset ahoraUtc,
            string versionAplicacion)
        {
            EliminarObservado = false;
            Creaciones++;
            if (Fallar)
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.RespaldoSeguridad,
                    "Fallo simulado.");
            }

            CreacionFinalizadaAntesDeEliminar = !EliminarObservado;
            return new ResultadoRespaldoLocal(
                rutaDestino,
                ahoraUtc,
                versionAplicacion,
                ModoAlmacenamientoLocal.Produccion,
                6,
                0,
                [],
                []);
        }

        public InspeccionRespaldoLocal Inspeccionar(string rutaRespaldo) =>
            throw new NotSupportedException();

        public ResultadoRestauracionLocal Restaurar(
            string rutaRespaldo,
            DateTimeOffset ahoraUtc,
            string versionAplicacion) =>
            throw new NotSupportedException();
    }
}
