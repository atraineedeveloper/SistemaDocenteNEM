## ADDED Requirements

### Requirement: Marcos de destino portables
Core, Data, Reporting, Core.Tests y Data.Tests SHALL usar `net10.0` sin un marco de destino específico de Windows.

#### Scenario: Inspección de marcos no visuales
- **WHEN** se inspeccionen los archivos de proyecto de las capas y pruebas no visuales
- **THEN** todos tendrán `net10.0` como destino y no dependerán de WPF

### Requirement: Configuración de WPF para targeting cruzado
App.Wpf SHALL usar `net10.0-windows`, habilitar WPF y establecer `EnableWindowsTargeting` en `true` para permitir restauración y compilación desde Fedora.

#### Scenario: Compilación cruzada desde Fedora
- **WHEN** un entorno Fedora con el SDK .NET 10 restaure y compile la solución
- **THEN** App.Wpf se restaurará y compilará como destino Windows sin requerir que la aplicación sea ejecutable en Fedora

### Requirement: Configuración común del compilador
Todos los proyectos SHALL aplicar `AnalysisLevel=latest-recommended`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `Nullable=enable` e `ImplicitUsings=enable` mediante configuración común. Las excepciones de advertencias MUST ser puntuales, justificadas y localizadas, y la solución MUST NOT usar `NoWarn` global ni añadir analizadores externos en esta etapa.

#### Scenario: Inspección de configuración
- **WHEN** se revisen la configuración común y las excepciones de cada proyecto
- **THEN** las cinco propiedades acordadas estarán activas y no existirán `NoWarn` globales ni analizadores externos adicionales

### Requirement: Selección reproducible del SDK
La fundación SHALL reutilizar el `global.json` existente con la versión de SDK `10.0.110` y `rollForward` establecido en `latestPatch`.

#### Scenario: Resolución del SDK
- **WHEN** la CLI de .NET resuelva el SDK desde la raíz del repositorio
- **THEN** usará la política del `global.json` existente para seleccionar `10.0.110` o el parche posterior permitido por `latestPatch`

### Requirement: Validación automatizada de la solución
La fundación SHALL considerarse técnicamente aceptada solo cuando `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` finalicen correctamente sobre la solución en los entornos aplicables.

#### Scenario: Validación en Fedora
- **WHEN** se ejecuten `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` en Fedora
- **THEN** restore y build incluirán la solución completa, las pruebas de Core y Data se ejecutarán sin WPF y la verificación de formato no detectará cambios

#### Scenario: Validación en Windows
- **WHEN** se ejecuten `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` en Windows
- **THEN** los cuatro comandos finalizarán correctamente para la solución

#### Scenario: Validación Windows pendiente desde Fedora
- **WHEN** la implementación se esté verificando únicamente desde Fedora
- **THEN** las tareas de validación técnica en Windows permanecerán pendientes hasta ejecutarse en un entorno Windows

### Requirement: Validación visual reservada para Windows
La ejecución de App.Wpf y su validación visual SHALL realizarse en Windows y MUST NOT ser un criterio de ejecución en Fedora.

#### Scenario: Revisión visual de WPF
- **WHEN** se deba comprobar el arranque, representación o comportamiento visual de App.Wpf
- **THEN** la comprobación se realizará en un entorno Windows

#### Scenario: Revisión visual pendiente desde Fedora
- **WHEN** no se disponga de un entorno Windows durante la implementación
- **THEN** la ejecución y validación visual de App.Wpf permanecerán pendientes y no se marcarán como completadas desde Fedora
