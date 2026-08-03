## MODIFIED Requirements

### Requirement: Grupo escolar con identidad y nombre visible
El sistema SHALL representar cada grupo mediante un `GrupoId` opaco y fuertemente tipado, basado en un `Guid`, un nombre visible obligatorio y una colección propia de estudiantes. Al crear un grupo nuevo, Core SHALL generar su identidad y el consumidor MUST NOT proporcionarla. Core SHALL ofrecer una fábrica pública neutral equivalente a `Grupo.Rehidratar` que acepte una identidad existente únicamente para reconstruir un snapshot completo sin generar una identidad nueva. El modelo SHALL NOT incorporar todavía grado, grupo, turno, escuela ni ciclo escolar como datos separados.

#### Scenario: Crear un grupo válido
- **WHEN** se crea un grupo nuevo con un nombre visible válido mediante `Grupo.Crear`
- **THEN** Core genera su `GrupoId` sin recibirlo del consumidor y almacena el nombre normalizado

#### Scenario: Identidades de grupo distintas
- **WHEN** se crean dos grupos nuevos válidos
- **THEN** cada grupo recibe un `GrupoId` distinto y fuertemente tipado

#### Scenario: Rehidratar un grupo persistido
- **WHEN** `Grupo.Rehidratar` recibe un `GrupoId` existente y un snapshot válido
- **THEN** Core conserva la identidad recibida y no genera un `GrupoId` nuevo

## ADDED Requirements

### Requirement: Identificadores existentes para rehidratación
Core SHALL proporcionar conversiones públicas y neutrales entre los valores `Guid` persistidos y `GrupoId` o `EstudianteId`, sin referencias a Data o SQLite. Estas conversiones SHALL usarse sólo para representar identidades existentes y MUST NOT cambiar la generación interna utilizada por `Grupo.Crear` y `AgregarEstudiante`.

#### Scenario: Reconstruir identificadores tipados
- **WHEN** Data convierte valores `Guid` existentes mediante las conversiones neutrales de Core
- **THEN** obtiene `GrupoId` y `EstudianteId` con exactamente los mismos valores sin generar identidades nuevas

### Requirement: Snapshot neutral de estudiante
Core SHALL definir un tipo público, inmutable y neutral para proporcionar a la rehidratación el `EstudianteId`, nombre visible, número de lista y estado activo/inactivo de cada estudiante. El tipo MUST NOT referenciar Data, SQLite ni tipos del proveedor.

#### Scenario: Representar datos persistidos de un estudiante
- **WHEN** Data prepara un estudiante leído de almacenamiento para rehidratación
- **THEN** puede expresar todos sus datos mediante el tipo neutral de Core sin perder identidad ni estado

### Requirement: Rehidratación atómica del agregado
`Grupo.Rehidratar` SHALL validar el nombre del grupo y el snapshot completo de estudiantes antes de devolver el agregado. SHALL aplicar las mismas invariantes de nombres, números y unicidad entre activos, conservar identidades y estados, y rechazar cualquier snapshot inválido o contradictorio sin devolver un agregado total o parcialmente reconstruido. La fábrica MUST NOT alterar el comportamiento de `Grupo.Crear` ni `AgregarEstudiante` y MUST NOT usar `InternalsVisibleTo`.

#### Scenario: Rehidratar estudiantes activos e inactivos
- **WHEN** la fábrica recibe un snapshot válido con estudiantes activos e inactivos
- **THEN** devuelve un agregado completo que conserva cada `EstudianteId`, nombre, número y estado

#### Scenario: Rechazar nombre manipulado
- **WHEN** el snapshot contiene un nombre que no cumple las reglas de normalización o longitud de Core
- **THEN** la fábrica rechaza el snapshot completo sin corregir silenciosamente el nombre ni devolver un agregado parcial

#### Scenario: Rechazar número o estado contradictorio
- **WHEN** el snapshot contiene un número no positivo o números repetidos entre estudiantes activos
- **THEN** la fábrica rechaza el snapshot completo sin devolver un agregado parcial
