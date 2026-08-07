# Arquitectura del sistema

## Visión general

Sistema Docente Local es una aplicación de escritorio WPF para la gestión cotidiana de un grupo de primaria. La solución está organizada en capas y se ensambla mediante composición manual. El dominio y los casos de uso permanecen independientes de WPF y SQLite; los adaptadores concretos se crean en la raíz de composición de la aplicación.

La solución usa .NET 10. Los proyectos productivos portables usan `net10.0`; la aplicación WPF usa `net10.0-windows`.

## Proyectos y responsabilidades

| Proyecto | Responsabilidad actual |
| --- | --- |
| `SistemaDocente.Core` | Modelo de dominio, identidades, invariantes y excepciones. Contiene grupo/estudiantes, asistencia, proyectos didácticos, actividades, estado de entrega, niveles de logro, contexto de grupo y elementos del expediente pedagógico. |
| `SistemaDocente.Application` | Casos de uso, puertos de persistencia, snapshots y coordinación entre agregados. Gestiona grupo, asistencia, proyectos/actividades, evaluación, expediente, contexto y construcción de fuentes para reportes. |
| `SistemaDocente.Data` | Adaptadores SQLite, inicialización/migración de esquema, extensiones versionadas, consultas y persistencia transaccional. Traduce errores técnicos en la frontera de infraestructura. |
| `SistemaDocente.Presentation` | MVVM portable: ViewModels, comandos, modelos visuales, confirmaciones, estado editable y fronteras de módulo. Incluye matriz de evaluación, reportes y configuración contextual sin depender de WPF ni SQLite. |
| `SistemaDocente.Reporting` | Modelos y cálculos puros para reporte individual/grupal, asistencia, cumplimiento de entregas y distribución de logro. No conoce SQLite ni WPF. |
| `SistemaDocente.App.Wpf` | Shell WPF (`MainWindow`), vistas de módulos, ventanas dedicadas, temas, recursos visuales, servicios WPF, notificaciones, raíz de composición y modo de demostración aislado. |

Los proyectos de pruebas existentes son:

- `SistemaDocente.Core.Tests`;
- `SistemaDocente.Application.Tests`;
- `SistemaDocente.Data.Tests`;
- `SistemaDocente.Presentation.Tests`;
- `SistemaDocente.App.Wpf.Tests`.

Las pruebas puras de `SistemaDocente.Reporting` viven actualmente en `SistemaDocente.Application.Tests`, que referencia explícitamente al proyecto Reporting para no introducir otro proyecto de pruebas en este corte.

## Grafo de dependencias

```text
SistemaDocente.Core
    ↑          ↑             ↑
Application   Data       Reporting
    ↑          ↑             ↑
Presentation  │             │
    ↑          │             │
    └──── App.Wpf ───────────┘
```

Referencias productivas relevantes:

```text
Application  → Core + Reporting
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

### Contexto del grupo

`ContextoGrupo` representa información contextual 1:1 asociada a `GrupoId`:

- ciclo escolar;
- escuela y CCT;
- entidad, municipio y localidad;
- grado, grupo y turno;
- etapa cognoscitiva grupal de referencia;
- docente responsable y periodo de responsabilidad;
- horario de entrada/salida.

La etapa de Piaget es una referencia pedagógica general del grupo, no un diagnóstico ni una clasificación individual.

### Asistencia

`AsistenciaDiaria` es el agregado de asistencia. Su identidad natural es `GrupoId + DateOnly`. Cada fecha se persiste de manera independiente y atómica.

El mes no es un agregado. Application construye una proyección mensual inmutable; Presentation conserva snapshot confirmado y copia editable. No existe tabla ni transacción mensual.

### Proyectos y actividades

`ProyectoDidactico` es un agregado independiente con identidad, grupo, periodo, estado, descripción, observaciones y versión para concurrencia optimista. Su ciclo de vida contempla Borrador, EnCurso y Finalizado, incluida reapertura explícita.

`ActividadProyecto` es un agregado independiente perteneciente a un proyecto y grupo. Mantiene título, descripción, fecha de realización, observaciones, estado, versión y el padrón histórico completo de entregas/evaluaciones de estudiantes.

La actividad, no el proyecto completo, es la unidad atómica para guardar el padrón asociado. No se persisten las entregas una por una como operaciones independientes.

### Entrega y nivel de logro

`EntregaActividad` mantiene dos dimensiones separadas:

```text
EstadoEntregaActividad
├── Pendiente
├── Entregada
└── NoEntregada

NivelLogro
├── Pendiente
├── Domina
├── Suficiente
├── EnProceso
├── RequiereApoyo
└── NoEntrego  (sólo legado/compatibilidad)
```

Invariantes actuales:

- actividad recién creada → `Pendiente + NivelLogro.Pendiente`;
- `Entregada + NivelLogro.Pendiente` es válida: trabajo recibido, evaluación aún pendiente;
- `NoEntregada` fuerza `NivelLogro.Pendiente`;
- asignar Domina/Suficiente/EnProceso/RequiereApoyo fuerza `Entregada`;
- `NivelLogro.NoEntrego` no se persiste en flujos nuevos: se normaliza a `NoEntregada + Pendiente`.

### Evaluación

La evaluación reutiliza el padrón histórico de `ActividadProyecto`; no introduce un agregado separado. `EvaluacionActividadesViewModel` proyecta una matriz:

```text
filas    = estudiantes presentes en al menos un padrón histórico del proyecto
columnas = actividades del proyecto
celda    = EstadoEntrega + NivelLogro + Observación
```

Una celda inexistente en el padrón histórico se representa como `—` y no puede editarse. Así, una alta posterior no se agrega retroactivamente a actividades previas y un estudiante inactivo puede conservarse en el historial.

Cada columna recibe un código visual estable con formato `A` + ocho caracteres hexadecimales derivados de `ActividadId`, por ejemplo `A4F2C91B7`. El código no depende del orden, fecha ni título, por lo que no se renumera. `ActividadId` sigue siendo la identidad real.

La representación compacta de celda es:

```text
P  pendiente de entrega
N  no entregada
✓  entregada, pendiente de evaluación
D  domina
S  suficiente
E  en proceso
R  requiere apoyo
—  no aplicable
```

Guardar la matriz no crea una transacción de proyecto. Presentation detecta actividades con cambios y llama secuencialmente al caso de uso para guardar el padrón completo de cada actividad. Cada actividad conserva su propia atomicidad y concurrencia optimista.

### Expediente y seguimiento individual

El expediente consolida información procedente de asistencia, actividades/evaluación y registros pedagógicos propios. La persistencia incluye notas pedagógicas y acuerdos con tutores vinculados por estudiante y grupo.

El diseño prioriza seguimiento formativo y evita tratar alertas pedagógicas como diagnósticos clínicos.

## Reporting

`SistemaDocente.Reporting` contiene cálculos puros. Application obtiene datos mediante los puertos existentes, construye `EstudianteReporteFuente` y delega los agregados al generador.

Reporting no consulta SQLite, no conoce Presentation y no produce WPF.

### Reporte individual

Incluye:

- identidad del estudiante y contexto del grupo;
- asistencia mensual y porcentaje agregado;
- entregadas, no entregadas, pendientes y porcentaje de cumplimiento;
- distribución de niveles de logro sólo sobre entregas;
- proyectos/actividades aplicables;
- fortalezas, dificultades, apoyos, observaciones y acuerdos del expediente.

### Reporte grupal

Incluye:

- matrícula histórica y activa;
- asistencia agregada;
- cumplimiento de entregas;
- distribución de niveles de logro;
- evolución mensual;
- tabla de seguimiento individual sin ranking competitivo.

El porcentaje de cumplimiento se define como:

```text
Entregadas / (Entregadas + NoEntregadas) * 100
```

Los estados `Pendiente` no entran en el denominador. Si no hay decisiones de entrega, se representa como valor indefinido (`—` en UI), no como 0 %.

## Persistencia

La persistencia local usa SQLite mediante `Microsoft.Data.Sqlite` directo, sin ORM ni micro-ORM.

El esquema base vigente continúa en:

```text
PRAGMA user_version = 6
```

### Extensión versionada de reportes/contexto/entregas

Para no reconstruir destructivamente una base v6 ya validada, el estado explícito y el contexto se incorporan mediante una extensión aditiva:

```text
esquema_extensiones
└── reportes-contexto-entregas = 1

configuracion_grupo
└── contexto 1:1 por GrupoId

estados_entrega_actividad
└── EstadoEntregaActividad por actividad + estudiante
```

La columna histórica `entregas_actividad.estado_entrega` conserva temporalmente `NivelLogro` por compatibilidad. `PersistenciaProyectosSqlite` realiza lectura combinada y escritura dual:

- nivel → columna histórica de `entregas_actividad`;
- estado explícito → `estados_entrega_actividad`.

Al inicializar por primera vez la extensión:

```text
NoEntrego legado       -> NoEntregada + Pendiente
Pendiente legado       -> Pendiente + Pendiente
Domina/Suf/etc. legado -> Entregada + mismo nivel
```

La inicialización de la extensión es transaccional e idempotente. Su versión se controla de forma independiente de `PRAGMA user_version`.

Las entradas Application legacy que sólo expresan `NivelLogro` se distinguen de las entradas nuevas. Cuando una edición legacy trae `Pendiente` sin expresar un cambio de estado, el caso de uso conserva el estado histórico existente para que editar metadatos no borre `Entregada + Pendiente` ni otros estados ya decididos.

### Principios SQLite vigentes

- `PRAGMA foreign_keys = ON`;
- consultas parametrizadas;
- fechas canónicas cuando se persisten como texto;
- transacciones por operación compuesta;
- asistencia atómica por día;
- actividad + padrón guardados en una sola transacción;
- concurrencia optimista donde se expone versión;
- restricciones relacionales de pertenencia a grupo/proyecto;
- historial pedagógico no se elimina accidentalmente;
- errores SQLite no llegan a Presentation ni UI.

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

`--demo` abre/crea exclusivamente almacenamiento de demostración. `--demo-reset` sólo borra archivos cuando `RutasAplicacion.EsDemostracion` es verdadero. `DemoDataSeeder` siembra datos escolares y `DemoContextSeeder` añade el contexto ficticio del grupo.

## Presentación y composición

Presentation usa MVVM propio y portable. Los ViewModels gestionan selección, edición, filtros, confirmaciones, cambios pendientes y comandos sin conocer WPF.

Asistencia dispone de una frontera explícita:

```text
ModuloAsistenciaViewModel
├── Diaria  → GestionAsistenciaViewModel
├── Mensual → GestionAsistenciaMensualViewModel
├── MostrarDiaria / MostrarMensual
└── comandos de cambio de vista
```

`MainWindowViewModel` coordina navegación global. `App.xaml.cs` es la raíz de composición: crea persistencias, casos de uso, ViewModels y servicios WPF, interpreta `--demo`/`--demo-reset` y conecta contexto/reportes.

La configuración contextual usa una sola instancia de `ConfiguracionGrupoViewModel` compartida por `GrupoView` y `ReportesView` mediante propiedades de dependencia del shell.

## Arquitectura de interfaz WPF

### MainWindow como shell

`MainWindow` ensambla `MainNavigationHeader`, vistas de módulos y feedback global. La navegación incluye:

- Grupo;
- Asistencia;
- Proyectos;
- Evaluación;
- Reportes.

Existe una sola navegación global superior; no hay sidebar duplicado.

### Vistas principales especializadas

Cada módulo recibe una frontera explícita:

- `GrupoView` → `GestionGrupoViewModel` + expediente + configuración contextual;
- `AsistenciaView` → `ModuloAsistenciaViewModel`;
- `ProyectosView` → `GestionProyectosViewModel`;
- `EvaluacionView` → `EvaluacionActividadesViewModel`;
- `ReportesView` → `GestionReportesViewModel` + configuración contextual.

`GrupoView` mantiene tabla virtualizada y añade acceso a `Configurar grupo` sin convertir el header global en un panel de configuración.

`ReportesView` contiene modos Individual/Grupal y puede abrir la misma ventana de configuración contextual.

`EvaluacionView` mantiene la matriz estudiante × actividad y congela `Núm.` + `Estudiante`. La actividad de contexto se deriva de la columna actual.

### Ventanas dedicadas

Las tareas complejas siguen en ventanas enfocadas:

- `EditorEstudianteWindow`;
- `DetalleProyectoWindow`;
- `DetalleActividadWindow`;
- `ExpedienteEstudianteWindow`;
- `EditarEvaluacionCeldaWindow` para estado de entrega, nivel y observación;
- `ConfiguracionGrupoWindow` para contexto escolar y pedagógico.

La jerarquía del dominio no se copia automáticamente como jerarquía visual. Master-detail sigue siendo opcional.

### Evaluación por teclado

Los atajos sólo se procesan cuando el foco pertenece a la grilla:

```text
D/S/E/R = nivel de logro (y fuerza Entregada)
T       = Entregada, pendiente de evaluación
N       = No entregada
P       = Pendiente de entrega
Enter/F2 o doble clic = editor compacto
Ctrl+S = guardar cambios
```

Los controles de texto quedan excluidos del manejo contextual.

### Diseño, temas y virtualización

La interfaz usa `DesignTokens.xaml` y soporta Claro, Oscuro y Alto contraste. Vistas y shell consumen recursos semánticos mediante `DynamicResource` o resuelven brushes del tema actual cuando las columnas se generan dinámicamente.

La aplicación usa `xml:lang="es-MX"`, foco visible y propiedades de automatización. Los `UserControl` con suscripciones las hacen idempotentes y las liberan en `Unloaded`.

Los `DataGrid` operativos controlan su propio scroll y conservan virtualización; no deben envolverse en un `ScrollViewer` exterior no acotado.

Las reglas visuales generales viven en `docs/UI-GUIDELINES.md`.

## Flujos principales

### Gestión de grupo y estudiantes

UI → Presentation → Application → Core/Data. La edición compleja de estudiante se realiza en ventana dedicada. La configuración contextual puede abrirse directamente desde Grupo.

### Asistencia mensual

Application proyecta días lectivos, matrícula activa e historial. Presentation mantiene cambios por fecha. Guardar varias fechas ejecuta operaciones diarias secuenciales y no afirma atomicidad mensual.

### Proyectos y actividades

`ProyectosView` es lista/punto de entrada. `DetalleProyectoWindow` y `DetalleActividadWindow` concentran edición compleja. La actividad conserva el padrón histórico.

### Evaluación matricial

El usuario selecciona un proyecto. Presentation carga actividades y construye la matriz. Mover la celda cambia la actividad de contexto. Estado de entrega y nivel se editan por separado y se persisten juntos por actividad.

### Reportes

Application obtiene contexto, grupo, asistencia, proyectos/actividades y expediente. Reporting calcula snapshots puros. Presentation selecciona modo/alumno y WPF muestra el resultado. La configuración se puede abrir desde Reportes y, al cerrarla, el reporte se refresca.

### Expediente

Desde Grupo se abre la ventana de expediente. Application consolida asistencia, entregas/evaluación y registros pedagógicos; Data persiste notas y acuerdos.

### Demostración

`--demo` usa un conjunto ficticio rico para probar listas, historial, asistencia, proyectos, estados de entrega, evaluación, expediente, contexto y reportes. La guía vive en `docs/demo-mode.md`.

## Estrategia de pruebas

- **Core:** invariantes de agregados, incluida separación entrega/logro.
- **Application:** casos de uso, compatibilidad legacy y contratos de entrada.
- **Data:** SQLite temporal real, extensión versionada, conversión legacy, restricciones y reapertura.
- **Reporting:** cálculos puros de cumplimiento y distribución de logro.
- **Presentation:** ViewModels, matriz, filtros, cambios pendientes y persistencia explícita.
- **App.Wpf:** composición, bindings, rutas demo, recursos, teclado contextual y configuración compartida.
- **Prueba manual:** apertura real, demo, redimensionamiento, foco, teclado, temas, scroll y ventanas dedicadas.

## Decisiones arquitectónicas vigentes

- Core permanece independiente y concentra invariantes.
- Los puertos viven en Application; Data aporta adaptadores concretos.
- SQLite se usa directamente mediante `Microsoft.Data.Sqlite`.
- El esquema base permanece en `user_version = 6`; capacidades nuevas de este corte usan una extensión aditiva versionada.
- La unidad atómica de asistencia es el día.
- `ProyectoDidactico` y `ActividadProyecto` son agregados separados.
- La actividad es la unidad atómica para su padrón de entrega/evaluación.
- Estado de entrega y nivel de logro son dimensiones distintas.
- La matriz de evaluación es una proyección de Presentation, no un nuevo agregado ni tabla SQLite.
- Los códigos visuales de actividad derivan de `ActividadId` y son estables.
- Guardar varias columnas significa operaciones atómicas por actividad ejecutadas secuencialmente.
- Reporting contiene cálculos puros y no accede a infraestructura.
- Presentation es portable y usa MVVM propio.
- WPF queda aislado en `SistemaDocente.App.Wpf`.
- La composición es manual.
- Grupo y Reportes reutilizan la misma configuración contextual.
- El modo demo usa almacenamiento separado y no toca producción.
- No se introducen ORM, repositorios genéricos ni framework de navegación sin decisión explícita.
- Master-detail no es una regla de UI.
- `MainWindow` actúa como shell visual y cada módulo recibe una frontera explícita.
- Los estilos y colores nuevos deben integrarse al sistema de diseño compartido.
- Las suscripciones WPF de larga vida deben ser idempotentes y liberarse con el ciclo de vida visual.
