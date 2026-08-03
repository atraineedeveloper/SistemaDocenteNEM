# Arquitectura fundacional

## Responsabilidades

- `SistemaDocente.Core`: futuro modelo, contratos, casos de uso y reglas pedagógicas independientes de infraestructura e interfaz.
- `SistemaDocente.Data`: futura persistencia local; implementará contratos definidos por Core. En esta fundación no contiene proveedor ni acceso SQLite.
- `SistemaDocente.Reporting`: futura preparación y generación de reportes a partir de contratos de Core. En esta fundación no genera reportes.
- `SistemaDocente.App.Wpf`: presentación WPF y futuro punto de composición. Sus ventanas, controles y code-behind se limitan a presentación, enlace de datos y delegación.
- `SistemaDocente.Core.Tests`: pruebas portables de Core.
- `SistemaDocente.Data.Tests`: pruebas portables de Data y de su integración con contratos de Core.

La lógica pedagógica, los cálculos de negocio, las validaciones de dominio y el acceso a datos no pertenecen a ventanas, controles ni code-behind de WPF.

## Referencias permitidas

```text
SistemaDocente.Core <- SistemaDocente.Data
SistemaDocente.Core <- SistemaDocente.Reporting
SistemaDocente.Core <- SistemaDocente.App.Wpf
SistemaDocente.Data <- SistemaDocente.App.Wpf
SistemaDocente.Reporting <- SistemaDocente.App.Wpf

SistemaDocente.Core <- SistemaDocente.Core.Tests
SistemaDocente.Core <- SistemaDocente.Data.Tests
SistemaDocente.Data <- SistemaDocente.Data.Tests
```

No se permiten otras referencias entre los proyectos fundacionales ni ciclos. Ningún proyecto portable referencia WPF.

## Inspección de archivos de proyecto

La inspección documentada de los seis archivos `.csproj` confirma:

| Proyecto | `ProjectReference` permitidos |
| --- | --- |
| Core | ninguno |
| Data | Core |
| Reporting | Core |
| App.Wpf | Core, Data, Reporting |
| Core.Tests | Core |
| Data.Tests | Core, Data |

La automatización de estas reglas arquitectónicas se pospone hasta una etapa funcional posterior.

## Plataformas de validación

Core, Data, Reporting y sus pruebas usan `net10.0` y se ejecutan en Fedora. App.Wpf usa `net10.0-windows` con targeting cruzado para restauración y compilación desde Fedora. La ejecución de WPF, la validación visual y la validación técnica completa en Windows permanecen pendientes hasta disponer de Windows.

## Decisiones pospuestas

Esta fundación no elige proveedor ni estrategia SQLite; Entity Framework, Dapper o acceso directo; toolkit o implementación MVVM; ni analizadores externos adicionales.
