# Group data export

## Purpose

Group data export creates teacher-facing `.xlsx` or `.csv` files from the current local application data. It is intended for review, school administration, analysis and controlled sharing.

Export is **not backup or restore**. XLSX/CSV files do not preserve the complete application state and are not presented as recovery packages. Backup/restore remains a separate future capability.

## Supported formats

### Excel workbook (`.xlsx`)

A workbook may contain multiple selected worksheets:

- `Contexto`
- `Alumnos`
- `Asistencia`
- `Proyectos`
- `Actividades`
- `Evaluacion`
- `Seguimiento` — optional and sensitive

The writer creates an ordinary macro-free workbook. Exported application text is written as values, never as spreadsheet formulas. Header rows are frozen and dates are written as spreadsheet date values with deterministic display formatting.

### CSV (`.csv`)

CSV exports exactly one selected dataset per file. The writer uses:

- UTF-8 with BOM for practical Excel compatibility;
- comma as delimiter;
- standard quoting for commas, quotes and embedded newlines;
- ISO `yyyy-MM-dd` dates;
- formula-injection neutralization for text beginning with spreadsheet formula markers such as `=`, `+`, `-` or `@`.

The neutralization happens only in the exported CSV representation; it does not modify stored application data.

## Export workflow

The WPF workflow is:

```text
Contenido → Alcance → Archivo → Resultado
```

1. **Contenido** — choose Excel or CSV and the datasets to include.
2. **Alcance** — choose an attendance period and/or project scope when those filters are relevant.
3. **Archivo** — review the suggested name and row-count summary, then choose the destination with the native Windows save dialog.
4. **Resultado** — shown only after the final destination file has been fully published.

Closing or cancelling before publication does not modify application data.

## Default privacy choices

The default complete Excel export includes:

- context;
- students;
- attendance;
- projects;
- activities;
- evaluation.

The following are off by default:

- student pedagogical observations;
- evaluation observations;
- student follow-up (`Seguimiento`).

Enabling any of those options makes the workflow display a visible sensitive-content warning. Teachers should store/share such exports according to applicable personal-data and school-handling requirements.

Raw exported rows and workbook/CSV contents are not written to technical logs.

## Dataset semantics

### Context

The `Contexto` worksheet exports structured values maintained by the group context rather than parsing the display name. Depending on configured data it includes:

- school name and CCT;
- entity, municipality/alcaldía and locality;
- school organization;
- school cycle;
- group key and shift;
- served primary grades;
- unigrade/multigrade modality;
- derived NEM phase(s);
- responsible teacher and responsibility dates;
- entry/exit times.

### Students

The student dataset includes:

- list number;
- display name;
- first surname;
- second surname;
- given names;
- primary grade;
- birth date;
- derived age when available;
- gender;
- admission date;
- active/inactive state.

Pedagogical observations are optional. CURP is absent because the product does not store it.

### Attendance

Attendance uses a normalized row-per-student-per-date representation:

- date;
- list number;
- student;
- grade;
- attendance state.

An inclusive date range is required. Export reads the stored historical attendance/applicability data; it does not fabricate earlier rows for students who were not part of a historical roster.

### Projects

Project rows include maintained project metadata such as:

- name and description;
- start/end dates;
- lifecycle state;
- NEM methodology;
- target grades;
- observations.

### Activities

Activity rows include:

- parent project;
- title and description;
- activity date/state;
- NEM formative field;
- target grades;
- general observations.

### Evaluation

Evaluation exports only applicable student/activity rows from each activity's historical roster. It includes:

- project/activity/date;
- student/list number/grade;
- a teacher-facing unified result;
- explicit delivery state;
- explicit achievement level.

The unified result preserves important distinctions such as:

- pending;
- delivered and awaiting evaluation;
- no delivery;
- Domina;
- Suficiente;
- En proceso;
- Requiere apoyo.

Evaluation observations are optional.

### Follow-up

`Seguimiento` is sensitive and off by default. When enabled, it exports pedagogical notes and tutor agreements already maintained in the student record, including dates and follow-up dates where applicable.

## Project scope

For project-dependent datasets (projects, activities and evaluation), the workflow supports:

- all projects in the group; or
- one selected project.

Selecting one project limits dependent activity/evaluation rows to that project.

## Suggested file names

File-name suggestions use structured context where available. Examples:

```text
Grupo_4A_2026-2027_2026-08-08.xlsx
Grupo_Multigrado_1-2-3_2026-2027_2026-08-08.xlsx
Alumnos_4A_2026-2027_2026-08-08.csv
```

Invalid Windows filename characters are sanitized. The teacher may change the name in the native save dialog.

## Atomic publication

The writer does not stream directly into the visible destination. It:

1. writes a temporary sibling file;
2. completes and closes serialization;
3. moves/replaces the requested destination only after success;
4. removes the temporary file on failure when possible.

A failed serialization therefore does not leave a new partial destination file that looks like a successful export.

## Troubleshooting

### The export cannot continue from Content

Ensure at least one dataset is selected. CSV requires exactly one dataset.

### Attendance scope cannot continue

Attendance requires valid `Desde` and `Hasta` dates, with the start date not after the end date.

### The destination cannot be written

Choose an existing folder where the current Windows user can write files. If another program has locked the destination file, close that file and retry.

### A CSV text value starts with an apostrophe

Formula-leading text is deliberately prefixed in the CSV representation so spreadsheet software treats it as data instead of executable formula content.

### I need to restore the application from an export

Do not use XLSX/CSV as a recovery mechanism. Backup/restore is intentionally designed as a separate capability because it must preserve the complete application state, not only selected tabular data.
