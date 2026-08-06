using SistemaDocente.Core;

namespace SistemaDocente.Application;

public interface IAlmacenamientoExpedientes
{
    ExpedienteEstudiante ObtenerExpediente(EstudianteId estudianteId, GrupoId grupoId);
    void RegistrarNotaPedagogica(EstudianteId estudianteId, GrupoId grupoId, TipoNotaPedagogica tipo, string contenido, DateTime fechaHora);
    void RegistrarAcuerdoTutor(EstudianteId estudianteId, GrupoId grupoId, string motivo, string acuerdo, DateOnly fechaReunion, DateOnly? fechaSeguimiento);
}
