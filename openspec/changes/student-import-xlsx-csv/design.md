# Design: student XLSX/CSV import

## Context

Student data is owned by the `Grupo` aggregate. `GestionGrupoCasosUso` loads a group, mutates it in memory and persists it through `IAlmacenamientoGrupos`. `PersistenciaGrupoSqlite.Guardar` writes the group and all students in one SQLite transaction. This existing aggregate boundary is suitable for an atomic confirmed import and avoids adding a special bulk SQL path.

The structured NEM/multigrade work adds `GradoPrimaria` to students and one or more served grades to group context. Import must use those values explicitly rather than infer grade from a group name.

## Decisions

### 1. Import is a preview-first two-phase workflow

File parsing and persistent commit are separate operations.

Phase A — preview:

1. select `.xlsx` or `.csv`;
2. select a worksheet when an XLSX contains multiple usable sheets;
3. inspect headers and map source columns to supported student fields;
4. normalize and validate rows;
5. correct normalized values or exclude rows;
6. resolve every `NeedsReview` row explicitly.

Phase B — commit:

1. reload the target group;
2. re-run duplicate/list-number/grade validation against the fresh group;
3. apply all included rows to one in-memory `Grupo` aggregate;
4. call `IAlmacenamientoGrupos.Guardar` exactly once;
5. return the committed result only after persistence succeeds.

Preview never mutates the aggregate or SQLite.

### 2. Add a dedicated interchange boundary

Spreadsheet/CSV parsing is not SQLite responsibility and should not be embedded in WPF code-behind. The implementation should introduce a small `SistemaDocente.Interchange` production project (or equivalently isolated adapter project if naming changes during implementation) for tabular file adapters.

Application declares neutral ports/models such as:

```text
ILectorImportacionTabular
DocumentoTabular
HojaTabular
FilaTabular
```

The interchange adapter implements those ports for XLSX and CSV. Application owns mapping semantics, validation orchestration, duplicate classification and commit. Presentation owns editable preview state. WPF owns the native file picker and dialog/window behavior.

This boundary is intentionally reusable by the later export change without coupling file formats to SQLite.

### 3. Supported destination fields are explicit

The mapping UI supports only fields that already exist in the student model:

- `Número de lista`
- `Nombre completo`
- `Primer apellido`
- `Segundo apellido`
- `Nombres`
- `Fecha de nacimiento`
- `Género`
- `Fecha de ingreso`
- `Grado`
- `Observaciones`

`CURP` is not a destination field.

A source workbook may contain additional columns; unmapped columns are ignored.

A valid row requires:

- a valid list number;
- enough name data to produce a non-empty display name;
- a resolvable grade according to the rules below.

When both `Nombre completo` and structured-name columns are mapped, structured fields are preserved and `Nombre completo` is the preferred display-name source. If `Nombre completo` is blank, the display name is built from mapped structured-name fields.

### 4. Grade resolution is context-aware and conservative

The import use case receives the active group's structured served-grade context.

- Exactly one configured served grade + blank source grade → default that one grade and mark the value as `DefaultedByGroup` in preview.
- Multiple configured served grades + blank source grade → `NeedsReview`; the teacher must choose a grade or exclude the row.
- No configured served grades + blank source grade → `NeedsReview`; no grade is guessed.
- Explicit real grade that is outside a non-empty configured served-grade set → `NeedsReview` and requires correction/exclusion.
- Explicit grade parsing may reuse deterministic `CatalogoNemPrimaria` parsing rules; ambiguous text is never guessed.

The final committed student always carries a real grade 1–6 in the updated import flow.

### 5. Row validation has distinct severity

Each preview row has one state:

- `Ready` — valid and safe to include;
- `NeedsReview` — potentially valid but requires an explicit teacher decision/correction;
- `Invalid` — cannot be included with current values;
- `Excluded` — explicitly omitted by the teacher.

Commit is enabled only when every included row is `Ready`. A `NeedsReview` or `Invalid` row must be corrected to `Ready` or explicitly excluded.

Per-field messages are preferred over one opaque row message.

### 6. Duplicate handling never overwrites

Hard conflicts:

- an included row duplicates another included row's active list number;
- an included row uses a list number already held by an active student in the target group.

These rows cannot become `Ready` until corrected or excluded.

Probable duplicates are review signals, not automatic matches. Initial rules should include:

- exact normalized display-name match against an existing active or inactive student;
- exact normalized structured-name match when enough structured fields are available;
- stronger signal when matching name data also shares birth date.

A probable duplicate becomes `NeedsReview`. The teacher may explicitly choose `Import as new` if no hard invariant is violated, or exclude the row. Import never edits/reactivates the matched existing record.

Duplicate rules are deterministic and testable; fuzzy/AI identity matching is out of scope.

### 7. Parsing is format-aware but produces neutral cells

#### XLSX

- Read workbook values, not presentation formatting.
- Ignore formulas/macros as executable content; import only the resulting stored/displayed cell value supported by the reader.
- Offer non-empty worksheets for selection.
- Preserve row numbers for diagnostics.
- Accept native Excel date/number cells when they can be converted deterministically.

#### CSV

- Support UTF-8 and UTF-8 with BOM.
- Support quoted fields and escaped quotes.
- Detect comma, semicolon or tab delimiter from the header/data sample when unambiguous; otherwise request an explicit choice.
- Treat the file as one logical sheet.

Both formats expose the same `DocumentoTabular` abstraction to Application/Presentation.

### 8. Normalization and correction happen outside the source file

Preview stores raw text/value plus normalized editable student values. Corrections modify only the preview model. The source file is never rewritten.

Dates accept native XLSX dates plus a small deterministic textual set, including ISO `yyyy-MM-dd` and localized day/month/year forms. Ambiguous dates are `NeedsReview` rather than guessed.

Gender accepts explicit Spanish labels such as `Mujer`/`Femenino`/`F` and `Hombre`/`Masculino`/`H`, case-insensitively. Ambiguous one-letter values such as `M` are not auto-resolved.

### 9. Confirmed import reuses the group aggregate transaction

Application should not introduce a row-by-row persistence API. It reloads the group, revalidates the final included rows and calls `Grupo.AgregarEstudiante` for each one in memory.

Only after all rows pass does it call `IAlmacenamientoGrupos.Guardar(grupo)` once. SQLite already persists the full group aggregate inside one transaction. Therefore any domain or persistence failure leaves the previously persisted group unchanged.

This design also guarantees that an imported student does not appear retroactively in historical activity rosters.

### 10. Fresh validation protects against preview staleness

The preview is advisory. The target group may change before confirmation. Commit reloads the current group and rechecks:

- target group still exists;
- active list-number availability;
- probable-duplicate signals relevant to explicit review decisions;
- configured grade compatibility when context changed;
- every included row still satisfies Core invariants.

If the current group conflicts with the preview, commit stops without persistence and returns rows requiring a refreshed review.

### 11. WPF workflow

Use one dedicated import window/dialog with visible steps rather than multiple nested message boxes:

```text
Archivo → Hoja/columnas → Vista previa → Confirmación → Resultado
```

Preview should show:

- source row number;
- list number;
- student name;
- grade;
- validation state;
- concise issue summary;
- include/exclude control.

The teacher can filter to `NeedsReview`/`Invalid`, correct mapped values and return to column mapping without losing the source document snapshot.

The final confirmation states how many students will be added and emphasizes that existing students will not be modified.

### 12. Result model

A successful commit returns at least:

- number imported;
- number explicitly excluded;
- target group identity;
- committed student identifiers/summaries needed to refresh Presentation.

Preview exposes counts for Ready, NeedsReview, Invalid and Excluded.

No success result is returned before the single aggregate save commits.

## Risks and mitigations

- **Spreadsheet column names vary widely.** Use explicit mapping with optional header suggestions; suggestions never commit data.
- **Regional CSV conventions vary.** Support common delimiters/UTF-8 and request explicit resolution when detection is ambiguous.
- **Names are not stable unique identifiers.** Treat name matches as review signals only; never overwrite automatically.
- **Multigrade files may omit grade.** Block unresolved rows instead of guessing.
- **Preview may become stale.** Reload/revalidate immediately before commit.
- **A parser dependency could spread through the solution.** Isolate it in the interchange adapter project behind Application ports.
- **Partial imports could leave inconsistent roster data.** Mutate one in-memory group and persist once.
- **Personal data could leak into logs.** Do not log raw rows or workbook contents.

## Validation

- Core: existing group/student invariants remain the authority.
- Application: mapping outcomes, grade resolution, duplicate classification, preview state and atomic commit behavior.
- Interchange: real CSV/XLSX fixtures for quoting, sheets, dates, blank rows, malformed files and neutral tabular projection.
- Presentation: step navigation, editable corrections, filters, counts and commit gating.
- WPF: file-picker integration, mapping/preview structure, keyboard operation and accessible state labels.
- Data integration: confirmed multi-row import persists once and rolls back completely on induced failure.
- Full Windows CI plus manual import tests using fictitious sample workbooks before merge.