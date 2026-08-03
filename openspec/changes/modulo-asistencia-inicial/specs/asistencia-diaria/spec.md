## Purpose

Define el registro diario de asistencia de un grupo y mantiene al día como única unidad de dominio, incluso cuando la interfaz consulta y edita un mes completo.

## ADDED Requirements

### Requirement: Asistencia diaria identificada por grupo y fecha
Core SHALL representar cada asistencia mediante `GrupoId` y `DateOnly`; la pareja grupo-fecha SHALL identificar de forma única el agregado sin introducir `AsistenciaId`.

#### Scenario: Días distintos
- **WHEN** un grupo registra dos fechas diferentes
- **THEN** existen dos agregados diarios independientes

### Requirement: Estados cerrados y exclusivos
Cada registro SHALL tener exactamente uno de los valores mutuamente excluyentes `Presente`, `Falta`, `Retardo` o `Justificada`. `Justificada` SHALL significar ausencia justificada sin motivo, documento ni evidencia.

#### Scenario: Estado inválido
- **WHEN** se crea, modifica o rehidrata un registro con un valor fuera del conjunto
- **THEN** Core rechaza la operación sin cambios parciales

### Requirement: Padrón diario único
Una `AsistenciaDiaria` SHALL contener como máximo un registro por `EstudianteId`, conservar sólo identidad y estado, exponer una vista de solo lectura y rechazar identidades duplicadas o ausentes al modificar.

#### Scenario: Cambiar un estado
- **WHEN** se cambia el estado de un estudiante perteneciente al padrón
- **THEN** sólo cambia ese registro y se conservan grupo, fecha e identidades

#### Scenario: Estudiante ausente
- **WHEN** se intenta modificar un estudiante que no pertenece al padrón
- **THEN** Core rechaza la operación y conserva todos los registros

### Requirement: Rehidratación neutral
Core SHALL rehidratar públicamente un día completo conservando grupo, fecha, identidades y estados, validando todo antes de devolverlo y sin depender de Data o SQLite.

#### Scenario: Snapshot contradictorio
- **WHEN** cualquier dato rehidratado incumple una invariante
- **THEN** no se devuelve un agregado parcial

### Requirement: El mes no es un agregado
Core SHALL permanecer ajeno a la proyección mensual. Un mes no SHALL tener identidad, mutaciones ni atomicidad de dominio y no SHALL sustituir a `AsistenciaDiaria`.

#### Scenario: Consultar un mes
- **WHEN** Application reúne varios días para una vista mensual
- **THEN** cada día conserva su agregado e invariantes independientes y Core no crea un agregado mensual
