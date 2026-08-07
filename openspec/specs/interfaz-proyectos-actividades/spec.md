# interfaz-proyectos-actividades Specification

## Purpose

Define la separación entre planeación de proyectos/actividades y la superficie operativa de Evaluación, usando ventanas dedicadas para edición compleja y una matriz para el seguimiento del grupo.

## Requirements

### Requirement: Navegación separada de Proyectos y Evaluación
El sistema SHALL mantener Proyectos y Evaluación como módulos principales separados dentro de la navegación global. Proyectos SHALL concentrar planeación y acceso a detalles; Evaluación SHALL concentrar el seguimiento matricial del proyecto seleccionado.

#### Scenario: Abrir módulo Evaluación
- **WHEN** el usuario selecciona `Evaluación` en la navegación principal
- **THEN** el sistema muestra la matriz de evaluación del grupo y permite seleccionar el proyecto didáctico

### Requirement: Detalle de proyecto en ventana dedicada
El módulo Proyectos SHALL permitir abrir `DetalleProyectoWindow` para editar datos del proyecto y gestionar el acceso a sus actividades sin convertir la vista principal en un master-detail obligatorio.

#### Scenario: Apertura de detalle de proyecto
- **WHEN** el usuario abre un proyecto desde la lista principal
- **THEN** el sistema muestra `DetalleProyectoWindow` con la información y acciones correspondientes al proyecto seleccionado

### Requirement: Detalle de actividad en ventana dedicada
La edición compleja de una actividad SHALL realizarse en `DetalleActividadWindow`, conservando la actividad como unidad de guardado de su padrón histórico.

#### Scenario: Apertura de actividad
- **WHEN** el usuario abre una actividad existente o crea una nueva desde el detalle del proyecto
- **THEN** el sistema muestra `DetalleActividadWindow` para editar sus datos

### Requirement: Evaluación matricial sin selector independiente de actividad
Evaluación SHALL representar estudiantes en filas y actividades en columnas. La columna de la celda actual SHALL definir la actividad de contexto para métricas y acciones masivas, sin reintroducir un selector independiente de actividad.

#### Scenario: Cambiar de actividad mediante la matriz
- **WHEN** el usuario mueve la celda actual a otra columna de actividad
- **THEN** las métricas y acciones de Evaluación usan esa actividad como contexto

### Requirement: Padrón histórico preservado
La interfaz SHALL respetar el padrón histórico de cada actividad y SHALL mostrar como no aplicable una celda correspondiente a un estudiante que todavía no pertenecía a esa actividad.

#### Scenario: Estudiante incorporado posteriormente
- **WHEN** un estudiante fue dado de alta después de una actividad anterior
- **THEN** la matriz muestra `—` para esa actividad previa y no permite editar la celda