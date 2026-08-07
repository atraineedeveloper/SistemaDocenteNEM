## ADDED Requirements

### Requirement: Preview before persistence
The system SHALL parse and validate a selected student import file without mutating SQLite or the target `Grupo` until the teacher explicitly confirms the final included rows.

#### Scenario: Teacher opens a valid file
- **WHEN** a teacher selects a supported XLSX or CSV file
- **THEN** the system shows a preview workflow and the persisted group remains unchanged

#### Scenario: Teacher cancels import
- **WHEN** the teacher closes or cancels the import before confirmation
- **THEN** no student is added, edited, activated or deactivated

### Requirement: Supported XLSX and CSV sources
The system SHALL accept `.xlsx` and `.csv` student-roster sources through a format adapter that produces one neutral tabular document model for upper layers.

#### Scenario: XLSX contains multiple non-empty worksheets
- **WHEN** the selected workbook contains more than one usable worksheet
- **THEN** the teacher can choose which worksheet to preview without importing rows from the others

#### Scenario: CSV uses a supported delimiter
- **WHEN** a UTF-8 CSV uses an unambiguous comma, semicolon or tab delimiter and valid quoted fields
- **THEN** the reader projects its headers and rows into the same neutral tabular model used for XLSX

#### Scenario: CSV delimiter is ambiguous
- **WHEN** delimiter detection is not deterministic
- **THEN** the system requires an explicit delimiter choice before mapping rows

### Requirement: Explicit source-column mapping
The system SHALL let the teacher map source columns only to supported student destination fields and SHALL ignore unmapped source columns.

Supported destination fields SHALL include list number, full display name, first surname, second surname, given names, birth date, gender, admission date, primary grade and pedagogical observations. CURP SHALL NOT be an import destination field.

#### Scenario: Workbook contains unrelated columns
- **WHEN** source columns are left unmapped
- **THEN** their values are not persisted in any student field

#### Scenario: Source contains a CURP column
- **WHEN** a file contains a CURP column
- **THEN** the import mapping does not offer CURP as a destination and the value is ignored unless the teacher maps other supported columns from the same row

### Requirement: Deterministic student-name construction
Each included row SHALL produce a non-empty student display name. When mapped full-name and structured-name data both exist, the system SHALL preserve structured fields and use the mapped full name as the preferred display name; when full name is blank, it SHALL build the display name from available structured-name fields.

#### Scenario: Full name and structured names are present
- **WHEN** a row maps `Nombre completo`, `Primer apellido`, `Segundo apellido` and `Nombres`
- **THEN** the structured fields are preserved and the explicit full-name value is used as the display name

#### Scenario: Only structured names are present
- **WHEN** the mapped structured-name fields can produce a non-empty name and full name is blank or unmapped
- **THEN** the system constructs the display name deterministically from those structured fields

#### Scenario: Name data is insufficient
- **WHEN** the mapped row cannot produce a non-empty display name
- **THEN** the row is `Invalid` and cannot be included until corrected

### Requirement: Context-aware primary-grade resolution
The system SHALL resolve student grade conservatively using the target group's structured served-grade context and SHALL NOT infer an ambiguous grade.

#### Scenario: Unigrade group and missing source grade
- **GIVEN** the target group has exactly one configured served grade
- **WHEN** an import row has no explicit grade
- **THEN** preview defaults the row to that one grade and identifies it as a group-derived default

#### Scenario: Multigrade group and missing source grade
- **GIVEN** the target group has multiple configured served grades
- **WHEN** an import row has no explicit grade
- **THEN** the row is `NeedsReview` until the teacher selects a real served grade or excludes the row

#### Scenario: Explicit grade is outside configured group scope
- **GIVEN** the target group has a non-empty configured served-grade set
- **WHEN** a row resolves to a real primary grade outside that set
- **THEN** the row is `NeedsReview` and cannot be committed without correction or exclusion

#### Scenario: Grade text is ambiguous
- **WHEN** a source grade cannot be parsed deterministically to one real `GradoPrimaria`
- **THEN** no grade is guessed and the row requires review

### Requirement: Preview-row validation states
Every source row SHALL have exactly one preview state: `Ready`, `NeedsReview`, `Invalid` or `Excluded`. Commit SHALL include only `Ready` rows and SHALL be disabled while any non-excluded row remains `NeedsReview` or `Invalid`.

#### Scenario: Teacher excludes an invalid row
- **WHEN** the teacher explicitly excludes an `Invalid` or `NeedsReview` row
- **THEN** that row no longer blocks commit and is not persisted

#### Scenario: Teacher corrects a review row
- **WHEN** editable normalized values are corrected so all required rules pass
- **THEN** the row becomes `Ready` without modifying the source file

### Requirement: Hard list-number conflicts
The system SHALL treat duplicate active list numbers as blocking conflicts both within the included import rows and against the fresh target group.

#### Scenario: Two included rows share a list number
- **WHEN** two included preview rows have the same list number
- **THEN** they cannot both be `Ready` until one is corrected or excluded

#### Scenario: List number is already active in the group
- **WHEN** an included row uses a list number held by an existing active student
- **THEN** the row cannot be committed with that number

### Requirement: Probable duplicate review without overwrite
The system SHALL detect deterministic probable-duplicate signals against active and inactive students, including exact normalized name matches and stronger matching structured-name/birth-date evidence. A probable duplicate SHALL require review and SHALL NOT update, overwrite or reactivate the existing student automatically.

#### Scenario: Existing student has the same normalized name
- **WHEN** an import row matches an existing active or inactive student's normalized name
- **THEN** the row is `NeedsReview` and identifies the probable existing match without modifying it

#### Scenario: Teacher intentionally imports a probable duplicate as new
- **WHEN** the teacher explicitly chooses to import the reviewed row as a new student and no hard invariant is violated
- **THEN** the row may become `Ready` while the existing record remains unchanged

### Requirement: Preview corrections do not rewrite source files
The system SHALL allow correction of normalized preview values such as list number, name, dates, gender and grade without modifying the selected XLSX or CSV source file.

#### Scenario: Teacher corrects a date in preview
- **WHEN** the teacher changes an ambiguous or invalid parsed date to a valid value
- **THEN** validation uses the corrected preview value and the original source file bytes remain unchanged

### Requirement: Conservative date and gender parsing
The system SHALL accept deterministic native XLSX values and documented textual date/gender forms and SHALL classify ambiguous values for review instead of guessing.

#### Scenario: Native XLSX date
- **WHEN** a mapped XLSX cell contains a valid native spreadsheet date value
- **THEN** preview converts it to the corresponding `DateOnly` value

#### Scenario: Ambiguous textual date
- **WHEN** a date string cannot be interpreted deterministically under the supported import formats
- **THEN** the affected field requires review rather than silently choosing one date interpretation

#### Scenario: Ambiguous one-letter gender value
- **WHEN** a source gender value is an ambiguous abbreviation such as `M`
- **THEN** the system does not guess a gender and requires review or correction

### Requirement: Fresh validation before commit
Immediately before persistence, the system SHALL reload the target group and relevant structured grade context and SHALL revalidate every included row against that fresh state.

#### Scenario: Group changes after preview
- **WHEN** another operation occupies a list number or changes relevant group-grade context after preview
- **THEN** confirmation stops without persistence and returns the affected rows to review

#### Scenario: Group no longer exists
- **WHEN** the target group cannot be reloaded at confirmation time
- **THEN** the import fails without creating students elsewhere

### Requirement: One aggregate save for confirmed import
A confirmed import SHALL apply every included row to one in-memory `Grupo` aggregate and SHALL invoke the group persistence boundary exactly once after all rows pass validation. The SQLite adapter SHALL therefore commit all imported students in one transaction or none of them.

#### Scenario: Confirm ten ready rows
- **WHEN** ten rows are confirmed and all remain valid against the fresh group
- **THEN** all ten are added to the in-memory group and the aggregate is persisted once

#### Scenario: One row fails before persistence
- **WHEN** any included row fails final domain validation while constructing the aggregate
- **THEN** the persistence boundary is not called and no imported student is stored

#### Scenario: SQLite save fails
- **WHEN** the single aggregate persistence operation fails
- **THEN** the previously persisted group remains unchanged and the UI does not report a partial import success

### Requirement: Existing history is not rewritten
Imported students SHALL be ordinary newly active students in the target group and SHALL NOT be inserted retroactively into historical attendance, project activities, evaluation rosters or student-record evidence.

#### Scenario: Import after an existing activity
- **WHEN** a student is imported after an activity's historical roster was created
- **THEN** the student remains non-applicable to that historical activity unless another explicit product workflow says otherwise

### Requirement: Import result counts
Preview SHALL expose counts for `Ready`, `NeedsReview`, `Invalid` and `Excluded`. A successful confirmed import SHALL report at least imported and excluded counts and SHALL not count unresolved or failed rows as imported.

#### Scenario: Mixed preview rows
- **WHEN** preview contains ready, review, invalid and excluded rows
- **THEN** the displayed counts match the current row states after every correction/exclusion

#### Scenario: Successful commit
- **WHEN** all non-excluded rows are ready and persistence commits
- **THEN** the result reports the number of students actually added and the number explicitly excluded

### Requirement: Personal data stays out of technical logs
The import workflow SHALL NOT write raw spreadsheet rows, full workbook contents or unrelated student personal data to technical logs. Error messages SHALL identify the source row and affected field/problem with the minimum data necessary for correction.

#### Scenario: Malformed row triggers an error
- **WHEN** a row contains an invalid required value
- **THEN** the diagnostic identifies the row number and field/rule without logging the entire row or workbook