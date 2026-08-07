using SistemaDocente.Core;
using SistemaDocente.Data;

namespace SistemaDocente.App.Wpf.Demo;

internal static class DemoContextSeeder
{
    internal static void AsegurarContexto(
        PersistenciaContextoGrupoSqlite contextos,
        GrupoId grupoId)
    {
        ArgumentNullException.ThrowIfNull(contextos);
        if (contextos.Cargar(grupoId) is not null) return;

        contextos.Guardar(ContextoGrupo.Crear(
            grupoId,
            cicloEscolar: "2026-2027",
            nombreEscuela: "Escuela Primaria Benito Juárez · DEMO",
            cct: "27DPR0000X",
            entidadFederativa: "Tabasco",
            municipio: "Centro",
            localidad: "Villahermosa",
            grado: "4.º",
            grupo: "A",
            turno: "Matutino",
            etapaCognoscitiva: EtapaDesarrolloCognoscitivo.NoEspecificada,
            docenteResponsable: "Docente de demostración",
            responsableDesde: new DateOnly(2026, 7, 1),
            horaEntrada: new TimeOnly(8, 0),
            horaSalida: new TimeOnly(12, 30),
            organizacionEscolar: OrganizacionEscolar.Completa,
            gradosAtendidos: [GradoPrimaria.Cuarto]));
    }
}