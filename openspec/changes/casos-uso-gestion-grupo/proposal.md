## Why

Core y Data ya resuelven las reglas del agregado y su persistencia, pero todavía no existe una capa que coordine ambas capacidades para los flujos que consumirá la interfaz. Incorporar una capa de aplicación evita que WPF contenga reglas, controle persistencia o dependa de SQLite concreto.

## What Changes

- Crear los proyectos `SistemaDocente.Application` y `SistemaDocente.Application.Tests`.
- Incorporar una fachada de casos de uso para crear, cargar, comprobar existencia y administrar un grupo y sus estudiantes.
- Definir en Application el puerto específico `IAlmacenamientoGrupos`, que Data implementará mediante el adaptador SQLite existente.
- Hacer que cada comando sobre un grupo existente cargue una instancia fresca, invoque una operación pública de Core y guarde exactamente una vez después del éxito, incluso si Core acepta la operación como idempotente.
- Exponer exclusivamente snapshots inmutables `GrupoDetalle` y `EstudianteDetalle`; nunca agregados, entidades ni colecciones internas.
- Establecer una sola frontera de traducción de errores: Data convertirá sus fallos en `ErrorPersistenciaAplicacionException`, preservará la causa técnica y Application no volverá a envolverla.
- Distinguir la ausencia mediante `GrupoNoEncontradoException`; `Existe` devolverá `false` únicamente cuando el grupo realmente no exista.
- Probar la orquestación con dobles del puerto y el adaptador Application–Data con SQLite real.
- Precisar el grafo de dependencias y la futura composición de App.Wpf sin implementar UI.

## Capabilities

### New Capabilities

- `casos-uso-gestion-grupo`: Define contratos, resultados, comandos y consultas para administrar grupos y estudiantes con persistencia automática, resultados inmutables e independencia de WPF y SQLite concreto.

### Modified Capabilities

- `solution-project-structure`: Añade Application y Application.Tests y fija el grafo permitido: Application depende sólo de Core; Data depende de Application y Core; la lógica visual futura depende de Application y Data queda limitada a la raíz de composición.

## Impact

- Nuevos proyectos: `src/SistemaDocente.Application` y `tests/SistemaDocente.Application.Tests`.
- Referencias: Application → Core; Data → Application y Core; Application.Tests → Application y Core; Data.Tests puede referenciar Data, Application y Core.
- El adaptador SQLite de Data implementará `IAlmacenamientoGrupos` y será la única frontera que traduzca errores propios de Data a errores de persistencia de Application.
- App.Wpf no se modificará funcionalmente. En una integración futura, ventanas, controles y ViewModels dependerán de Application; Data sólo se usará en la raíz de composición y App.Wpf no referenciará `Microsoft.Data.Sqlite`.
- Reporting podrá componerse posteriormente según la capacidad que se diseñe, sin añadir reportes funcionales en este cambio.
- No se modifican las reglas de Core ni el esquema SQLite.
- Permanecen fuera de alcance WPF funcional, ViewModels, navegación, contenedor DI, API asíncrona, `CancellationToken`, caché, concurrencia, asistencia, actividades, evaluación, reportes funcionales e importación.
