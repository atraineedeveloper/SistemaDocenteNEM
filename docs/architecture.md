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
| `SistemaDocente.Presentation` | MVVM portable: ViewModels, comandos, modelos visuales, confirmaciones y estado editable. No depende de WPF, Data ni SQLite. |
| `SistemaDocente.Reporting` | Frontera reservada para reportes. Sigue separado del resto de la interfaz. |
| `SistemaDocente.App.Wpf` | Shell WPF, ventanas dedicadas, temas, recursos visuales, servicios WPF, notificaciones y raíz de composición. |

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

La evaluación de actividades reutiliza el padrón histórico de `ActividadProyecto`. El dominio usa `NivelLogro` con los valores vigentes:

- Pendiente;
- Domina;
- Suficiente;
- EnProceso;
- RequiereApoyo;
- NoEntrego.

La interfaz de Evaluación está separada de la edición de proyectos para evitar mezclar planeación, mantenimiento de actividades y evaluación síncrona de alumnos en una sola pantalla.

### Expediente y seguimiento individual

El expediente del estudiante consolida información procedente de asistencia, actividades/evaluación y registros pedagógicos propios. La persistencia actual incluye notas pedagógicas y acuerdos con tutores vinculados por estudiante y grupo.

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

## Presentación y composición

Presentation usa MVVM propio y portable. Los ViewModels gestionan selección, edición, filtros, confirmaciones, cambios pendientes y comandos sin conocer WPF.

`App.xaml.cs` es la raíz de composición. Allí se crean actualmente, entre otros:

- persistencia de grupos;
- persistencia de asistencia;
- persistencia de proyectos/actividades;
- persistencia de expediente;
- casos de uso correspondientes;
- ViewModels de Grupo, Asistencia diaria/mensual, Proyectos, Evaluación y Expediente;
- servicios WPF de confirmación y notificación;
- `MainWindowViewModel` y `MainWindow`.

La aplicación registra errores no controlados en un log local de diagnóstico y evita mostrar trazas técnicas al usuario final.

## Arquitectura de interfaz WPF

### MainWindow como shell

`MainWindow` aloja la navegación principal y las superficies operativas de alto nivel. La navegación actual incluye:

- Grupo;
- Asistencia;
- Proyectos;
- Evaluación.

También incluye selector de grupo, selector de tema, progreso global y notificaciones tipo toast.

### Vistas principales especializadas

Se usa una superficie amplia dentro de `MainWindow` para tareas que necesitan panorama o mucho espacio, por ejemplo:

- lista de estudiantes;
- asistencia mensual;
- lista de proyectos;
- evaluación de alumnos.

### Ventanas dedicadas

El sistema abandonó como regla el master-detail de tres zonas para proyectos porque comprimía demasiado las tareas. La experiencia vigente usa ventanas enfocadas para operaciones complejas, entre ellas:

- `EditorEstudianteWindow`;
- `DetalleProyectoWindow`;
- `DetalleActividadWindow`;
- `ExpedienteEstudianteWindow`.

La regla arquitectónica de UI es ahora:

> la jerarquía del dominio no se copia automáticamente como jerarquía visual.

Se elige entre lista principal, ventana dedicada o pantalla especializada según la tarea real del usuario. Master-detail queda como patrón opcional, no obligatorio.

### Diseño y temas

La interfaz dispone de un sistema de diseño centralizado basado en recursos WPF, incluyendo `DesignTokens.xaml`, y soporta temas:

- Claro;
- Oscuro;
- Alto contraste.

Los recursos visuales deben consumirse mediante recursos compartidos en lugar de colores y estilos repetidos por ventana.

La aplicación usa `xml:lang="es-MX"`, recursos localizables en partes de la UI y propiedades de automatización/accesibilidad en controles relevantes.

Las reglas de interfaz vigentes se documentan en:

```text
docs/UI-GUIDELINES.md
```

## Flujos principales

### Gestión de grupo y estudiantes

La UI delega al ViewModel; Presentation delega a Application; Application modifica el agregado `Grupo` y Data confirma la operación SQLite. La edición compleja de estudiante se realiza en ventana dedicada.

### Asistencia mensual

Application proyecta únicamente días lectivos visibles, reúne matrícula activa e historial y devuelve snapshots inmutables. Presentation mantiene cambios por fecha. Guardar un día confirma sólo esa fecha; guardar varias fechas ejecuta operaciones diarias secuenciales y no afirma atomicidad mensual.

### Proyectos y actividades

La vista principal de Proyectos funciona como lista/punto de entrada. El detalle de proyecto se edita en ventana dedicada y desde allí se gestionan sus actividades. Cada actividad se edita en su propia ventana. La actividad conserva el padrón histórico de estudiantes.

### Evaluación

Evaluación es un módulo principal independiente. Selecciona proyecto y actividad y dedica la superficie disponible a registrar niveles de logro de los alumnos sin mezclar el formulario de planeación del proyecto.

### Expediente

Desde Grupo se abre la ventana de expediente del alumno. Application consolida asistencia, entregas/evaluación y registros pedagógicos; Data persiste notas y acuerdos propios del seguimiento.

## Estrategia de pruebas

- **Core:** invariantes, transiciones y comportamiento de agregados.
- **Application:** casos de uso con dobles de puertos, snapshots y conflictos.
- **Data:** pruebas contra SQLite temporal real, migraciones, restricciones, rollback y reapertura.
- **Presentation:** ViewModels y comandos sin WPF ni SQLite.
- **App.Wpf:** composición, bindings/patrones verificables y límites del code-behind.
- **Prueba manual:** apertura real de ventanas, redimensionamiento, foco, teclado, temas y experiencia visual.
- **Auditoría independiente:** revisión de arquitectura, persistencia, UX y regresiones antes de cerrar cambios relevantes.

## Estado funcional actual

El repositorio ya contiene, como mínimo:

- fundación técnica;
- gestión de grupos y estudiantes;
- asistencia diaria y mensual;
- proyectos didácticos;
- actividades con padrón histórico;
- evaluación de actividades mediante niveles de logro;
- expediente y seguimiento individual;
- temas claro, oscuro y alto contraste;
- sistema de diseño WPF centralizado;
- notificaciones y validación visual;
- ventanas dedicadas para edición compleja.

Siguen existiendo líneas de evolución como reportes, respaldos, importación/exportación, planeación NEM más rica y otros módulos definidos en el roadmap.

## Decisiones arquitectónicas vigentes

- Core permanece independiente y concentra invariantes del dominio.
- Los puertos viven en Application; Data aporta adaptadores concretos.
- SQLite se usa directamente mediante `Microsoft.Data.Sqlite`.
- La unidad atómica de asistencia es el día.
- `ProyectoDidactico` y `ActividadProyecto` son agregados separados.
- La actividad es la unidad atómica para su padrón de entregas/evaluación.
- Presentation es portable y usa MVVM propio.
- WPF queda aislado en `SistemaDocente.App.Wpf`.
- La composición es manual.
- No se introducen ORM, repositorios genéricos ni framework de navegación sin una decisión explícita.
- Master-detail no es una regla de UI; se usa sólo cuando mejora realmente la tarea.
- Las tareas complejas pueden usar ventanas dedicadas y las tareas intensivas usan superficies principales amplias.
- Los estilos y colores nuevos deben integrarse al sistema de diseño compartido.
