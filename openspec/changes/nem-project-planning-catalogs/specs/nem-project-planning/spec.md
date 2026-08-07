## ADDED Requirements

### Requirement: Structured NEM project methodology
The system SHALL represent project methodology with a stable structured value containing `No especificada`, `Aprendizaje Basado en Proyectos Comunitarios`, `Aprendizaje Basado en Indagación (STEAM como enfoque)`, `Aprendizaje Basado en Problemas (ABP)` and `Aprendizaje Servicio (AS)`.

The methodology SHALL describe the teacher's planning choice and MUST NOT be automatically forced by formative field.

#### Scenario: Teacher selects a project methodology
- **WHEN** a teacher creates or edits a project and chooses one of the four supported NEM methodologies
- **THEN** the selected methodology is persisted and returned in project details

#### Scenario: Legacy project has no methodology
- **WHEN** an existing project is opened after migration
- **THEN** its methodology is `No especificada` and no methodology is guessed

### Requirement: Structured formative field per activity
The system SHALL represent an activity's formative field with one of `No especificado`, `Lenguajes`, `Saberes y Pensamiento Científico`, `Ética, Naturaleza y Sociedades`, or `De lo Humano y lo Comunitario`.

New activity flows SHOULD require a real formative field before save. Rehydrated legacy activities MAY remain `No especificado` until edited.

#### Scenario: Activity identifies its formative field
- **WHEN** a teacher saves an activity as `Saberes y Pensamiento Científico`
- **THEN** the field is persisted and visible when the activity is reopened

#### Scenario: Legacy activity remains unspecified
- **WHEN** an activity created before this capability is migrated
- **THEN** its formative field remains `No especificado` without inference from title or description

### Requirement: Project target grades
A project SHALL support an ordered unique set of target `GradoPrimaria` values. New explicit planning SHALL use only real grades 1–6. An empty set SHALL be reserved for legacy or unspecified scope and SHALL NOT be interpreted as evidence of a historical grade decision.

#### Scenario: Multigrade project targets a subset
- **GIVEN** a classroom serving grades 1, 2 and 3
- **WHEN** the teacher configures a project for grades 2 and 3
- **THEN** the project stores exactly grades 2 and 3

#### Scenario: Duplicate target grades
- **WHEN** duplicate grade values are supplied
- **THEN** the aggregate normalizes them to a unique ordered set or rejects invalid non-primary values

### Requirement: Activity target grades
An activity SHALL support an ordered unique set of target primary grades. If its containing project has an explicit non-empty target-grade set, every explicit activity target grade MUST belong to the project set.

#### Scenario: Activity targets one grade inside a multigrade project
- **GIVEN** a project explicitly targeting grades 1, 2 and 3
- **WHEN** an activity targets grade 2
- **THEN** the activity is accepted with grade 2 as its explicit scope

#### Scenario: Activity targets a grade outside the project
- **GIVEN** a project explicitly targeting grades 1 and 2
- **WHEN** an activity targets grade 3
- **THEN** the save is rejected without partial persistence

### Requirement: Grade-targeted activity roster
When a new activity has explicit target grades, its initial historical roster SHALL contain exactly the active students whose individual `GradoPrimaria` belongs to the target set. Inactive students, students from other grades and students without a matching real grade SHALL NOT be added.

Once created, the activity roster SHALL remain historical and SHALL NOT be rewritten because a student changes grade or the group configuration changes later.

#### Scenario: Create activity for grade 2 only
- **GIVEN** a multigrade group with active grade-1, grade-2 and grade-3 students
- **WHEN** a new activity explicitly targets grade 2
- **THEN** its initial roster contains only the active grade-2 students

#### Scenario: Student changes grade later
- **GIVEN** an existing activity with a stored historical roster
- **WHEN** a student later changes grade in the group
- **THEN** the existing activity roster remains unchanged

### Requirement: Unigrade defaulting
When the active classroom context has exactly one served grade, Presentation SHALL preselect that grade for new project and activity target-grade editing. The teacher SHOULD NOT need repetitive grade selection for normal unigrade work.

#### Scenario: Fourth-grade classroom
- **GIVEN** a group configured only for fourth grade
- **WHEN** the teacher opens a new project or activity editor
- **THEN** fourth grade is already selected

### Requirement: Additive persistence and conservative migration
The new metadata SHALL use an independently versioned SQLite extension named `nem-planeacion-proyectos` while keeping `PRAGMA user_version = 6`.

Migration SHALL create metadata rows for existing projects and activities with unspecified methodology/formative field and SHALL leave target-grade tables empty for legacy records.

#### Scenario: Upgrade existing database
- **WHEN** a v6 database containing projects and activities is opened by the new build
- **THEN** the additive extension is applied transactionally, base project/activity rows remain intact, methodology/formative field become unspecified, and no historical target grades are invented

### Requirement: Atomic metadata persistence
Methodology and project grades SHALL be saved in the same transaction as the project aggregate. Formative field and activity grades SHALL be saved in the same transaction as the activity aggregate.

#### Scenario: Metadata write fails
- **WHEN** a metadata constraint or storage failure occurs during an aggregate save
- **THEN** neither the base aggregate changes nor its NEM metadata are partially committed