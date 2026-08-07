# Proposal: student XLSX/CSV import

## Why

Teachers frequently receive or already maintain student rosters in spreadsheets. Re-entering every student manually is slow and creates avoidable transcription errors. The application now has a structured student model, explicit primary grade and unigrade/multigrade context, so it has enough information to support a safe import workflow.

Import must not behave like a blind database load. The teacher needs to see what the file contains, map columns, resolve invalid or ambiguous rows and confirm exactly which new students will be added before any persistent write occurs.

## What changes

- Add student import from `.xlsx` and `.csv` files.
- Separate file reading from domain/application logic through a dedicated interchange adapter boundary.
- Provide a multi-step preview flow: choose file/sheet, map source columns, normalize and validate rows, resolve review items, then confirm.
- Map source data to the existing student model without introducing CURP or other fields that the product intentionally does not store.
- Detect hard conflicts such as duplicate active list numbers and probable duplicates such as matching normalized student identity data.
- Allow preview-row corrections and explicit row exclusion without modifying the source file.
- Default a missing student grade only when the active group has exactly one configured served grade.
- Require explicit grade resolution for multigrade groups or when no safe single-grade default exists.
- Revalidate against a fresh group snapshot immediately before commit.
- Add all confirmed students to one in-memory `Grupo` aggregate and persist that aggregate once, preserving one SQLite transaction for the entire confirmed import.
- Never update, overwrite, reactivate or deactivate an existing student as an implicit import side effect.
- Show imported, excluded and unresolved/error counts before and after confirmation.

## Safety and privacy

- Parsing and preview do not write SQLite.
- Unmapped source columns are ignored rather than persisted opportunistically.
- Preview data stays in memory and must not be written to logs.
- Import errors should identify the row/field problem without dumping unrelated personal data.
- Existing students are never overwritten automatically, even when a source row appears to match them.
- A failed confirmed import leaves the group unchanged.

## Compatibility

- No SQLite schema change is required: confirmed rows become ordinary students inside the existing `Grupo` aggregate and are saved through the existing group persistence boundary.
- Existing manual student creation/editing remains unchanged.
- Historical activity rosters remain unchanged; newly imported students participate only in future workflows unless another existing feature explicitly includes them.
- The import feature depends on the structured-grade foundation from PR #11 and uses the current student model from PR #12's stacked base.

## Out of scope

- Updating or merging existing students from spreadsheets.
- Importing attendance, projects, activities, evaluation, student-record notes or family agreements.
- CURP import.
- Importing arbitrary formulas, macros, images or workbook formatting.
- Automatic cloud synchronization.
- Export; that remains the next independent interchange change.
- Backup/restore.