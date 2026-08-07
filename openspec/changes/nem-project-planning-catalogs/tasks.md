# Tasks

## Domain
- [x] 1. Add structured NEM project-methodology and formative-field catalogs.
- [x] 2. Extend `ProyectoDidactico` with methodology and target grades while preserving legacy creation/rehydration paths.
- [x] 3. Extend `ActividadProyecto` with formative field and target grades while preserving historical rosters.

## Application
- [x] 4. Extend project/activity input and output records with pedagogical metadata.
- [x] 5. Build new activity rosters from active students matching explicit target grades.
- [x] 6. Validate activity target grades against explicit project target grades.
- [x] 7. Preserve existing Evaluation applicability through the stored historical activity roster.

## Persistence
- [x] 8. Add SQLite extension `nem-planeacion-proyectos` without changing `PRAGMA user_version = 6`.
- [x] 9. Persist methodology, formative field and target grades atomically with their aggregate.
- [x] 10. Migrate legacy projects/activities to unspecified metadata without inventing target grades.

## Presentation and WPF
- [x] 11. Add project methodology and target-grade editing.
- [x] 12. Add activity formative-field and target-grade editing.
- [x] 13. Auto-select the only grade for unigrade groups and make multigrade selection explicit.
- [x] 14. Surface compact NEM metadata in project/activity summaries.
- [x] 15. Update Demo mode with representative NEM planning metadata.

## Documentation
- [x] 16. Document project methodology, formative fields and multigrade targeting in English engineering docs.
- [x] 17. Keep Spanish UI/domain terminology where already established.

## Quality
- [x] 18. Add Core tests for catalogs and target-grade invariants.
- [x] 19. Add Application tests for grade-targeted activity rosters.
- [x] 20. Add Data tests for extension migration and round-trip persistence.
- [x] 21. Add Presentation/WPF regressions for project/activity catalog editing.
- [x] 22. Run Windows CI: format, Release build, tests, OpenSpec and whitespace.
- [x] 23. Perform manual Demo validation for unigrade and multigrade planning flows.
