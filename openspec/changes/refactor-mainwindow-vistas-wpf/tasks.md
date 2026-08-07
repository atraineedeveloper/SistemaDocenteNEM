# Tasks: Refactor MainWindow a vistas WPF

- [x] Extraer `Views/GrupoView.xaml(.cs)` con la presentación del módulo Grupo (bienvenida, lista, búsqueda, DataGrid, EmptyState, editor de nombre, apertura de `EditorEstudianteWindow` y `ExpedienteEstudianteWindow`) <!-- id: 1 -->
- [x] Extraer `Views/AsistenciaView.xaml(.cs)` con la vista diaria y mensual, `GrillaMensual`, atajos P/F/R/J (sólo con foco en la grilla), Ctrl+S, PageUp/Down y selector compacto <!-- id: 2 -->
- [x] Extraer `Views/ProyectosView.xaml(.cs)` con la lista principal, filtros y apertura de `DetalleProyectoWindow` (sin reintroducir master-detail) <!-- id: 3 -->
- [x] Extraer `Views/EvaluacionView.xaml(.cs)` con selectores, métricas, grilla y atajos D/S/E/R/N/P (sólo con foco en la grilla) <!-- id: 4 -->
- [x] Extraer `Controls/MainNavigationHeader.xaml(.cs)` con branding, selector de grupo, navegación, selector de tema e indicador de pestaña activa <!-- id: 5 -->
- [x] Reducir `MainWindow.xaml` a shell: encabezado + cuatro vistas + toast global + progreso global <!-- id: 6 -->
- [x] Reducir `MainWindow.xaml.cs` a asuntos del shell (cierre, toast, exposición del ViewModel) <!-- id: 7 -->
- [x] Actualizar `SistemaDocente.App.Wpf.Tests` para verificar la estructura separada y añadir pruebas de regresión <!-- id: 8 -->
- [ ] Validar con `dotnet build`, `dotnet test` y `openspec validate --all` <!-- id: 9 -->
- [ ] Completar validación manual: lista, búsqueda, teclado, scroll, temas, redimensionamiento y ventanas dedicadas <!-- id: 10 -->

## Correcciones posteriores a auditoría independiente

- [x] Introducir `ModuloAsistenciaViewModel` como frontera del módulo para evitar que `AsistenciaView` dependa del `MainWindowViewModel` completo <!-- id: 11 -->
- [x] Hacer idempotentes y liberables las suscripciones de `MainNavigationHeader`, `GrupoView` y `AsistenciaView` <!-- id: 12 -->
- [x] Sustituir colores semánticos hardcodeados del shell y vistas extraídas por tokens/recursos compatibles con temas <!-- id: 13 -->
- [x] Mantener el `DataGrid` de Grupo fuera de un `ScrollViewer` exterior para preservar scroll propio y virtualización <!-- id: 14 -->
- [x] Restringir PageUp/PageDown y atajos simples de asistencia al contexto real de `GrillaMensual` <!-- id: 15 -->
- [x] Desacoplar `GrupoView` de la clase concreta `MainWindow` mediante una propiedad de dependencia para `Expediente` <!-- id: 16 -->
- [x] Añadir pruebas estructurales para frontera de asistencia, suscripciones, temas, virtualización y teclado contextual <!-- id: 17 -->
- [x] Anclar explícitamente al `RootWindow` los bindings de `DataContext` y `Visibility` de cada vista para evitar que el DataContext local cambie la fuente del binding <!-- id: 18 -->
- [x] Corregir precedencia de diccionarios en `ThemeService`: conservar `DesignTokens` y colocar el tema activo al final como override <!-- id: 19 -->
- [x] Ajustar contraste del encabezado y ancho mínimo de la ventana para mantener navegación legible en Light/Dark/HighContrast <!-- id: 20 -->
