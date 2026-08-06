# interfaz-proyectos-actividades Specification

## Purpose
TBD - created by archiving change rediseno-ux-proyectos-evaluacion. Update Purpose after archive.
## Requirements
### Requirement: NAVEGACION_VISTAS_PROYECTOS
El sistema DEBE organizar el flujo de trabajo en dos espacios principales separados:
1. **Módulo de Proyectos (Planeación):** Lista amplia de proyectos que permite abrir ventanas dedicadas de detalle para proyectos y actividades.
2. **Módulo de Evaluación (Aulas):** Pestaña superior independiente que permite seleccionar un proyecto y actividad para evaluar síncronamente a los estudiantes a pantalla completa.

#### Scenario: Selección de pestaña Evaluación
- **Given** que el usuario está en la navegación principal del sistema
- **When** selecciona la pestaña "Evaluación"
- **Then** el sistema muestra la vista dedicada de evaluación con los selectores de Proyecto y Actividad.

#### Scenario: Apertura de detalle de proyecto en ventana dedicada
- **Given** que el usuario está en la lista de proyectos
- **When** hace doble clic o selecciona un proyecto para ver/editar
- **Then** el sistema abre la ventana dedicada `DetalleProyectoWindow` para editar sus datos y gestionar sus actividades.

#### Scenario: Apertura de detalle de actividad en ventana dedicada
- **Given** que el usuario está en la ventana de detalle del proyecto
- **When** hace clic en una actividad o en "+ Nueva Actividad"
- **Then** el sistema abre la ventana dedicada `DetalleActividadWindow` para editar los datos de esa actividad.

