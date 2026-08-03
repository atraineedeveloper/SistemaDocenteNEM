## Context

El repositorio todavía no tiene la fundación de la solución. El producto será una aplicación local para un docente de primaria: la interfaz se ejecutará en Windows mediante WPF, mientras que una parte sustancial del desarrollo y las pruebas también ocurrirá en Fedora. La fundación debe separar el comportamiento pedagógico de la tecnología de interfaz y permitir validar las capas no visuales sin Windows.

Las tecnologías acordadas son C# y .NET 10, WPF con `net10.0-windows`, SQLite, xUnit, Git y OpenSpec. Este cambio documenta la estructura y su futura creación, pero no implementa código.

## Goals / Non-Goals

**Goals:**

- Definir seis proyectos bajo `src/` y `tests/`, con responsabilidades y dependencias unidireccionales.
- Mantener Core independiente de WPF, SQLite y detalles de presentación.
- Hacer compilable el proyecto WPF desde Fedora y dejar su ejecución/validación visual para Windows.
- Permitir pruebas de Core y Data en Fedora.
- Fijar configuración coherente de compilación y criterios de aceptación reproducibles.

**Non-Goals:**

- Crear entidades de dominio, casos de uso docentes o reglas pedagógicas.
- Diseñar o crear tablas, migraciones, repositorios concretos o archivos SQLite.
- Implementar reportes, ventanas, controles, navegación o composición definitiva de dependencias.
- Validar visualmente WPF fuera de Windows.

## Decisions

### Estructura y responsabilidades

- `SistemaDocente.Core` contendrá en el futuro el modelo y las reglas pedagógicas, contratos y casos de uso independientes de infraestructura y UI. No referenciará otros proyectos de `src/`.
- `SistemaDocente.Data` contendrá en el futuro la persistencia local SQLite y las implementaciones de contratos de persistencia definidos por Core. Referenciará únicamente `SistemaDocente.Core` entre los proyectos productivos.
- `SistemaDocente.Reporting` contendrá en el futuro la preparación/generación de reportes y podrá consumir el modelo y contratos de Core. Referenciará únicamente `SistemaDocente.Core` entre los proyectos productivos; cualquier necesidad futura de acceso a datos deberá entrar por contratos de Core, no mediante referencia directa a Data.
- `SistemaDocente.App.Wpf` será la capa de presentación y el punto de composición. Podrá referenciar Core, Data y Reporting para ensamblar la aplicación, pero ninguna de esas capas podrá referenciar WPF.
- `SistemaDocente.Core.Tests` referenciará Core.
- `SistemaDocente.Data.Tests` referenciará Data y podrá referenciar Core para preparar y verificar contratos/modelos compartidos.

Esta dirección evita ciclos y preserva un núcleo portable. Se descarta permitir referencias libres entre proyectos porque haría difícil comprobar los límites y trasladaría decisiones de UI o persistencia al dominio.

En esta etapa, el grafo de referencias se comprobará mediante una inspección documentada de los archivos `.csproj`. No se incorporará todavía una biblioteca ni una prueba automatizada de arquitectura: esa automatización se pospone hasta una etapa funcional posterior, cuando existan componentes cuya evolución justifique mantenerla.

### Lógica fuera de WPF

Ventanas, controles, recursos y code-behind se limitarán a presentación, enlace de datos y delegación de acciones. No contendrán reglas pedagógicas, cálculos de negocio, validaciones de dominio ni acceso SQLite. Esa lógica deberá residir en Core y ser invocada mediante objetos/casos de uso independientes de WPF. Las pruebas de reglas se ubicarán en `Core.Tests`, sin construir ventanas.

No se elige todavía un toolkit ni una implementación concreta de MVVM. La separación se expresa como una restricción observable de dependencias y ubicación de lógica, suficiente para esta fundación.

### Destinos y compilación multiplataforma

Core, Data, Reporting y sus pruebas usarán `net10.0`. WPF usará `net10.0-windows`, habilitará WPF y declarará `EnableWindowsTargeting=true`, lo que permite restaurar y compilar el destino Windows desde Fedora cuando estén disponibles el SDK y los paquetes necesarios. Esto no convierte WPF en ejecutable ni visualmente verificable en Fedora.

Se descarta multidestinar Core o Data a Windows porque no aporta valor a la fundación y reduciría la portabilidad de sus pruebas.

### Configuración común y advertencias

La solución reutilizará el `global.json` existente y fijará el SDK en `10.0.110` con `rollForward` igual a `latestPatch`. No se creará un segundo archivo de selección de SDK.

La configuración común de MSBuild se centralizará en `Directory.Build.props` con `AnalysisLevel=latest-recommended`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `Nullable=enable` e `ImplicitUsings=enable`. No se añadirán analizadores externos en esta etapa. Cualquier excepción de advertencia deberá ser puntual, estar justificada y localizarse en el elemento o proyecto afectado; no se usará `NoWarn` de forma global.

Se descarta una política permisiva de advertencias porque permitiría degradación silenciosa de la fundación. También se descarta elegir analizadores adicionales antes de disponer de código funcional que permita valorar su coste y utilidad.

### Estrategia de validación

La aceptación exigirá ejecutar `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` sobre la solución. En Fedora, restore/build incluirán el proyecto WPF mediante Windows targeting, mientras que las pruebas ejecutables serán `Core.Tests` y `Data.Tests`. Las verificaciones correspondientes a Windows, junto con la ejecución y validación visual de WPF, no pueden completarse desde Fedora y permanecerán pendientes hasta ejecutarse en Windows. En esta etapa no se exige una prueba UI automatizada.

## Risks / Trade-offs

- [La compilación cruzada de WPF puede depender de paquetes de targeting accesibles durante restore] → Fijar el SDK .NET 10 y verificar restore desde Fedora; tratar los problemas de entorno por separado de la ejecución, que sigue siendo exclusiva de Windows.
- [La referencia de App.Wpf a Data y Reporting permite que la UI alcance detalles concretos] → Restringir su uso al punto de composición y hacer que ventanas/controles dependan de abstracciones o servicios de Core.
- [La inspección manual de `.csproj` puede dejar de escalar cuando crezca la solución] → Documentar el resultado ahora y posponer una comprobación automatizada hasta una etapa funcional posterior.
- [`TreatWarningsAsErrors=true` puede introducir fricción con código generado o advertencias excepcionales] → Permitir únicamente excepciones puntuales, justificadas y localizadas, sin `NoWarn` global.
- [Data.Tests puede variar por comportamiento nativo de SQLite] → Definir en un cambio posterior la estrategia de pruebas SQLite; esta propuesta solo exige que sean ejecutables en Fedora.

## Migration Plan

No hay aplicación ni datos existentes que migrar. La implementación futura creará primero la solución y los proyectos vacíos, aplicará referencias/configuración y luego validará los comandos de aceptación en Fedora y Windows. Si la fundación no valida, se revertirán únicamente los archivos estructurales creados; no habrá datos de usuario afectados.

## Open Questions

- ¿Qué proveedor y estrategia SQLite se elegirán cuando se diseñe la persistencia?
- ¿El acceso a datos futuro usará Entity Framework, Dapper o acceso directo?
- ¿Qué toolkit o implementación de MVVM se adoptará cuando se implemente funcionalidad visual?
- ¿Qué analizadores externos adicionales, si alguno, se justificarán en una etapa funcional posterior?
