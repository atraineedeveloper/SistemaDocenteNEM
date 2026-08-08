## ADDED Requirements

### Requirement: Existing individual report can be exported as PDF
The system SHALL allow the teacher to save the currently generated `ReporteIndividualAlumno` as a PDF without recalculating report semantics in the PDF layer.

#### Scenario: Individual report is selected
- **WHEN** the teacher chooses `Guardar PDF` while an individual student report is available
- **THEN** the generated PDF represents that report's existing attendance, compliance, achievement, activity and follow-up values

### Requirement: Existing group report can be exported as PDF
The system SHALL allow the teacher to save the currently generated `ReporteGrupal` as a PDF without introducing student ranking.

#### Scenario: Group report is selected
- **WHEN** the teacher chooses `Guardar PDF` while the group report is active
- **THEN** the PDF contains the existing group metrics and ordered follow-up summary without a competitive ranking

### Requirement: PDF layout is independent from WPF
Application SHALL define the PDF export port, a non-WPF adapter SHALL render PDF syntax/layout, and WPF SHALL own only native file selection and window feedback.

#### Scenario: PDF is rendered
- **WHEN** a report is exported
- **THEN** no PDFsharp/MigraDoc type is required by Core, Reporting, Presentation or WPF view logic

### Requirement: PDF reuses authoritative report calculations
The PDF adapter SHALL consume `ReporteIndividualAlumno` and `ReporteGrupal` values already produced by `SistemaDocente.Reporting`. It SHALL NOT independently recompute attendance percentages, delivery compliance or achievement distributions.

#### Scenario: Compliance is undefined
- **WHEN** the report model has no decided deliveries and `PorcentajeCumplimiento` is null
- **THEN** the PDF displays an undefined marker rather than inventing 0%

### Requirement: PDF includes identifiable school/report context
Each PDF SHALL include AulaRaíz identity, report type, group name and available structured school context. Individual PDFs SHALL also identify the selected student.

#### Scenario: Structured NEM context exists
- **WHEN** the report contains configured grades and phases
- **THEN** the PDF surfaces those values as contextual information without altering them

### Requirement: PDF save is destination-safe
The renderer SHALL write to a temporary sibling file and publish the requested `.pdf` path only after rendering completes successfully.

#### Scenario: Rendering fails
- **WHEN** PDF rendering or writing fails before publication
- **THEN** a new incomplete destination is not presented as a completed report and temporary output is removed when possible

### Requirement: PDF export warns about sensitive information
The WPF workflow SHALL warn before file selection that the report can contain personal and pedagogical information and should only be stored/shared through appropriate channels.

#### Scenario: Teacher cancels privacy confirmation
- **WHEN** the teacher declines the warning
- **THEN** no save dialog is opened and no PDF is written

### Requirement: Suggested PDF filenames are deterministic and Windows-safe
The Reports workflow SHALL create an ASCII-safe suggested filename using the `AulaRaiz` file brand, report type, report subject/group and generation date.

#### Scenario: Student/group name contains filename-invalid characters
- **WHEN** a filename suggestion is created
- **THEN** invalid Windows filename characters are replaced or removed before the suggestion is shown

### Requirement: PDF generation does not log report contents
Technical failures SHALL NOT log PDF body text, student observations, family agreements or other report contents.

#### Scenario: PDF render fails
- **WHEN** an infrastructure error occurs
- **THEN** the user receives an operational error without dumping report contents

### Requirement: Initial PDF scope is limited to existing report models
The first PDF feature SHALL NOT silently implement period grades, dedicated attendance-only reports, project-completion reports, family-meeting summaries or direct printer integration.

#### Scenario: PDF feature is completed
- **WHEN** this change is accepted
- **THEN** those additional output/report types remain separate roadmap work
