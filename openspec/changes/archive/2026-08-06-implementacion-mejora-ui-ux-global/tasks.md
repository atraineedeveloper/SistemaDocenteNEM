# Tasks: Implementación de Mejora UI/UX Global

## 1. Design system y tokens (base)

- [x] 1.1 Crear `ResourceDictionary` `Themes/DesignTokens.xaml` con tokens de color, espaciado, tipografía y elevación.
- [x] 1.2 Fusionar `DesignTokens.xaml` en `App.xaml` como diccionario combinado.
- [x] 1.3 Migrar colores hardcodeados en `App.xaml` a `DynamicResource`.
- [x] 1.4 Migrar colores hardcodeados en `MainWindow.xaml` a `DynamicResource`.
- [x] 1.5 Migrar colores hardcodeados en `ExpedienteEstudianteWindow.xaml`, `DetalleProyectoWindow.xaml` y `DetalleActividadWindow.xaml` a `DynamicResource`.
- [x] 1.6 Eliminar `Window.Resources` locales que sobrescriban estilos base sin justificación.
- [x] 1.7 Unificar `FontSize` inline a estilos semánticos (`Heading1`, `Heading2`, `FormLabel`, etc.).
- [x] 1.8 Establecer márgenes internos de 24 unidades en contenedores raíz de ventanas y diálogos.
- [x] 1.9 Crear componentes reutilizables `FormField`, `MetricCard` y `EmptyState` en `Controls/`.

## 2. Accesibilidad y foco

- [x] 2.1 Definir `FocusVisualStyle` global en `App.xaml`.
- [x] 2.2 Configurar `TabIndex` e `IsTabStop` lógicos en `MainWindow.xaml`.
- [x] 2.3 Configurar `TabIndex` e `IsTabStop` lógicos en ventanas de diálogo.
- [x] 2.4 Agregar mnemónicos (`_Guardar`, `_Cancelar`, `_Agregar`, etc.) en botones principales.
- [x] 2.5 Implementar atajos `Enter` para guardar y `Escape` para cancelar en paneles de edición.
- [x] 2.6 Corregir colores de bajo contraste (`#78A9C8`, `#B0C4DE`) a tonos con ratio >= 4.5:1.
- [x] 2.7 Configurar `AutomationProperties.Name` en iconos, tarjetas, celdas y `ComboBox`.
- [x] 2.8 Configurar `AutomationProperties.LiveSetting="Polite"` en toasts y mensajes dinámicos.

## 3. Formularios y validación

- [x] 3.1 Extender `ViewModelBase` con `INotifyDataErrorInfo`.
- [x] 3.2 Definir `Validation.ErrorTemplate` global con borde y mensaje inline.
- [x] 3.3 Aplicar `FormField` en formularios de estudiante, grupo, proyecto y actividad.
- [x] 3.4 Reemplazar `TextBox` de fechas por `DatePicker` con validación de rango.
- [x] 3.5 Agregar `EmptyState` en `DataGrid` y `ListBox` sin elementos.

## 4. Retroalimentación y estados

- [x] 4.1 Definir `INotificationService` en `SistemaDocente.Presentation`.
- [x] 4.2 Implementar `WpfNotificationService` con toasts de éxito/advertencia/error.
- [x] 4.3 Reemplazar `MessageBox` nativos por diálogos custom del sistema de diseño.
- [x] 4.4 Vincular `ProgressBar` indeterminada a la propiedad `EstaOcupado` del ViewModel.
- [x] 4.5 Agregar animaciones sutiles (150-250 ms) en hover, pressed y apertura de diálogos.

## 5. Tematización e internacionalización

- [x] 5.1 Crear `Themes/Light.xaml`, `Themes/Dark.xaml` y `Themes/HighContrast.xaml`.
- [x] 5.2 Implementar mecanismo de cambio de tema en caliente sin reinicio (`ThemeService`).
- [x] 5.3 Extraer cadenas de texto visibles a archivos `.resx` en `SistemaDocente.App.Wpf`.
- [x] 5.4 Declarar `xml:lang="es-MX"` en ventanas principales y diálogos.

## 6. Pulido UX

- [x] 6.1 Habilitar `CanUserSortColumns="True"` en grillas donde aplique.
- [x] 6.2 Implementar caja de búsqueda filtrable en la grilla de estudiantes.
- [x] 6.3 Actualizar `Title` de la ventana principal con grupo activo / vista actual.
- [x] 6.4 Agregar breadcrumb en diálogos anidados (proyecto → actividad → evaluación).
- [x] 6.5 Agregar `ToolTip` descriptivos en encabezados mensuales y columnas ambiguas.
- [x] 6.6 Eliminar código muerto identificado (`BoolToActiveTagConverter`, `OnProyectoPrincipalDobleClic`, etc.).
- [x] 6.7 Corregir `MarcarTodosEntregadaCommand`.

## 7. Validación y pruebas

- [x] 7.1 Ejecutar `openspec validate --all` y corregir cualquier issue.
- [x] 7.2 Compilar la solución con `dotnet build`.
- [x] 7.3 Ejecutar `dotnet test`.
- [x] 7.4 Verificar navegación por teclado en ventanas principales.
- [x] 7.5 Verificar contraste y nombres accesibles con inspector de accesibilidad.
