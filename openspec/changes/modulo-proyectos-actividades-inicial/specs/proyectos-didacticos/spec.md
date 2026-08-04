## Purpose

Define el proyecto didáctico de un grupo como contenedor pedagógico y temporal con identidad, periodo, estados y ciclo de vida propios.

## ADDED Requirements

### Requirement: Identidad y datos del proyecto
El sistema SHALL representar cada proyecto mediante `ProyectoId`, `GrupoId`, nombre, descripción, fecha inicial, fecha final, estado, observaciones y versión de concurrencia. El nombre SHALL ser obligatorio, normalizar espacios y limitarse a 150 caracteres; descripción y observaciones SHALL ser opcionales y limitarse a 2000 caracteres cada una. `GrupoId` SHALL permanecer inmutable.

#### Scenario: Crear proyecto válido
- **WHEN** se crea un proyecto con grupo, nombre y periodo válidos
- **THEN** se asigna una identidad estable, se normaliza el nombre y se conserva el grupo sin permitir trasladarlo

#### Scenario: Nombre inválido
- **WHEN** el nombre está vacío, contiene sólo espacios o excede 150 caracteres
- **THEN** la creación o actualización se rechaza sin cambios parciales

### Requirement: Periodo flexible y válido
`FechaInicio` SHALL ser menor o igual que `FechaTermino`. El dominio SHALL permitir cualquier duración válida; una duración menor de 14 días o mayor de 31 SHALL producir sólo una advertencia de interfaz y no bloquear el guardado.

#### Scenario: Periodo invertido
- **WHEN** la fecha inicial es posterior a la fecha final
- **THEN** el proyecto se rechaza sin modificar su estado anterior

#### Scenario: Duración atípica
- **WHEN** el periodo dura menos de 14 días o más de 31
- **THEN** el proyecto puede guardarse y la interfaz informa una advertencia no bloqueante

### Requirement: Ciclo de vida explícito
Todo proyecto nuevo SHALL iniciar en `Borrador`. SHALL permitirse `Borrador`→`EnCurso`, `EnCurso`→`Finalizado` y la reapertura explícita `Finalizado`→`EnCurso` después de confirmación. Las demás transiciones SHALL rechazarse. Finalizar o reabrir no SHALL eliminar proyectos ni actividades.

#### Scenario: Flujo normal
- **WHEN** un proyecto pasa de Borrador a EnCurso y después a Finalizado
- **THEN** cada transición válida conserva identidad, periodo y contenido

#### Scenario: Reabrir finalizado
- **WHEN** el docente confirma explícitamente la reapertura de un proyecto Finalizado
- **THEN** el estado cambia a EnCurso y sus actividades vuelven a admitir edición según sus propias reglas

#### Scenario: Transición inválida
- **WHEN** se intenta finalizar directamente un proyecto Borrador
- **THEN** la operación se rechaza y el estado permanece Borrador

### Requirement: Cambio de periodo compatible con actividades
Antes de reducir o desplazar el periodo, el sistema SHALL comprobar todas las actividades existentes. Si alguna fecha queda fuera del nuevo rango, SHALL bloquear el cambio e informar las fechas incompatibles; MUST NOT mover ni eliminar actividades automáticamente.

#### Scenario: Actividad fuera del nuevo periodo
- **WHEN** se intenta actualizar el periodo y una actividad quedaría fuera
- **THEN** el proyecto conserva su periodo anterior y se informa al menos la fecha incompatible

### Requirement: Eliminación restringida del proyecto
Un proyecto SHALL eliminarse físicamente sólo cuando esté en `Borrador` y no contenga actividades. Un proyecto `EnCurso`, `Finalizado` o con cualquier actividad SHALL conservarse y la eliminación SHALL rechazarse. No SHALL existir borrado en cascada de historial pedagógico.

#### Scenario: Eliminar borrador vacío
- **WHEN** un proyecto Borrador no contiene actividades y se confirma su eliminación
- **THEN** se elimina el proyecto

#### Scenario: Rechazar eliminación con historial
- **WHEN** el proyecto contiene una actividad o no está en Borrador
- **THEN** no se elimina ningún dato

### Requirement: Concurrencia optimista
Cada proyecto persistido SHALL tener una versión incremental. Una actualización, cambio de estado o eliminación SHALL exigir la versión leída; si difiere de la vigente, SHALL rechazarse como conflicto sin sobrescribir cambios ajenos.

#### Scenario: Versión obsoleta
- **WHEN** dos copias intentan guardar y la segunda usa una versión anterior
- **THEN** la segunda operación informa conflicto y conserva el estado ya confirmado

