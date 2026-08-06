# Proposal: Implementación de Mejora UI/UX Global

## Why

La auditoría UI/UX del 2026-08-06 determinó que la aplicación WPF del Sistema Docente NEM tiene una base visual moderna pero presenta brechas importantes en accesibilidad, consistencia del sistema de diseño, validación de formularios, retroalimentación de estados y pulido general de la experiencia de usuario. Esta change implementa las mejoras necesarias para alcanzar un nivel de UX maduro y conforme con WCAG 2.1 AA.

## What Changes

- Implementar indicadores de foco accesibles y navegación completa por teclado en todas las ventanas principales.
- Corregir ratios de contraste y aplicar comunicación multimodal (color + texto/icono) en estados.
- Centralizar tokens de diseño en un `ResourceDictionary` y migrar colores/tipografías/espaciados hardcodeados a `DynamicResource`.
- Reemplazar emojis por iconos vectoriales con `AutomationProperties.Name`.
- Crear componentes reutilizables (`FormField`, `MetricCard`, `EmptyState`).
- Implementar `INotifyDataErrorInfo`, `Validation.ErrorTemplate` y `DatePicker` en formularios.
- Crear `INotificationService` con toasts accesibles, indicadores de progreso y diálogos custom.
- Agregar soporte de temas (claro/oscuro/alto contraste) y preparar recursos localizables.
- Pulir UX con ordenación de columnas, búsqueda de estudiantes, título dinámico, breadcrumbs y limpieza de código muerto.

## Capabilities

No se introducen ni modifican capabilities a nivel spec; los requisitos de comportamiento están definidos en la spec principal `openspec/specs/mejora-ui-ux-global/spec.md`. Esta change es pura implementación de dicha spec.

## Impact

- **SistemaDocente.App.Wpf**: Ventanas XAML, `App.xaml`, estilos, recursos y code-behind.
- **SistemaDocente.Presentation**: ViewModels, servicios de notificación y validación.
- **SistemaDocente.App.Wpf.Tests**: Pruebas de accesibilidad, navegación por teclado y rendimiento.
