## Purpose

Coordina la preparación, consulta y persistencia completa de una asistencia diaria sin exponer agregados, infraestructura ni estado mutable entre llamadas.

## ADDED Requirements

### Requirement: Puerto específico de asistencia
Application SHALL definir un contrato específico que permita cargar una asistencia por grupo y fecha, comprobar su existencia y guardar una asistencia completa. El contrato SHALL usar únicamente tipos de Core y no SHALL exponer SQLite, SQL ni tipos visuales.

#### Scenario: Ausencia real
- **WHEN** el puerto consulta una pareja grupo-fecha que no está almacenada
- **THEN** la carga devuelve ausencia y la comprobación de existencia devuelve `false`

#### Scenario: Error técnico al comprobar existencia
- **WHEN** la infraestructura falla durante la comprobación
- **THEN** el error se conserva como fallo identificable y no se convierte en `false`

### Requirement: Snapshots inmutables de asistencia
Application SHALL devolver records inmutables para el detalle del día y de cada estudiante. El detalle SHALL incluir grupo, fecha, `EsPersistido` y una lista de filas; cada fila SHALL incluir `EstudianteId`, nombre visible y número de lista obtenidos de la matrícula actual, estado de asistencia e indicador de actividad actual. Las listas SHALL materializarse como arreglos nuevos y no SHALL exponer `Grupo`, `Estudiante`, `AsistenciaDiaria`, `RegistroAsistencia` ni colecciones internas.

#### Scenario: Obtener detalle
- **WHEN** se obtiene un snapshot de asistencia
- **THEN** modificar una colección externa no altera el estado de dominio ni snapshots posteriores

### Requirement: Preparar un día no guardado
El caso de uso de preparación SHALL cargar una instancia fresca del grupo y consultar la asistencia de la fecha. Si no existe, SHALL crear en memoria una fila `Presente` por cada estudiante actualmente activo, en el orden proporcionado por Application, marcar el resultado como no guardado y no invocar `Guardar`.

#### Scenario: Primera apertura de una fecha
- **WHEN** el grupo existe y la fecha no tiene asistencia
- **THEN** se devuelven sólo sus estudiantes activos con estado `Presente`, indicador no guardado y cero guardados

#### Scenario: Grupo inexistente
- **WHEN** se prepara una fecha para un grupo ausente
- **THEN** se lanza `GrupoNoEncontradoException` y no se guarda asistencia

### Requirement: Cargar un día guardado
Al preparar una fecha ya guardada, Application SHALL cargar el agregado histórico completo y devolver todos sus estados confirmados. SHALL obtener nombre, número de lista y situación activa desde la matrícula actual, sin pretender reconstruir una fotografía histórica de esos datos. Cada fila SHALL indicar si el estudiante está actualmente activo.

#### Scenario: Estudiante desactivado después del guardado
- **WHEN** se abre un día guardado que contiene a un estudiante ahora inactivo
- **THEN** su registro histórico aparece, indica que está actualmente inactivo y continúa siendo editable

#### Scenario: Estudiante agregado después del guardado
- **WHEN** se abre un día guardado anterior a la incorporación de un estudiante actualmente activo
- **THEN** el estudiante nuevo no se agrega automáticamente al día histórico

#### Scenario: Estudiante reactivado
- **WHEN** se abre un día guardado que contiene a un estudiante posteriormente desactivado y reactivado
- **THEN** se muestra el mismo registro histórico con su identidad y estado conservados

### Requirement: Guardado completo y único
El comando de guardado SHALL recibir grupo, fecha y entradas con identidad y estado, cargar estado fresco, validar el conjunto completo mediante Core y llamar a `Guardar` exactamente una vez sólo después del éxito de todas las operaciones. SHALL rechazar identidades faltantes, duplicadas o ajenas, devolver el snapshot confirmado únicamente después del guardado y no SHALL conservar agregados entre llamadas.

#### Scenario: Guardar día nuevo
- **WHEN** la fecha no está guardada y existe exactamente una entrada válida por cada estudiante actualmente activo
- **THEN** se crea el agregado con ese padrón, se guarda exactamente una vez y se devuelve como persistido

#### Scenario: Actualizar día existente
- **WHEN** existe exactamente una entrada válida por cada fila del padrón histórico mostrado
- **THEN** se actualizan todos los estados sobre la misma instancia, se conservan grupo, fecha e identidades y se realiza un único guardado final sin eliminar registros

#### Scenario: Conjunto incompleto o ajeno
- **WHEN** la entrada omite una identidad esperada, la duplica o contiene una identidad ajena
- **THEN** la operación falla antes de guardar y no produce cambios parciales

#### Scenario: Error de dominio durante la actualización
- **WHEN** cualquiera de los estados o mutaciones del agregado es rechazado por Core
- **THEN** se realizan cero guardados y permanece confirmado el estado persistido anterior

#### Scenario: Fallo al guardar
- **WHEN** la persistencia falla durante el único guardado
- **THEN** no se devuelve éxito y una llamada posterior vuelve a cargar el último estado persistido

### Requirement: Consultar existencia sin efectos laterales
Application SHALL permitir comprobar si existe una asistencia para grupo y fecha sin crearla, guardarla ni mantenerla en memoria.

#### Scenario: Consultar fecha ausente
- **WHEN** se consulta una fecha sin asistencia
- **THEN** se devuelve `false` y no se invoca ningún guardado

### Requirement: Orden determinista
Las filas SHALL ordenarse primero por número de lista, después por nombre visible y finalmente por `EstudianteId` como desempate determinista.

#### Scenario: Número y nombre coincidentes
- **WHEN** dos filas tienen el mismo número y nombre visible
- **THEN** su orden se resuelve de forma estable mediante `EstudianteId`

### Requirement: Frontera única de errores
El adaptador de Data SHALL traducir sus errores técnicos a `ErrorPersistenciaAplicacionException`, conservando la causa como `InnerException`. Los casos de uso no SHALL volver a envolver esa excepción ni las excepciones de dominio.

#### Scenario: Error de infraestructura
- **WHEN** Data no puede cargar o guardar una asistencia
- **THEN** Application entrega un error de persistencia identificable con la causa técnica original

#### Scenario: Error de dominio
- **WHEN** Core rechaza un estado o snapshot
- **THEN** la excepción de dominio se conserva y no se invoca el guardado
