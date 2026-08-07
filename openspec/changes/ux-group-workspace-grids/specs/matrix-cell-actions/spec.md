# matrix-cell-actions Specification

## ADDED Requirements

### Requirement: Attendance matrix cells MUST expose direct state actions

The monthly Attendance matrix MUST allow the user to open the state action menu from a single pointer click on an editable day cell.

#### Scenario: Clicking an Attendance cell

- **GIVEN** a monthly Attendance day cell is selected
- **WHEN** the user clicks the cell
- **THEN** a compact menu is shown for `Presente`, `Falta`, `Retardo` and `Justificada`
- **AND** choosing an option updates only that selected student/day cell.

#### Scenario: Using Attendance keyboard shortcuts

- **GIVEN** focus is inside an editable Attendance day cell
- **WHEN** the user presses P, F, R or J
- **THEN** the corresponding state is applied without opening the menu.

### Requirement: Evaluation matrix cells MUST expose direct result actions

The Evaluation matrix MUST allow the user to open the teacher-facing result menu from a single pointer click on an applicable editable activity cell.

#### Scenario: Clicking an Evaluation cell

- **GIVEN** an applicable editable Evaluation cell is selected
- **WHEN** the user clicks the cell
- **THEN** a compact result menu is shown
- **AND** the menu includes `Pendiente`, `Entregada · evaluar después`, `Domina`, `Suficiente`, `En proceso`, `Requiere apoyo`, `No entregó` and `Más opciones…`.

#### Scenario: Opening more options

- **GIVEN** the Evaluation result menu is open
- **WHEN** the user chooses `Más opciones…`
- **THEN** the full cell editor is opened
- **AND** the user can edit the result and pedagogical observation.