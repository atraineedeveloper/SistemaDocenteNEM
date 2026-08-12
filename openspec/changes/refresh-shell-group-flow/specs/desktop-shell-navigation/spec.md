## ADDED Requirements

### Requirement: The shell SHALL separate teacher navigation from application utilities

The desktop shell SHALL present product/group context and infrequent application utilities on a different visual level from the primary teacher-work navigation.

#### Scenario: Primary navigation is teacher-focused
- **WHEN** a teacher has an active group and is not in a transient creation flow
- **THEN** the primary navigation SHALL expose Resumen, Asistencia, Proyectos, Evaluación and Reportes
- **AND** backup, update and appearance actions SHALL NOT occupy equivalent primary-navigation positions.

#### Scenario: Utilities remain reachable
- **WHEN** the teacher needs backup, update or appearance actions
- **THEN** those actions SHALL remain reachable through secondary shell controls without entering a teacher module.

### Requirement: The active-group context SHALL remain explicit

The shell SHALL show the active group separately from the module navigation and SHALL provide group switching, `Mis grupos` and `Crear nuevo grupo…` from that context surface.

#### Scenario: Teacher opens the group picker
- **WHEN** an active group exists
- **THEN** the group picker SHALL list available groups
- **AND** SHALL include `Mis grupos`
- **AND** SHALL include `Crear nuevo grupo…`.

### Requirement: Group creation SHALL be reversible before commit

Starting normal group creation SHALL NOT clear or replace the currently confirmed group before the teacher successfully submits the new group.

#### Scenario: Create is opened from an existing group module
- **GIVEN** a confirmed active group and any teacher module that permits navigation
- **WHEN** the teacher opens `Crear nuevo grupo…`
- **THEN** AulaRaíz SHALL show the dedicated create-group wizard
- **AND** the confirmed group id SHALL remain unchanged
- **AND** the previous module context SHALL be retained for cancellation.

#### Scenario: Teacher cancels creation from an existing module
- **GIVEN** the create-group wizard was opened from an existing group/module
- **WHEN** the teacher chooses Cancelar, or chooses Volver from the first step
- **THEN** AulaRaíz SHALL discard the complete uncommitted wizard draft
- **AND** SHALL return to the exact prior group/module context.

#### Scenario: Teacher cancels creation from Mis grupos
- **GIVEN** the wizard was opened from `Mis grupos`
- **WHEN** the teacher cancels creation
- **THEN** AulaRaíz SHALL return to `Mis grupos`
- **AND** SHALL NOT route through the historical welcome/recovery surface.

### Requirement: Initial group setup SHALL use a progressive wizard

The create-group route SHALL present five ordered steps: Grupo, Grados, Escuela, Ubicación and Confirmar.

Only the group display name SHALL be required to finish creation. Grades, school name, CCT, school cycle, shift, state, municipality and locality SHALL be optional during initial setup.

#### Scenario: Teacher advances without a group name
- **WHEN** the teacher attempts to leave the Grupo step with an empty or whitespace-only display name
- **THEN** the wizard SHALL remain on the first step
- **AND** SHALL explain that the group name is required.

#### Scenario: Teacher skips optional setup
- **WHEN** the teacher chooses `Omitir por ahora` on Grados, Escuela or Ubicación
- **THEN** the wizard SHALL advance without manufacturing placeholder values
- **AND** the omitted fields SHALL remain unspecified.

#### Scenario: Teacher navigates backward
- **WHEN** the teacher chooses Volver from any step after the first
- **THEN** the wizard SHALL show the previous step
- **AND** previously entered draft values SHALL remain available.

### Requirement: Wizard context SHALL reuse existing group context persistence

Optional setup supplied during creation SHALL be persisted through the existing `ContextoGrupo` model and storage associated with the newly created `GrupoId`.

No separate wizard-only school/group metadata store SHALL be introduced.

#### Scenario: Teacher supplies optional setup
- **WHEN** a group is created with grades and/or school/location values entered in the wizard
- **THEN** those values SHALL be available through the normal group-configuration context for the new group.

#### Scenario: Teacher supplies only a group name
- **WHEN** all optional steps are skipped
- **THEN** the group SHALL still be creatable
- **AND** its optional context SHALL remain unspecified and editable later.

### Requirement: Successful group creation SHALL enter the new group summary

A successful group creation SHALL close the transient wizard, keep the newly created group selected and navigate the shell to that group's `Resumen`.

#### Scenario: Teacher confirms a valid wizard
- **WHEN** the teacher confirms a non-empty group name from the final step
- **THEN** the existing group-creation business operation SHALL create and select the group
- **AND** supplied optional context SHALL be associated with the new group
- **AND** the wizard SHALL close
- **AND** the shell SHALL navigate to the new group's `Resumen`.

### Requirement: Normal group creation SHALL NOT expose stale-reference recovery actions

The dedicated normal create-group route SHALL provide only normal creation/navigation actions and SHALL NOT expose stale-reference recovery as part of that workflow.

#### Scenario: Normal creation wizard is displayed
- **WHEN** the shell-level create-group route is active
- **THEN** the flow SHALL provide Back, Cancel, optional Skip/Next and Create actions as appropriate
- **AND** SHALL NOT present `Olvidar referencia` as a normal creation action.

### Requirement: Student birth date SHALL be presented as optional

The student editor SHALL describe date of birth as optional, consistent with the existing nullable student date-of-birth data path.

#### Scenario: Teacher leaves birth date empty
- **WHEN** all other required student fields are valid and no birth date is selected
- **THEN** the UI SHALL NOT describe the missing birth date as a validation error
- **AND** the student SHALL remain saveable with no birth date.

### Requirement: Refreshed shell controls SHALL preserve accessibility semantics

The refreshed shell and create-group controls SHALL remain keyboard-focusable, semantically labeled and compatible with the application's semantic theme resources.

#### Scenario: Teacher uses keyboard or semantic themes
- **WHEN** the refreshed shell/create wizard is used with keyboard navigation or Light, Dark or High Contrast resources
- **THEN** actionable controls SHALL remain focusable and labeled
- **AND** required versus optional fields SHALL be explicit in text
- **AND** active navigation state SHALL not rely on color alone
- **AND** the implementation SHALL use semantic theme resources rather than fixed visual colors.
