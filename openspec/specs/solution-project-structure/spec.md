# solution-project-structure Specification

## Purpose
TBD - created by archiving change fundacion-tecnica-inicial. Update Purpose after archive.
## Requirements
### Requirement: Estructura de la solución
La solución SHALL contener `src/SistemaDocente.Core`, `src/SistemaDocente.Data`, `src/SistemaDocente.Reporting`, `src/SistemaDocente.App.Wpf`, `tests/SistemaDocente.Core.Tests` y `tests/SistemaDocente.Data.Tests` como proyectos separados.

#### Scenario: Inspección de proyectos
- **WHEN** se inspeccionen la solución y el árbol de proyectos
- **THEN** estarán presentes exactamente los proyectos fundacionales previstos bajo `src/` y `tests/`

### Requirement: Responsabilidades de proyectos productivos
La solución SHALL asignar a Core el comportamiento pedagógico y los contratos independientes de infraestructura; a Data la persistencia SQLite; a Reporting la generación de reportes; y a App.Wpf la presentación y composición de la aplicación.

#### Scenario: Ubicación de una responsabilidad
- **WHEN** se agregue posteriormente una pieza de comportamiento, persistencia, reporte o presentación
- **THEN** su proyecto de destino se determinará por las responsabilidades definidas y no por conveniencia de acceso desde WPF

### Requirement: Referencias productivas permitidas
Core MUST NOT referenciar Data, Reporting ni App.Wpf; Data SHALL referenciar como máximo Core; Reporting SHALL referenciar como máximo Core; y App.Wpf MAY referenciar Core, Data y Reporting. Ningún proyecto productivo SHALL formar ciclos de referencias.

#### Scenario: Verificación del grafo de referencias
- **WHEN** se inspeccionen las referencias entre proyectos productivos
- **THEN** todas seguirán las direcciones permitidas y el grafo será acíclico

### Requirement: Comprobación documentada de referencias
Durante esta etapa fundacional, las referencias entre proyectos SHALL comprobarse mediante inspección documentada de los archivos `.csproj`; la solución MUST NOT incorporar todavía una prueba o herramienta automatizada de reglas arquitectónicas.

#### Scenario: Revisión fundacional de referencias
- **WHEN** se complete la configuración de referencias de los seis proyectos
- **THEN** se registrará la inspección de cada `.csproj` y la automatización arquitectónica permanecerá pospuesta para una etapa funcional posterior

### Requirement: Referencias de proyectos de prueba
Core.Tests SHALL referenciar Core y MUST NOT requerir App.Wpf; Data.Tests SHALL referenciar Data y MAY referenciar Core, pero MUST NOT requerir App.Wpf.

#### Scenario: Independencia de pruebas no visuales
- **WHEN** se restauren y ejecuten Core.Tests y Data.Tests en Fedora
- **THEN** ninguna prueba necesitará cargar ensamblados, ventanas o controles WPF

### Requirement: Separación de lógica pedagógica y WPF
Las ventanas, controles y code-behind de App.Wpf MUST NOT contener reglas pedagógicas, cálculos de negocio, validaciones de dominio ni acceso directo a SQLite; SHALL limitarse a presentación, enlace de datos y delegación hacia comportamiento independiente de WPF alojado en Core.

#### Scenario: Incorporación de una regla pedagógica
- **WHEN** una funcionalidad futura requiera una regla pedagógica invocada desde una ventana
- **THEN** la regla se implementará en Core, se probará sin WPF y la ventana únicamente delegará su ejecución

### Requirement: Alcance exclusivamente fundacional
La implementación de esta propuesta MUST NOT crear entidades docentes, tablas o migraciones SQLite, funciones pedagógicas, reportes funcionales ni comportamiento de ventanas.

#### Scenario: Revisión del alcance implementado
- **WHEN** se revise la implementación de la fundación
- **THEN** solo existirán estructura, referencias, configuración y pruebas fundacionales sin modelo ni funcionalidad docente

