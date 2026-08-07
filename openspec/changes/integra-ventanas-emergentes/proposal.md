# Change: Integrar modernización segura de ventanas emergentes

## Why

La rama histórica de modernización de ventanas emergentes contiene mejoras visuales útiles, pero ya no puede fusionarse directamente porque `main` incorporó Reportes y la separación explícita entre estado de entrega y nivel de logro. La integración debe rescatar únicamente las mejoras compatibles sin revertir la semántica nueva.

## What Changes

- Corregir `FormField` para que el contenido editable use una propiedad propia y no oculte su etiqueta.
- Agregar estilos compartidos para ventanas secundarias mediante `PopupStyles.xaml`.
- Modernizar Editor de estudiante, Detalle de proyecto, Detalle de actividad y diálogo de mensajes.
- Modernizar el editor de evaluación conservando `EstadoEntregaActividad` y `NivelLogro` como dimensiones separadas.
- Mantener Core, Application, Data y SQLite sin cambios.
- Añadir regresiones WPF que aseguren estilos compartidos y la semántica nueva de Evaluación.

## Capabilities

### New Capabilities

- `ventanas-emergentes-wpf`: define consistencia visual, accesibilidad y composición segura para las ventanas secundarias seleccionadas.

### Modified Capabilities

- Ninguna capacidad de negocio.

## Impact

- **App.Wpf:** cambios visuales y de composición en ventanas secundarias y `FormField`.
- **Tests:** nuevas regresiones estructurales WPF.
- **Core/Application/Data/SQLite:** sin cambios.
- **Compatibilidad:** la Evaluación conserva entrega y logro separados; no se recupera el modelo anterior de la rama histórica.
