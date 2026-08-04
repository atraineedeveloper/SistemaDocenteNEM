## Purpose

Coordina proyectos, actividades y entregas mediante contratos específicos y snapshots inmutables sin exponer agregados ni infraestructura.

## ADDED Requirements

### Requirement: Casos de uso específicos de proyectos
Application SHALL ofrecer operaciones para crear, obtener, listar por grupo, actualizar, cambiar estado y eliminar un proyecto Borrador sin actividades mediante puertos específicos, sin repositorio genérico. Las operaciones SHALL validar grupo, versión, periodo, actividades incompatibles y reglas de estado antes de persistir.

#### Scenario: Reducir periodo incompatible
- **WHEN** ActualizarProyecto detecta actividades fuera del nuevo periodo
- **THEN** no guarda el proyecto y devuelve un conflicto con las fechas incompatibles

### Requirement: Casos de uso específicos de actividades
Application SHALL ofrecer operaciones para preparar, crear, obtener, listar, actualizar, guardar entregas, anular y eliminar una actividad sin seguimiento. Cada escritura SHALL cargar proyecto, grupo y estado vigentes, validar pertenencias, periodo, versión y padrón completo, y efectuar una única persistencia del agregado de actividad.

#### Scenario: Preparar sin persistir
- **WHEN** se prepara una actividad en un proyecto editable
- **THEN** se devuelve un borrador con estudiantes activos en Pendiente sin invocar guardado

#### Scenario: Guardar actividad completa
- **WHEN** la entrada contiene proyecto, fecha, versión y padrón válidos
- **THEN** se persiste exactamente una actividad completa mediante una sola llamada al puerto

### Requirement: Snapshots inmutables y completos
Application SHALL devolver `ProyectoResumen`, `ProyectoDetalle`, `ActividadProyectoDetalle` y `EntregaActividadDetalle` equivalentes, materializando arreglos nuevos. Los snapshots SHALL incluir identidades internas, versión, datos, estado, conteos y situación activa actual, pero MUST NOT exponer agregados mutables, SQLite ni tipos de Data.

#### Scenario: Dos consultas
- **WHEN** se consulta dos veces el mismo proyecto o actividad
- **THEN** se obtienen colecciones materializadas independientes y modificar una colección externa no altera consultas posteriores

### Requirement: Orden contractual
Los proyectos SHALL ordenarse por estado `EnCurso`, `Borrador`, `Finalizado`, luego FechaInicio descendente, nombre e identidad. Las actividades SHALL ordenarse por FechaRealizacion ascendente, título e identidad. Las entregas SHALL ordenarse por número de lista, nombre visible e identidad.

#### Scenario: Listados deterministas
- **WHEN** varios elementos coinciden en campos principales
- **THEN** los desempates por nombre e identidad producen el mismo orden en consultas repetidas

### Requirement: Conteos sin calificación
Cada actividad SHALL informar total del padrón y conteos de Pendiente, Entregada y NoEntregada, excluyendo actividades anuladas de agregaciones de proyecto. Application MUST NOT calcular calificaciones, promedio, rúbrica ni porcentaje académico.

#### Scenario: Contar entregas
- **WHEN** una actividad activa contiene dos Entregadas, una NoEntregada y una Pendiente
- **THEN** devuelve total cuatro y los tres conteos separados sin calificación

### Requirement: Errores y conflictos identificables
Application SHALL distinguir validación, periodo incompatible, concurrencia y persistencia. Los conflictos y fallos MUST NOT presentar datos como guardados ni descartar la entrada editable del consumidor; las causas técnicas SHALL permanecer encapsuladas para que Presentation muestre mensajes seguros.

#### Scenario: Persistencia falla
- **WHEN** el puerto informa un error técnico al guardar una actividad
- **THEN** Application no devuelve snapshot confirmado y el consumidor puede conservar toda su edición

