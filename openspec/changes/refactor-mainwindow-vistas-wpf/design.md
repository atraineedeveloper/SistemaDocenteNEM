# Design: Refactor MainWindow a vistas WPF

## Contexto
`MainWindow` concentra el shell y la presentación de los cuatro módulos. Esto aumenta el tamaño del XAML/code-behind y el riesgo de regresiones cruzadas. El refactor aísla la presentación de cada módulo en `UserControl` sin cambiar el modelo de composición manual ni el `MainWindowViewModel`.

## Decisiones

### DataContext por binding, sin Service Locator
Cada `UserControl` recibe su `DataContext` directamente desde `MainWindow.xaml` mediante binding simple (p. ej. `DataContext="{Binding Grupo}"`). La visibilidad de cada vista se liga a la propiedad `MostrarX` del `MainWindowViewModel` existente, sin modificar el ViewModel. No se introducen bindings `DataContext.DataContext...`.

### Shell puro
`MainWindow` sólo conoce: ventana, encabezado (`MainNavigationHeader`), las cuatro vistas (toggleadas por visibilidad), toast global y progreso global. No contiene DataGrid ni handlers de módulos.

### Code-behind orientado al shell y a la vista
Los handlers contextuales se mueven a la vista dueña del comportamiento:
- `GrupoView`: apertura de `EditorEstudianteWindow`/`ExpedienteEstudianteWindow`, foco inicial, Escape de edición.
- `AsistenciaView`: columnas mensuales, P/F/R/J, selector compacto, Ctrl+S y PageUp/Down (sólo vista mensual).
- `ProyectosView`: apertura de `DetalleProyectoWindow`.
- `EvaluacionView`: atajos D/S/E/R/N/P (sólo con foco en la grilla, nunca en `TextBoxBase`).
- `MainNavigationHeader`: indicador de pestaña activa y selección de tema.

El code-behind puede abrir ventanas, manipular foco y procesar routed events; no contiene reglas de dominio, SQL ni acceso a agregados.

### Coordinación entre módulos
`GrupoView` resuelve `Expediente` desde el `MainWindowViewModel` vía `Window.GetWindow(this)`, manteniendo la apertura de la ventana dedicada como comportamiento puramente WPF (Owner = ventana propietaria).

### Recursos y temas
No se duplican diccionarios. Las vistas consumen `DynamicResource` de `App.xaml`/`Themes`, por lo que el cambio de tema desde `MainNavigationHeader` (vía `ThemeService`) actualiza automáticamente todas las vistas y ventanas abiertas.

## Alternativas consideradas
- **Frame/NavigationService:** descartado; el requisito prohíbe frameworks de navegación y la visibilidad por binding ya funciona.
- **Extraer todo a una librería de controles separada:** fuera de alcance; se mantiene dentro de `SistemaDocente.App.Wpf`.
- **Mover el encabezado inline:** se extrae a `MainNavigationHeader` porque agrupa branding+navegación+tema y reduce el shell.

## Riesgos y mitigaciones
- **Bindings rotos al mover DataContext:** mitigado manteniendo exactamente las mismas expresiones de binding por módulo y verificando con build + smoke test STA.
- **Atajos que interceptan escritura:** `AsistenciaView` y `EvaluacionView` verifican `Keyboard.FocusedElement is TextBoxBase` antes de actuar.
- **Pérdida de columnas dinámicas:** `AsistenciaView` se suscribe a `AsistenciaMensual.Dias` y reconstruye columnas, igual que antes.
