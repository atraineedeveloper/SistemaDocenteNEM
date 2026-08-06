# Proposal: Rediseño de Interfaz de Proyectos (Ventanas Dedicadas) y Módulo Independiente de Evaluación NEM

## Why

La interfaz anterior combinaba 3 columnas apretadas en una sola pantalla, generando una experiencia saturada. Se requiere separar la planeación (gestión de proyectos/actividades) de la ejecución cotidiana (evaluación pedagógica NEM en aula).

## What Changes

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
