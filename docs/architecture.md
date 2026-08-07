# Arquitectura del sistema

## Visión general

Sistema Docente Local es una aplicación de escritorio WPF para la gestión cotidiana de un grupo de primaria. La solución está organizada en capas y se ensambla mediante composición manual. El dominio y los casos de uso permanecen independientes de WPF y SQLite; los adaptadores concretos se crean en la raíz de composición de la aplicación.

La solución usa .NET 10. Los proyectos productivos portables usan `net10.0`; la aplicación WPF usa `net10.0-windows`.

## Proyectos y responsabilidades

| Proyecto | Responsabilidad actual |
| --- | --- |
| `SistemaDocente.Core` | Modelo de dominio, identidades, invariantes y excepciones. Contiene grupo/estudiantes, asistencia, proyectos didácticos, actividades, entregas/niveles de logro y elementos del expediente pedagógico. |
| `SistemaDocente.Application` | Casos de uso, puertos de persistencia, snapshots y coordinación entre agregados. Gestiona grupo, asistencia, proyectos/actividades, evaluación y expediente. |
| `SistemaDocente.Data` | Adaptadores SQLite, inicialización/migración de esquema, consultas y persistencia transaccional. Traduce errores técnicos en la frontera de infraestructura. |
| `SistemaDocente.Presentation` | MVVM portable: ViewModels, comandos, modelos visuales, confirmaciones, estado editable y fronteras de módulo. Incluye la matriz visual de evaluación, pero no depende de WPF, Data ni SQLite. |
| `SistemaDocente.Reporting` | Frontera reservada para reportes. Sigue separado del resto de la interfaz. |
| `SistemaDocente.App.Wpf` | Shell WPF (`MainWindow`), vistas de módulos, ventanas dedicadas, temas, recursos visuales, servicios WPF, notificaciones, raíz de composición y modo de demostración aislado. |

Los proyectos de pruebas existentes son:

- `SistemaDocente.Core.Tests`;
- `SistemaDocente.Application.Tests`;
- `SistemaDocente.Data.Tests`;
- `SistemaDocente.Presentation.Tests`;
- `SistemaDocente.App.Wpf.Tests`.

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

Referencias productivas:

```text
Application  → Core
Data         → Application + Core
Presentation → Application
Reporting    → Core
App.Wpf      → Presentation + Application + Data
Core         → ningún otro proyecto productivo
```

No existen ciclos. Data se instancia desde `SistemaDocente.App.Wpf`; Presentation no conoce SQLite ni adaptadores concretos.

## Modelo de dominio

### Grupo

`Grupo` es el agregado responsable de su nombre, estudiantes, números de lista, situación activa y consistencia de la matrícula.

### Asistencia

`AsistenciaDiaria` es el agregado de asistencia. Su identidad natural es `GrupoId + DateOnly`. Cada fecha se persiste de manera independiente y atómica.

El mes no es un agregado. Application construye una proyección mensual inmutable; Presentation conserva snapshot confirmado y copia editable. No existe tabla ni transacción mensual.

### Proyectos y actividades

`ProyectoDidactico` es un agregado independiente con identidad, grupo, periodo, estado, descripción, observaciones y versión para concurrencia optimista. Su ciclo de vida contempla Borrador, EnCurso y Finalizado, incluida reapertura explícita.

`ActividadProyecto` es un agregado independiente perteneciente a un proyecto y grupo. Mantiene título, descripción, fecha de realización, observaciones, estado, versión y el padrón histórico completo de entregas/evaluaciones de estudiantes.

La actividad, no el proyecto completo, es la unidad atómica para guardar el padrón asociado. No se persisten las entregas una por una como operaciones independientes.

### Evaluación

La evaluación reutiliza el padrón histórico de `ActividadProyecto`. El dominio usa `NivelLogro` con los valores:

- Pendiente;
- Domina;
- Suficiente;
- EnProceso;
- RequiereApoyo;
- NoEntrego.

La interfaz no crea un nuevo agregado de evaluación. `EvaluacionActividadesViewModel` proyecta una matriz visual:

```text
filas    = estudiantes presentes en al menos un padrón histórico del proyecto
columnas = actividades del proyecto
celda    = entrega/evaluación de ese estudiante en esa actividad
```

Una celda inexistente en el padrón histórico se representa como `—` y no puede editarse. Así, una alta posterior no se agrega retroactivamente a actividades previas y un estudiante inactivo puede conservarse en el historial.

Cada columna recibe un código visual estable con formato `A` + seis caracteres hexadecimales derivados de la identidad inmutable `ActividadId`, por ejemplo `A4F2C91`. El código no depende del orden, fecha ni título de la actividad y por tanto no se renumera cuando otras actividades cambian. Es sólo una referencia de interfaz; `ActividadId` continúa siendo la identidad real y no se requiere una migración SQLite para el código visual.

Guardar la matriz no crea una transacción de proyecto. Presentation detecta las actividades con cambios y llama secuencialmente al caso de uso existente para guardar el padrón completo de cada actividad. Cada actividad conserva su propia atomicidad y concurrencia optimista. Si una actividad posterior falla, las anteriores ya confirmadas permanecen guardadas y el resto conserva la edición local.

### Expediente y seguimiento individual

El expediente del estudiante consolida información procedente de asistencia, actividades/evaluación y registros pedagógicos propios. La persistencia incluye notas pedagógicas y acuerdos con tutores vinculados por estudiante y grupo.

El diseño del módulo prioriza seguimiento formativo y evita tratar alertas pedagógicas como diagnósticos clínicos.

## Persistencia

La persistencia local usa SQLite mediante `Microsoft.Data.Sqlite` directo, sin ORM ni micro-ORM.

El esquema vigente usa:

```text
PRAGMA user_version = 6
```

La base ha evolucionado mediante migraciones incrementales; una actualización no debe reconstruir destructivamente la base ni perder datos existentes.

Principios vigentes:

- `PRAGMA foreign_keys = ON`;
- consultas parametrizadas;
- fechas canónicas cuando se persisten como texto;
- transacciones por operación compuesta;
- asistencia atómica por día;
- actividad + padrón guardados en una sola transacción;
- concurrencia optimista en entidades que exponen versión;
- restricciones relacionales para mantener pertenencia a grupo/proyecto;
- historial pedagógico no debe eliminarse accidentalmente mediante cascadas inapropiadas;
- errores SQLite no deben filtrarse a Presentation ni a la UI.

`app-state.json` conserva estado mínimo de reapertura; los datos de dominio permanecen en SQLite.

### Almacenamiento de demostración

El modo demo nunca comparte archivos con producción:

```text
Producción
%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json

Demostración
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\app-state.json
```

`--demo` abre/crea exclusivamente el almacenamiento de demostración. `--demo-reset` sólo puede borrar archivos cuando `RutasAplicacion.EsDemostracion` es verdadero y vuelve a sembrar datos ficticios. El seeder usa Core/Application y adaptadores existentes; no contiene SQL de negocio dentro de WPF.

## Presentación y composición

Presentation usa MVVM propio y portable. Los ViewModels gestionan selección, edición, filtros, confirmaciones, cambios pendientes y comandos sin conocer WPF.

Asistencia dispone de una frontera explícita de módulo:

```text
ModuloAsistenciaViewModel
├── Diaria  → GestionAsistenciaViewModel
├── Mensual → GestionAsistenciaMensualViewModel
├── MostrarDiaria / MostrarMensual
└── comandos de cambio de vista
```

`AsistenciaView` consume esta frontera en lugar del `MainWindowViewModel` completo. `MainWindowViewModel` coordina la navegación global y expone `ModoDemostracion` únicamente como estado del shell.

`App.xaml.cs` es la raíz de composición. Allí se crean persistencias, casos de uso, ViewModels y servicios WPF. También interpreta `--demo`/`--demo-reset`, selecciona las rutas apropiadas y ejecuta `DemoDataSeeder` antes de construir los ViewModels cuando corresponde.

La aplicación registra errores no controlados en un log local de diagnóstico y evita mostrar trazas técnicas al usuario final.

## Arquitectura de interfaz WPF

### MainWindow como shell

`MainWindow` es únicamente el shell visual: ensambla `MainNavigationHeader`, las vistas de módulos y el feedback global. La navegación incluye:

- Grupo;
- Asistencia;
- Proyectos;
- Evaluación.

Existe una sola navegación global superior. No se duplica mediante sidebar. El encabezado usa una superficie clara, mantiene el acento institucional guinda para marca/selección/acciones primarias e incorpora un badge `DEMO` cuando el almacenamiento es ficticio.

### Vistas principales especializadas

Cada módulo recibe una frontera explícita por binding:

- `GrupoView` → `GestionGrupoViewModel`;
- `AsistenciaView` → `ModuloAsistenciaViewModel`;
- `ProyectosView` → `GestionProyectosViewModel`;
- `EvaluacionView` → `EvaluacionActividadesViewModel`.

`GrupoView` usa jerarquía de página, búsqueda, métricas compactas, tabla virtualizada y una barra de acciones donde `Agregar estudiante` es primaria.

`AsistenciaView` conserva una densidad alta apropiada para captura operativa, con métricas, filtros y grilla mensual congelando número/nombre.

`ProyectosView` funciona como superficie de consulta y entrada; la edición sigue en ventanas dedicadas.

`EvaluacionView` funciona como matriz estudiante × actividad y congela `Núm.` + `Estudiante`. Los encabezados dinámicos muestran el código visual estable y exponen nombre + fecha mediante tooltip/nombre accesible. La columna de la celda actual define la actividad seleccionada para métricas y acciones masivas.

### Ventanas dedicadas

Las tareas complejas siguen en ventanas enfocadas:

- `EditorEstudianteWindow`;
- `DetalleProyectoWindow`;
- `DetalleActividadWindow`;
- `ExpedienteEstudianteWindow`;
- `EditarEvaluacionCeldaWindow` para nivel/observación de una celda sin ensanchar la matriz.

La regla sigue siendo:

> la jerarquía del dominio no se copia automáticamente como jerarquía visual.

Master-detail queda como patrón opcional, no obligatorio.

### Diseño, temas y recursos dinámicos

La interfaz usa `DesignTokens.xaml` y soporta Claro, Oscuro y Alto contraste. Vistas y shell deben consumir recursos semánticos mediante `DynamicResource` o resolver brushes del tema actual cuando las columnas se generan en code-behind.

La aplicación usa `xml:lang="es-MX"`, foco visible, propiedades de automatización y teclado contextual. Los tooltips no son el único medio para comunicar información esencial: los encabezados dinámicos también reciben nombre accesible y el contexto de la actividad seleccionada se muestra sobre la matriz.

Las reglas de interfaz se documentan en `docs/UI-GUIDELINES.md`.

### Ciclo de vida y virtualización

Los `UserControl` que escuchan `PropertyChanged` o eventos estáticos mantienen suscripciones idempotentes y las liberan en `Unloaded`.

Los `DataGrid` operativos controlan su propio scroll y conservan virtualización. No deben envolverse en un `ScrollViewer` exterior que les entregue altura no acotada.

## Flujos principales

### Gestión de grupo y estudiantes

La UI delega al ViewModel; Presentation delega a Application; Application modifica `Grupo` y Data confirma SQLite. La edición compleja de estudiante se realiza en ventana dedicada.

### Asistencia mensual

Application proyecta días lectivos, matrícula activa e historial. Presentation mantiene cambios por fecha. Guardar varias fechas ejecuta operaciones diarias secuenciales y no afirma atomicidad mensual.

P/F/R/J, Enter, Home/End y PageUp/PageDown sólo se procesan cuando el foco pertenece a `GrillaMensual`; los controles de texto quedan excluidos. Ctrl+S es atajo del módulo.

### Proyectos y actividades

La vista principal de Proyectos es lista/punto de entrada. `DetalleProyectoWindow` y `DetalleActividadWindow` concentran edición compleja. La actividad conserva el padrón histórico.

### Evaluación matricial

El usuario selecciona únicamente un proyecto. Presentation carga sus actividades y construye la matriz. Mover la celda seleccionada cambia implícitamente la actividad de contexto.

D/S/E/R/N/P modifican sólo la celda actual y únicamente cuando el foco pertenece a la grilla. Enter/F2 o doble clic abre el editor compacto de nivel/observación. Las acciones masivas afectan sólo la actividad seleccionada y nunca las celdas `—`.

### Expediente

Desde Grupo se abre la ventana de expediente. Application consolida asistencia, entregas/evaluación y registros pedagógicos; Data persiste notas y acuerdos propios del seguimiento.

### Demostración

`--demo` usa un conjunto ficticio suficientemente rico para probar listas largas, historial, asistencia, proyectos, evaluación y expediente. La guía de uso vive en `docs/demo-mode.md`.

## Estrategia de pruebas

- **Core:** invariantes, transiciones y comportamiento de agregados.
- **Application:** casos de uso, snapshots y conflictos.
- **Data:** SQLite temporal real, migraciones, restricciones, rollback y reapertura.
- **Presentation:** ViewModels, matriz, filtros, cambios pendientes, códigos visuales estables y guardado secuencial sin WPF.
- **App.Wpf:** composición, bindings, rutas demo, recursos semánticos, teclado contextual y límites del code-behind.
- **Prueba manual:** apertura real, modo demo, redimensionamiento, foco, teclado, temas, scroll horizontal/vertical y ventanas dedicadas.
- **Auditoría independiente:** arquitectura, persistencia, UX y regresiones antes de cerrar cambios relevantes.

## Decisiones arquitectónicas vigentes

- Core permanece independiente y concentra invariantes del dominio.
- Los puertos viven en Application; Data aporta adaptadores concretos.
- SQLite se usa directamente mediante `Microsoft.Data.Sqlite`.
- La unidad atómica de asistencia es el día.
- `ProyectoDidactico` y `ActividadProyecto` son agregados separados.
- La actividad es la unidad atómica para su padrón de entregas/evaluación.
- La matriz de evaluación es una proyección de Presentation, no un nuevo agregado ni una tabla SQLite.
- Los códigos visuales de actividad son estables porque se derivan de `ActividadId`; la identidad real sigue siendo `ActividadId`.
- Guardar varias columnas modificadas significa operaciones atómicas por actividad ejecutadas secuencialmente.
- Presentation es portable y usa MVVM propio.
- WPF queda aislado en `SistemaDocente.App.Wpf`.
- La composición es manual.
- El modo demo usa almacenamiento separado y no toca producción.
- No se introducen ORM, repositorios genéricos ni framework de navegación sin una decisión explícita.
- Master-detail no es una regla de UI.
- `MainWindow` actúa como shell visual y cada módulo recibe una frontera explícita.
- Las tareas complejas pueden usar ventanas dedicadas y las tareas intensivas usan superficies principales amplias.
- Los estilos y colores nuevos deben integrarse al sistema de diseño compartido.
- Las suscripciones WPF de larga vida deben ser idempotentes y liberarse con el ciclo de vida visual.
