# Tasks

## Contract and dependency
- [x] 1. Define the PDF report exporter port and infrastructure exception in Application.
- [x] 2. Add PDFsharp/MigraDoc 6.2.4 to the Interchange adapter project.
- [x] 3. Add Windows font resolution without redistributing proprietary font files.

## PDF renderer
- [x] 4. Implement destination-safe PDF publication through a temporary sibling file.
- [x] 5. Render AulaRaíz report header/footer and structured school/group context.
- [x] 6. Render the existing individual report model with attendance, compliance, achievement, activities and follow-up.
- [x] 7. Render the existing group report model with enrollment, attendance, compliance, achievement and non-ranked follow-up.
- [x] 8. Keep null/undefined metrics explicit instead of converting them to zero.

## Presentation and WPF
- [x] 9. Add optional PDF export support to `GestionReportesViewModel` and Windows-safe filename suggestions.
- [x] 10. Add the Reports `Guardar PDF` action for the active individual/group view.
- [x] 11. Add a privacy confirmation before the native save dialog.
- [x] 12. Compose the concrete PDF exporter in `App.xaml.cs`.

## Tests and documentation
- [x] 13. Add renderer integration tests that produce real non-empty PDF files from fictitious reports and validate the PDF header.
- [x] 14. Add Presentation tests for active-report dispatch and filename sanitization.
- [x] 15. Add WPF structural regression coverage for the PDF action/privacy boundary.
- [x] 16. Add maintained PDF-report documentation and update README/roadmap architecture notes.
- [x] 17. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [x] 18. Manually generate Demo individual/group PDFs, open/render them and inspect page layout before merge.
