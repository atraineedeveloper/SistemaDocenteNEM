# Proposal: group data XLSX/CSV export

## Why

The application now owns a meaningful offline record of group context, students, attendance, NEM projects/activities, formative evaluation and student follow-up. Teachers need to take selected information outside the application for school administration, review, sharing and further spreadsheet work without copying records manually.

Export is not backup. An exported workbook is a teacher-facing representation of selected data, while backup/restore must later preserve the complete application state for recovery. Keeping those concerns separate avoids presenting a spreadsheet as a recovery mechanism.

## What changes

- Add teacher-initiated export of current group data to `.xlsx` and UTF-8 `.csv`.
- Reuse and extend `SistemaDocente.Interchange` with a write boundary that accepts neutral tabular output models from Application.
- Support one complete multi-sheet XLSX workbook for a selected group.
- Support focused CSV exports for one selected dataset at a time.
- Cover students, attendance, projects, activities and delivery/evaluation data in the initial export workflow.
- Allow optional student follow-up export only after an explicit sensitive-content choice.
- Allow date/content filtering where the dataset has a meaningful period.
- Include structured NEM metadata such as grade, phase, project methodology, formative field and target grades where applicable.
- Generate safe deterministic file names without making the file name the source of truth for group identity.
- Write output through a temporary file and publish the destination only after successful completion.
- Protect spreadsheet outputs from accidental formula execution: XLSX cells are written as values and CSV text that could be interpreted as a formula is escaped as text.

## Privacy and safety

- Export never mutates SQLite or application history.
- The teacher explicitly chooses the target group, output format and included datasets before writing a file.
- Sensitive follow-up content is excluded by default and requires an explicit opt-in.
- Technical logs do not contain exported student rows or workbook contents.
- CSV/XLSX output does not create formulas, macros or executable workbook content.
- A failed export does not leave a misleading partial destination file.

## Compatibility

- No SQLite schema change is required.
- Existing import behavior remains unchanged.
- Existing reports remain independent; this change exports structured data rather than replacing printable/PDF reporting.
- The implementation builds on the `SistemaDocente.Interchange` project introduced by student import.

## Out of scope

- Backup/restore of SQLite, configuration or evidence files.
- PDF/print rendering.
- Cloud synchronization or automatic upload.
- Editing application data by modifying an exported file.
- Round-trip import of attendance, projects, activities, evaluation or follow-up.
- Arbitrary user-authored workbook templates, formulas, macros, pivot tables or charts.
