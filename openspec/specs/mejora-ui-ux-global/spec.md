# mejora-ui-ux-global Specification

## Purpose

Establece el contrato de comportamiento para la mejora integral del diseño UI/UX de la aplicación WPF del Sistema Docente NEM, abordando accesibilidad (WCAG 2.1 AA), consistencia del sistema de diseño, validación de formularios, retroalimentación de estados, tematización, internacionalización y pulido de la experiencia de usuario en todas las ventanas y vistas auditadas.

## Requirements

### Requirement: FOCUS_VISUAL_GLOBAL

El sistema SHALL definir un `FocusVisualStyle` global en `App.xaml` que proporcione un indicador de foco visible y coherente para todos los controles interactivos de la aplicación WPF.

#### Scenario: Foco visible en botón

- **WHEN** el usuario navega hasta un botón mediante la tecla `Tab`
- **THEN** el botón muestra un indicador de foco claro definido por el estilo global

#### Scenario: Foco visible en campo de texto

- **WHEN** el usuario navega hasta un `TextBox` mediante la tecla `Tab`
- **THEN** el campo muestra un borde o indicador de foco visible y coherente con el sistema de diseño

### Requirement: CONTRASTE_WCAG_AA

El sistema SHALL garantizar que todo el texto y los elementos interactivos cumplan con un ratio de contraste mínimo de 4.5:1 según WCAG 2.1 AA, reemplazando los colores de bajo contraste identificados en la auditoría.

#### Scenario: Texto sobre fondo claro

- **WHEN** se muestra texto principal sobre fondo claro en cualquier ventana
- **THEN** el ratio de contraste entre texto y fondo es al menos 4.5:1

#### Scenario: Estados de asistencia legibles

- **WHEN** se muestran indicadores de asistencia con color de estado
- **THEN** el texto o icono asociado mantiene un ratio de contraste mínimo de 4.5:1 respecto a su fondo

### Requirement: AUTOMATION_PROPERTIES_NAME

El sistema SHALL configurar `AutomationProperties.Name` en todos los elementos no textuales que transmiten información, incluyendo iconos, tarjetas, celdas personalizadas, `ComboBox` y botones con contenido gráfico.

#### Scenario: Icono con nombre accesible

- **WHEN** un icono o indicador visual representa una acción o estado
- **THEN** el lector de pantalla anuncia el `AutomationProperties.Name` asociado

#### Scenario: Tarjeta con nombre accesible

- **WHEN** el usuario navega mediante lector de pantalla sobre una tarjeta de contenido (`ContentCard` / `SectionCard`)
- **THEN** la tarjeta expone un nombre descriptivo de su contenido o propósito

### Requirement: KEYBOARD_NAVIGATION

El sistema SHALL permitir operar todas las funciones principales mediante teclado, estableciendo una secuencia lógica de `TabIndex` e `IsTabStop`, y proporcionando atajos de teclado para acciones frecuentes como guardar, cancelar, agregar y cerrar.

#### Scenario: Navegación lógica con Tab

- **WHEN** el usuario presiona `Tab` repetidamente en una ventana de edición
- **THEN** el foco se desplaza en orden lógico de arriba hacia abajo y de izquierda a derecha

#### Scenario: Guardar con Enter

- **WHEN** el usuario presiona `Enter` en un formulario de edición válido
- **THEN** el sistema ejecuta la acción de guardado predeterminada

#### Scenario: Cancelar con Escape

- **WHEN** el usuario presiona `Escape` en un diálogo de edición
- **THEN** el sistema cancela la operación y cierra el diálogo sin guardar

### Requirement: LIVE_REGIONS

El sistema SHALL marcar los mensajes dinámicos (toasts, `MensajeEdicion`, banners de error) como regiones vivas (`AutomationProperties.LiveSetting="Polite"`) para que los lectores de pantalla los anuncien cuando aparecen.

#### Scenario: Toast anunciado por lector de pantalla

- **WHEN** se muestra un toast de éxito o error
- **THEN** el lector de pantalla anuncia el contenido del toast

#### Scenario: Mensaje de edición anunciado

- **WHEN** cambia el texto del mensaje contextual de edición
- **THEN** el lector de pantalla notifica el nuevo mensaje sin robar el foco


### Requirement: DESIGN_TOKENS

El sistema SHALL crear un `ResourceDictionary` centralizado de tokens de diseño (colores, espaciado, tipografía, elevación) que sea referenciado globalmente y sirva como única fuente de verdad para los estilos visuales.

#### Scenario: Token de color reutilizado

- **WHEN** un estilo necesita aplicar el color primario de la aplicación
- **THEN** el estilo referencia el token de color del `ResourceDictionary` en lugar de un color hardcodeado

#### Scenario: Token de espaciado reutilizado

- **WHEN** un control define márgenes internos o externos
- **THEN** los valores provienen de los tokens de espaciado definidos en el sistema de diseño

### Requirement: DYNAMIC_RESOURCES

El sistema SHALL migrar todos los colores, tamaños de fuente y espaciados hardcodeados en XAML y code-behind a referencias `DynamicResource` que apunten a los tokens del sistema de diseño.

#### Scenario: Color hardcodeado reemplazado

- **WHEN** se inspecciona cualquier archivo XAML de ventanas o controles
- **THEN** no se encuentran valores de color hardcodeados que deban formar parte del sistema de diseño

#### Scenario: Estilo dinámico en diálogo

- **WHEN** un diálogo modal carga sus recursos
- **THEN** utiliza `DynamicResource` para obtener colores y espaciados del tema activo sin sobrescribir estilos base locales

### Requirement: SEMANTIC_TYPOGRAPHY

El sistema SHALL unificar todos los tamaños de fuente inline a estilos tipográficos semánticos (`Heading1`, `Heading2`, `Heading3`, `FormLabel`, `Caption`, `SectionSubtitle`) definidos en `App.xaml`.

#### Scenario: Encabezado con estilo semántico

- **WHEN** se muestra un título de sección
- **THEN** aplica el estilo `Heading1` o `Heading2` correspondiente en lugar de un `FontSize` inline

#### Scenario: Etiqueta de formulario con estilo semántico

- **WHEN** se muestra una etiqueta de campo de formulario
- **THEN** aplica el estilo `FormLabel` definido en el sistema de diseño

### Requirement: FLUENT_ICONS

El sistema SHALL reemplazar todos los emojis usados como iconos por elementos vectoriales (`Path`) o la fuente Segoe Fluent Icons, acompañados de `AutomationProperties.Name` descriptivo.

#### Scenario: Icono de acción sin emoji

- **WHEN** un botón o indicador requiere un icono
- **THEN** se representa mediante un `Path` o glifo de icono vectorial, sin emojis

#### Scenario: Icono con accesibilidad

- **WHEN** un lector de pantalla enfoca un icono funcional
- **THEN** anuncia el nombre descriptivo configurado en `AutomationProperties.Name`

### Requirement: UNIFIED_MARGENS_VENTANA

El sistema SHALL establecer márgenes internos consistentes de 24 unidades en el contenedor raíz de todas las ventanas y diálogos, eliminando márgenes irregulares entre vistas.

#### Scenario: Margen consistente en ventana principal

- **WHEN** se muestra la ventana principal
- **THEN** el contenido principal tiene un margen interno de 24 unidades respecto al borde de la ventana

#### Scenario: Margen consistente en diálogo modal

- **WHEN** se muestra un diálogo modal de detalle
- **THEN** el contenido del diálogo respeta el margen interno de 24 unidades sin sobrescribir el padding global

### Requirement: REUSABLE_COMPONENTS

El sistema SHALL crear `UserControl` reutilizables para elementos de interfaz recurrentes: `FormField` (etiqueta + campo + mensaje de validación), `MetricCard` (tarjeta de métrica) y `EmptyState` (estado vacío).

#### Scenario: Campo de formulario reutilizable

- **WHEN** se requiere mostrar un campo de entrada con etiqueta y validación
- **THEN** se utiliza el componente `FormField` en lugar de repetir la estructura XAML

#### Scenario: Estado vacío reutilizable

- **WHEN** una lista o grilla no contiene elementos

### Requirement: DATA_ERROR_INFO

El sistema SHALL implementar `INotifyDataErrorInfo` en el `ViewModelBase` de la capa de presentación para permitir validaciones asíncronas y notificación de errores por propiedad.

#### Scenario: Validación con error

- **WHEN** el usuario ingresa un valor inválido en un campo obligatorio
- **THEN** el ViewModel reporta el error mediante `INotifyDataErrorInfo.ErrorsChanged`

#### Scenario: Limpieza de error

- **WHEN** el usuario corrige el valor inválido
- **THEN** el ViewModel limpia el error y notifica que la propiedad ya no tiene errores

### Requirement: VALIDATION_ERROR_TEMPLATE

El sistema SHALL definir un `Validation.ErrorTemplate` global que muestre un borde distintivo y un mensaje de error inline junto al campo con error.

#### Scenario: Campo con error visual

- **WHEN** una propiedad del ViewModel reporta un error de validación
- **THEN** el campo correspondiente muestra un borde de error y un mensaje inline

#### Scenario: Mensaje de error contextual

- **WHEN** se produce un error de validación
- **THEN** el mensaje indica claramente qué campo falló y por qué, sin depender de cuadros de diálogo genéricos

### Requirement: DATE_PICKER_FECHAS

El sistema SHALL reemplazar los `TextBox` usados para capturar fechas por controles `DatePicker` que ofrezcan selección de fecha y validación integrada.

#### Scenario: Selección de fecha

- **WHEN** el usuario necesita ingresar una fecha en un formulario
- **THEN** el control es un `DatePicker` con formato localizado

#### Scenario: Fecha inválida

- **WHEN** el usuario selecciona o escribe una fecha fuera del rango permitido
- **THEN** el sistema muestra un error de validación contextual

### Requirement: EMPTY_STATES_DATAGRID

El sistema SHALL mostrar estados vacíos (`EmptyState`) cuando un `DataGrid` o `ListBox` no contenga elementos, en lugar de dejar la superficie en blanco.

#### Scenario: Lista sin estudiantes

- **WHEN** un grupo no tiene estudiantes registrados
- **THEN** el `DataGrid` muestra el componente `EmptyState` con instrucciones para agregar el primer estudiante

#### Scenario: Proyectos sin actividades

- **WHEN** un proyecto no tiene actividades
- **THEN** la lista de actividades muestra un estado vacío con mensaje descriptivo


### Requirement: NOTIFICATION_SERVICE

El sistema SHALL implementar un `INotificationService` que muestre toasts de éxito, advertencia y error con diseño coherente al sistema de diseño, accesibles mediante `AutomationProperties.LiveSetting`.

#### Scenario: Toast de éxito

- **WHEN** se completa una operación exitosa (guardar estudiante, crear grupo)
- **THEN** se muestra un toast verde con icono, mensaje descriptivo y opción de descarte

#### Scenario: Toast de error

- **WHEN** ocurre un error de dominio o infraestructura
- **THEN** se muestra un toast rojo con mensaje claro y, cuando aplique, acción de reintentar

#### Scenario: Toast accesible

- **WHEN** aparece cualquier toast
- **THEN** los lectores de pantalla anuncian el mensaje mediante `LiveSetting="Polite"`

### Requirement: PROGRESS_BUSY

El sistema SHALL mostrar una indicación visual de progreso (`ProgressBar` indeterminada o spinner) cuando el `ViewModel` reporte estado ocupado (`EstaOcupado`).

#### Scenario: Carga de datos

- **WHEN** el ViewModel inicia una operación de carga prolongada
- **THEN** la interfaz muestra una `ProgressBar` indeterminada en la zona afectada

#### Scenario: Operación finalizada

- **WHEN** la operación de carga termina
- **THEN** el indicador de progreso desaparece y se habilita la interacción

### Requirement: CUSTOM_DIALOGS

El sistema SHALL reemplazar los `MessageBox` nativos de Windows por diálogos personalizados que apliquen el sistema de diseño, iconografía consistente y botones con acciones claras.

#### Scenario: Diálogo de confirmación

- **WHEN** el sistema requiere confirmar una acción destructiva
- **THEN** muestra un diálogo custom con título, mensaje, icono y botones primario/secundario alineados al diseño

#### Scenario: Diálogo de error

- **WHEN** ocurre un error crítico que requiere atención del usuario
- **THEN** muestra un diálogo custom con mensaje descriptivo y botón de aceptación

### Requirement: SUBTLE_ANIMATIONS

El sistema SHALL aplicar animaciones sutiles (duración entre 150 ms y 250 ms) en transiciones de estado como `hover`, `pressed`, aparición de diálogos y cambio de pestañas.

#### Scenario: Transición de hover

- **WHEN** el cursor entra en un botón o tarjeta interactiva
- **THEN** el cambio visual se realiza mediante una animación de 150-250 ms

#### Scenario: Aparición de diálogo

- **WHEN** se abre un diálogo modal
- **THEN** la ventana aparece con una animación de fade o escala de duración corta


### Requirement: THEME_DICTIONARY

El sistema SHALL proporcionar un `ThemeDictionary` que soporte al menos los temas claro, oscuro y de alto contraste, permitiendo cambiar la apariencia global sin reiniciar la aplicación.

#### Scenario: Cambio a tema oscuro

- **WHEN** el usuario selecciona el tema oscuro en la configuración
- **THEN** todos los controles actualizan sus colores mediante `DynamicResource` sin requerir reinicio

#### Scenario: Tema de alto contraste

- **WHEN** Windows está en modo de alto contraste
- **THEN** la aplicación respeta o proporciona un tema `HighContrast` accesible

### Requirement: LOCALIZED_RESOURCES

El sistema SHALL extraer todas las cadenas de texto visibles del código XAML y C# a archivos de recursos `.resx`, eliminando los textos hardcodeados en español.

#### Scenario: Texto de botón desde recurso

- **WHEN** se muestra un botón con texto estático
- **THEN** el contenido proviene de un recurso localizado, no de una cadena inline

#### Scenario: Mensaje de error desde recurso

- **WHEN** se muestra un mensaje de error al usuario
- **THEN** el mensaje se obtiene del archivo de recursos correspondiente

### Requirement: XML_LANG

El sistema SHALL declarar `xml:lang` en cada ventana principal y diálogo modal para indicar el idioma de la interfaz a lectores de pantalla y motores de búsqueda.

#### Scenario: Idioma declarado en ventana principal

- **WHEN** se carga `MainWindow.xaml`
- **THEN** el elemento raíz contiene el atributo `xml:lang="es-MX"` u otro idioma activo

#### Scenario: Idioma declarado en diálogo

- **WHEN** se abre un diálogo modal
- **THEN** el elemento raíz del diálogo declara `xml:lang` con el idioma activo


### Requirement: SORT_COLUMNS

El sistema SHALL habilitar `CanUserSortColumns="True"` en las grillas de datos donde ordenar columnas aporte valor a la experiencia del usuario, manteniendo la ordenación por defecto definida por el ViewModel.

#### Scenario: Ordenar por nombre

- **WHEN** el usuario hace clic en el encabezado de la columna de nombre
- **THEN** las filas del `DataGrid` se ordenan ascendentemente por ese campo

#### Scenario: Ordenar por número de lista

- **WHEN** el usuario hace clic en el encabezado de la columna número de lista
- **THEN** las filas se ordenan numéricamente respetando el tipo de dato

### Requirement: SEARCH_STUDENTS

El sistema SHALL proporcionar una caja de búsqueda en la grilla de estudiantes que filtre resultados por nombre o número de lista mientras el usuario escribe.

#### Scenario: Filtrar por nombre

- **WHEN** el usuario escribe texto en la caja de búsqueda de estudiantes
- **THEN** la grilla muestra únicamente los estudiantes cuyo nombre contiene el texto

#### Scenario: Sin resultados

- **WHEN** la búsqueda no coincide con ningún estudiante
- **THEN** se muestra el componente `EmptyState` indicando que no hay coincidencias

### Requirement: DYNAMIC_TITLE

El sistema SHALL actualizar el título de la ventana principal (`Title`) para reflejar el grupo activo o la vista actual, mejorando la orientación del usuario.

#### Scenario: Título con grupo activo

- **WHEN** el usuario selecciona un grupo
- **THEN** el título de la ventana incluye el nombre del grupo activo

#### Scenario: Título en vista de evaluación

- **WHEN** el usuario cambia a la pestaña de evaluación
- **THEN** el título refleja que se encuentra en el módulo de evaluación

### Requirement: BREADCRUMB_DIALOGS

El sistema SHALL mostrar un breadcrumb en los diálogos anidados (proyecto → actividad → evaluación) para indicar la ubicación actual y permitir navegar hacia niveles superiores.

#### Scenario: Breadcrumb en detalle de actividad

- **WHEN** el usuario abre el detalle de una actividad desde un proyecto
- **THEN** el diálogo muestra la ruta `Proyecto > Actividad` en un breadcrumb

#### Scenario: Navegación mediante breadcrumb

- **WHEN** el usuario hace clic en un nivel superior del breadcrumb
- **THEN** el sistema navega al diálogo o vista correspondiente

### Requirement: CLEAN_DEAD_CODE

El sistema SHALL eliminar el código muerto identificado en la auditoría (conversores sin uso, manejadores de eventos obsoletos, comandos con bugs como `MarcarTodosEntregadaCommand`) y corregir los errores de nomenclatura o lógica encontrados.

#### Scenario: Conversor sin uso eliminado

- **WHEN** se revisan los recursos y code-behind de la aplicación
- **THEN** no se encuentran conversores declarados que no sean referenciados

#### Scenario: Comando corregido

- **WHEN** se invoca el comando de marcar todas las actividades como entregadas
- **THEN** ejecuta la acción correcta con el nombre y valor apropiados

### Requirement: TOOLTIP_HEADERS

El sistema SHALL agregar `ToolTip` descriptivos en los encabezados mensuales y otros encabezados de columna cuyo significado pueda no ser evidente para el usuario.

#### Scenario: Tooltip en encabezado mensual

- **WHEN** el usuario coloca el cursor sobre un encabezado de mes en la grilla
- **THEN** se muestra un tooltip con la descripción del período o acciones disponibles

