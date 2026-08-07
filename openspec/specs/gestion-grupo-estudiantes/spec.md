# gestion-grupo-estudiantes Specification

## Purpose

Define the minimum Core contract for school groups and students, including internal identity, reversible status and verifiable rules for names, list numbers and active-student queries.

## Requirements

### Requirement: School group with identity and visible name
The system SHALL represent each group with an opaque, strongly typed `GrupoId` based on a `Guid`, a required visible name and its own student collection. When a new group is created, Core SHALL generate its identity and the consumer MUST NOT provide it. Core SHALL expose a neutral public factory equivalent to `Grupo.Rehidratar` that accepts an existing identity only to rebuild a complete snapshot without generating a new identity. School context such as served grades, group key, shift, school and school year SHALL remain in `ContextoGrupo` rather than being duplicated as mutable `Grupo` aggregate fields.

#### Scenario: Create a valid group
- **WHEN** a new group is created with a valid visible name through `Grupo.Crear`
- **THEN** Core generates its `GrupoId` without receiving it from the consumer and stores the normalized name

#### Scenario: Distinct group identities
- **WHEN** two valid new groups are created
- **THEN** each group receives a distinct strongly typed `GrupoId`

#### Scenario: Rehydrate a persisted group
- **WHEN** `Grupo.Rehidratar` receives an existing `GrupoId` and a valid snapshot
- **THEN** Core preserves the supplied identity and does not generate a new `GrupoId`

### Requirement: Group-name normalization and validation
The system SHALL trim leading/trailing spaces from the group name, SHALL reduce every internal whitespace sequence to one space and SHALL validate the normalized result. The normalized name MUST contain at least one character and MUST be no longer than 100 characters. The system SHALL preserve case, accents and punctuation entered by the user.

#### Scenario: Normalize group-name spacing
- **WHEN** a group is created with `  Quinto   “A”  `
- **THEN** the stored name is `Quinto “A”`

#### Scenario: Accept group name at the limit
- **WHEN** the normalized group name contains exactly 100 characters
- **THEN** the system accepts it

#### Scenario: Reject group name that is too long
- **WHEN** the normalized group name contains 101 characters
- **THEN** the system throws `DomainValidationException` and does not create the group

#### Scenario: Reject empty group name after normalization
- **WHEN** a group is created with an empty or whitespace-only name
- **THEN** the system throws `DomainValidationException` and does not create the group

### Requirement: Student with identity, name, list number, status and optional structured primary grade
The system SHALL represent each student with an opaque, strongly typed `EstudianteId` based on a Core-generated `Guid`, one visible name, a list number, active/inactive status and a structured `GradoPrimaria` value. The consumer MUST NOT provide the identity when adding a student. The visible name MUST NOT be used as identity and a new student SHALL initially be active. `GradoPrimaria.NoEspecificado` MAY be retained for legacy/incomplete data while structured group configuration is being completed.

#### Scenario: Add a valid student
- **WHEN** a student is added to a group with a valid available name and list number
- **THEN** Core generates the `EstudianteId`, stores the data and marks the student active

#### Scenario: Allow duplicate visible names
- **WHEN** two students with the same valid visible name and different list numbers are added to the same group
- **THEN** the system accepts both and assigns distinct identities

### Requirement: Student-name normalization and validation
The system SHALL trim leading/trailing spaces from the student's visible name, SHALL reduce every internal whitespace sequence to one space and SHALL validate the normalized result. The normalized name MUST contain at least one character and MUST be no longer than 150 characters. The system SHALL preserve accents, case, hyphens and apostrophes.

#### Scenario: Normalize student-name spacing
- **WHEN** a student is added or renamed using `  María   José  `
- **THEN** the stored name is `María José`

#### Scenario: Preserve name characters
- **WHEN** the valid name `Ángel O'Connor-López` is used
- **THEN** the system preserves accents, case, apostrophe and hyphen

#### Scenario: Accept student name at the limit
- **WHEN** the normalized name contains exactly 150 characters
- **THEN** the system accepts it

#### Scenario: Reject student name that is too long
- **WHEN** the normalized name contains 151 characters
- **THEN** the system throws `DomainValidationException` without changing the student or group

#### Scenario: Reject empty student name after normalization
- **WHEN** a student is added or renamed with an empty or whitespace-only name
- **THEN** the system throws `DomainValidationException` without changing the student or group

### Requirement: Positive list number without required continuity
The list number SHALL be an integer greater than zero. Core SHALL NOT impose an upper bound or require contiguous numbers.

#### Scenario: Reject zero
- **WHEN** a student is added or their list number is changed to zero
- **THEN** the system throws `DomainValidationException` without changing the group

#### Scenario: Reject a negative number
- **WHEN** a student is added or their list number is changed to a negative integer
- **THEN** the system throws `DomainValidationException` without changing the group

#### Scenario: Allow gaps and large numbers
- **WHEN** a group contains active students with list numbers 1 and 1000000
- **THEN** the system accepts both without requiring intermediate numbers or applying an additional upper limit

### Requirement: Uniqueness only among active students in the same group
The system SHALL require each list number to be unique among active students in the same group. The same number SHALL be usable in different groups and by inactive students. A conflict SHALL throw `DomainConflictException`, SHALL NOT trigger automatic reassignment and SHALL leave the group unchanged.

#### Scenario: Reject duplicate among active students
- **WHEN** an active student is added or changed to a number already used by another active student in the same group
- **THEN** the system throws `DomainConflictException` without adding, reassigning or modifying students

#### Scenario: Allow the same number in different groups
- **WHEN** two students belong to different groups and use the same valid number
- **THEN** the system accepts both numbers

#### Scenario: Allow an active student to reuse an inactive student's number
- **WHEN** an inactive student retains a number and another student in the same group is explicitly added or changed to that number
- **THEN** the system accepts the operation because uniqueness applies only among active students

### Requirement: Explicit list-number change
The system SHALL allow the list number of an active or inactive student to be changed explicitly, applying validation and, for an active student, uniqueness among active students. It SHALL NOT change other students' numbers automatically.

#### Scenario: Change an active student's number
- **WHEN** an active student is assigned a valid number not used by another active student in the group
- **THEN** the system updates only that student's number

#### Scenario: Prepare reactivation with an explicit change
- **WHEN** an inactive student's number is occupied by an active student and another valid number is explicitly assigned
- **THEN** the system keeps the student inactive and updates only the number

### Requirement: Reversible, idempotent status without permanent deletion
The system SHALL allow students to be deactivated and reactivated while preserving identity, name and list number. Deactivating an already inactive student and reactivating an already active student SHALL be idempotent. The model SHALL NOT include permanent deletion.

#### Scenario: Deactivate an active student
- **WHEN** an active student is deactivated
- **THEN** identity and data are preserved, the student becomes inactive and no longer appears in the active-student query

#### Scenario: Deactivate an already inactive student
- **WHEN** an inactive student is deactivated
- **THEN** the operation completes successfully and the student remains unchanged

#### Scenario: Reactivate without conflict
- **WHEN** an inactive student whose number is not used by another active student is reactivated
- **THEN** identity and data are preserved and the student becomes active

#### Scenario: Reactivate an already active student
- **WHEN** an active student is reactivated
- **THEN** the operation completes successfully and the student remains unchanged

#### Scenario: Reject reactivation with conflict
- **WHEN** a student is reactivated while their number is already used by another active student in the group
- **THEN** the system throws `DomainConflictException` and the student remains inactive with the same identity and data

### Requirement: Atomic operations and domain errors
The system SHALL throw `DomainValidationException` for invalid values and `DomainConflictException` for invariant conflicts. Every failed operation SHALL be atomic and leave the group, collection and students without partial changes.

#### Scenario: Atomicity for invalid value
- **WHEN** an operation receives any invalid value after normalization
- **THEN** it throws `DomainValidationException` and the complete observable group state remains unchanged

#### Scenario: Atomicity for conflict
- **WHEN** an operation would violate group invariants
- **THEN** it throws `DomainConflictException` and the complete observable group state remains unchanged

### Requirement: Read-only collections and deterministic active query
The system SHALL expose read-only views and MUST NOT return a mutable collection that bypasses invariants. The active-student query SHALL exclude inactive students and SHALL sort deterministically by ascending list number and then visible name.

#### Scenario: Do not expose a mutable collection
- **WHEN** a consumer obtains a group student collection
- **THEN** the consumer cannot add, remove or replace elements through that collection

#### Scenario: Query active students deterministically
- **WHEN** the group contains active and inactive students added in any order
- **THEN** the query returns only active students ordered by ascending list number and then visible name

### Requirement: Core technology independence
The group/student model SHALL live in Core and SHALL work without references to SQLite, repositories, migrations, WPF, ViewModels or other graphical-interface components.

#### Scenario: Test the model in isolation
- **WHEN** model unit tests are compiled and executed
- **THEN** all rules are validated without initializing persistence or graphical UI

### Requirement: Existing identifiers for rehydration
Core SHALL provide public neutral conversions between persisted `Guid` values and `GrupoId` / `EstudianteId`, without Data or SQLite references. These conversions SHALL be used only to represent existing identities and MUST NOT change internal generation used by `Grupo.Crear` and `AgregarEstudiante`.

#### Scenario: Rebuild typed identifiers
- **WHEN** Data converts existing `Guid` values through neutral Core conversions
- **THEN** it obtains `GrupoId` and `EstudianteId` with exactly the same values without generating new identities

### Requirement: Neutral student snapshot
Core SHALL define a public immutable neutral type that supplies rehydration with each student's `EstudianteId`, visible name, list number, active/inactive status and compatible structured profile data. The type MUST NOT reference Data, SQLite or provider types.

#### Scenario: Represent persisted student data
- **WHEN** Data prepares a student read from storage for rehydration
- **THEN** it can express the complete persisted student data through the neutral Core type without losing identity or status

### Requirement: Atomic aggregate rehydration
`Grupo.Rehidratar` SHALL validate the group name and complete student snapshot before returning the aggregate. It SHALL apply the same name, list-number and active-uniqueness invariants, preserve identities/status and reject any invalid or contradictory snapshot without returning a partially rebuilt aggregate. The factory MUST NOT alter the behavior of `Grupo.Crear` or `AgregarEstudiante` and MUST NOT use `InternalsVisibleTo`.

#### Scenario: Rehydrate active and inactive students
- **WHEN** the factory receives a valid snapshot with active and inactive students
- **THEN** it returns a complete aggregate preserving each `EstudianteId`, name, list number and status

#### Scenario: Reject manipulated name
- **WHEN** the snapshot contains a name that violates Core normalization or length rules
- **THEN** the factory rejects the complete snapshot without silently correcting the name or returning a partial aggregate

#### Scenario: Reject invalid number or contradictory state
- **WHEN** the snapshot contains a non-positive number or duplicate numbers among active students
- **THEN** the factory rejects the complete snapshot without returning a partial aggregate
