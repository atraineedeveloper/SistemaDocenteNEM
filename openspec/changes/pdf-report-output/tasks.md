# Tasks

## Contract and dependency
- [x] 1. Define the PDF report exporter port and infrastructure exception in Application.
- [ ] 2. Add PDFsharp/MigraDoc 6.2.4 to the Interchange adapter project.
- [ ] 3. Add Windows font resolution without redistributing proprietary font files.

## PDF renderer
- [ ] 4. Implement destination-safe PDF publication through a temporary sibling file.
- [ ] 5. Render AulaRaíz report header/footer and structured school/group context.
- [ ] 6. Render the existing individual report model with attendance, compliance, achievement, activities and follow-up.
- [ ] 7. Render the existing group report model with enrollment, attendance, compliance, achievement and non-ranked follow-up.
- [ ] 8. Keep null/undefined metrics explicit instead of converting them to zero.

## Presentation and WPF
- [ ] 9. Add optional PDF export support to `GestionReportesViewModel` and Windows-safe filename suggestions.
- [ ] 10. Add the Reports `Guardar PDF` action for the active individual/group view.
- [ ] 11. Add a privacy confirmation before the native save dialog.
- [ ] 12. Compose the concrete PDF exporter in `App.xaml.cs`.

## Tests and documentation
- [ ] 13. Add renderer integration tests that produce parseable non-empty PDF files from fictitious reports.
- [ ] 14. Add Presentation tests for active-report dispatch and filename sanitization.
- [ ] 15. Add WPF structural regression coverage for the PDF action/privacy boundary.
- [ ] 16. Add maintained PDF-report documentation and update architecture/README/roadmap.
- [ ] 17. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [ ] 18. Manually generate Demo individual/group PDFs, open/render them and inspect page layout before merge.
