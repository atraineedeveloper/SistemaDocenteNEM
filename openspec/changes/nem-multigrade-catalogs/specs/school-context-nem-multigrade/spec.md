# school-context-nem-multigrade Specification

## ADDED Requirements

### Requirement: Primary grades MUST be structured values

The system MUST represent primary grades as structured values from first through sixth grade. Free-text grade strings MUST NOT be the source of truth for newly configured groups.

#### Scenario: Configure a one-grade group

- **WHEN** the teacher selects fourth grade as the only served grade
- **THEN** the structured group context stores fourth grade
- **AND** the interface presents the grade as `4.º`.

#### Scenario: Configure a multigrade group

- **WHEN** the teacher selects first, second and third grades
- **THEN** the context stores all three grades as one group configuration
- **AND** the application does not create three separate groups.

### Requirement: NEM phases MUST be derived from primary grade

The system MUST derive NEM phases using the primary-school mapping 1.º–2.º → Phase 3, 3.º–4.º → Phase 4, and 5.º–6.º → Phase 5. NEM phase MUST NOT be independently editable.

#### Scenario: One phase in a unigrade group

- **WHEN** a group serves fourth grade
- **THEN** the application derives Phase 4.

#### Scenario: Multiple phases in a multigrade group

- **WHEN** a group serves second and third grades
- **THEN** the application derives Phases 3 and 4.

### Requirement: Classroom modality MUST be derived from grades served

The system MUST classify the classroom as `Unigrado` when one real primary grade is configured and `Multigrado` when two or more real grades are configured.

#### Scenario: Derive multigrade modality

- **WHEN** two or more served grades are selected
- **THEN** the interface shows `Multigrado`
- **AND** the teacher cannot create an inconsistent manual modality value.

### Requirement: School organization MUST be independent from classroom modality

The system MUST store school organization independently from the derived classroom modality. Supported school-organization options MUST include unspecified, unitaria/unidocente, bidocente, tridocente, tetradocente, pentadocente and organización completa.

#### Scenario: Tridocente school with multigrade classroom

- **WHEN** school organization is `Tridocente` and the current classroom serves third and fourth grades
- **THEN** school organization remains `Tridocente`
- **AND** classroom modality is derived as `Multigrado`.

### Requirement: Every student MUST support an individual primary grade

The student model MUST support a structured primary grade. In a fully configured multigrade classroom, each active student MUST be assignable to one of the grades served by that group.

#### Scenario: Default grade in a unigrade group

- **WHEN** a group serves only fourth grade and a new student is created without a different grade choice
- **THEN** the UI defaults the student to fourth grade.

#### Scenario: Distinguish students in a multigrade roster

- **WHEN** a group serves first, second and third grades
- **AND** students are assigned to different served grades
- **THEN** the roster can expose each student's individual grade.

### Requirement: School geography MUST use offline entity and municipality catalogs

The group configuration MUST provide the 32 Mexican federal entities as local catalog options. Selecting an entity MUST restrict municipality options to that entity. Locality MUST remain free text.

#### Scenario: Filter municipalities by state

- **WHEN** the teacher selects `Tabasco`
- **THEN** the municipality selector shows municipalities from Tabasco rather than municipalities from unrelated entities.

#### Scenario: Enter locality

- **WHEN** the teacher enters a locality name
- **THEN** the system stores the trimmed free-text value without requiring the full INEGI locality catalog.

### Requirement: Developmental references MUST remain non-diagnostic

The system MUST derive any Piaget-stage guidance as a general pedagogical reference from the grades served. It MUST NOT require the teacher to diagnose or classify individual students by cognitive stage.

#### Scenario: Display a transition reference

- **WHEN** a group includes first grade
- **THEN** the interface may indicate a transition between preoperational and concrete operations
- **AND** it labels the information as a general reference rather than a diagnosis.

### Requirement: Legacy context migration MUST be conservative

The additive migration MUST convert legacy free-text grade values only when a deterministic primary grade can be resolved. It MUST NOT guess an unknown or multigrade structure from arbitrary text.

#### Scenario: Migrate a deterministic legacy grade

- **WHEN** an existing configuration contains `4.º`
- **THEN** the migration records fourth grade structurally
- **AND** existing students in that one-grade group receive fourth grade unless they already have a structured grade.

#### Scenario: Preserve ambiguous legacy data for correction

- **WHEN** an existing grade string cannot be mapped deterministically
- **THEN** the original text remains available for compatibility
- **AND** the structured grade remains unspecified until the user corrects the configuration.