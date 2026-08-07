# expedientes-alumnos Specification

## Purpose

Consolida el seguimiento individual de cada estudiante mediante una ficha pedagógica que reúne información existente y registros cualitativos propios, sin convertir alertas en diagnósticos clínicos.

## Requirements

### Requirement: Expediente individual consolidado
El sistema SHALL proveer un expediente individual por estudiante que consolide asistencia, entregas de actividades, evaluación formativa y notas pedagógicas cualitativas disponibles.

#### Scenario: Consulta de expediente consolidado del estudiante
- **WHEN** el usuario selecciona un estudiante del grupo y abre su expediente individual
- **THEN** el sistema muestra la información consolidada disponible del estudiante, incluida asistencia, actividad/evaluación y seguimiento pedagógico

### Requirement: Registro de notas pedagógicas
El expediente SHALL permitir registrar fortalezas, dificultades, apoyos aplicados y observaciones cronológicas vinculadas al estudiante, con fecha de registro y contenido pedagógico validado.

#### Scenario: Registrar un apoyo aplicado
- **WHEN** el docente registra un apoyo pedagógico aplicado
- **THEN** el sistema conserva la nota vinculada al estudiante y la presenta en su expediente

### Requirement: Registro de acuerdos con familiares o tutores
El expediente SHALL permitir registrar acuerdos con familiares o tutores, incluyendo motivo, compromisos y fechas de seguimiento cuando correspondan.

#### Scenario: Registrar acuerdo con tutor
- **WHEN** el docente registra el resultado de una reunión con tutor o familiar
- **THEN** el sistema conserva el acuerdo y sus compromisos dentro del expediente del estudiante

### Requirement: Alertas pedagógicas sin diagnósticos
Las alertas derivadas de asistencia, seguimiento de actividades o notas pedagógicas SHALL ser orientativas y SHALL evitar emitir diagnósticos clínicos, médicos o psicológicos.

#### Scenario: Estudiante con incidencias de seguimiento
- **WHEN** el expediente detecta información que requiere atención pedagógica
- **THEN** la interfaz puede destacarla como alerta de seguimiento sin etiquetar al estudiante con un diagnóstico