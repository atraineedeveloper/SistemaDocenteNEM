# Tasks

## Structured domain
- [x] 1. Add structured primary-grade, NEM-phase and school-organization values.
- [x] 2. Add deterministic grade-to-phase and developmental-reference helpers.
- [x] 3. Extend group context with served grades, derived modality/phases and school organization.
- [x] 4. Add individual primary grade to students while preserving legacy construction paths.

## Offline geography
- [x] 5. Package the 32 Mexican federal entities and municipality catalog for offline use.
- [x] 6. Add state-dependent municipality selection while keeping locality free text.
- [x] 7. Document geographic catalog provenance and refresh procedure.

## Persistence and migration
- [x] 8. Add additive SQLite extension `nem-contexto-multigrado` without changing `PRAGMA user_version = 6`.
- [x] 9. Persist group grades, school organization and student grades.
- [x] 10. Migrate deterministic legacy grade text transactionally and idempotently.
- [x] 11. Preserve the existing textual group-context projection for compatibility and reporting.

## Presentation and WPF
- [x] 12. Redesign group configuration around catalogs and multi-grade selection.
- [x] 13. Show derived NEM phase(s), classroom modality and non-diagnostic developmental reference.
- [x] 14. Add student-grade editing/defaulting and expose grade in multigrade rosters.
- [x] 15. Update Demo mode with structured school context and grade assignments.

## Documentation
- [x] 16. Translate README, root Markdown and `docs/` to English.
- [x] 17. Translate current specs and non-archived OpenSpec changes to English.
- [x] 18. Verify `openspec/changes/archive/**` is unchanged.
- [x] 19. Refresh the roadmap/checklist to reflect current implementation and future NEM/import/export work.

## Quality
- [x] 20. Add Core tests for grade/phase/modality/developmental rules.
- [x] 21. Add Data tests for extension migration and round-trip persistence.
- [x] 22. Add Presentation/WPF regressions for structured configuration and student grade.
- [x] 23. Run Windows CI: format, Release build, tests, OpenSpec and whitespace.
- [x] 24. Perform manual Demo validation for unigrade and multigrade groups, Light/Dark/High Contrast, and 100/125/150% scaling.
