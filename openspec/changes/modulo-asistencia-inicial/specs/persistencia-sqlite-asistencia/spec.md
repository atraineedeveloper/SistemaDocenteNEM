## Purpose

Conserva asistencias diarias completas en la base SQLite local mediante un esquema versionado, restricciones verificables y transacciones sin pérdida de datos existentes.

## ADDED Requirements

### Requirement: Evolución segura a esquema versión 2
Data SHALL usar `PRAGMA user_version = 2` para el esquema con asistencia. Una base nueva y vacía SHALL crearse directamente en versión 2; una base versión 1 SHALL validarse completamente antes de migrarse. La creación del índice auxiliar, tablas, índices y el cambio de `user_version` SHALL ocurrir en una sola transacción, estableciendo la versión 2 únicamente al final. Una versión 2 SHALL aceptarse sólo si toda su estructura es compatible.

#### Scenario: Crear base nueva
- **WHEN** se inicializa una base vacía con `user_version = 0`
- **THEN** se crean el esquema completo y la versión 2

#### Scenario: Migrar versión 1
- **WHEN** se inicializa una base versión 1 auténtica y compatible con grupos y estudiantes almacenados
- **THEN** se agregan todos los objetos de asistencia en una transacción, `user_version` pasa a 2 al final y los datos anteriores permanecen exactos

#### Scenario: Rollback de migración
- **WHEN** ocurre un fallo después de crear alguno de los objetos nuevos y antes de completar la migración
- **THEN** la base conserva `user_version = 1`, no contiene objetos parciales y no altera grupos ni estudiantes

#### Scenario: Inicialización idempotente
- **WHEN** una base compatible en versión 2 se inicializa de nuevo
- **THEN** no se recrean ni alteran sus objetos o datos

#### Scenario: Versión posterior o estructura incompatible
- **WHEN** la versión es mayor que 2 o la estructura declarada no coincide con la versión
- **THEN** Data rechaza la base sin borrarla, repararla ni recrearla

#### Scenario: Versión 0 con objetos
- **WHEN** una base con versión 0 contiene objetos preexistentes
- **THEN** Data la rechaza sin modificarla

### Requirement: Esquema relacional de asistencia
La versión 2 SHALL contener una tabla de días identificada de forma única por grupo y fecha, y una tabla de registros identificada de forma única por grupo, fecha y estudiante. SHALL incluir claves foráneas que aseguren la existencia del grupo, del día y la pertenencia del estudiante al mismo grupo, además de índices para consultar por grupo-fecha y por estudiante.

#### Scenario: Duplicado del mismo estudiante y fecha
- **WHEN** SQLite recibe dos registros para el mismo grupo, fecha y estudiante
- **THEN** la restricción única rechaza el duplicado

#### Scenario: Estudiante de otro grupo
- **WHEN** se intenta asociar al día un estudiante perteneciente a otro grupo
- **THEN** la integridad referencial rechaza la escritura

#### Scenario: Grupo o estudiante inexistente
- **WHEN** se intenta guardar una referencia inexistente con claves foráneas activas
- **THEN** SQLite rechaza la escritura

### Requirement: Representación restringida de fecha y estado
Data SHALL almacenar fechas exactamente como texto ISO `yyyy-MM-dd`. SQLite SHALL aplicar una restricción que rechace formato no canónico y fechas imposibles; Data SHALL analizarlas estrictamente mediante `DateOnly`, comprobar el recorrido canónico y rechazar, sin normalizar ni reparar, cualquier valor manipulado. Los estados SHALL almacenarse mediante valores enteros estables correspondientes exclusivamente a `Presente`, `Falta`, `Retardo` y `Justificada`.

#### Scenario: Fecha no canónica
- **WHEN** se intenta escribir una cadena vacía, un formato distinto, una fecha con hora, mes 13 o día 00
- **THEN** SQLite rechaza el valor

#### Scenario: Fecha imposible
- **WHEN** se intenta escribir 31 de febrero o 29 de febrero de un año no bisiesto
- **THEN** SQLite rechaza el valor y Data no lo normaliza

#### Scenario: Fecha bisiesta válida
- **WHEN** se escribe el 29 de febrero de un año bisiesto en formato `yyyy-MM-dd`
- **THEN** SQLite y el recorrido estricto de Data conservan exactamente la fecha

#### Scenario: Estado fuera de rango
- **WHEN** se intenta escribir un estado distinto de los cuatro valores asignados
- **THEN** SQLite rechaza el valor

### Requirement: Claves foráneas por conexión
Data SHALL ejecutar `PRAGMA foreign_keys = ON` en cada conexión que abra para inicializar, cargar, comprobar existencia o guardar.

#### Scenario: Nueva conexión
- **WHEN** Data abre una conexión operativa
- **THEN** las claves foráneas están activas antes de ejecutar operaciones

### Requirement: Carga y existencia
Data SHALL cargar una asistencia completa por `GrupoId` y fecha conservando exactamente todas las identidades y estados, devolver ausencia normal cuando no exista y distinguirla de un fallo técnico.

#### Scenario: Reapertura
- **WHEN** se guarda una asistencia, se cierran las conexiones y se abre otra instancia sobre el mismo archivo
- **THEN** la carga devuelve el mismo grupo, fecha, estudiantes y estados

#### Scenario: Fecha inexistente
- **WHEN** la pareja grupo-fecha no tiene fila de día
- **THEN** la carga devuelve ausencia y `Existe` devuelve `false`

### Requirement: Guardado transaccional completo
Data SHALL insertar o actualizar el día y todos sus registros mediante una única transacción. SHALL actualizar los estados existentes sin borrar físicamente registros históricos y SHALL confirmar sólo si toda la operación termina correctamente.

#### Scenario: Actualizar estados
- **WHEN** se vuelve a guardar una asistencia existente con estados modificados
- **THEN** existe una sola fila por estudiante y refleja el último estado confirmado

#### Scenario: Fallo real a mitad del guardado
- **WHEN** un trigger temporal ejecuta `RAISE(ABORT)` después de que parte de la operación ya se ejecutó
- **THEN** la transacción revierte el día y todos sus registros al estado anterior

#### Scenario: Registro histórico inactivo
- **WHEN** se guarda un día existente que contiene un estudiante ahora inactivo
- **THEN** su registro continúa almacenado y puede actualizar su estado sin ser eliminado físicamente

### Requirement: Archivos temporales aislados en pruebas
Las pruebas de Data SHALL usar un archivo SQLite real y único por caso, sin estado compartido ni instalación externa, y SHALL comprobar migración, restricciones, reapertura y rollback.

#### Scenario: Pruebas consecutivas
- **WHEN** dos pruebas crean datos incompatibles entre sí
- **THEN** cada una observa únicamente el contenido de su archivo temporal
