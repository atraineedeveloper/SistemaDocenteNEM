## ADDED Requirements

### Requirement: The group dashboard SHALL prioritize student work over persistent action chrome

The Resumen module SHALL present group identity, compact summary metrics and the student list without a permanent bottom action bar.

#### Scenario: Teacher opens a populated group
- **WHEN** a group with students is shown in Resumen
- **THEN** the page SHALL show the group heading, Total and Activos metrics, and the student-list card
- **AND** SHALL NOT show an average-age KPI
- **AND** SHALL NOT show the former permanent bottom action bar.

### Requirement: Frequent group actions SHALL live in the student-list toolbar

The student-list toolbar SHALL keep `Agregar estudiante` directly visible and SHALL keep import/export reachable from a compact secondary table-actions menu.

#### Scenario: Teacher needs a global student-list action
- **WHEN** the student list is visible
- **THEN** `Agregar estudiante` SHALL be directly reachable
- **AND** `Importar alumnos…` and `Exportar datos…` SHALL remain reachable without scrolling to a footer.

### Requirement: Search, filtering and ordering SHALL compose without modifying data

The student list SHALL allow search by the existing search semantics, status filtering for Todos/Activos/Inactivos, and ordering by Nombre A–Z, Nombre Z–A or Número de lista.

#### Scenario: Teacher filters and orders the list
- **WHEN** a teacher changes search, status filter or ordering
- **THEN** the visible list SHALL reflect all active criteria
- **AND** no student record SHALL be mutated by those view controls.

### Requirement: Row-specific actions SHALL target the interacted student

Every student row SHALL expose a visible overflow action and SHALL support right-click context actions. Before a contextual action is executed, AulaRaíz SHALL select the row that initiated the interaction.

#### Scenario: Teacher right-clicks a non-selected row
- **GIVEN** another student is currently selected
- **WHEN** the teacher right-clicks a different student row
- **THEN** that row SHALL become `EstudianteSeleccionado`
- **AND** the context menu SHALL operate on that student.

#### Scenario: Teacher uses the overflow affordance
- **WHEN** the teacher activates `⋮` on a student row
- **THEN** that row SHALL become selected
- **AND** the same contextual action set used by right-click SHALL be shown.

### Requirement: Student contextual actions SHALL reflect active state

The contextual action set SHALL include Ver expediente, Editar estudiante and exactly the applicable state transition.

#### Scenario: Active student menu
- **WHEN** the selected student is active
- **THEN** the menu SHALL offer Desactivar estudiante
- **AND** SHALL NOT offer Reactivar estudiante.

#### Scenario: Inactive student menu
- **WHEN** the selected student is inactive
- **THEN** the menu SHALL offer Reactivar estudiante
- **AND** SHALL NOT offer Desactivar estudiante.

### Requirement: Double-click SHALL provide a desktop shortcut to expediente

Double-clicking a student row SHALL select that row and SHALL open the existing expediente flow for that student.

#### Scenario: Teacher double-clicks a student
- **WHEN** a student row is double-clicked
- **THEN** that student SHALL become selected
- **AND** AulaRaíz SHALL open that student's expediente using the existing expediente flow.

### Requirement: The redesigned dashboard SHALL preserve semantic themes and discoverability

The redesigned dashboard SHALL use the application's semantic theme resources, SHALL keep frequent actions keyboard-focusable and labeled, and SHALL provide a visible alternative to right-click for row actions.

#### Scenario: Teacher uses themes, keyboard or mouse
- **WHEN** the dashboard is used in Light, Dark or High Contrast or with keyboard navigation
- **THEN** semantic theme resources SHALL be used rather than screenshot-specific fixed colors
- **AND** frequent actions SHALL remain keyboard-focusable and semantically labeled
- **AND** right-click SHALL NOT be the only way to discover student actions.