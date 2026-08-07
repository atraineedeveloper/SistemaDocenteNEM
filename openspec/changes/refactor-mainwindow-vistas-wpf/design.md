# Design: Refactor MainWindow a vistas WPF

## Contexto
`MainWindow` concentraba el shell y la presentación de los cuatro módulos. Esto aumentaba el tamaño del XAML/code-behind y el riesgo de regresiones cruzadas. El refactor aísla la presentación de cada módulo en `UserControl`, mantiene composición manual y evita introducir un framework de navegación.

## Decisiones

### DataContext por frontera de módulo, sin Service Locator
Cada `UserControl` recibe un `DataContext` explícito desde `MainWindow.xaml`.

- `GrupoView` → `GestionGrupoViewModel`.
- `AsistenciaView` → `ModuloAsistenciaViewModel`.
- `ProyectosView` → `GestionProyectosViewModel`.
- `EvaluacionView` → `EvaluacionActividadesViewModel`.

`ModuloAsistenciaViewModel` agrupa la presentación diaria y mensual y el cambio entre ambas vistas. De esta forma `AsistenciaView` no depende del `MainWindowViewModel` completo. `MainWindowViewModel` conserva aliases de compatibilidad para `Asistencia` y `AsistenciaMensual` y coordina la navegación global.

Los bindings de `DataContext`, `Visibility` y dependencias cruzadas que pertenecen al shell se anclan explícitamente a `RootWindow` mediante `ElementName`. Esto evita que, después de asignar un DataContext local a la vista, otros bindings del mismo elemento intenten resolver propiedades como `MostrarGrupo` o `MostrarProyectos` sobre el ViewModel del módulo equivocado.

No se introduce Service Locator ni contenedor de DI.

### Shell puro
`MainWindow` sólo conoce: ventana, encabezado (`MainNavigationHeader`), las cuatro vistas, toast global y progreso global. No contiene DataGrid ni handlers de módulos.

### Code-behind orientado al shell y a la vista
Los handlers contextuales viven en la vista dueña del comportamiento:

- `GrupoView`: apertura de `EditorEstudianteWindow`/`ExpedienteEstudianteWindow`, foco inicial y Escape de edición.
- `AsistenciaView`: columnas mensuales, P/F/R/J, selector compacto, Ctrl+S, PageUp/Down y navegación contextual.
- `ProyectosView`: apertura de `DetalleProyectoWindow`.
- `EvaluacionView`: atajos D/S/E/R/N/P sólo con foco real en la grilla y nunca en `TextBoxBase`.
- `MainNavigationHeader`: indicador de módulo activo y selección de tema.

El code-behind puede abrir ventanas, manipular foco y procesar routed events; no contiene reglas de dominio, SQL ni acceso directo a persistencia.

### Coordinación de expediente sin acoplar Grupo al shell concreto
`GrupoView` recibe `GestionExpedienteViewModel` mediante una `DependencyProperty` enlazada desde `MainWindow`. La vista puede abrir `ExpedienteEstudianteWindow` usando `Window.GetWindow(this)` como Owner sin conocer la clase concreta `MainWindow` ni resolver el ViewModel raíz.

### Suscripciones con ciclo de vida explícito
Los controles que escuchan `PropertyChanged` o eventos estáticos mantienen una referencia al objeto suscrito, evitan altas duplicadas y liberan la suscripción en `Unloaded`.

Esto aplica especialmente a:

- `MainNavigationHeader` → `MainWindowViewModel.PropertyChanged`;
- `GrupoView` → `GestionGrupoViewModel.PropertyChanged`;
- `AsistenciaView` → `GestionAsistenciaMensualViewModel.PropertyChanged` y `ThemeService.ThemeChanged`.

### Recursos y temas
No se duplican diccionarios. Las vistas consumen recursos semánticos de `DesignTokens.xaml` y de los temas Light/Dark/HighContrast.

`DesignTokens.xaml` permanece cargado como base. `ThemeService` elimina únicamente el diccionario nominal de tema anterior y agrega el nuevo diccionario al final de `MergedDictionaries`, de modo que sus claves tengan precedencia sobre los valores base. Nunca se confunde `DesignTokens.xaml` con un tema reemplazable.

Los elementos generados en code-behind —como las columnas dinámicas de asistencia— resuelven los brushes semánticos actuales y se reconstruyen al recibir `ThemeService.ThemeChanged`. No se mantienen colores físicos hardcodeados en las vistas extraídas ni en el shell.

El encabezado utiliza `TextOnPrimaryBrush` sobre `PrimaryBrush` para conservar contraste coherente en los tres temas. La ventana define un ancho mínimo consistente con la huella real de la navegación global.

### Virtualización de listas
El `DataGrid` principal de Grupo mantiene una fila `*` dentro de un `Grid` y controla su propio scroll. No se envuelve la gestión normal en un `ScrollViewer` externo, para evitar medición infinita y conservar la virtualización de filas.

### Teclado contextual
Los atajos de una sola letra y PageUp/PageDown sólo se procesan cuando el foco pertenece al componente operativo correspondiente.

- Asistencia mensual: P/F/R/J, Enter, Home/End y PageUp/PageDown se restringen a `GrillaMensual`.
- Evaluación: D/S/E/R/N/P se restringen a `GrillaEntregasEvaluacion`.
- `TextBoxBase` queda excluido.
- Ctrl+S se conserva como atajo del módulo mediante `InputBindings`.

## Alternativas consideradas
- **Frame/NavigationService:** descartado; la visibilidad por binding ya resuelve la navegación actual y no se necesita framework adicional.
- **Extraer todo a una librería de controles separada:** fuera de alcance; se mantiene dentro de `SistemaDocente.App.Wpf`.
- **Master-detail de tres zonas:** descartado como regla; las tareas complejas siguen en ventanas dedicadas.
- **Pasar todo `MainWindowViewModel` a AsistenciaView:** descartado tras auditoría porque rompe el aislamiento conceptual del módulo.
- **Bindings de visibilidad relativos al DataContext local de cada UserControl:** descartados porque cambian de fuente al asignar el ViewModel del módulo; el shell raíz es la fuente explícita de navegación.

## Riesgos y mitigaciones
- **Bindings rotos al mover DataContext:** bindings anclados a `RootWindow`, smoke test STA y pruebas estructurales de composición.
- **Atajos que interceptan escritura:** validación de foco y `TextBoxBase` antes de ejecutar comandos.
- **Pérdida de columnas dinámicas al cambiar de tema:** reconstrucción de columnas ante `ThemeService.ThemeChanged`.
- **Tema que no sobreescribe tokens base:** el diccionario activo se agrega al final y `DesignTokens` se excluye de la detección de tema reemplazable.
- **Fugas por eventos:** suscripciones idempotentes y liberación en `Unloaded`.
- **Pérdida de virtualización:** `DataGrid` de Grupo permanece fuera de `ScrollViewer` exterior.
- **Regresiones visuales:** la validación manual de temas, redimensionamiento, scroll y ventanas dedicadas permanece obligatoria antes de cerrar el cambio.
