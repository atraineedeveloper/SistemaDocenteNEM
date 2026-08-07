# Tasks: Refactor MainWindow a vistas WPF

- [ ] Extraer `Views/GrupoView.xaml(.cs)` con la presentación del módulo Grupo (bienvenida, lista, búsqueda, DataGrid, EmptyState, editor de nombre, apertura de `EditorEstudianteWindow` y `ExpedienteEstudianteWindow`) <!-- id: 1 -->
- [ ] Extraer `Views/AsistenciaView.xaml(.cs)` con la vista diaria y mensual, `GrillaMensual`, atajos P/F/R/J (sólo con foco en la grilla), Ctrl+S, PageUp/Down y selector compacto <!-- id: 2 -->
- [ ] Extraer `Views/ProyectosView.xaml(.cs)` con la lista principal, filtros y apertura de `DetalleProyectoWindow` (sin reintroducir master-detail) <!-- id: 3 -->
- [ ] Extraer `Views/EvaluacionView.xaml(.cs)` con selectores, métricas, grilla y atajos D/S/E/R/N/P (sólo con foco en la grilla) <!-- id: 4 -->
- [ ] Extraer `Controls/MainNavigationHeader.xaml(.cs)` con branding, selector de grupo, navegación, selector de tema e indicador de pestaña activa <!-- id: 5 -->
- [ ] Reducir `MainWindow.xaml` a shell: encabezado + cuatro vistas + toast global + progreso global <!-- id: 6 -->
- [ ] Reducir `MainWindow.xaml.cs` a asuntos del shell (cierre, toast, exposición del ViewModel) <!-- id: 7 -->
- [ ] Actualizar `SistemaDocente.App.Wpf.Tests` para verificar la estructura separada y añadir pruebas de regresión <!-- id: 8 -->
- [ ] Validar con `dotnet build`, `dotnet test` y `openspec validate --all` <!-- id: 9 -->
- [ ] Dejar registro de validación manual pendiente (lista, búsqueda, teclado, scroll, temas, redimensionamiento) <!-- id: 10 -->
