# Proposal: Refactor MainWindow a vistas WPF

## Summary
Refactor estructural de la capa Presentation/WPF: convertir `MainWindow` en un shell visual pequeño y extraer la presentación de cada módulo (Grupo, Asistencia, Proyectos y Evaluación) a `UserControl` dedicados bajo `Views/`, y el encabezado global a `Controls/MainNavigationHeader`. No se introducen frameworks de navegación ni contenedor de DI; `MainWindowViewModel` se mantiene intacto y cada vista recibe su `DataContext` por binding.

## Intent
- Reducir el tamaño y la responsabilidad de `MainWindow.xaml`/`.cs`, que hoy actúa como shell y como vista de los cuatro módulos a la vez.
- Disminuir el riesgo de romper una pantalla al modificar otra.
- Facilitar que agentes automaticen cambios seguros sobre cada módulo de forma aislada.
- Conservar íntegramente el comportamiento, la navegación funcional, las reglas de dominio y la escala de niveles de logro vigentes.

## Proposed Changes

### `SistemaDocente.App.Wpf` (Presentation/WPF)
- Extraer `Views/GrupoView.xaml(.cs)`, `Views/AsistenciaView.xaml(.cs)`, `Views/ProyectosView.xaml(.cs)` y `Views/EvaluacionView.xaml(.cs)` desde `MainWindow`.
- Extraer `Controls/MainNavigationHeader.xaml(.cs)` (branding, selector de grupo, navegación, selector de tema e indicador de pestaña activa).
- Reducir `MainWindow.xaml` a ensamblar el encabezado, las cuatro vistas (visibles según `MostrarX`), el toast global y el indicador de progreso.
- Reducir `MainWindow.xaml.cs` a asuntos del shell: cierre, feedback global (toast) y exposición del ViewModel. Los handlers contextuales (P/F/R/J, D/S/E/R/N/P, Ctrl+S, apertura de ventanas dedicadas) se mueven a la vista correspondiente.

### `SistemaDocente.App.Wpf.Tests`
- Actualizar las pruebas de composición existentes para que verifiquen la estructura separada (vistas en lugar de contenido inline en `MainWindow`).
- Añadir pruebas de regresión: MainWindow ensambla vistas separadas, no contiene las DataGrid principales de los módulos, los atajos simples no están registrados globalmente en MainWindow y no existe SQL en vistas/code-behind.

## Non-goals
- No se rediseñan las pantallas ni se cambian colores, paleta, temas ni la escala `NivelLogro`.
- No se reescriben ViewModels, ni se modifica Core/Application/Data.
- No se introducen Prism, ReactiveUI, CommunityToolkit, framework de navegación ni contenedor DI.
- No se reintroduce master-detail; los formularios complejos siguen en ventanas dedicadas.

## System Capability Impact
- **Capabilities Added:** Separación de responsabilidades visuales por módulo en Presentation/WPF. No añade capacidad funcional nueva; es un refactor estructural.
- **User Experience:** Sin cambios funcionales para el docente; misma navegación, mismos atajos y mismos temas.
