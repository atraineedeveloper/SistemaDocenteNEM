# interfaz-evaluacion-matriz Specification

## ADDED Requirements

### Requirement: Evaluation MUST expose one teacher-facing result concept

The Evaluation UI MUST present one result choice while preserving separate internal delivery state and achievement level values.

#### Scenario: Selecting an achievement level

- **WHEN** the user selects `Domina`, `Suficiente`, `En proceso` or `Requiere apoyo`
- **THEN** the cell delivery state becomes `Entregada`
- **AND** the selected achievement level is stored.

#### Scenario: Selecting no delivery

- **WHEN** the user selects `No entregó`
- **THEN** the cell delivery state becomes `NoEntregada`
- **AND** the internal achievement level remains `Pendiente`.

#### Scenario: Selecting delivered but not evaluated

- **WHEN** the user selects `Entregada · evaluar después`
- **THEN** the delivery state becomes `Entregada`
- **AND** the achievement level remains `Pendiente`.

#### Scenario: Selecting pending

- **WHEN** the user selects `Pendiente`
- **THEN** both delivery state and achievement level become `Pendiente`.

### Requirement: The full Evaluation cell editor MUST prioritize pedagogical input

The full cell editor MUST show the unified result selector and observation field without requiring the teacher to manipulate the technical delivery state separately.

#### Scenario: Editing an observation

- **WHEN** the user opens `Más opciones…`
- **THEN** the editor shows the current unified result
- **AND** an observation field of up to 500 characters
- **AND** applying the editor changes the local matrix state until Evaluation is saved.