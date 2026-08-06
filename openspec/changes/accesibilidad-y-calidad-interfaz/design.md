# Design: Accesibilidad y Calidad de Interfaz Transversal

## Design Decisions

### 1. Navegación por Teclado y Mnemónicos
- Se definirá un esquema de `TabIndex` continuo de arriba a abajo y de izquierda a derecha.
- Los botones principales incluirán la tecla de acceso directo mediante guion bajo en el contenido (e.g. `_Guardar estudiante`, `_Agregar estudiante`, `_Ver Expediente`).
- El panel de edición de estudiante soportará la tecla `Enter` para guardar cambios y `Escape` para cancelar la edición.

### 2. Contraste de Color y Comunicación Multimodal
- Todos los colores de texto principales utilizarán un tono `#1D2939` sobre fondo blanco/gris claro `#F8F9FA`, superando el estándar WCAG AA de contraste 4.5:1.
- Para los estados de asistencia y de activación de alumnos, se mantendrá el color descriptivo acompañado siempre de un texto explícito (e.g. `[FALTA]`, `[RETARDO]`, `[PRESENTE]`, `[INACTIVO]`), garantizando que la información no dependa únicamente del color.

### 3. Adaptabilidad a Escalado y Resoluciones Reducidas
- La ventana principal establecerá un `MinHeight="600"` y `MinWidth="900"`.
- Todos los formularios y grillas estarán contenidos en `ScrollViewer` con barras de desplazamiento visibles cuando el tamaño de la ventana o el escalado del sistema operativo (125%, 150%) reduzca el espacio disponible.

### 4. Rendimiento con Grupos de 40 Estudiantes
- Las grillas DataGrid continuarán utilizando la virtualización UI nativa de WPF (`VirtualizingStackPanel.IsVirtualizing="True"`), permitiendo una respuesta fluida instantánea en listas largas de 40 a 50 estudiantes por grupo.

## Risks and Trade-offs
- **Virtualización WPF:** Al activar virtualización en DataGrid, se deben mantener alturas de renglón estables para evitar saltos en el scroll.
- **Teclas de acceso rápido:** Evitar conflictos entre atajos globales de Windows y mnemónicos de la aplicación.
