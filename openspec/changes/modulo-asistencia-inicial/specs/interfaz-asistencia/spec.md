## Purpose

Permite que un docente capture y corrija asistencia desde WPF con interacción segura, eficiente y coherente con el último estado confirmado por persistencia.

## ADDED Requirements

### Requirement: Navegación mínima entre módulos
La ventana principal SHALL ofrecer navegación directa entre gestión del grupo y asistencia sin introducir un framework de navegación. La lógica visual SHALL depender de Application; sólo la raíz de composición SHALL crear implementaciones de Data.

#### Scenario: Abrir asistencia
- **WHEN** el docente activa la opción Asistencia
- **THEN** la ventana muestra el módulo de asistencia del grupo actual sin exponer identidades ni detalles de SQLite

#### Scenario: Volver a gestión
- **WHEN** no hay cambios pendientes y se activa la opción Grupo
- **THEN** se muestra de nuevo la gestión del grupo en la misma ventana

### Requirement: Selección de fecha
El módulo SHALL iniciar con la fecha local actual, permitir seleccionar otra fecha válida mediante un control de fecha y preparar el día seleccionado sin persistirlo por el solo hecho de abrirlo.

#### Scenario: Primera apertura
- **WHEN** se abre el módulo
- **THEN** se selecciona hoy según la hora local y se muestran los estudiantes del día sin ejecutar un guardado

#### Scenario: Fecha no seleccionada
- **WHEN** el control no contiene una fecha válida
- **THEN** no se prepara ni guarda asistencia y se muestra un mensaje claro

### Requirement: Captura tabular eficiente
La interfaz SHALL mostrar una fila editable por integrante del padrón devuelto por Application, conservando ese orden, con número de lista, nombre y los cuatro estados disponibles. SHALL presentar `Justificada` con el texto «Falta justificada». No SHALL mostrar identidades ni abrir una ventana por estudiante y SHALL ser utilizable con teclado para grupos de 30 a 40 filas.

#### Scenario: Cambiar estado con teclado
- **WHEN** el docente recorre y modifica filas mediante teclado
- **THEN** puede completar la captura sin cambiar de ventana

#### Scenario: Estudiantes inactivos
- **WHEN** un día persistido contiene a un estudiante actualmente inactivo
- **THEN** aparece editable con el texto «Inactivo actualmente» sin depender sólo del color

#### Scenario: Día nuevo excluye inactivos
- **WHEN** se prepara un día todavía no guardado
- **THEN** aparecen únicamente los estudiantes actualmente activos

#### Scenario: Grupo de tamaño habitual
- **WHEN** el padrón visible contiene entre 30 y 40 filas
- **THEN** todas pueden recorrerse y editarse en la misma tabla mediante teclado

### Requirement: Indicador guardado y cambios pendientes
El ViewModel SHALL mantener el último snapshot confirmado y una copia editable, compararlos y exponer de forma clara si el día nunca se ha guardado, está guardado sin cambios o tiene cambios pendientes. Un día nuevo SHALL tener `EsPersistido = false` y se considerará pendiente aunque todas sus filas estén en Presente. Guardar SHALL estar deshabilitado únicamente para un día persistido sin diferencias.

#### Scenario: Día nuevo sin edición
- **WHEN** se prepara una fecha ausente con estados `Presente`
- **THEN** se indica que aún no está guardada y Guardar está habilitado para confirmar por primera vez

#### Scenario: Día guardado sin cambios
- **WHEN** el estado editable coincide con el último snapshot persistido
- **THEN** se indica guardado y Guardar está deshabilitado

#### Scenario: Revertir edición local
- **WHEN** el docente modifica un estado y luego lo devuelve al valor confirmado
- **THEN** el día deja de aparecer como modificado

### Requirement: Guardado visualmente atómico
Una acción Guardar, incluido `Ctrl+S`, SHALL enviar una sola operación completa a Application, deshabilitar acciones incompatibles mientras se ejecuta y actualizar el snapshot confirmado sólo después del éxito.

#### Scenario: Guardado exitoso
- **WHEN** Application confirma el guardado
- **THEN** la lista, conteos e indicador se actualizan con el snapshot devuelto

#### Scenario: Fallo de persistencia
- **WHEN** Application informa un error al guardar
- **THEN** se muestra un mensaje técnico seguro, se conservan la entrada editable y el último snapshot confirmado, y no se presenta el cambio como guardado

### Requirement: Confirmación de cambios pendientes
Al cambiar de fecha, navegar específicamente a Grupo o cerrar con cambios pendientes —incluido un borrador nunca guardado— la interfaz SHALL reutilizar el mismo flujo `Guardar`, `Descartar` o `Cancelar`. Guardar SHALL continuar únicamente si persiste con éxito; Descartar SHALL abandonar la copia editable sin guardar y continuar; Cancelar SHALL mantener íntegramente la fecha, módulo, ventana y edición actuales.

#### Scenario: Guardar antes de cambiar fecha
- **WHEN** se elige Guardar y el guardado termina correctamente
- **THEN** se carga la fecha solicitada

#### Scenario: Fallo al guardar antes de salir
- **WHEN** se elige Guardar y la persistencia falla
- **THEN** no se cambia de fecha, no se navega y no se cierra la ventana

#### Scenario: Descartar cambios
- **WHEN** se elige Descartar
- **THEN** no se guarda, se abandona la edición local y continúa la acción solicitada

#### Scenario: Cancelar salida
- **WHEN** se elige Cancelar
- **THEN** se conserva íntegramente la edición actual

### Requirement: Acciones masivas y conteos
La interfaz SHALL permitir marcar todas las filas históricas visibles como `Presente` y SHALL mostrar total y conteos de `Presente`, `Falta`, `Retardo` y «Falta justificada», actualizados inmediatamente con el estado visual. Los conteos SHALL incluir todas las filas visibles, aunque algún estudiante esté actualmente inactivo.

#### Scenario: Marcar todos presentes
- **WHEN** el docente activa Marcar todos presentes
- **THEN** todas las filas editables cambian a `Presente`, los conteos se recalculan y se actualiza la condición de cambios pendientes

#### Scenario: Cambiar una fila
- **WHEN** una fila pasa de Presente a Falta
- **THEN** disminuye Presente, aumenta Falta y el total permanece igual

### Requirement: Tratamiento seguro de errores
El ViewModel SHALL distinguir validación de dominio, conflicto y error de persistencia, SHALL mostrar mensajes claros en español mediante un servicio de diálogo y SHALL impedir que esas excepciones cierren la aplicación. El code-behind SHALL limitarse a conducta visual y cierre de ventana.

#### Scenario: Error de dominio
- **WHEN** Application rechaza una operación por validación o conflicto
- **THEN** la ventana permanece abierta, conserva la edición y muestra una explicación adecuada

### Requirement: Presentación comprobable sin WPF real
Las decisiones del ViewModel SHALL poder probarse con dobles manuales de casos de uso, diálogo y reloj, sin abrir ventanas ni cargar Data, Microsoft.Data.Sqlite o controles WPF reales.

#### Scenario: Ejecutar pruebas de ViewModel
- **WHEN** se prueban selección, edición, guardado, confirmaciones y errores
- **THEN** las pruebas se ejecutan sin crear una ventana ni una base SQLite
