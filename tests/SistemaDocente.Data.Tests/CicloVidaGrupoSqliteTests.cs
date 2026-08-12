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
        var almacenamiento = new PersistenciaGrupoCicloVidaSqlite(db.Persistencia);
        var vacio = Grupo.Crear("Grupo vacío");
        almacenamiento.Guardar(vacio);

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
        InsertarGrafo(db, grupoId, estudianteId, proyectoId, actividadId);

        var resumen = almacenamiento.ObtenerResumenEliminacion(objetivo.Id);
        Assert.Equal(1, resumen.Estudiantes);
        Assert.Equal(1, resumen.DiasAsistencia);
        Assert.Equal(1, resumen.Proyectos);
        Assert.Equal(1, resumen.Actividades);
        Assert.Equal(1, resumen.Entregas);

        almacenamiento.Eliminar(objetivo.Id);

        Assert.False(almacenamiento.Existe(objetivo.Id));
        Assert.True(almacenamiento.Existe(otro.Id));
        foreach (var tabla in new[]
        {
            "estudiantes",
            "asistencias_diarias",
            "registros_asistencia",
            "proyectos_didacticos",
            "actividades_proyecto",
            "entregas_actividad",
            "ciclo_vida_grupo",
        })
        {
            Assert.Equal(0, Contar(db, tabla, grupoId));
        }
    }

    [Fact]
    public void DecoradorRespaldaAntesDeEliminarGrupoConDatos()
    {
        var eventos = new List<string>();
        var grupo = Grupo.Crear("Quinto A");
        var inner = new AlmacenamientoFalso(
            grupo,
            new ResumenEliminacionGrupo(1, 0, 0, 0, 0, 0),
            eventos);
        var recuperacion = new RecuperacionFalsa(eventos);
        var directorio = CrearDirectorioTemporal();

        try
        {
            var almacenamiento = new AlmacenamientoGruposConRespaldoEliminacion(
                inner,
                recuperacion,
                directorio);

            almacenamiento.Eliminar(grupo.Id);

            Assert.Collection(
                eventos,
                evento => Assert.Equal("backup", evento),
                evento => Assert.Equal("delete", evento));
            Assert.True(inner.Eliminado);
        }
        finally
        {
            LimpiarDirectorio(directorio);
        }
    }

    [Fact]
    public void FalloDeRespaldoImpideEliminarGrupoConDatos()
    {
        var eventos = new List<string>();
        var grupo = Grupo.Crear("Quinto A");
        var inner = new AlmacenamientoFalso(
            grupo,
            new ResumenEliminacionGrupo(1, 0, 0, 0, 0, 0),
            eventos);
        var recuperacion = new RecuperacionFalsa(eventos, fallar: true);
        var directorio = CrearDirectorioTemporal();

        try
        {
            var almacenamiento = new AlmacenamientoGruposConRespaldoEliminacion(
                inner,
                recuperacion,
                directorio);

            Assert.Throws<RecuperacionLocalException>(() => almacenamiento.Eliminar(grupo.Id));
            Assert.Equal("backup", Assert.Single(eventos));
            Assert.False(inner.Eliminado);
        }
        finally
        {
            LimpiarDirectorio(directorio);
        }
    }

    [Fact]
    public void GrupoVacioSeEliminaSinCrearRespaldoAutomatico()
    {
        var eventos = new List<string>();
        var grupo = Grupo.Crear("Error de captura");
        var inner = new AlmacenamientoFalso(
            grupo,
            new ResumenEliminacionGrupo(0, 0, 0, 0, 0, 0),
            eventos);
        var recuperacion = new RecuperacionFalsa(eventos);
        var directorio = CrearDirectorioTemporal();

        try
        {
            var almacenamiento = new AlmacenamientoGruposConRespaldoEliminacion(
                inner,
                recuperacion,
                directorio);
            almacenamiento.Eliminar(grupo.Id);

            Assert.Equal("delete", Assert.Single(eventos));
            Assert.True(inner.Eliminado);
        }
        finally
        {
            LimpiarDirectorio(directorio);
        }
    }

    private static void InsertarGrafo(
        BaseSqliteTemporal db,
        string grupoId,
        string estudianteId,
        string proyectoId,
        string actividadId)
    {
        using var conexion = db.AbrirConexion();
        using var comando = conexion.CreateCommand();
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

    private static int LeerEntero(BaseSqliteTemporal db, string sql)
    {
        using var conexion = db.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static int Contar(BaseSqliteTemporal db, string tabla, string grupoId)
    {
        using var conexion = db.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = $"SELECT COUNT(*) FROM {tabla} WHERE grupo_id = $grupoId;";
        comando.Parameters.AddWithValue("$grupoId", grupoId);
        return Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string CrearDirectorioTemporal() => Path.Combine(
        Path.GetTempPath(),
        "AulaRaiz.Tests",
        Guid.NewGuid().ToString("N"));

    private static void LimpiarDirectorio(string directorio)
    {
        if (Directory.Exists(directorio)) Directory.Delete(directorio, true);
    }

    private sealed class AlmacenamientoFalso : IAlmacenamientoGrupos
    {
        private Grupo? _grupo;
        private readonly ResumenEliminacionGrupo _resumen;
        private readonly List<string> _eventos;

        internal AlmacenamientoFalso(
            Grupo grupo,
            ResumenEliminacionGrupo resumen,
            List<string> eventos)
        {
            _grupo = grupo;
            _resumen = resumen;
            _eventos = eventos;
        }

        internal bool Eliminado { get; private set; }

        public Grupo? Cargar(GrupoId grupoId) => _grupo?.Id == grupoId ? _grupo : null;
        public bool Existe(GrupoId grupoId) => _grupo?.Id == grupoId;
        public void Guardar(Grupo grupo) => _grupo = grupo;
        public IReadOnlyList<Grupo> ListarTodos() => _grupo is null ? [] : [_grupo];
        public ResumenEliminacionGrupo ObtenerResumenEliminacion(GrupoId grupoId) => _resumen;

        public void Eliminar(GrupoId grupoId)
        {
            _eventos.Add("delete");
            Eliminado = true;
            _grupo = null;
        }
    }

    private sealed class RecuperacionFalsa : IServicioRecuperacionLocal
    {
        private readonly List<string> _eventos;
        private readonly bool _fallar;

        internal RecuperacionFalsa(List<string> eventos, bool fallar = false)
        {
            _eventos = eventos;
            _fallar = fallar;
        }

        public ModoAlmacenamientoLocal ModoActual => ModoAlmacenamientoLocal.Produccion;

        public ResultadoRespaldoLocal CrearRespaldo(
            string rutaDestino,
            DateTimeOffset ahoraUtc,
            string versionAplicacion)
        {
            _eventos.Add("backup");
            if (_fallar)
            {
                throw new RecuperacionLocalException(
                    CategoriaErrorRecuperacionLocal.RespaldoSeguridad,
                    "Fallo simulado.");
            }

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