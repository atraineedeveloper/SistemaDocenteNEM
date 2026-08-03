## Context

App.Wpf es una ventana mínima. Application ofrece una fachada síncrona que trabaja por `GrupoId`, pero no enumera grupos; Data recibe una ruta explícita. La presentación necesita recordar localmente la identidad del único grupo sin cambiar Application, Core ni el esquema SQLite.

## Goals / Non-Goals

### Goals

- Separar ViewModels comprobables de WPF e infraestructura.
- Mantener una composición manual, pequeña y visible.
- Conservar siempre el último snapshot confirmado.
- Proporcionar interacción completa con teclado y errores contextualizados.
- Evitar infraestructura genérica o dependencias innecesarias.

### Non-Goals

- Introducir toolkit MVVM, contenedor DI, navegación, asincronía o cancelación.
- Cambiar Core, Application o el esquema SQLite.
- Añadir ventanas modales de edición o múltiples grupos.
- Añadir funciones docentes fuera de la gestión básica del grupo.

## Decisions

### MVVM básico propio en Presentation

Se crearán `SistemaDocente.Presentation` y `SistemaDocente.Presentation.Tests`. Presentation referenciará únicamente Application y contendrá ViewModels, `ViewModelBase`, `RelayCommand` y servicios abstractos. La infraestructura implementará sólo `INotifyPropertyChanged`, `ICommand`, notificación de `CanExecute` y lo estrictamente requerido.

Se descarta CommunityToolkit.Mvvm y cualquier toolkit para evitar una dependencia innecesaria en esta primera pantalla. También se descarta una jerarquía genérica de comandos, navegación o mensajería.

### Límite entre Presentation y App.Wpf

Presentation no referenciará WPF, Data ni SQLite. App.Wpf contendrá XAML, MainWindow, implementaciones WPF de confirmación/mensajes, control de foco y la raíz de composición. App.Wpf referenciará Presentation, Application y Data, pero no añadirá el paquete `Microsoft.Data.Sqlite`.

El code-behind se limitará a comportamiento visual como asignar foco inicial cuando cambie el panel. No capturará reglas, persistencia ni coordinación de comandos.

### Referencia del único grupo en app-state.json

La raíz resolverá `%LOCALAPPDATA%\SistemaDocenteNEM\data`. SQLite usará `sistema-docente.db` y un almacenamiento de estado de aplicación usará `app-state.json`. El JSON contendrá únicamente `GrupoId`; no incluirá nombres ni datos personales.

La referencia se escribirá sólo después de que `CrearGrupo` termine correctamente. La escritura será atómica: serializar a un temporal en el mismo directorio, vaciar/cerrar y reemplazar o mover sobre el destino. Un fallo no deberá destruir una referencia anterior válida.

- Archivo ausente: bienvenida.
- Archivo vacío, JSON inválido, estructura inesperada o `GrupoId` inválido: mensaje general y bienvenida.
- `GrupoId` válido cuyo grupo no existe: mensaje de inconsistencia y acción explícita para olvidar la referencia.
- Olvidar la referencia elimina únicamente `app-state.json`; nunca borra, repara o recrea SQLite.

### MainWindow única con paneles integrados

MainWindow alternará estados de bienvenida y gestión. Crear/editar estudiante y cambiar el nombre del grupo usarán paneles integrados en la misma ventana. Escape cancelará la edición y restaurará el estado anterior; Enter confirmará la acción principal; al abrir cada panel, App.Wpf asignará foco al primer campo mediante comportamiento visual.

La desactivación utilizará una confirmación pequeña mediante el servicio abstracto. No habrá ventanas modales adicionales para edición.

### DataGrid contractual

La lista será un DataGrid de sólo lectura con columnas número, nombre y estado, selección de fila y acciones fuera de cada fila. No mostrará IDs. Los inactivos se distinguirán mediante texto y estilo, no sólo color. Los encabezados no permitirán un orden alternativo que altere la secuencia recibida de Application.

El ViewModel copiará la secuencia recibida sin reordenarla. Los IDs podrán conservarse privadamente en modelos visuales para dirigir comandos, pero no se expondrán como columnas, textos o entradas.

### Estado ocupado y API síncrona

Cada comando comprobará y establecerá `EstaOcupado`, notificará `CanExecute`, ejecutará Application y restaurará el estado en `finally`. No se usarán `async`, `Task.Run` ni `CancellationToken`. El requisito garantizado es impedir comandos duplicados y restaurar el estado; en operaciones instantáneas el indicador puede no alcanzar a renderizarse por la naturaleza síncrona.

### Mensajes y estado confirmado

Validation y conflictos se mostrarán junto al panel editable y conservarán la entrada. Los errores técnicos mostrarán un texto general sin SQL, rutas, `InnerException` ni trazas. Las excepciones esperadas serán capturadas en la frontera del ViewModel y no cerrarán la aplicación.

El ViewModel conservará el último snapshot confirmado. Encabezado y lista se reemplazarán sólo después de éxito, usando resultados confirmados o una consulta posterior a Application. Ante fallo de persistencia se conservará la pantalla anterior.

### Pruebas sin WPF

Presentation.Tests construirá ViewModels con dobles manuales de la fachada/servicios abstractos y sin cargar Data, SQLite o ventanas. Cubrirá arranque, app-state, comandos, entradas conservadas, cancelaciones, estado ocupado, orden, ausencia de IDs visibles y consistencia tras fallos.

Las pruebas del almacenamiento JSON podrán ejercitar una implementación neutral ubicada en Presentation sólo si depende de abstracciones de sistema de archivos; si la implementación concreta usa `System.IO`, vivirá en App.Wpf y se probará sin abrir ventanas desde el proyecto adecuado, manteniendo Presentation.Tests libre de WPF y Data.

## Risks / Trade-offs

- [La API síncrona puede impedir que el indicador se renderice] → Documentar la limitación y garantizar bloqueo y `finally`; abordar asincronía en otro cambio.
- [Un app-state corrupto puede ocultar un grupo válido] → Informar, volver a bienvenida y no tocar SQLite.
- [La escritura atómica varía por plataforma y existencia del destino] → Usar temporal en el mismo directorio y probar creación, reemplazo y fallo.
- [IDs privados en modelos visuales podrían mostrarse accidentalmente] → DataGrid con columnas explícitas y pruebas de superficie visible.
- [Acciones repetidas por teclado o ratón] → `EstaOcupado` y `CanExecute` bloquean duplicados.

## Migration Plan

1. Crear Presentation y Presentation.Tests con el grafo aprobado.
2. Implementar MVVM mínimo, servicios abstractos y ViewModels.
3. Implementar el almacenamiento atómico de `app-state.json` y sus estados de error.
4. Configurar la raíz de composición con las dos rutas bajo LocalApplicationData.
5. Construir MainWindow, paneles integrados, DataGrid, comandos y foco.
6. Implementar mensajes, confirmación y conservación del snapshot confirmado.
7. Añadir pruebas de Presentation y app-state.
8. Ejecutar restore, format, build, test y verificación manual de WPF.

Rollback: restaurar MainWindow mínima, retirar Presentation y el almacenamiento de app-state. No borrar ni modificar la base SQLite; `app-state.json` puede conservarse o retirarse de forma explícita sin afectar datos del grupo.
