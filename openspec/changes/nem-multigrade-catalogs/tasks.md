# Tasks

## Structured domain
- [ ] 1. Add structured primary-grade, NEM-phase and school-organization values.
- [ ] 2. Add deterministic grade-to-phase and developmental-reference helpers.
- [ ] 3. Extend group context with served grades, derived modality/phases and school organization.
- [ ] 4. Add individual primary grade to students while preserving legacy construction paths.

## Offline geography
- [ ] 5. Package the 32 Mexican federal entities and municipality catalog for offline use.
- [ ] 6. Add state-dependent municipality selection while keeping locality free text.
- [ ] 7. Document geographic catalog provenance and refresh procedure.

## Persistence and migration
- [ ] 8. Add additive SQLite extension `nem-contexto-multigrado` without changing `PRAGMA user_version = 6`.
- [ ] 9. Persist group grades, school organization and student grades.
- [ ] 10. Migrate deterministic legacy grade text transactionally and idempotently.
- [ ] 11. Preserve the existing textual group-context projection for compatibility and reporting.

## Presentation and WPF
- [ ] 12. Redesign group configuration around catalogs and multi-grade selection.
- [ ] 13. Show derived NEM phase(s), classroom modality and non-diagnostic developmental reference.
- [ ] 14. Add student-grade editing/defaulting and expose grade in multigrade rosters.
- [ ] 15. Update Demo mode with structured school context and grade assignments.

## Documentation
- [ ] 16. Translate README, root Markdown and `docs/` to English.
- [ ] 17. Translate current specs and non-archived OpenSpec changes to English.
- [ ] 18. Verify `openspec/changes/archive/**` is unchanged.
- [ ] 19. Refresh the roadmap/checklist to reflect current implementation and future NEM/import/export work.

## Quality
- [ ] 20. Add Core tests for grade/phase/modality/developmental rules.
- [ ] 21. Add Data tests for extension migration and round-trip persistence.
- [ ] 22. Add Presentation/WPF regressions for structured configuration and student grade.
- [ ] 23. Run Windows CI: format, Release build, tests, OpenSpec and whitespace.
- [ ] 24. Perform manual Demo validation for unigrade and multigrade groups, Light/Dark/High Contrast, and 100/125/150% scaling.