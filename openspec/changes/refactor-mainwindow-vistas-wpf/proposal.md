# Change: Refactor MainWindow a vistas WPF

## Why

`MainWindow` concentraba demasiadas responsabilidades visuales y de composición para Grupo, Asistencia, Proyectos y Evaluación. Separar las superficies por módulo reduce acoplamiento, permite cambios auditables y conserva el shell como coordinador visual pequeño sin introducir un framework de navegación ni un contenedor DI.

## What Changes

- Extraer Grupo, Asistencia, Proyectos y Evaluación a `UserControl` dedicados bajo `Views/`.
- Extraer el encabezado global a `Controls/MainNavigationHeader`.
- Mantener `MainWindow` como shell que ensambla navegación, vistas, feedback global y cierre.
- Añadir `ModuloAsistenciaViewModel` como frontera explícita para las vistas diaria y mensual de Asistencia.
- Pasar dependencias específicas de una vista mediante bindings o propiedades dedicadas en vez de resolver el `MainWindowViewModel` concreto desde code-behind.
- Conservar temas, accesibilidad, teclado contextual y virtualización de las grillas después de separar las vistas.
- Mantener las tareas complejas en ventanas dedicadas y no reintroducir master-detail obligatorio.
- Mantener fuera de alcance cambios de Core, reglas de negocio, ORM, framework de navegación, Prism, ReactiveUI, CommunityToolkit o contenedor DI.

## Capabilities

### New Capabilities

- `shell-wpf-modular`: composición visual por vistas especializadas, encabezado global reutilizable y fronteras de módulo explícitas.

### Modified Capabilities

- Ninguna. El cambio reorganiza Presentation/WPF sin alterar capacidades pedagógicas ni reglas de dominio.

## Impact

- **Presentation:** `ModuloAsistenciaViewModel` y coordinación de navegación conservada en `MainWindowViewModel`.
- **App.Wpf:** `MainWindow` reducido, vistas dedicadas por módulo y encabezado global extraído.
- **Arquitectura:** code-behind de vistas no accede a SQLite ni replica reglas de negocio; las dependencias se entregan explícitamente.
- **Rendimiento:** las grillas operativas conservan virtualización y control de su propio desplazamiento.
- **Pruebas:** regresiones de composición, bindings, teclado contextual, recursos semánticos y smoke test STA del shell.