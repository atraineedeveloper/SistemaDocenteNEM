## 1. Presentation Layer

- [ ] 1.1 Crear `EvaluacionActividadesViewModel` para la pestaña de Evaluación con selección de proyecto, actividad, filtrado por niveles NEM (D, S, EP, RA, NE, P), conteos y atajos masivos.
- [ ] 1.2 Actualizar `GestionProyectosViewModel` para manejar la apertura de ventanas/diálogos dedicados de detalle de proyecto y detalle de actividad.
- [ ] 1.3 Actualizar `MainWindowViewModel` para soportar la navegación a la pestaña `Evaluación`.

## 2. WPF View Layer (`SistemaDocente.App.Wpf`)

- [ ] 2.1 Actualizar la barra de navegación superior en `MainWindow.xaml` para incluir el botón `Evaluación`.
- [ ] 2.2 Crear `DetalleProyectoWindow.xaml` y `DetalleProyectoWindow.xaml.cs` para ver/editar el proyecto y listar sus actividades con botón `Nueva Actividad`.
- [ ] 2.3 Crear `DetalleActividadWindow.xaml` y `DetalleActividadWindow.xaml.cs` para ver/editar datos de una actividad.
- [ ] 2.4 Rediseñar la pestaña `Proyectos` en `MainWindow.xaml` para mostrar la lista limpia y amplia de proyectos.
- [ ] 2.5 Crear el panel de la pestaña `Evaluación` en `MainWindow.xaml` con la grilla de desempeño NEM maximizada a pantalla completa.

## 3. Pruebas y Verificación

- [ ] 3.1 Actualizar y agregar pruebas unitarias en `SistemaDocente.Presentation.Tests` para el nuevo ViewModel y navegación.
- [ ] 3.2 Actualizar las pruebas de composición WPF en `SistemaDocente.App.Wpf.Tests`.
- [ ] 3.3 Ejecutar `dotnet build` y `dotnet test` asegurando 100% pruebas pasando.
- [ ] 3.4 Validar la especificación con `openspec validate --all`.
