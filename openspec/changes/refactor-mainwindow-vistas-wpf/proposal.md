# Proposal: Refactor MainWindow a vistas WPF

## Summary
Refactor estructural de la capa Presentation/WPF: convertir `MainWindow` en un shell visual pequeño y extraer la presentación de cada módulo (Grupo, Asistencia, Proyectos y Evaluación) a `UserControl` dedicados bajo `Views/`, y el encabezado global a `Controls/MainNavigationHeader`. No se introducen frameworks de navegación ni contenedor de DI. Cada vista recibe una frontera de presentación explícita por binding; Asistencia incorpora `ModuloAsistenciaViewModel` para agrupar sus vistas diaria y mensual sin depender del `MainWindowViewModel` completo.

## Intent
- Reducir el tamaño y la responsabilidad de `MainWindow.xaml`/`.cs`.
- Disminuir el riesgo de romper una pantalla al modificar otra.
- Facilitar cambios aislados y auditables por módulo.
- Conservar el comportamiento, navegación funcional, reglas de dominio y escala de niveles de logro vigentes.
- Mantener temas, accesibilidad, teclado contextual y virtualización después de separar las vistas.

## Proposed Changes

### `SistemaDocente.Presentation`
- Añadir `ModuloAsistenciaViewModel` como frontera de presentación que agrupa `GestionAsistenciaViewModel`, `GestionAsistenciaMensualViewModel` y el cambio entre vista diaria/mensual.
- Mantener `MainWindowViewModel` como coordinador de navegación global, conservando aliases de compatibilidad para los ViewModels de asistencia existentes.

### `SistemaDocente.App.Wpf`
- Extraer `Views/GrupoView.xaml(.cs)`, `Views/AsistenciaView.xaml(.cs)`, `Views/ProyectosView.xaml(.cs)` y `Views/EvaluacionView.xaml(.cs)` desde `MainWindow`.
- Extraer `Controls/MainNavigationHeader.xaml(.cs)` para branding, selector de grupo, navegación, selector de tema e indicador de módulo activo.
- Reducir `MainWindow.xaml` a ensamblar encabezado, cuatro vistas, toast global e indicador de progreso.
- Reducir `MainWindow.xaml.cs` a cierre y feedback global.
- Mantener atajos simples únicamente dentro de sus grillas operativas.
- Mantener la gestión normal de Grupo fuera de un `ScrollViewer` exterior para preservar virtualización del `DataGrid`.
- Consumir colores semánticos mediante recursos compartidos compatibles con Light/Dark/HighContrast.
- Gestionar suscripciones a eventos de manera idempotente y liberarlas en `Unloaded` cuando corresponda.
- Pasar `GestionExpedienteViewModel` a `GrupoView` mediante propiedad de dependencia en lugar de resolver el `MainWindowViewModel` concreto.

### `SistemaDocente.App.Wpf.Tests`
- Verificar que MainWindow ensambla vistas separadas y no contiene las principales grillas internas de los módulos.
- Verificar la frontera propia de Asistencia.
- Verificar teclado contextual, ausencia de colores físicos en las vistas extraídas, virtualización y suscripciones idempotentes.
- Mantener smoke test STA de construcción/layout de MainWindow.

## Non-goals
- No se rediseñan los flujos funcionales ni se reintroduce master-detail.
- No se modifica Core, Application ni Data.
- No se cambia `NivelLogro` ni reglas de asistencia/proyectos/evaluación.
- No se introducen Prism, ReactiveUI, CommunityToolkit, framework de navegación ni contenedor DI.
- No se convierten las ventanas dedicadas en paneles inline.

## System Capability Impact
- **Capabilities Added:** separación de responsabilidades visuales y una frontera explícita para el módulo de Asistencia. No añade capacidad pedagógica nueva.
- **User Experience:** se conserva la navegación y funcionalidad; las correcciones mejoran consistencia de temas, teclado y rendimiento de listas sin alterar el flujo docente.
