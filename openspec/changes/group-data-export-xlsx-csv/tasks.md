# Tasks

## Export foundation
- [x] 1. Define neutral tabular output models and an Application export port reusable by XLSX and CSV writers.
- [x] 2. Extend `SistemaDocente.Interchange` with value-only XLSX writing and formula-safe UTF-8 CSV writing.
- [x] 3. Publish exports through temporary sibling files so failed serialization does not leave misleading destination files.

## Application export workflow
- [x] 4. Add explicit export dataset/format/scope models and result summaries.
- [x] 5. Project current group context and structured NEM metadata into export rows without parsing display names.
- [x] 6. Export students with structured names, grade, dates, gender, active state and opt-in pedagogical observations.
- [x] 7. Export attendance as normalized student/date rows with an inclusive date range.
- [x] 8. Export projects and activities with lifecycle, methodology, formative field and target-grade metadata.
- [x] 9. Export delivery/evaluation rows while preserving teacher-facing result semantics and optional observations.
- [x] 10. Add opt-in sensitive student follow-up export with explicit warning metadata.
- [x] 11. Support complete multi-sheet XLSX export and exactly one focused dataset per CSV export.
- [x] 12. Generate deterministic Windows-safe file-name suggestions and human-readable catalog labels.

## Presentation and WPF
- [x] 13. Add an export ViewModel with `Contenido → Alcance → Archivo → Resultado` stages.
- [x] 14. Add format/dataset gating so XLSX may contain multiple selected datasets while CSV requires exactly one.
- [x] 15. Add project/date scope controls only when relevant to the selected datasets.
- [x] 16. Keep sensitive options off by default and display an explicit privacy warning when enabled.
- [x] 17. Add a dedicated WPF export window and native Windows save-file picker without file-format logic in code-behind.
- [x] 18. Show estimated/exported row counts and the final destination only after successful publication.

## Documentation and roadmap
- [x] 19. Add maintained export documentation covering formats, datasets, privacy defaults, CSV safety and troubleshooting.
- [x] 20. Update README/architecture/roadmap to show student import merged and group export as the active change.
- [x] 21. Keep backup/restore documented as a separate future recovery capability.

## Quality
- [x] 22. Add Application tests for dataset selection, filtering, NEM labels, privacy defaults and file-name suggestions.
- [x] 23. Add Interchange round-trip/inspection tests for generated XLSX and CSV, including Unicode, dates, quotes/newlines and formula-leading text.
- [x] 24. Add failure tests proving incomplete exports do not publish the destination file.
- [x] 25. Add Presentation/WPF regressions for format gating, sensitive-content warnings, export-window construction and save integration.
- [x] 26. Stress representative Demo exports with 30–40 students and attendance/evaluation rows.
- [ ] 27. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [ ] 28. Manually export fictitious Demo data to XLSX/CSV and open the generated files in Excel-compatible software before merge.
