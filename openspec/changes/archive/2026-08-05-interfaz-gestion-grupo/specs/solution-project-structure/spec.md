## MODIFIED Requirements

### Requirement: Estructura de la solución
La solución SHALL contener `SistemaDocente.Core`, `SistemaDocente.Application`, `SistemaDocente.Presentation`, `SistemaDocente.Data`, `SistemaDocente.Reporting`, `SistemaDocente.App.Wpf`, `SistemaDocente.Core.Tests`, `SistemaDocente.Application.Tests`, `SistemaDocente.Presentation.Tests` y `SistemaDocente.Data.Tests` como proyectos separados bajo `src/` y `tests/`.

#### Scenario: Inspección de proyectos
- **WHEN** se inspeccionen la solución y el árbol de proyectos
- **THEN** estarán presentes los proyectos productivos y de pruebas definidos, incluidos Presentation y Presentation.Tests

### Requirement: Responsabilidades de proyectos productivos
Core SHALL contener dominio; Application SHALL contener casos de uso, puertos y snapshots; Presentation SHALL contener ViewModels, comandos MVVM y servicios visuales abstractos; Data SHALL contener SQLite; Reporting SHALL reservarse para reportes aprobados; y App.Wpf SHALL contener XAML, MainWindow, servicios WPF y raíz de composición.

#### Scenario: Ubicar una responsabilidad
- **WHEN** se inspeccione una regla, caso de uso, ViewModel, implementación SQLite o vista WPF
- **THEN** estará respectivamente en Core, Application, Presentation, Data o App.Wpf

### Requirement: Referencias productivas permitidas
Core MUST NOT referenciar capas superiores. Application SHALL referenciar únicamente Core. Presentation SHALL referenciar únicamente Application y MUST NOT referenciar WPF, Data ni SQLite. Data SHALL referenciar Application y Core. App.Wpf SHALL referenciar Presentation, Application y Data. App.Wpf MUST NOT añadir `PackageReference` a `Microsoft.Data.Sqlite`. Reporting SHALL conservar las referencias previamente aprobadas. Ningún proyecto SHALL formar ciclos.

#### Scenario: Verificar el grafo exacto
- **WHEN** se inspeccionen los archivos de proyecto
- **THEN** se observarán Presentation → Application, Presentation.Tests → Presentation y Application, y App.Wpf → Presentation, Application y Data, sin ciclos

### Requirement: Referencias de pruebas de presentación
Presentation.Tests SHALL referenciar únicamente Presentation y Application. Sus pruebas MUST NOT cargar WPF, Data, SQLite ni ventanas reales.

#### Scenario: Ejecutar pruebas de ViewModels
- **WHEN** se ejecuten las pruebas de Presentation
- **THEN** los ViewModels se comprobarán con dobles de servicios abstractos sin infraestructura visual o de datos

### Requirement: Separación de Presentation, WPF y Data
Los ViewModels MUST NOT construir Data, resolver carpetas del sistema ni ejecutar SQL. Data SHALL utilizarse únicamente desde la raíz de composición de App.Wpf. Ventanas, controles y code-behind MUST NOT contener reglas de dominio o coordinación de casos de uso; el code-behind SHALL limitarse a foco y comportamiento puramente visual.

#### Scenario: Ejecutar una acción docente
- **WHEN** un control inicia una acción
- **THEN** el enlace invoca el comando del ViewModel, éste delega en Application y sólo la raíz de composición conoce Data
