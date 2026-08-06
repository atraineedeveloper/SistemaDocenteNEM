# expedientes-alumnos Specification

## Purpose
TBD - created by archiving change expedientes-alumnos-seguimiento. Update Purpose after archive.
## Requirements
### Requirement: EXPEDIENTE_INDIVIDUAL_ESTUDIANTE
El sistema DEBE proveer un expediente individual por estudiante que consolide en una sola ficha la asistencia, entregas de actividades, evaluaciones de desempeño NEM y notas pedagógicas cualitativas.

#### Scenario: Consulta de expediente consolidado del estudiante
- **Given** que el usuario está en la lista de estudiantes del grupo
- **When** selecciona un estudiante y abre su expediente individual
- **Then** el sistema muestra la ficha consolidada con porcentaje de asistencia, historial de proyectos y resumen de desempeño NEM.

#### Scenario: Registro de fortalezas, dificultades y apoyos aplicados
- **Given** que el usuario está en la ficha del estudiante
- **When** registra una fortaleza, dificultad o apoyo pedagógico aplicado
- **Then** el sistema guarda síncronamente la nota cualitativa vinculada al estudiante con su fecha de registro.

#### Scenario: Registro de acuerdos con familiares o tutores
- **Given** que el usuario se reunió con la familia o tutor del estudiante
- **When** registra el motivo y acuerdo alcanzado
- **Then** el sistema guarda el acuerdo con fecha y compromisos contraídos.

#### Scenario: Alertas pedagógicas sin emision de diagnósticos
- **Given** un estudiante con inasistencias acumuladas o entregas en nivel "Requiere apoyo" / "No entregó"
- **When** el docente consulta su expediente
- **Then** el sistema destaca alertas pedagógicas orientativas sin emitir diagnósticos clínicos o médicos.

