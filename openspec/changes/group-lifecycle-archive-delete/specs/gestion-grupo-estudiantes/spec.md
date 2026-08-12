## ADDED Requirements

### Requirement: Group lifecycle SHALL be reversible through archive and restore
The system SHALL represent every group as active or archived. New and existing groups without a lifecycle row SHALL be active by default. Archiving and restoring SHALL preserve group identity and all associated classroom data.

Archive and restore SHALL be idempotent.

#### Scenario: Archive an active group
- **WHEN** an active group is archived
- **THEN** its `GrupoId`, visible name and associated data remain unchanged
- **AND** its lifecycle state becomes archived.

#### Scenario: Archive an already archived group
- **WHEN** archive is requested for an archived group
- **THEN** the operation succeeds without changing its preserved data.

#### Scenario: Restore an archived group
- **WHEN** an archived group is restored
- **THEN** it becomes active with the same identity and associated data.

### Requirement: Archived groups SHALL NOT be editable working contexts
Archived groups SHALL be excluded from the normal active-group picker and teacher-work modules until restored. Ordinary group/student mutations SHALL be rejected while a group is archived.

#### Scenario: Archived group appears in Mis grupos
- **WHEN** a group is archived
- **THEN** it no longer appears among active groups
- **AND** it appears in the archived-groups section with Restore and Delete actions
- **AND** it does not expose Open as an active workspace action.

#### Scenario: Mutation targets an archived group
- **WHEN** an ordinary rename/student mutation is attempted against an archived group
- **THEN** the operation is rejected without changing persisted data.

### Requirement: Permanent group deletion SHALL be explicit and risk-adaptive
The system SHALL provide permanent group deletion. Empty groups MAY use a simple destructive confirmation. A group with associated data SHALL require the teacher to enter the exact current visible group name before permanent deletion can proceed.

#### Scenario: Delete an empty accidental group
- **WHEN** an empty group is selected for deletion and the teacher confirms the simple warning
- **THEN** the group is permanently removed.

#### Scenario: Populated group confirmation does not match
- **WHEN** a populated group is selected for deletion and the typed confirmation is not an exact ordinal match for its current visible name
- **THEN** permanent deletion remains unavailable and no data changes.

#### Scenario: Populated group confirmation matches
- **WHEN** the typed confirmation exactly matches the current visible name and all safety preconditions succeed
- **THEN** the destructive operation may proceed.

### Requirement: Data-bearing permanent deletion SHALL create a safety backup first
Before permanently deleting a group that contains associated classroom data, the system SHALL successfully create a full application-managed local safety backup using the existing version-1 backup behavior.

If safety-backup creation fails, permanent deletion SHALL NOT start.

#### Scenario: Safety backup fails
- **WHEN** a populated group has valid destructive confirmation but the safety backup cannot be created
- **THEN** the group and all associated live data remain unchanged
- **AND** the UI reports that deletion could not proceed safely.

#### Scenario: Safety backup succeeds
- **WHEN** a populated group has valid destructive confirmation and safety-backup creation succeeds
- **THEN** the system may enter the transactional deletion boundary.

### Requirement: Permanent group deletion SHALL be relationally atomic
Permanent deletion SHALL remove the group and all known associated classroom records in a single SQLite transaction while foreign-key enforcement remains enabled.

Any SQL/integrity failure SHALL roll back the complete deletion.

#### Scenario: Complete group graph is deleted
- **WHEN** permanent deletion succeeds for a group with students, attendance, projects, activities, deliveries and extension metadata
- **THEN** no known row associated with that group remains
- **AND** unrelated groups remain unchanged.

#### Scenario: Dependent deletion fails
- **WHEN** any required dependent deletion fails before commit
- **THEN** the transaction is rolled back and the original group graph remains available.

### Requirement: Existing databases SHALL gain lifecycle state through a versioned additive extension
The system SHALL persist group archive state in an independently versioned SQLite lifecycle extension while keeping the validated base `PRAGMA user_version` unchanged.

When the lifecycle extension is initialized for a supported existing database, every group without a lifecycle row SHALL receive active state without changing its identity or classroom data.

#### Scenario: Open an existing v6 database
- **WHEN** the current application initializes a valid base-v6 database that has no group-lifecycle extension yet
- **THEN** the lifecycle extension is initialized independently
- **AND** the base `PRAGMA user_version` remains 6
- **AND** every preexisting group remains available as active with the same identity and data.

### Requirement: Selected-group state SHALL not retain an unavailable archived or deleted workspace
When the currently selected group is archived or deleted, the application SHALL clear that selected-group reference and return to a safe group-selection state. It SHALL NOT silently open archived data as an active workspace.

#### Scenario: Archive currently selected group
- **WHEN** the current group is archived from group management
- **THEN** the local selected-group reference is cleared
- **AND** AulaRaíz returns to `Mis grupos`.

#### Scenario: Delete currently selected group
- **WHEN** the current group is permanently deleted
- **THEN** the deleted id is not retained as a valid local working-group reference.

### Requirement: Group lifecycle controls SHALL remain accessible and theme-safe
Archive, Restore and Delete actions and destructive confirmation SHALL be keyboard-focusable, explicitly labeled and compatible with semantic Light, Dark and High Contrast resources. Destructive meaning SHALL NOT rely on color alone.

#### Scenario: Teacher uses keyboard or semantic theme
- **WHEN** group lifecycle management is used with keyboard navigation or a supported semantic theme
- **THEN** every lifecycle action remains identifiable and operable
- **AND** confirmation requirements are conveyed in text.
