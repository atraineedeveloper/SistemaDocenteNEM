## Purpose

Define cada actividad de proyecto como unidad independiente y atómica para registrar el trabajo realizado y las entregas de su padrón histórico.

## ADDED Requirements

### Requirement: Actividad identificada y ligada al proyecto
Cada actividad SHALL tener `ActividadId`, `ProyectoId`, `GrupoId`, título, descripción, fecha de realización, observaciones generales, estado y versión de concurrencia. SHALL pertenecer exactamente a un proyecto y grupo, y MUST NOT trasladarse a otro proyecto o grupo. El título SHALL normalizar espacios, ser obligatorio y limitarse a 200 caracteres; descripción y observaciones generales SHALL limitarse a 2000 caracteres.

#### Scenario: Crear actividad válida
- **WHEN** se crea una actividad con título, proyecto, grupo y fecha válidos
- **THEN** obtiene identidad propia y conserva permanentemente sus pertenencias

#### Scenario: Intentar mover actividad
- **WHEN** una actualización cambia ProyectoId o GrupoId
- **THEN** se rechaza toda la actualización

### Requirement: Fecha y estado del proyecto
La fecha de realización SHALL estar dentro del periodo inclusivo del proyecto. No SHALL crearse ni editarse una actividad cuando el proyecto esté `Finalizado`; sus datos permanecerán consultables y volverán a admitir edición sólo después de reabrir el proyecto.

#### Scenario: Fecha fuera del periodo
- **WHEN** la fecha de realización queda antes del inicio o después del término
- **THEN** la actividad se rechaza sin persistencia parcial

#### Scenario: Proyecto finalizado
- **WHEN** se intenta crear o editar una actividad de un proyecto Finalizado
- **THEN** la operación se bloquea y la actividad existente sigue disponible en sólo lectura

### Requirement: Padrón completo de entregas
Una actividad nueva SHALL incluir exactamente un registro `Pendiente` por cada estudiante activo del grupo. Cada registro SHALL contener `EstudianteId`, `EstadoEntrega` y observación opcional de hasta 500 caracteres. Los estados válidos SHALL ser exclusivamente `Pendiente`, `Entregada` y `NoEntregada`; no SHALL haber duplicados ni estudiantes ajenos al grupo.

#### Scenario: Crear padrón inicial
- **WHEN** se prepara una actividad para un grupo con estudiantes activos
- **THEN** existe exactamente una entrega Pendiente por cada estudiante activo y ninguna por estudiantes inactivos

#### Scenario: Padrón inválido
- **WHEN** faltan registros, hay duplicados o aparece un estudiante de otro grupo
- **THEN** la actividad completa se rechaza sin cambios parciales

### Requirement: Historial de matrícula
Una actividad guardada SHALL conservar su padrón histórico. Un estudiante desactivado posteriormente SHALL permanecer visible como “Inactivo actualmente”; uno agregado después MUST NOT incorporarse retroactivamente. Nombres y números visibles SHALL provenir de la matrícula actual y esta versión no SHALL conservar una fotografía histórica de esos datos.

#### Scenario: Desactivación posterior
- **WHEN** un estudiante del padrón se desactiva después del guardado
- **THEN** permanece en la actividad con identidad, entrega y observación conservadas e indicador inactivo

#### Scenario: Alta posterior
- **WHEN** se agrega un estudiante después de guardar una actividad
- **THEN** no aparece en esa actividad pero sí en nuevas actividades posteriores mientras esté activo

### Requirement: Guardado atómico por actividad
La actividad, sus datos y todas sus entregas SHALL guardarse como una única unidad atómica. No SHALL existir una transacción independiente por entrega ni una transacción que abarque el proyecto y todas sus actividades.

#### Scenario: Fallo durante entregas
- **WHEN** falla la persistencia después de escribir parte de una actividad
- **THEN** se revierten encabezado y todas las entregas y el proyecto y otras actividades permanecen intactos

### Requirement: Anulación y eliminación de actividad
Una actividad SHALL poder eliminarse físicamente, con confirmación, sólo cuando todos sus registros sigan `Pendiente`. Si contiene al menos una `Entregada` o `NoEntregada`, MUST NOT eliminarse y SHALL poder anularse con confirmación. Una actividad `Anulada` SHALL permanecer visible en historial, no participar en conteos, no admitir edición ni cambios de entrega y no borrarse físicamente.

#### Scenario: Eliminar sin seguimiento
- **WHEN** todos los registros están Pendiente y se confirma eliminar
- **THEN** se elimina atómicamente la actividad y sus entregas

#### Scenario: Anular con seguimiento
- **WHEN** existe una entrega Entregada o NoEntregada y se confirma la anulación
- **THEN** la actividad queda Anulada, visible y excluida de conteos sin borrar su historial

### Requirement: Concurrencia optimista de actividad
Cada actividad persistida SHALL tener una versión incremental. Actualizar, guardar entregas, anular o eliminar SHALL exigir la versión vigente y rechazar versiones obsoletas sin sobrescribir datos.

#### Scenario: Guardado concurrente
- **WHEN** se guardan entregas usando una versión que ya fue reemplazada
- **THEN** se informa conflicto y se conservan tanto el snapshot confirmado como la edición local no guardada

