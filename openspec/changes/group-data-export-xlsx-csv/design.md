# Design: group data XLSX/CSV export

## Context

`main` now contains the structured NEM/multigrade model plus safe student XLSX/CSV import. The import change introduced `SistemaDocente.Interchange` as the adapter boundary for spreadsheet formats. Export should reuse that boundary instead of putting Open XML or CSV formatting into WPF, Application or SQLite adapters.

The application already exposes domain/application projections for students, attendance, projects, activities, evaluation and student follow-up. Export should read through those existing boundaries rather than issue special SQL from the file writer.

## Decisions

### 1. Export and backup remain separate capabilities

Export produces teacher-facing files containing selected data. Backup/restore will later preserve application state for disaster recovery.

An XLSX/CSV export SHALL NOT be presented as a restore source, and the export workflow SHALL NOT copy the SQLite database or application configuration.

### 2. Application owns export meaning; Interchange owns file syntax

Application introduces neutral output contracts such as:

```text
DocumentoTabularSalida
HojaTabularSalida
ColumnaTabularSalida
FilaTabularSalida
CeldaTabularSalida
IExportadorTabular
```

Application decides:

- which datasets are included;
- their columns and semantic ordering;
- group/date/project filters;
- privacy-sensitive inclusion choices;
- NEM labels and values;
- output file-name suggestion.

`SistemaDocente.Interchange` decides how those neutral values become XLSX or CSV bytes.

Presentation manages selection state and validation. WPF owns the native save-file picker and dialog/window behavior.

### 3. Two export shapes are supported

#### Complete group XLSX

A complete group workbook may contain these sheets, depending on selection and available data:

1. `Contexto`
2. `Alumnos`
3. `Asistencia`
4. `Proyectos`
5. `Actividades`
6. `Evaluacion`
7. `Seguimiento` — optional and sensitive

The initial default selection includes Context, Students, Attendance, Projects, Activities and Evaluation. `Seguimiento` is off by default.

A worksheet is omitted when the teacher deselects its dataset. Empty selected datasets may still produce a sheet with headers and an explanatory empty-state row when that improves clarity.

#### Focused CSV

CSV is single-table by nature. The workflow therefore exports one selected dataset per CSV file. It does not zip multiple CSV files in this change.

Supported focused datasets:

- students;
- attendance;
- projects;
- activities;
- evaluation;
- optional follow-up.

Context metadata that is useful to interpret a focused CSV may be repeated as explicit columns where appropriate rather than represented as a second table.

### 4. Group context and NEM metadata are explicit

Exports should not depend on parsing `NombreVisible`.

Where applicable, output includes structured values such as:

- school name and CCT;
- entity, municipality and locality;
- school organization;
- school cycle;
- group key;
- served grades;
- unigrade/multigrade modality;
- NEM phase or phases;
- teacher responsibility;
- student grade;
- project methodology;
- project/activity target grades;
- activity formative field.

Human-readable labels are exported rather than enum numeric values.

### 5. Dataset-specific columns

#### Students

Initial columns:

- list number;
- display name;
- first surname;
- second surname;
- given names;
- grade;
- birth date;
- age when derivable;
- gender;
- admission date;
- active/inactive state;
- pedagogical observations only when explicitly included.

CURP remains absent because the product does not store it.

#### Attendance

Use a normalized row-per-student-per-date shape suitable for filtering and analysis:

- date;
- list number;
- student;
- grade;
- attendance state;
- applicable/active-history semantics where needed.

The teacher can select an inclusive date range. The export does not fabricate attendance rows outside stored/applicable history.

#### Projects

One row per project with project identity, name, dates, lifecycle state, NEM methodology, target grades and notes/description fields appropriate for export.

#### Activities

One row per activity with parent project, activity date/title, formative field, target grades and other maintained activity metadata.

#### Evaluation

Use one row per applicable student/activity pair, preserving the distinction between delivery and achievement internally while exposing the same teacher-facing result vocabulary used by the application. Include observations only when that dataset option is enabled.

#### Follow-up

Follow-up may contain sensitive pedagogical/family information. It is excluded by default. When explicitly enabled, it uses a dedicated sheet/CSV and a visible warning before export.

The first implementation should export pedagogical follow-up entries that are already appropriate for the teacher's own local records; it should not silently broaden into every future sensitive attachment or evidence type.

### 6. Period/content filters are explicit

Students and Context represent a current snapshot and do not need a date filter.

Attendance requires a date range; defaults may use the current school cycle or available data bounds but must remain visible/editable.

Projects, Activities and Evaluation may be filtered by project selection and/or date range. The first implementation should prefer a simple visible scope:

- all current group content; or
- selected project; and
- optional inclusive date range for attendance.

Avoid a complicated query builder in the first iteration.

### 7. XLSX output is value-only and teacher-friendly

The XLSX writer SHALL:

- create ordinary `.xlsx` files without macros;
- write text as text, not formulas;
- write dates as native spreadsheet dates with deterministic display formatting;
- create one worksheet per selected dataset;
- use stable worksheet names and sanitize invalid Excel sheet-name characters;
- freeze header rows for tabular sheets;
- apply readable header formatting and sensible column widths;
- preserve Unicode/Spanish accents;
- never rely on formulas for required totals or semantics.

Charts, pivot tables and advanced templates are out of scope.

### 8. CSV output is UTF-8 and formula-safe

CSV SHALL:

- use UTF-8 with BOM for practical Excel compatibility;
- use comma as the initial deterministic export delimiter;
- quote fields according to CSV rules;
- emit dates as ISO `yyyy-MM-dd` text;
- preserve embedded commas, quotes and newlines through quoting;
- escape leading spreadsheet formula markers in text values (`=`, `+`, `-`, `@`) so opening the CSV in spreadsheet software does not execute exported text as a formula.

The formula-safety escape is part of file serialization, not a mutation of application data.

### 9. Export is published atomically

The file writer uses the destination directory but writes to a temporary sibling file first.

1. create/write temporary output;
2. flush and close;
3. replace/move to the requested destination only after successful serialization;
4. remove the temporary file on failure when possible.

A failed export therefore does not leave a destination file that looks successfully complete.

The export workflow never writes SQLite.

### 10. File names are helpful but not authoritative

Application suggests names such as:

```text
Grupo_4A_2026-2027_2026-08-08.xlsx
Grupo_Multigrado_1-2-3_2026-2027_2026-08-08.xlsx
Asistencia_4A_2026-08-01_a_2026-08-31.csv
```

Invalid Windows filename characters are removed/replaced. The teacher can change the name in the native save dialog.

### 11. Privacy choices are visible

The export window shows selected datasets and highlights sensitive options.

Defaults:

- student pedagogical observations: off;
- evaluation observations: off unless specifically requested;
- follow-up: off.

When sensitive content is enabled, the confirmation text states that the resulting file may contain personal/pedagogical information and should be stored/shared appropriately.

No raw exported rows or workbook contents are written to technical logs.

### 12. WPF workflow

Use one dedicated export window rather than multiple message boxes:

```text
Contenido → Alcance → Archivo → Resultado
```

The teacher can:

- choose XLSX complete workbook or focused CSV;
- select datasets;
- select project/date scope when applicable;
- opt into sensitive fields;
- see an estimated row count/summary before saving;
- choose destination using the native Windows save dialog;
- receive a final file path/result summary only after successful publication.

For CSV, selecting a second dataset should either replace the first or be prevented with a clear explanation.

### 13. Result model

A successful export returns at least:

- destination path;
- format;
- group identity;
- included datasets/sheets;
- row counts;
- whether sensitive content was included.

No success is reported before the destination file has been fully published.

## Risks and mitigations

- **Large attendance/evaluation datasets.** Build rows incrementally where practical and test with realistic 30–40 student groups and multi-week data.
- **Sensitive information leaves the local database.** Sensitive datasets/fields are opt-in and visibly warned.
- **CSV formula injection.** Escape formula-leading text during CSV serialization.
- **Spreadsheet formulas/macros could become executable content.** XLSX writer emits values only and never creates macros/formulas.
- **Partial output could be mistaken for success.** Publish through a temporary file and move only after completion.
- **Export logic could duplicate domain semantics.** Application reuses existing cases/projections and human-readable catalog labels.
- **CSV cannot represent multiple datasets.** Restrict CSV to one focused dataset; use XLSX for complete group export.

## Validation

- Application tests for dataset selection, filtering, privacy defaults, row projection, NEM labels and file-name suggestions.
- Interchange tests that open generated XLSX files and re-read CSV values, including Unicode, quotes/newlines, dates and formula-leading text.
- Failure tests proving no misleading destination file remains after serialization failure.
- Presentation tests for format/dataset gating, sensitive-content warnings and result summary.
- WPF tests for export-window construction, native save integration boundary and supported themes/accessibility structure.
- Manual export of fictitious Demo data to XLSX and CSV, followed by opening the generated files in Excel-compatible software before merge.
