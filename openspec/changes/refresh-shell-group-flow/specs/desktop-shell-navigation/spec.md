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
- **THEN** AulaRaíz SHALL show the dedicated create-group form
- **AND** the confirmed group id SHALL remain unchanged
- **AND** the previous module context SHALL be retained for cancellation.

#### Scenario: Teacher cancels creation from an existing module
- **GIVEN** the create-group form was opened from an existing group/module
- **WHEN** the teacher chooses `Volver` or `Cancelar`
- **THEN** AulaRaíz SHALL discard the uncommitted group-name draft
- **AND** SHALL return to the exact prior group/module context.

#### Scenario: Teacher cancels creation from Mis grupos
- **GIVEN** the create-group form was opened from `Mis grupos`
- **WHEN** the teacher chooses `Volver` or `Cancelar`
- **THEN** AulaRaíz SHALL return to `Mis grupos`
- **AND** SHALL NOT route through the historical welcome/recovery surface.

### Requirement: Successful group creation SHALL enter the new group summary

A successful group creation SHALL close the transient creation route, keep the newly created group selected and navigate the shell to that group's `Resumen`.

#### Scenario: Teacher creates a valid group
- **WHEN** the teacher submits a valid group name from the dedicated create form
- **THEN** the existing group-creation business operation SHALL create and select the group
- **AND** the create form SHALL close
- **AND** the shell SHALL navigate to the new group's `Resumen`.

### Requirement: Normal group creation SHALL NOT expose stale-reference recovery actions

The dedicated normal create-group route SHALL provide only normal creation/navigation actions and SHALL NOT expose stale-reference recovery as part of that workflow.

#### Scenario: Normal creation form is displayed
- **WHEN** the shell-level create-group route is active
- **THEN** the form SHALL provide Back, Cancel and Create actions
- **AND** SHALL NOT present `Olvidar referencia` as a normal creation action.

### Requirement: Refreshed shell controls SHALL preserve accessibility semantics

The refreshed shell and create-group controls SHALL remain keyboard-focusable, semantically labeled and compatible with the application's semantic theme resources.

#### Scenario: Teacher uses keyboard or semantic themes
- **WHEN** the refreshed shell/create form is used with keyboard navigation or Light, Dark or High Contrast resources
- **THEN** actionable controls SHALL remain focusable and labeled
- **AND** active navigation state SHALL not rely on color alone
- **AND** the implementation SHALL use semantic theme resources rather than fixed visual colors.
