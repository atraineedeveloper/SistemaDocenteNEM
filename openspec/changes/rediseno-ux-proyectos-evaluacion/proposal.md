# Proposal: Rediseño de Interfaz de Proyectos (Ventanas Dedicadas) y Módulo Independiente de Evaluación NEM

## Intent

Separar la planeación y gestión de proyectos didácticos de la evaluación de actividades con alumnos en el aula, eliminando el amontonamiento de tres zonas en una sola pantalla. La nueva experiencia proporciona una lista limpia de proyectos en pantalla completa, ventanas dedicadas para ver y editar los detalles del proyecto y sus actividades, y una nueva pestaña superior dedicada exclusivamente a la Evaluación de Desempeño NEM a pantalla completa.

## Scope

1. **Presentation Layer:**
   - Crear `EvaluacionActividadesViewModel` para la gestión y filtrado síncrono del flujo de evaluación por proyecto y actividad.
   - Actualizar `GestionProyectosViewModel` para enfocarse en la administración y selección de proyectos/actividades en ventanas dedicadas.
   - Actualizar `MainWindowViewModel` agregando la navegación a la nueva pestaña `Evaluación`.

2. **WPF UI Layer (`SistemaDocente.App.Wpf`):**
   - Incorporar el botón `Evaluación` en la barra superior de `MainWindow.xaml`.
   - Crear `DetalleProyectoWindow.xaml` y `DetalleProyectoWindow.xaml.cs` para ver/editar datos del proyecto y la lista de sus actividades.
   - Crear `DetalleActividadWindow.xaml` y `DetalleActividadWindow.xaml.cs` para ver/editar detalles de una actividad.
   - Rediseñar el panel de Proyectos en `MainWindow.xaml` para mostrar la lista limpia de proyectos en espacio amplio.
   - Crear el panel completo dedicado para Evaluación en `MainWindow.xaml` con selectores superiores de Proyecto/Actividad y la grilla maximizada con atajos D/S/E/R/N/P.

3. **Pruebas y Verificación:**
   - Pruebas unitarias de ViewModels para el módulo de evaluación y la apertura de diálogos/ventanas.
   - Pruebas de integración de interfaz WPF para bindings y navegación.
