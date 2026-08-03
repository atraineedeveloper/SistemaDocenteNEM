# Arquitectura del sistema

## Visión general

Sistema Docente Local es una aplicación de escritorio WPF organizada en capas y ensamblada mediante composición manual. El dominio y los casos de uso permanecen independientes de WPF y SQLite; los adaptadores concretos se crean únicamente en la raíz de composición de la aplicación.

La solución usa .NET 10. Los proyectos productivos portables usan `net10.0`; la aplicación WPF usa `net10.0-windows`.

## Proyectos y responsabilidades

| Proyecto | Responsabilidad y elementos principales |
| --- | --- |
| `SistemaDocente.Core` | Modelo de dominio, identidades, invariantes y excepciones de dominio. Contiene los agregados `Grupo` y `AsistenciaDiaria`, estudiantes, registros y estados de asistencia. No conoce casos de uso, persistencia ni interfaz. |
| `SistemaDocente.Application` | Casos de uso y puertos de persistencia. Coordina gestión de grupo y estudiantes, asistencia diaria y consulta/guardado mensual. Define snapshots y entradas inmutables, calendario lectivo y errores de aplicación. |
| `SistemaDocente.Data` | Adaptadores SQLite de los puertos de Application, inicialización y migración del esquema, carga y persistencia de grupos y asistencias. Traduce errores técnicos en la frontera de infraestructura. |
| `SistemaDocente.Presentation` | MVVM portable: ViewModels, comandos, modelos visuales y adaptadores de presentación. Gestiona estado editable, selección, filtros, confirmaciones y notificaciones sin depender de WPF, Data ni SQLite. |
| `SistemaDocente.Reporting` | Frontera reservada para preparación y generación de reportes a partir del dominio. Actualmente depende sólo de Core y no contiene la funcionalidad de reportes pendiente. |
| `SistemaDocente.App.Wpf` | Interfaz WPF y raíz de composición. Construye adaptadores, casos de uso, servicios y ViewModels; aloja `MainWindow`, servicios WPF y el almacenamiento mínimo del estado de la aplicación. El code-behind se limita al comportamiento visual propio de WPF. |

Los proyectos de pruebas existentes son:

- `SistemaDocente.Core.Tests`;
- `SistemaDocente.Application.Tests`;
- `SistemaDocente.Data.Tests`;
- `SistemaDocente.Presentation.Tests`;
- `SistemaDocente.App.Wpf.Tests`.

Reporting todavía no tiene un proyecto de pruebas propio porque su funcionalidad productiva sigue pendiente.

## Grafo de dependencias

```text
SistemaDocente.Core
    ↑          ↑             ↑
Application   Data       Reporting
    ↑          ↑
Presentation  │
    ↑          │
    └──── App.Wpf ────────┘
```

Referencias productivas exactas:

```text
Application  → Core
Data         → Application + Core
Presentation → Application
Reporting    → Core
App.Wpf      → Presentation + Application + Data
Core         → ningún otro proyecto productivo
```

No existen ciclos. Data sólo es consumido por `SistemaDocente.App.Wpf`, en la raíz de composición; ningún ViewModel ni proyecto portable instancia o referencia adaptadores SQLite.

## Modelo de dominio y agregados

`Grupo` es el agregado responsable de su nombre, estudiantes, números de lista, situación activa y reglas de consistencia de la matrícula.

`AsistenciaDiaria` es el agregado de asistencia. Su identidad natural es la pareja `GrupoId + DateOnly`; contiene como máximo un registro por estudiante y garantiza un único estado válido por registro. Cada fecha es una unidad de dominio y persistencia independiente.

El mes no es un agregado. Application construye una proyección mensual inmutable a partir de la matrícula y los agregados diarios; Presentation conserva su snapshot confirmado y una copia editable. No existe identidad mensual, transacción mensual ni tabla mensual.

## Persistencia

La persistencia local usa SQLite mediante acceso directo con `Microsoft.Data.Sqlite`, sin ORM ni micro-ORM. El esquema vigente usa `PRAGMA user_version = 2`.

- Una base nueva se crea directamente en versión 2.
- La migración de v1 a v2 valida la estructura anterior y se ejecuta dentro de una transacción; `user_version` cambia sólo al completar correctamente todos los objetos.
- Cada agregado se persiste transaccionalmente.
- La asistencia es atómica por día: encabezado y registros de una `AsistenciaDiaria` se confirman o revierten juntos.
- Guardar varios días ejecuta transacciones diarias sucesivas; no existe atomicidad mensual.
- `app-state.json` conserva únicamente el `GrupoId` necesario para reabrir el grupo actual. Los datos del dominio permanecen en SQLite.

## Presentación y composición

Presentation usa una implementación MVVM básica propia: `ViewModelBase`, `RelayCommand`, ViewModels y servicios abstractos comprobables. No usa un toolkit MVVM ni contiene referencias a WPF, Data o SQLite.

`SistemaDocente.App.Wpf` realiza composición manual en el inicio de la aplicación. Allí se crean las persistencias SQLite, casos de uso, adaptadores de presentación, servicios WPF y `MainWindowViewModel`.

`MainWindow` integra la gestión de grupo y la asistencia. La asistencia mensual es la vista principal y la vista diaria se conserva como alternativa. La grilla mensual genera únicamente columnas lectivas de lunes a viernes; los viernes con otra fecha lectiva posterior muestran un separador semanal visual, sin crear columnas artificiales.

## Flujos principales

### Gestión de grupo y estudiantes

La interfaz envía comandos al ViewModel; Presentation delega en los casos de uso de Application. Application carga o modifica el agregado `Grupo` mediante sus invariantes y lo persiste a través de `IAlmacenamientoGrupos`. Data ejecuta la transacción SQLite y devuelve el estado confirmado para actualizar la vista.

### Preparación, edición y guardado de asistencia

Application prepara una fecha desde el grupo actual y, si existe, desde su `AsistenciaDiaria` histórica. Un día nuevo presenta a los estudiantes activos en estado Presente sin guardado implícito. Presentation mantiene snapshot confirmado y copia editable. Al guardar, Application valida la entrada completa y Data persiste una única `AsistenciaDiaria` atómicamente.

### Consulta mensual

Application calcula el intervalo real del mes, carga matrícula y asistencias mediante una consulta de rango, proyecta sólo fechas lectivas y reúne estudiantes activos con padrones históricos. Devuelve snapshots inmutables; Presentation añade selección, edición, filtros, conteos y estado de cambios pendientes.

### Guardado de día

Guardar día persiste únicamente la columna activa. Está disponible para un día nuevo o modificado. Tras el éxito, Presentation reemplaza inmediatamente el snapshot confirmado de esa fecha, marca la columna como persistida y conserva los borradores de otras fechas.

### Guardado de varios días

Application ordena las fechas modificadas y las guarda secuencialmente mediante transacciones diarias independientes. Cada éxito previo permanece confirmado si una fecha posterior falla; se detiene el proceso y se informa la fecha fallida y el progreso alcanzado. Este flujo no ofrece ni afirma atomicidad mensual.

## Estrategia de pruebas

- **Dominio:** pruebas unitarias de creación, rehidratación, identidades, invariantes, atomicidad de mutaciones y colecciones de sólo lectura.
- **Application:** pruebas de casos de uso con dobles manuales de los puertos, incluidas proyecciones mensuales, conjuntos históricos, guardados diarios y fallos intermedios.
- **Data:** pruebas de contrato e integración contra archivos SQLite temporales reales, incluida inicialización, migración, restricciones, rollback, reapertura y consulta por intervalo.
- **Presentation:** pruebas de ViewModels y comandos sin abrir WPF ni cargar Data o SQLite.
- **Composición WPF:** pruebas no interactivas de ensamblado, servicios, navegación y límites del code-behind.
- **Auditoría independiente:** comprobaciones de referencias entre ensamblados, ausencia de dependencias prohibidas y validaciones de arquitectura, formato, compilación, pruebas y especificaciones.

## Estado funcional

Está completada la fundación técnica, el dominio y persistencia de grupo/estudiantes, SQLite v2 y su migración, los casos de uso, la interfaz de gestión de grupo y la asistencia diaria y mensual.

Permanecen como líneas generales pendientes:

- actividades;
- evaluación;
- reportes;
- respaldos;
- importación.

## Decisiones arquitectónicas vigentes

- Core permanece independiente y concentra invariantes del dominio.
- Los puertos viven en Application y Data aporta adaptadores concretos.
- SQLite se usa directamente mediante `Microsoft.Data.Sqlite`.
- `Grupo` y `AsistenciaDiaria` son agregados separados; el mes es sólo una proyección.
- La unidad atómica de asistencia es el día; un guardado mensual es secuencial y puede quedar parcialmente completado.
- Presentation es portable y usa MVVM básico propio; WPF queda aislado en `SistemaDocente.App.Wpf`.
- La composición es manual y Data sólo se instancia en la raíz de composición.
- No se introducen repositorios genéricos, contenedor de inyección de dependencias, ORM ni framework de navegación.
