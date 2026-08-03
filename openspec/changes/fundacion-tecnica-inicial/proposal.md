## Why

El Sistema Docente Local necesita una base técnica explícita antes de incorporar comportamiento pedagógico, persistencia o interfaz, de modo que el desarrollo multiplataforma conserve una arquitectura verificable y no acople la lógica del sistema a WPF.

## What Changes

- Definir una solución .NET 10 con proyectos separados para dominio, persistencia SQLite, generación de reportes e interfaz WPF, además de proyectos de pruebas para Core y Data.
- Establecer las responsabilidades y referencias permitidas de cada proyecto, manteniendo la lógica pedagógica fuera de ventanas, controles y code-behind de WPF.
- Estandarizar configuración de compilación: tipos de referencia anulables, usings implícitos, tratamiento/análisis de advertencias y destino `net10.0-windows` con `EnableWindowsTargeting` para WPF.
- Establecer una matriz de validación donde restauración y compilación puedan ejecutarse desde Fedora, Core y Data puedan probarse allí, y la ejecución y validación visual de WPF se realicen en Windows.
- Definir `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` como criterios de aceptación de la futura implementación.
- Limitar esta fundación a estructura y configuración: no se crearán todavía entidades, tablas SQLite ni funciones docentes.

## Capabilities

### New Capabilities

- `solution-project-structure`: Define los proyectos de la solución, sus responsabilidades, referencias permitidas y límites arquitectónicos respecto de WPF.
- `cross-platform-build-validation`: Define la configuración común de .NET, la compilación cruzada de WPF y las validaciones que corresponden a Fedora y Windows.

### Modified Capabilities

Ninguna.

## Impact

- Afectará la futura solución .NET, los archivos de proyecto bajo `src/` y `tests/`, y posiblemente configuración común de MSBuild a nivel de repositorio.
- Introducirá dependencias de plataforma y herramientas sobre .NET 10, WPF, SQLite y xUnit, sin incorporar aún modelo de dominio ni esquema de datos.
- Establecerá restricciones que deberán respetar las posteriores funcionalidades docentes y sus pruebas.
