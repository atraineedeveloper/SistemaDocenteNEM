## ADDED Requirements

### Requirement: Teacher-selected export never mutates application data
The system SHALL export selected group information without modifying SQLite, group history, student state, attendance, projects, activities, evaluation or follow-up records.

#### Scenario: Teacher cancels before choosing a destination
- **WHEN** the export workflow is closed before a file is published
- **THEN** no application data changes and no successful export is reported

#### Scenario: Export serialization fails
- **WHEN** file generation fails
- **THEN** the persisted application state remains unchanged

### Requirement: XLSX and CSV export formats have explicit shapes
The system SHALL support complete multi-sheet `.xlsx` group export and focused UTF-8 `.csv` export. A CSV export SHALL contain exactly one selected tabular dataset, while an XLSX export MAY contain multiple selected datasets as separate worksheets.

#### Scenario: Teacher selects complete XLSX
- **WHEN** multiple datasets are selected for XLSX export
- **THEN** each selected dataset is written to its own worksheet in one workbook

#### Scenario: Teacher selects CSV
- **WHEN** CSV is the chosen format
- **THEN** the workflow requires exactly one focused dataset before export can continue

### Requirement: Complete group workbook uses stable semantic worksheets
A complete XLSX export SHALL support selected worksheets for `Contexto`, `Alumnos`, `Asistencia`, `Proyectos`, `Actividades`, `Evaluacion` and optional sensitive `Seguimiento` data.

#### Scenario: Follow-up is not selected
- **WHEN** the teacher exports a complete group workbook with default privacy choices
- **THEN** no `Seguimiento` worksheet is created

#### Scenario: Teacher deselects a dataset
- **WHEN** a dataset is not included in the final selection
- **THEN** its worksheet is omitted from the workbook

### Requirement: Structured group and NEM values are exported explicitly
The system SHALL export structured group/school/NEM values from maintained models and SHALL NOT derive authoritative values by parsing the group's display name.

Where applicable, human-readable output SHALL include served grades, unigrade/multigrade modality, NEM phase or phases, school organization, student grade, project methodology, project/activity target grades and activity formative field.

#### Scenario: Multigrade group is exported
- **WHEN** a group serves more than one primary grade
- **THEN** export identifies the served grades and derived multigrade/NEM-phase context using structured values

#### Scenario: Project planning metadata is exported
- **WHEN** projects and activities are included
- **THEN** methodology, formative field and target-grade labels are human readable rather than enum numeric values

### Requirement: Student export uses the supported student model
Student export SHALL support list number, display name, structured surnames/given names, grade, birth date, derived age when available, gender, admission date and active/inactive state. Pedagogical observations SHALL be excluded by default and included only through an explicit option. CURP SHALL NOT be exported because the product does not store it.

#### Scenario: Default student export
- **WHEN** the teacher exports students without enabling sensitive observations
- **THEN** student pedagogical observations are absent from the output

#### Scenario: Teacher explicitly includes student observations
- **WHEN** the teacher enables the pedagogical-observations option
- **THEN** those observations are exported and the workflow displays a sensitive-content warning

### Requirement: Attendance export is normalized and period-scoped
Attendance export SHALL use one row per applicable student/date record and SHALL allow the teacher to choose an inclusive date range.

The output SHALL include date, list number, student, grade and attendance state, and SHALL preserve applicable historical roster semantics rather than inventing records for dates where the student was not part of the stored attendance context.

#### Scenario: Teacher selects a monthly date range
- **WHEN** attendance is exported for an inclusive start/end date
- **THEN** only stored/applicable attendance records inside that range are exported

#### Scenario: Student joined after earlier attendance history
- **WHEN** the selected range includes dates before the student joined the relevant historical roster
- **THEN** export does not fabricate attendance rows for those earlier dates

### Requirement: Projects and activities export preserves planning metadata
Project export SHALL produce one row per selected project and activity export SHALL produce one row per selected activity, including parent relationship, dates, lifecycle or maintained status, NEM methodology/formative field, target grades and maintained descriptive metadata appropriate to the dataset.

#### Scenario: Selected project scope
- **WHEN** the teacher limits export to one project
- **THEN** project-dependent activities/evaluation rows are limited to that project

### Requirement: Evaluation export preserves delivery and formative result meaning
Evaluation export SHALL produce one row per applicable student/activity pair and SHALL preserve the application's teacher-facing distinction among pending, delivered-awaiting-evaluation, non-delivery and formative achievement results. Evaluation observations SHALL be excluded by default unless explicitly enabled.

#### Scenario: Delivered but not yet evaluated work
- **WHEN** an applicable student/activity row is delivered but has no achievement level yet
- **THEN** export identifies it as delivered awaiting evaluation rather than as non-delivery or a fabricated achievement result

#### Scenario: Historical non-applicable student/activity pair
- **WHEN** a student was not part of an activity's historical roster
- **THEN** the export does not misrepresent that pair as a failed/non-delivered submission

### Requirement: Sensitive follow-up export is explicit opt-in
Student follow-up export SHALL be off by default and SHALL require an explicit teacher choice before sensitive pedagogical/family follow-up data is written to XLSX or CSV.

#### Scenario: Teacher enables follow-up
- **WHEN** follow-up is selected for export
- **THEN** the workflow displays a visible warning that the destination may contain sensitive personal/pedagogical information

#### Scenario: Teacher keeps defaults
- **WHEN** the teacher exports without enabling follow-up
- **THEN** follow-up data is absent from the output

### Requirement: XLSX output contains values, not executable workbook content
The XLSX writer SHALL create ordinary macro-free `.xlsx` workbooks and SHALL write exported values without creating formulas, macros or executable workbook content.

Dates SHALL be represented as deterministic spreadsheet date values or equivalent safe values with stable display formatting. Spanish/Unicode text SHALL be preserved.

#### Scenario: Source text begins with an equals sign
- **WHEN** an application text value begins with `=`
- **THEN** the generated XLSX stores it as text/value content rather than a formula

### Requirement: CSV output is UTF-8, quoted correctly and formula-safe
CSV export SHALL use UTF-8 with BOM, deterministic comma delimiting and standard quoting for commas, quotes and embedded newlines. Text values beginning with spreadsheet formula markers such as `=`, `+`, `-` or `@` SHALL be escaped so opening the CSV in spreadsheet software does not execute them as formulas.

Dates SHALL be emitted as ISO `yyyy-MM-dd` text.

#### Scenario: Observation contains comma and newline
- **WHEN** a selected exported text field contains commas, quotes or line breaks
- **THEN** the CSV remains structurally valid and re-reading it yields one logical field value

#### Scenario: Text resembles a spreadsheet formula
- **WHEN** an exported text value begins with a formula marker
- **THEN** CSV serialization neutralizes formula execution while preserving the visible text meaning

### Requirement: Export publication is atomic at the destination level
The file writer SHALL serialize to a temporary sibling file and SHALL publish/replace the requested destination only after successful completion. On failure, it SHALL remove the temporary artifact when possible and SHALL NOT report the destination as successfully exported.

#### Scenario: Writer fails after creating temporary output
- **WHEN** serialization throws before publication
- **THEN** no new partial destination file is presented as a successful export

#### Scenario: Export completes
- **WHEN** serialization and publication succeed
- **THEN** the final destination exists and the result model reports it

### Requirement: File-name suggestions are deterministic and Windows-safe
Application SHALL suggest human-readable Windows-safe file names derived from structured group/scope metadata and current export date, while allowing the teacher to change the name in the native save dialog.

#### Scenario: Group display/context contains invalid filename characters
- **WHEN** suggested metadata contains characters invalid in Windows file names
- **THEN** the suggestion replaces/removes those characters without changing stored group data

### Requirement: Export workflow exposes content, scope, destination and result stages
The WPF workflow SHALL use one dedicated export experience with visible stages equivalent to `Contenido → Alcance → Archivo → Resultado` and SHALL NOT place XLSX/CSV serialization logic in code-behind.

#### Scenario: Selected dataset needs no period
- **WHEN** only current-snapshot data such as students is selected
- **THEN** irrelevant date-range controls are not required to complete export

#### Scenario: Attendance is selected
- **WHEN** attendance is included
- **THEN** the workflow exposes an inclusive date range before saving

### Requirement: Export result is reported only after successful publication
A successful export result SHALL include destination path, format, target group identity, included datasets/sheets, row counts and whether sensitive content was included. The system SHALL NOT report success before the destination file has been published.

#### Scenario: Multi-sheet workbook succeeds
- **WHEN** the complete XLSX workbook is published
- **THEN** the result identifies the created file and selected sheet/dataset row counts

### Requirement: Exported personal data stays out of technical logs
The export workflow SHALL NOT write raw exported rows, workbook contents, CSV contents or unrelated student personal data to technical logs. Error messages SHALL provide the minimum operational context needed to correct the export problem.

#### Scenario: Destination write fails
- **WHEN** an export cannot be written to the requested destination
- **THEN** the diagnostic identifies the file-operation problem without dumping exported student rows or sensitive follow-up content

### Requirement: Export is not backup or restore
The export workflow SHALL NOT claim that XLSX/CSV files can restore the application database and SHALL NOT include raw SQLite/application configuration as part of this change.

#### Scenario: Teacher opens export workflow
- **WHEN** the workflow explains output purpose
- **THEN** it describes XLSX/CSV as data export and keeps backup/restore as a separate recovery capability
