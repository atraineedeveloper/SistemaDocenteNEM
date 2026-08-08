using SistemaDocente.Application;
using SistemaDocente.Data;

namespace SistemaDocente.Cli;

public sealed class ServiciosCli
{
    public ServiciosCli(bool modoDemostracion, string? localApplicationData = null)
    {
        ModoDemostracion = modoDemostracion;
        var local = localApplicationData
            ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Rutas = RutasAlmacenamientoLocal.DesdeLocalApplicationData(local, modoDemostracion);

        var grupos = new PersistenciaGrupoSqlite(Rutas.BaseSqlite);
        var asistencias = new PersistenciaAsistenciaSqlite(Rutas.BaseSqlite);
        var proyectos = new PersistenciaProyectosSqlite(Rutas.BaseSqlite);
        var expedientes = new PersistenciaExpedienteSqlite(Rutas.BaseSqlite);
        var contextos = new PersistenciaContextoGrupoSqlite(Rutas.BaseSqlite);

        Grupos = new GestionGrupoCasosUso(grupos);
        Asistencia = new GestionAsistenciaCasosUso(grupos, asistencias);
        Reportes = new GestionReportesCasosUso(
            grupos,
            asistencias,
            proyectos,
            proyectos,
            expedientes,
            contextos);
        ContextoAgente = new GestionContextoAgenteCasosUso(Reportes);
        Diagnosticos = RegistroDiagnosticoSeguroArchivo.DesdeLocalApplicationData(local, modoDemostracion);
    }

    public bool ModoDemostracion { get; }
    public RutasAlmacenamientoLocal Rutas { get; }
    public GestionGrupoCasosUso Grupos { get; }
    public GestionAsistenciaCasosUso Asistencia { get; }
    public GestionReportesCasosUso Reportes { get; }
    public GestionContextoAgenteCasosUso ContextoAgente { get; }
    public RegistroDiagnosticoSeguroArchivo Diagnosticos { get; }
    public string ModoTexto => ModoDemostracion ? "demo" : "production";
}