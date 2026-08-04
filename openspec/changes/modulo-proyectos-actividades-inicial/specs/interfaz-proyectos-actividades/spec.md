## Purpose

Permite gestionar proyectos, actividades y entregas desde una experiencia WPF integrada, comprobable y segura para el trabajo cotidiano docente.

## ADDED Requirements

### Requirement: Navegación integrada a Proyectos
`MainWindow` SHALL ofrecer Grupo, Asistencia y Proyectos sin framework de navegación. Proyectos SHALL usar una vista redimensionable con lista de proyectos, actividades del seleccionado y detalle/grilla de entregas de la actividad seleccionada; no SHALL abrir una ventana por estudiante ni mostrar identidades internas.

#### Scenario: Abrir Proyectos
- **WHEN** el docente activa Proyectos
- **THEN** la misma ventana muestra las tres zonas y conserva disponibles los demás módulos

### Requirement: Gestión visual de proyectos
La interfaz SHALL permitir listar, filtrar por estado, seleccionar, crear, editar, guardar, cambiar estado, reabrir y confirmar eliminación. SHALL mostrar una advertencia no bloqueante para periodos menores de 14 o mayores de 31 días y deshabilitar comandos durante operaciones.

#### Scenario: Duración atípica
- **WHEN** se captura un periodo válido fuera del intervalo recomendado
- **THEN** se muestra una advertencia y Guardar continúa disponible

#### Scenario: Reapertura confirmada
- **WHEN** se solicita reabrir un proyecto Finalizado y se confirma
- **THEN** la interfaz ejecuta la transición y habilita nuevamente sus acciones editables

### Requirement: Gestión visual de actividades
La interfaz SHALL listar actividades cronológicamente, buscar por texto o fecha, seleccionar, crear, editar, guardar, anular y eliminar según reglas. Un proyecto Finalizado y una actividad Anulada SHALL mostrarse en sólo lectura con explicación visible.

#### Scenario: Actividad anulada
- **WHEN** se selecciona una actividad Anulada
- **THEN** sus datos e historial son visibles pero Guardar y cambios de entrega están deshabilitados

### Requirement: Captura de entregas eficiente
La grilla SHALL mostrar número, nombre, “Inactivo actualmente”, estado y observación por estudiante, con conteos de total, Pendiente, Entregada y NoEntregada. SHALL permitir marcar selección o todos, filtros Todos/Pendientes/Entregadas/No entregadas/Sólo incidencias/Activos/Activos e inactivos históricos y atajos `E`, `N`, `P` y `Ctrl+S`. No SHALL usar un `ComboBox` permanente cuando el selector compacto esté cerrado.

#### Scenario: Captura por teclado
- **WHEN** se presiona E, N o P sobre filas seleccionadas editables
- **THEN** cambia sólo su estado, se recalculan conteos y la edición queda pendiente hasta Guardar

#### Scenario: Incidencias
- **WHEN** la actividad ya fue realizada y se filtra Sólo incidencias
- **THEN** permanecen estudiantes Pendiente o NoEntregada

### Requirement: Snapshot, cambios pendientes y confirmaciones
Los ViewModels SHALL mantener snapshot confirmado, copia editable y `TieneCambios`. Cambiar actividad, proyecto, módulo o cerrar SHALL reutilizar Guardar/Descartar/Cancelar. Guardar continuará la transición sólo tras éxito; un fallo o conflicto SHALL conservar selección y edición local.

#### Scenario: Fallo antes de cambiar actividad
- **WHEN** se elige Guardar y la persistencia falla
- **THEN** no cambia la actividad y se conservan estados y observaciones editados

### Requirement: Mensajes seguros y CanExecute real
Validación, periodo, concurrencia y persistencia SHALL producir mensajes corregibles en español sin SQL, rutas, `InnerException` ni trazas. Comandos SHALL recalcular `CanExecute` al cambiar selección, edición, estado del proyecto o actividad, cambios pendientes y estado ocupado; nunca SHALL mostrarse como guardado un fallo.

#### Scenario: Conflicto concurrente
- **WHEN** el guardado usa una versión obsoleta
- **THEN** se informa que los datos cambiaron, se conserva la edición y se ofrece recargar sin cerrar la aplicación

### Requirement: Presentación portable y composición comprobable
Las decisiones de presentación SHALL residir en ViewModels sin referencias a WPF, Data ni SQLite. WPF SHALL limitarse a controles, bindings, foco, teclado y composición manual, sin contenedor DI, asincronía, `Task.Run`, paquetes UI externos ni acceso SQL.

#### Scenario: Auditar dependencias
- **WHEN** se inspeccionan ensamblados y código de Presentation y WPF
- **THEN** Presentation depende sólo de Application y únicamente la raíz WPF crea adaptadores Data

