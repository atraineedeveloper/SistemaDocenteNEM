# interfaz-proyectos-actividades Specification

## Purpose

Define the separation between project/activity planning and the operational Evaluation surface, using dedicated windows for complex editing and a matrix for group follow-up.

## Requirements

### Requirement: Separate Projects and Evaluation navigation
The system SHALL keep Projects and Evaluation as separate main modules in global navigation. Projects SHALL focus on planning and access to details; Evaluation SHALL focus on matrix follow-up for the selected project.

#### Scenario: Open Evaluation module
- **WHEN** the user selects `Evaluación` in the main navigation
- **THEN** the system shows the group evaluation matrix and allows a teaching project to be selected

### Requirement: Project detail in a dedicated window
The Projects module SHALL allow `DetalleProyectoWindow` to be opened to edit project data and manage access to activities without turning the main view into a mandatory master-detail layout.

#### Scenario: Open project detail
- **WHEN** the user opens a project from the main list
- **THEN** the system shows `DetalleProyectoWindow` with the information and actions for the selected project

### Requirement: Activity detail in a dedicated window
Complex activity editing SHALL take place in `DetalleActividadWindow`, preserving the activity as the save unit for its historical roster.

#### Scenario: Open an activity
- **WHEN** the user opens an existing activity or creates a new one from project detail
- **THEN** the system shows `DetalleActividadWindow` for editing its data

### Requirement: Matrix evaluation without an independent activity selector
Evaluation SHALL represent students in rows and activities in columns. The current cell's column SHALL define the activity context for metrics and bulk actions, without reintroducing an independent activity selector.

#### Scenario: Change activity through the matrix
- **WHEN** the user moves the current cell to another activity column
- **THEN** Evaluation metrics and actions use that activity as context

### Requirement: Preserve historical roster
The interface SHALL respect each activity's historical roster and SHALL display a cell as not applicable when the student did not yet belong to that activity.

#### Scenario: Student admitted later
- **WHEN** a student joined after an earlier activity
- **THEN** the matrix shows `—` for that earlier activity and does not allow the cell to be edited