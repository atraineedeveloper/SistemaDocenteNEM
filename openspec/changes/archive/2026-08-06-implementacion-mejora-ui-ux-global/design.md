# Design: Implementación de Mejora UI/UX Global

## Context

La aplicación WPF (`SistemaDocente.App.Wpf`) comparte estilos globales en `App.xaml` pero los diálogos y ventanas secundarias contienen colores, márgenes y recursos locales que rompen la consistencia. La capa `SistemaDocente.Presentation` contiene los ViewModels actuales sin soporte de validación por `INotifyDataErrorInfo`. Ver `proposal.md` para la motivación completa y `openspec/specs/mejora-ui-ux-global/spec.md` para los requirements de comportamiento.

## Goals / Non-Goals

**Goals:**
- Establecer un único sistema de tokens de diseño reutilizable en toda la app.
- Hacer la interfaz operable completamente por teclado y accesible para lectores de pantalla.
- Unificar la retroalimentación de errores, carga y éxito mediante servicios y componentes compartidos.
- Crear componentes reutilizables que reduzcan duplicación de XAML.
- Implementar temas claro/oscuro/alto contraste preparando la arquitectura para futuras localizaciones.

**Non-Goals:**
- No se reemplaza el framework WPF ni se migra a otra tecnología de UI.
- No se implementa sincronización en la nube ni cambios de arquitectura de persistencia.
- No se agregan nuevas funcionalidades pedagógicas fuera del alcance de UI/UX.

## Decisions

### 1. ResourceDictionary de tokens (`DesignTokens.xaml`)
- **Opción A:** Tokens en `App.xaml`. **Opción B:** Diccionario separado `Themes/DesignTokens.xaml` fusionado en `App.xaml`.
- **Elegida:** Opción B. Permite versionar y cambiar temas sin tocar `App.xaml`.
- **Tokens incluidos:** colores (`PrimaryBrush`, `TextPrimaryBrush`), espaciado (`SpacingSmall`, `SpacingMedium`, `SpacingLarge`), tipografía (`FontSizeHeading1`, etc.) y elevación/sombras.

### 2. Migración a `DynamicResource`
- Se reemplazarán colores hardcodeados por `{DynamicResource ...}` para permitir cambio de tema en caliente.
- Se eliminarán `Window.Resources` locales que sobrescriban estilos base, salvo excepciones justificadas.

### 3. Componentes reutilizables
- `FormField`: `UserControl` con `HeaderContent` (etiqueta), `Content` (campo) y `ErrorContent`.
- `MetricCard`: tarjeta con título, valor e icono.
- `EmptyState`: ilustración/icono + mensaje + acción opcional.
- Ubicación: `SistemaDocente.App.Wpf/Controls/`.

### 4. Validación de formularios
- Extender `ViewModelBase` con `INotifyDataErrorInfo`.
- Agregar `Validation.ErrorTemplate` global en `App.xaml` (borde rojo + texto inline).
- Reemplazar `TextBox` de fechas por `DatePicker` con `SelectedDate` y validación de rango.

### 5. Notificaciones y estados de carga
- Crear `INotificationService` en `SistemaDocente.Presentation` con implementación `WpfNotificationService` en `SistemaDocente.App.Wpf`.
- Toast con niveles `Success`, `Warning`, `Error`, `Info`.
- Mostrar `ProgressBar` indeterminada vinculada a `ViewModelBase.EstaOcupado`.

### 6. Iconografía
- Reemplazar emojis por `Path` vectoriales o fuente Segoe Fluent Icons (`#E700`+).
- Todos los iconos funcionales llevarán `AutomationProperties.Name`.

### 7. Temas
- Crear `Themes/Light.xaml`, `Themes/Dark.xaml` y `Themes/HighContrast.xaml`.
- Cambio de tema mediante `Application.Current.Resources.MergedDictionaries` sin reinicio.
- Declarar `xml:lang="es-MX"` en ventanas principales.

### 8. Orden de implementación
1. Design system y tokens (Fase 2) — base para todo lo demás.
2. Accesibilidad y foco (Fase 1).
3. Formularios y validación (Fase 3).
4. Retroalimentación y estados (Fase 4).
5. Tematización e i18n (Fase 5).
6. Pulido UX (Fase 6).

## Risks / Trade-offs

- **Riesgo:** Cambiar `DynamicResource` en muchos archivos puede romper estilos no auditados. **Mitigación:** validación visual manual ventana por ventana y pruebas de smoke.
- **Riesgo:** Componentes `UserControl` genéricos pueden no cubrir todos los casos edge. **Mitigación:** diseñar propiedades de dependencia flexibles (`Header`, `Content`, `Error`, `ActionCommand`).
- **Riesgo:** Temas oscuro/alto contraste requieren ajustes masivos de color. **Mitigación:** implementar primero el tema claro con tokens correctos; los otros temas serán principalmente remapeo de tokens.
- **Trade-off:** `CanUserSortColumns` activado puede conflictuar con el orden por defecto del ViewModel. **Mitigación:** permitir ordenación solo en columnas donde el ViewModel no imponga orden funcional.
