# gestion-grupo-estudiantes Specification

## Purpose
Establece el contrato mínimo para representar grupos escolares y estudiantes en Core, con identidad interna, estado reversible y reglas verificables de nombres, números de lista y consulta de estudiantes activos.
## Requirements
### Requirement: Grupo escolar con identidad y nombre visible
El sistema SHALL representar cada grupo mediante un `GrupoId` opaco y fuertemente tipado, basado en un `Guid` generado por Core, un nombre visible obligatorio y una colección propia de estudiantes. El consumidor MUST NOT proporcionar la identidad al crear un grupo. El modelo SHALL NOT incorporar todavía grado, grupo, turno, escuela ni ciclo escolar como datos separados, ni rutas de reconstrucción desde persistencia.

#### Scenario: Crear un grupo válido
- **WHEN** se crea un grupo con un nombre visible válido
- **THEN** Core genera su `GrupoId` sin recibirlo del consumidor y almacena el nombre normalizado

#### Scenario: Identidades de grupo distintas
- **WHEN** se crean dos grupos válidos
- **THEN** cada grupo recibe un `GrupoId` distinto y fuertemente tipado

### Requirement: Normalización y validación del nombre del grupo
El sistema SHALL quitar espacios iniciales y finales del nombre del grupo, SHALL reducir cada secuencia interna de espacios a un solo espacio y SHALL validar el resultado normalizado. El nombre normalizado MUST contener al menos un carácter y MUST tener como máximo 100 caracteres. El sistema SHALL conservar mayúsculas, acentos y signos escritos.

#### Scenario: Normalizar espacios del nombre del grupo
- **WHEN** se crea un grupo con el nombre `  Quinto   “A”  `
- **THEN** el nombre almacenado es `Quinto “A”`

#### Scenario: Aceptar nombre de grupo en el límite
- **WHEN** el nombre normalizado del grupo tiene exactamente 100 caracteres
- **THEN** el sistema acepta el nombre

#### Scenario: Rechazar nombre de grupo demasiado largo
- **WHEN** el nombre normalizado del grupo tiene 101 caracteres
- **THEN** el sistema lanza `DomainValidationException` y no crea el grupo

#### Scenario: Rechazar nombre de grupo vacío después de normalizar
- **WHEN** se intenta crear un grupo con un nombre vacío o compuesto únicamente por espacios
- **THEN** el sistema lanza `DomainValidationException` y no crea el grupo

### Requirement: Estudiante con identidad, nombre, número y estado
El sistema SHALL representar cada estudiante mediante un `EstudianteId` opaco y fuertemente tipado, basado en un `Guid` generado por Core, un único nombre visible, un número de lista y un estado activo o inactivo. El consumidor MUST NOT proporcionar la identidad al agregar un estudiante. El nombre visible MUST NOT utilizarse como identidad y el estudiante SHALL estar activo inicialmente.

#### Scenario: Agregar un estudiante válido
- **WHEN** se agrega a un grupo un estudiante con nombre y número de lista válidos y disponibles
- **THEN** Core genera su `EstudianteId`, almacena sus datos y lo registra como activo

#### Scenario: Permitir nombres repetidos
- **WHEN** se agregan al mismo grupo dos estudiantes con el mismo nombre visible válido y números de lista distintos
- **THEN** el sistema acepta ambos y les asigna identidades distintas

### Requirement: Normalización y validación del nombre del estudiante
El sistema SHALL quitar espacios iniciales y finales del nombre visible del estudiante, SHALL reducir cada secuencia interna de espacios a un solo espacio y SHALL validar el resultado normalizado. El nombre normalizado MUST contener al menos un carácter y MUST tener como máximo 150 caracteres. El sistema SHALL conservar acentos, mayúsculas, guiones y apóstrofos escritos.

#### Scenario: Normalizar espacios del nombre del estudiante
- **WHEN** se agrega o renombra un estudiante usando `  María   José  `
- **THEN** el nombre almacenado es `María José`

#### Scenario: Conservar caracteres del nombre
- **WHEN** se usa el nombre válido `Ángel O'Connor-López`
- **THEN** el sistema conserva sus acentos, mayúsculas, apóstrofo y guion

#### Scenario: Aceptar nombre de estudiante en el límite
- **WHEN** el nombre normalizado tiene exactamente 150 caracteres
- **THEN** el sistema acepta el nombre

#### Scenario: Rechazar nombre de estudiante demasiado largo
- **WHEN** el nombre normalizado tiene 151 caracteres
- **THEN** el sistema lanza `DomainValidationException` sin cambiar el estudiante ni el grupo

#### Scenario: Rechazar nombre de estudiante vacío después de normalizar
- **WHEN** se intenta agregar o renombrar un estudiante usando un nombre vacío o compuesto únicamente por espacios
- **THEN** el sistema lanza `DomainValidationException` sin cambiar el estudiante ni el grupo

### Requirement: Número de lista positivo sin continuidad obligatoria
El número de lista SHALL ser un entero mayor que cero. Core SHALL NOT imponer un límite superior ni exigir que los números sean contiguos.

#### Scenario: Rechazar cero
- **WHEN** se intenta agregar un estudiante o cambiar su número de lista a cero
- **THEN** el sistema lanza `DomainValidationException` sin cambiar el grupo

#### Scenario: Rechazar un número negativo
- **WHEN** se intenta agregar un estudiante o cambiar su número de lista a un entero negativo
- **THEN** el sistema lanza `DomainValidationException` sin cambiar el grupo

#### Scenario: Permitir huecos y números grandes
- **WHEN** un grupo contiene estudiantes activos con números 1 y 1000000
- **THEN** el sistema acepta ambos sin exigir números intermedios ni aplicar un límite superior adicional

### Requirement: Unicidad sólo entre estudiantes activos del mismo grupo
El sistema SHALL exigir que cada número de lista sea único entre los estudiantes activos de un mismo grupo. El mismo número SHALL poder utilizarse en grupos diferentes y por estudiantes inactivos. Un conflicto SHALL lanzar `DomainConflictException`, SHALL NOT provocar reasignaciones automáticas y SHALL dejar el grupo sin cambios.

#### Scenario: Rechazar duplicado entre activos
- **WHEN** se intenta agregar un estudiante activo o cambiar su número al que ya usa otro estudiante activo del mismo grupo
- **THEN** el sistema lanza `DomainConflictException` sin agregar, reasignar ni modificar estudiantes

#### Scenario: Permitir el mismo número en grupos diferentes
- **WHEN** dos estudiantes pertenecen a grupos distintos y usan el mismo número válido
- **THEN** el sistema acepta ambos números

#### Scenario: Permitir que un activo reutilice el número de un inactivo
- **WHEN** un estudiante inactivo conserva un número y otro estudiante del mismo grupo se agrega o cambia explícitamente a ese número
- **THEN** el sistema acepta la operación porque la unicidad se aplica sólo entre activos

### Requirement: Cambio explícito de número de lista
El sistema SHALL permitir cambiar explícitamente el número de lista de un estudiante activo o inactivo, aplicando la validación y, cuando el estudiante esté activo, la unicidad entre activos. No SHALL cambiar números de otros estudiantes automáticamente.

#### Scenario: Cambiar el número de un estudiante activo
- **WHEN** se asigna a un estudiante activo un número válido no usado por otro activo del grupo
- **THEN** el sistema actualiza únicamente el número de ese estudiante

#### Scenario: Preparar la reactivación con un cambio explícito
- **WHEN** un estudiante inactivo tiene un número ocupado por un activo y se le asigna explícitamente otro número válido
- **THEN** el sistema conserva su estado inactivo y actualiza únicamente su número

### Requirement: Estado reversible, idempotente y sin eliminación
El sistema SHALL permitir desactivar y reactivar estudiantes conservando identidad, nombre y número de lista. Desactivar a un estudiante ya inactivo y reactivar a uno ya activo SHALL ser idempotente. El modelo SHALL NOT incluir eliminación definitiva.

#### Scenario: Desactivar un estudiante activo
- **WHEN** se desactiva un estudiante activo
- **THEN** conserva identidad y datos, pasa a inactivo y deja de aparecer en la consulta de activos

#### Scenario: Desactivar un estudiante ya inactivo
- **WHEN** se desactiva un estudiante inactivo
- **THEN** la operación termina correctamente y el estudiante permanece sin cambios

#### Scenario: Reactivar un estudiante sin conflicto
- **WHEN** se reactiva un estudiante inactivo cuyo número no usa otro estudiante activo del grupo
- **THEN** conserva identidad y datos y pasa a activo

#### Scenario: Reactivar un estudiante ya activo
- **WHEN** se reactiva un estudiante activo
- **THEN** la operación termina correctamente y el estudiante permanece sin cambios

#### Scenario: Rechazar reactivación con conflicto
- **WHEN** se intenta reactivar un estudiante cuyo número ya usa otro estudiante activo del grupo
- **THEN** el sistema lanza `DomainConflictException` y el estudiante permanece inactivo con la misma identidad y los mismos datos

### Requirement: Operaciones atómicas y errores de dominio
El sistema SHALL lanzar `DomainValidationException` para valores inválidos y `DomainConflictException` para conflictos de invariantes. Toda operación fallida SHALL ser atómica y dejar el grupo, su colección y todos sus estudiantes sin cambios parciales.

#### Scenario: Atomicidad ante valor inválido
- **WHEN** una operación recibe cualquier valor inválido después de normalizarlo
- **THEN** lanza `DomainValidationException` y el estado observable completo del grupo permanece igual

#### Scenario: Atomicidad ante conflicto
- **WHEN** una operación produciría un conflicto con las invariantes del grupo
- **THEN** lanza `DomainConflictException` y el estado observable completo del grupo permanece igual

### Requirement: Colecciones de solo lectura y consulta activa ordenada
El sistema SHALL exponer vistas de solo lectura y MUST NOT devolver una colección mutable que permita eludir las invariantes. La consulta de estudiantes activos SHALL excluir inactivos y SHALL ordenar de forma determinista primero por número de lista ascendente y después por nombre visible.

#### Scenario: No exponer una colección mutable
- **WHEN** un consumidor obtiene una colección de estudiantes del grupo
- **THEN** no puede agregar, retirar ni sustituir elementos mediante esa colección

#### Scenario: Consultar activos en orden determinista
- **WHEN** el grupo contiene estudiantes activos e inactivos agregados en cualquier orden
- **THEN** la consulta devuelve sólo los activos ordenados por número ascendente y después por nombre visible

### Requirement: Independencia tecnológica de Core
El modelo de grupo y estudiantes SHALL residir en Core y SHALL funcionar sin referencias a SQLite, repositorios, migraciones, WPF, ViewModels ni otros componentes de interfaz gráfica.

#### Scenario: Probar el modelo de forma aislada
- **WHEN** se compilan y ejecutan las pruebas unitarias del modelo
- **THEN** todas las reglas se validan sin inicializar persistencia ni interfaz gráfica

