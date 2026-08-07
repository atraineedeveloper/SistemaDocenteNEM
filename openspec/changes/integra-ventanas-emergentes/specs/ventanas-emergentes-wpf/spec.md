## Purpose

Define una experiencia consistente para las ventanas secundarias WPF sin alterar reglas de negocio ni revertir la separación vigente entre entrega y nivel de logro.

## ADDED Requirements

### Requirement: FormField conserva etiqueta y contenido
`FormField` SHALL exponer una propiedad de contenido propia para el control editable y SHALL mantener la etiqueta visible independientemente del contenido hospedado.

#### Scenario: Campo de formulario con TextBox
- **WHEN** una ventana declara un `TextBox` dentro de `FormField`
- **THEN** la etiqueta y el control editable permanecen visibles simultáneamente

### Requirement: Estilos compartidos de ventanas secundarias
Las ventanas seleccionadas SHALL reutilizar `PopupStyles.xaml` para header, footer, cards y patrones visuales comunes, y SHALL consumir recursos semánticos del tema activo.

#### Scenario: Cambiar tema
- **WHEN** la aplicación cambia entre Claro, Oscuro o Alto contraste
- **THEN** las ventanas modernizadas consumen los recursos semánticos correspondientes sin depender de colores hexadecimales locales

### Requirement: Acciones primarias y destructivas diferenciadas
Detalle de proyecto y Detalle de actividad SHALL mostrar la acción de guardar como primaria y las acciones destructivas mediante un estilo visual diferenciado.

#### Scenario: Editar un proyecto
- **WHEN** se abre Detalle de proyecto
- **THEN** Guardar cambios es la acción primaria y Eliminar borrador se presenta como acción destructiva diferenciada

### Requirement: Evaluación conserva entrega y logro separados
El editor de evaluación SHALL permitir editar `EstadoEntregaActividad`, `NivelLogro` y observación sin volver al modelo legacy en el que la no entrega era un nivel de logro.

#### Scenario: Trabajo entregado pendiente de evaluación
- **WHEN** el docente marca la actividad como Entregada y todavía no asigna un nivel
- **THEN** el editor conserva `Entregada + NivelLogro.Pendiente` como estado válido

### Requirement: Editor de estudiante mantiene etiquetas y guardado rápido
El editor de estudiante SHALL mostrar etiquetas explícitas para sus campos y SHALL conservar `Ctrl+S` como atajo de guardado.

#### Scenario: Captura por teclado
- **WHEN** el docente completa el formulario y presiona `Ctrl+S`
- **THEN** se ejecuta la acción de guardar estudiante sin ocultar las etiquetas de los campos
