# Tasks

## Interchange foundation
- [x] 1. Add an isolated tabular interchange adapter boundary reusable by import/export.
- [x] 2. Define neutral workbook/sheet/header/row/cell models and Application reader ports.
- [x] 3. Implement `.xlsx` reading with worksheet discovery, native value/date handling and safe non-execution of workbook content.
- [x] 4. Implement UTF-8 `.csv` reading with quoted fields and comma/semicolon/tab delimiter handling.

## Application import workflow
- [x] 5. Define explicit supported student destination fields and column-mapping models.
- [x] 6. Add deterministic name, date, gender and grade normalization.
- [x] 7. Add `Ready` / `NeedsReview` / `Invalid` / `Excluded` row-state classification with per-field issues.
- [x] 8. Add active list-number conflict detection within the file and against the target group.
- [x] 9. Add deterministic probable-duplicate review against active/inactive students without automatic overwrite/reactivation.
- [x] 10. Apply unigrade grade defaults and require explicit multigrade/unconfigured grade resolution.
- [x] 11. Revalidate against a fresh group/context snapshot immediately before commit.
- [x] 12. Apply all included students to one in-memory `Grupo` and call `IAlmacenamientoGrupos.Guardar` exactly once.
- [x] 13. Return committed import counts/summaries only after the aggregate save succeeds.

## Presentation and WPF
- [x] 14. Add an import ViewModel with file/sheet, mapping, preview, correction, filtering, exclusion and confirmation stages.
- [x] 15. Add a dedicated WPF import window with keyboard-accessible step navigation and validation-state labels that do not rely on color alone.
- [x] 16. Add native file selection for `.xlsx` and `.csv` without putting parsing logic in code-behind.
- [x] 17. Show preview counts and an explicit final message that existing students will not be modified.
- [x] 18. Refresh the group/student roster after successful commit while preserving normal unsaved-change/navigation rules.

## Privacy and documentation
- [x] 19. Ensure raw rows/workbook contents are not written to technical logs or persisted as preview artifacts.
- [x] 20. Document supported fields, CSV conventions, grade-default rules, duplicate semantics and troubleshooting.
- [x] 21. Update architecture/roadmap after the implementation shape is final.

## Quality
- [x] 22. Add Application tests for mapping, normalization, grade resolution, duplicate classification and stale-preview revalidation.
- [x] 23. Add real XLSX/CSV adapter fixtures for worksheets, quotes, delimiters, native dates, blanks and malformed input.
- [x] 24. Add Data/integration tests proving one confirmed import is all-or-nothing through the existing group transaction.
- [x] 25. Add Presentation/WPF regressions for correction/exclusion, commit gating, accessibility and result counts.
- [x] 26. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [x] 27. Manually validate fictitious XLSX/CSV imports covering the normal flow, duplicate conflicts, invalid/review rows, multigrade grade resolution and explicit ambiguous-CSV delimiter selection.

## Follow-up visual QA
- [ ] 28. Recheck the import wizard in Light/Dark/High Contrast and at 100/125/150% scaling as part of the broader UI visual QA pass; this is not a functional merge blocker.
