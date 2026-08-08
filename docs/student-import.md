# Student XLSX/CSV import

## Purpose

Student import reduces manual roster transcription while keeping the application's existing group aggregate, privacy rules and historical-roster semantics intact. The workflow is intentionally preview-first: selecting or parsing a file never changes SQLite.

The supported workflow is:

```text
File → Sheet / column mapping → Preview and correction → Confirmation → Result
```

The feature adds new students only. It does not update, reactivate, deactivate or merge existing student records.

## Supported files

### XLSX

The interchange adapter reads `.xlsx` workbooks through the Open XML SDK in read-only mode.

- Non-empty worksheets are offered to the teacher for selection.
- Shared strings, inline strings, numbers, booleans and deterministic spreadsheet dates are projected into a neutral tabular model.
- Stored cell values are read; formulas, macros, images and workbook formatting are not executed or imported.
- Empty worksheets and completely blank rows are ignored.
- Corrupt or unsupported workbook content is reported as an import error without exposing unrelated workbook data.

### CSV

CSV input must be UTF-8 or UTF-8 with BOM. The reader supports:

- comma (`,`);
- semicolon (`;`);
- tab;
- quoted fields;
- escaped quotes;
- embedded newlines inside quoted fields.

The reader tries to detect the delimiter deterministically. If comma, semicolon and tab are ambiguous, the wizard stays on the file step and asks the teacher to choose the delimiter explicitly before retrying the same file.

## Importable student fields

The mapping step exposes only fields already supported by the student model:

- list number;
- full display name;
- first surname;
- second surname;
- given names;
- birth date;
- gender;
- admission date;
- primary grade;
- pedagogical observations.

CURP is intentionally not an import destination. Extra source columns remain unmapped and are ignored.

At minimum, an included row must produce a valid positive list number, a non-empty student display name and a resolvable real primary grade before confirmation.

When both a full display name and structured name columns are mapped, the structured components are retained while the explicit full name remains the preferred display name. This import-specific behavior does not change the default manual student editor behavior.

## Grade resolution

Grade resolution uses the structured group context rather than the textual group name.

- In a unigrade group with exactly one configured served grade, a blank source grade is defaulted to that grade and identified internally as a group-derived default.
- In a multigrade group, a blank source grade requires review; the teacher must provide a served grade or exclude the row.
- If no served grade is configured, a blank grade requires review.
- An explicit grade outside the configured served-grade set requires correction or exclusion.
- Ambiguous grade text is never guessed.
- Immediately before commit, grade compatibility is checked again against the current group context. If the context changed after preview, the affected row returns to review.

## Dates and gender

Text dates are intentionally conservative. Supported deterministic forms include ISO `yyyy-MM-dd` and common day/month/year variants such as `dd/MM/yyyy`. Native XLSX dates are accepted directly.

A value that cannot be interpreted deterministically is returned to review rather than guessed.

Gender accepts explicit Spanish values such as:

- `F`, `Femenino`, `Mujer` → `Mujer`;
- `H`, `Hombre`, `Masculino` → `Hombre`;
- blank → `NoEspecificado`.

The single-letter value `M` is considered ambiguous because it can be interpreted differently in real source files; it requires explicit correction.

## Row states

Each preview row has exactly one state:

- **Ready (`Lista`)** — safe to include;
- **Needs review (`Requiere revisión`)** — potentially valid but requires an explicit teacher decision or correction;
- **Invalid (`Inválida`)** — violates a hard requirement and cannot be included as-is;
- **Excluded (`Excluida`)** — explicitly omitted from the confirmed import.

The UI shows text labels for these states and does not rely on color alone. Confirmation is enabled only when every non-excluded row is ready and at least one row remains ready to import.

Corrections are made in the in-memory preview. The XLSX or CSV source file is never rewritten.

## Duplicate rules

### Hard list-number conflicts

A row is invalid when its list number:

- is already used by an active student in the target group; or
- is duplicated by another included import row.

The number must be corrected or the row excluded.

### Probable identity duplicates

Names are not treated as unique identifiers. Exact normalized display-name or structured-name matches are deterministic review signals only. A matching birth date strengthens the warning.

A probable duplicate never updates or reactivates the existing student. The teacher may explicitly choose **Import duplicate as new** when no hard invariant is violated, or exclude the row.

No fuzzy or AI identity matching is used.

## Atomic confirmation

Preview state is advisory because the group can change while the wizard is open. Confirmation therefore performs a fresh read of the group and group context and repeats the relevant checks.

If all included rows remain valid:

1. Application clones the freshly loaded `Grupo` aggregate;
2. every ready student is added to that in-memory clone;
3. if any domain rule fails, persistence is never called;
4. when all rows succeed, `IAlmacenamientoGrupos.Guardar` is invoked exactly once;
5. the existing SQLite group persistence transaction commits all new students or rolls back all of them.

Integration coverage includes an induced SQLite failure on the second imported student and verifies that the first student is rolled back as well.

## Historical behavior

Imported students become normal active students for future classroom work. Import does not retroactively rewrite:

- historical attendance rosters;
- existing project/activity applicability;
- previous evaluation matrices;
- existing student-record evidence.

This preserves the system's established historical-roster semantics.

## Privacy and diagnostics

The import workflow must not write raw spreadsheet rows or workbook contents to technical logs. Diagnostics identify the source row and affected field/rule with only the information needed for correction.

Selecting or cancelling a file before confirmation does not persist preview data. Existing students are never modified implicitly.

## Architecture

The feature is split deliberately:

- **Application** owns neutral tabular contracts, mapping semantics, normalization, duplicate classification, grade rules, preview validation and atomic confirmation.
- **Interchange** implements XLSX/CSV adapters behind Application-owned ports.
- **Presentation** owns wizard state, editable preview rows, counts, filters and commands.
- **WPF** owns the native file picker and desktop window composition only.
- **Data** is unchanged for import-specific schema; confirmed students reuse the existing transactional `Grupo` persistence path.

This boundary is intended to be reusable by the later export feature without coupling spreadsheet libraries to WPF or SQLite.

## Manual validation checklist

Before merging the implementation, validate with fictitious data only:

1. normal XLSX with one and multiple non-empty worksheets;
2. comma and semicolon CSV;
3. ambiguous CSV followed by explicit delimiter selection;
4. quoted CSV fields and embedded newlines;
5. unigrade file with blank grade defaulting to the group's configured grade;
6. multigrade file with blank grade requiring correction/exclusion;
7. active list-number conflict;
8. duplicate list numbers inside the file;
9. probable identity duplicate, testing both exclude and explicit import-as-new;
10. row correction and exclusion;
11. cancellation before confirmation, verifying that no student is added;
12. successful confirmation and immediate group-roster refresh;
13. Light, Dark and High Contrast themes;
14. Windows display scaling at 100%, 125% and 150% where available.