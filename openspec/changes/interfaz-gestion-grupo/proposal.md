## Why

El sistema ya dispone de dominio, persistencia y casos de uso, pero App.Wpf sólo muestra una ventana vacía. Este cambio ofrece el primer flujo utilizable por un docente para crear su único grupo local y administrar estudiantes sin conocer identidades internas ni detalles de SQLite.

## What Changes

- Crear `SistemaDocente.Presentation` y `SistemaDocente.Presentation.Tests` para alojar y probar ViewModels sin WPF, Data ni SQLite.
- Implementar MVVM básico propio con `ViewModelBase`, `RelayCommand` y únicamente la infraestructura imprescindible.
- Configurar en App.Wpf una raíz de composición manual que resuelva `%LOCALAPPDATA%`, construya Data, Application, servicios WPF y ViewModels, y entregue el ViewModel a `MainWindow`.
- Usar `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db` para SQLite y `%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json` para recordar únicamente el `GrupoId` del único grupo.
- Escribir `app-state.json` atómicamente y sólo después de crear el grupo; tratar ausencia, corrupción y referencias huérfanas sin modificar la base.
- Construir una sola MainWindow con paneles de bienvenida, gestión y edición integrados, más una confirmación pequeña antes de desactivar.
- Mostrar estudiantes en un DataGrid de sólo lectura, sin IDs y sin orden alternativo, diferenciando activos e inactivos con texto y estilo.
- Incorporar navegación completa por teclado, estado ocupado, bloqueo de comandos duplicados y restauración en `finally` sin `async`, `Task.Run` ni `CancellationToken`.
- Mostrar validaciones y conflictos junto a la edición conservando entradas; ocultar SQL, rutas, excepciones internas y trazas en errores técnicos.
- Actualizar encabezado y lista exclusivamente desde resultados confirmados por Application.

## Capabilities

### New Capabilities

- `interfaz-gestion-grupo`: Define la experiencia WPF para crear, cargar y administrar el único grupo local, incluidos app-state, edición, teclado, estados visuales y errores.

### Modified Capabilities

- `solution-project-structure`: Añade Presentation y Presentation.Tests y fija el grafo exacto entre Presentation, Application, App.Wpf y Data.

## Impact

- Nuevos proyectos: `src/SistemaDocente.Presentation` y `tests/SistemaDocente.Presentation.Tests`.
- Referencias: Presentation → Application; Presentation.Tests → Presentation y Application; App.Wpf → Presentation, Application y Data.
- ViewModels y servicios abstractos vivirán en Presentation; XAML, MainWindow, servicios WPF y composición concreta vivirán en App.Wpf.
- App.Wpf no añadirá `PackageReference` a `Microsoft.Data.Sqlite`; Data sólo se usará en la raíz de composición.
- No se modificarán Core, el esquema SQLite ni las reglas de Application.
- Permanecen fuera de alcance asistencia, actividades, evaluación, reportes, importación, múltiples grupos, múltiples usuarios, sincronización, instalador, actualización automática y datos personales adicionales.
