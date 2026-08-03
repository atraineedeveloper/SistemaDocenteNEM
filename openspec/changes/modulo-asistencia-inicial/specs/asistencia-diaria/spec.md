## Purpose

Define el registro diario de asistencia de un grupo y protege su fecha, sus estados y la conservación íntegra de sus registros históricos.

## ADDED Requirements

### Requirement: Asistencia diaria identificada por grupo y fecha
Core SHALL representar cada asistencia diaria mediante un `GrupoId` existente y una `DateOnly`; la pareja grupo-fecha SHALL identificar de forma única el día sin introducir una identidad adicional.

#### Scenario: Crear asistencia para una fecha
- **WHEN** se crea una asistencia para un grupo y una fecha
- **THEN** conserva exactamente el grupo y la fecha recibidos

#### Scenario: Dos fechas del mismo grupo
- **WHEN** se crean asistencias para dos fechas distintas del mismo grupo
- **THEN** se consideran días de asistencia distintos

### Requirement: Estados de asistencia cerrados
Core SHALL definir `EstadoAsistencia` con exactamente los valores mutuamente excluyentes `Presente`, `Falta`, `Retardo` y `Justificada`. `Justificada` SHALL significar ausencia justificada y no SHALL contener motivo, documento, evidencia ni efecto adicional en este cambio. Cada estudiante del padrón del día SHALL tener exactamente un estado.

#### Scenario: Estados admitidos
- **WHEN** se registra cualquiera de los cuatro estados definidos
- **THEN** el registro conserva ese estado exactamente

#### Scenario: Falta justificada exclusiva
- **WHEN** un estudiante tiene estado `Justificada`
- **THEN** se considera ausente con justificación y no tiene simultáneamente Presente, Falta ni Retardo

#### Scenario: Estado fuera del conjunto
- **WHEN** se intenta crear o rehidratar un registro con un valor fuera del conjunto cerrado
- **THEN** Core rechaza el snapshot completo con una excepción de validación de dominio

### Requirement: Un registro por estudiante
Una `AsistenciaDiaria` SHALL contener como máximo un `RegistroAsistencia` por `EstudianteId`, y cada registro SHALL conservar exactamente la identidad del estudiante y su estado.

#### Scenario: Registro único
- **WHEN** se agrega un estado para un estudiante que aún no está registrado en el día
- **THEN** la asistencia contiene un único registro para esa identidad

#### Scenario: Identidad repetida
- **WHEN** la creación o rehidratación contiene dos registros para el mismo estudiante
- **THEN** Core rechaza la operación de forma atómica

### Requirement: Datos conservados por el agregado
La asistencia SHALL conservar únicamente grupo, fecha, identidad del estudiante y estado. No SHALL duplicar nombre visible, número de lista ni situación activa; esos datos SHALL obtenerse de la matrícula actual cuando se proyecte el día.

#### Scenario: Cambio posterior de matrícula
- **WHEN** cambia el nombre, número o situación activa de un estudiante después de guardar un día
- **THEN** el registro histórico conserva identidad y estado sin contener una copia anterior del nombre o número

### Requirement: Edición atómica en memoria
La asistencia SHALL permitir cambiar el estado de un estudiante ya incluido sin cambiar grupo, fecha ni identidad. Una operación inválida SHALL dejar el agregado sin cambios parciales.

#### Scenario: Cambiar estado
- **WHEN** se cambia el estado de un registro existente a otro estado admitido
- **THEN** sólo cambia el estado de ese estudiante

#### Scenario: Estudiante ausente del día
- **WHEN** se intenta cambiar el estado de una identidad que no forma parte de la asistencia
- **THEN** Core rechaza la operación y conserva todos los registros anteriores

### Requirement: Rehidratación neutral y completa
Core SHALL ofrecer una vía pública neutral para rehidratar una asistencia con su grupo, fecha y registros existentes, sin depender de Data ni SQLite. SHALL validar el snapshot completo antes de devolver el agregado y no SHALL generar identidades ni estados sustitutos.

#### Scenario: Rehidratar datos válidos
- **WHEN** se rehidrata un snapshot válido
- **THEN** se conservan exactamente el grupo, la fecha, las identidades y los estados

#### Scenario: Rehidratar datos contradictorios
- **WHEN** cualquier parte del snapshot incumple una invariante
- **THEN** no se devuelve un agregado parcial

### Requirement: Vistas de solo lectura
Core SHALL exponer los registros mediante una vista de solo lectura que no permita modificar la colección interna.

#### Scenario: Consultar registros
- **WHEN** un consumidor obtiene los registros de una asistencia
- **THEN** no puede agregar, quitar ni reemplazar elementos en la colección del agregado
