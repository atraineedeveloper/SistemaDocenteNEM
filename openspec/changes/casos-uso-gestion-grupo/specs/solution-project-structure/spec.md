## MODIFIED Requirements

### Requirement: Estructura de la solución
La solución SHALL contener `src/SistemaDocente.Core`, `src/SistemaDocente.Application`, `src/SistemaDocente.Data`, `src/SistemaDocente.Reporting`, `src/SistemaDocente.App.Wpf`, `tests/SistemaDocente.Core.Tests`, `tests/SistemaDocente.Application.Tests` y `tests/SistemaDocente.Data.Tests` como proyectos separados.

#### Scenario: Inspección de proyectos
- **WHEN** se inspeccionen la solución y el árbol de proyectos
- **THEN** estarán presentes los proyectos de dominio, aplicación, persistencia, reportes, presentación y sus pruebas previstas bajo `src/` y `tests/`

### Requirement: Responsabilidades de proyectos productivos
La solución SHALL asignar a Core las reglas e invariantes del dominio; a Application la coordinación de casos de uso, los puertos y los snapshots; a Data la implementación SQLite de persistencia; a Reporting las capacidades de reportes que se aprueben posteriormente; y a App.Wpf la presentación y su futura raíz de composición.

#### Scenario: Ubicación de una responsabilidad
- **WHEN** se agregue una regla de dominio, una orquestación, una implementación de persistencia o una presentación
- **THEN** se ubicará respectivamente en Core, Application, Data o App.Wpf y no por conveniencia de acceso desde WPF

### Requirement: Referencias productivas permitidas
Core MUST NOT referenciar Application, Data, Reporting ni App.Wpf. Application SHALL referenciar únicamente Core. Data SHALL referenciar únicamente Application y Core. Reporting SHALL referenciar como máximo Core hasta que una capacidad posterior defina su composición. La lógica visual de App.Wpf SHALL depender de Application; Data MAY usarse únicamente desde la raíz de composición de App.Wpf. Ventanas, controles y ViewModels MUST NOT usar clases concretas de Data, y App.Wpf MUST NOT referenciar `Microsoft.Data.Sqlite`. Ningún proyecto productivo SHALL formar ciclos.

#### Scenario: Verificación del grafo de referencias
- **WHEN** se inspeccionen las referencias entre proyectos productivos
- **THEN** se observarán `Application → Core` y `Data → Application y Core`, sin referencias de Application a Data y sin ciclos

#### Scenario: Limitar Data a la composición futura
- **WHEN** App.Wpf integre posteriormente los casos de uso
- **THEN** su lógica visual dependerá de Application y sólo la raíz de composición conocerá la implementación concreta de Data

#### Scenario: Componer Reporting posteriormente
- **WHEN** se proponga una capacidad de reportes
- **THEN** sus dependencias se decidirán de acuerdo con esa capacidad sin introducir reportes funcionales en este cambio

### Requirement: Referencias de proyectos de prueba
Core.Tests SHALL referenciar Core. Application.Tests SHALL referenciar Application y Core y MUST NOT referenciar Data, SQLite ni App.Wpf. Data.Tests MAY referenciar Data, Application y Core y MUST NOT requerir App.Wpf.

#### Scenario: Independencia de pruebas de aplicación
- **WHEN** se ejecuten Application.Tests con dobles de `IAlmacenamientoGrupos`
- **THEN** no cargarán Data, SQLite, ventanas ni controles WPF

#### Scenario: Integración de persistencia
- **WHEN** se ejecuten Data.Tests
- **THEN** podrán verificar el adaptador del puerto con SQLite real usando referencias a Data, Application y Core

## ADDED Requirements

### Requirement: Presentación desacoplada de implementaciones Data
Las ventanas, controles y ViewModels futuros SHALL consumir casos de uso y snapshots de Application y MUST NOT usar `PersistenciaGrupoSqlite`, otras clases concretas de Data, `Microsoft.Data.Sqlite` ni SQL. La selección del adaptador Data SHALL limitarse a una futura raíz de composición.

#### Scenario: Consumir gestión de grupo desde una presentación futura
- **WHEN** una ventana, control o ViewModel solicite administrar grupos o estudiantes
- **THEN** invocará Application sin ejecutar SQL ni referenciar clases concretas de Data
