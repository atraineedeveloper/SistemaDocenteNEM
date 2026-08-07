# Tasks

## Domain
- [ ] 1. Add structured NEM project-methodology and formative-field catalogs.
- [ ] 2. Extend `ProyectoDidactico` with methodology and target grades while preserving legacy creation/rehydration paths.
- [ ] 3. Extend `ActividadProyecto` with formative field and target grades while preserving historical rosters.

## Application
- [ ] 4. Extend project/activity input and output records with pedagogical metadata.
- [ ] 5. Build new activity rosters from active students matching explicit target grades.
- [ ] 6. Validate activity target grades against explicit project target grades.
- [ ] 7. Preserve existing Evaluation applicability through the stored historical activity roster.

## Persistence
- [ ] 8. Add SQLite extension `nem-planeacion-proyectos` without changing `PRAGMA user_version = 6`.
- [ ] 9. Persist methodology, formative field and target grades atomically with their aggregate.
- [ ] 10. Migrate legacy projects/activities to unspecified metadata without inventing target grades.

## Presentation and WPF
- [ ] 11. Add project methodology and target-grade editing.
- [ ] 12. Add activity formative-field and target-grade editing.
- [ ] 13. Auto-select the only grade for unigrade groups and make multigrade selection explicit.
- [ ] 14. Surface compact NEM metadata in project/activity summaries.
- [ ] 15. Update Demo mode with representative NEM planning metadata.

## Documentation
- [ ] 16. Document project methodology, formative fields and multigrade targeting in English engineering docs.
- [ ] 17. Keep Spanish UI/domain terminology where already established.

## Quality
- [ ] 18. Add Core tests for catalogs and target-grade invariants.
- [ ] 19. Add Application tests for grade-targeted activity rosters.
- [ ] 20. Add Data tests for extension migration and round-trip persistence.
- [ ] 21. Add Presentation/WPF regressions for project/activity catalog editing.
- [ ] 22. Run Windows CI: format, Release build, tests, OpenSpec and whitespace.
- [ ] 23. Perform manual Demo validation for unigrade and multigrade planning flows.