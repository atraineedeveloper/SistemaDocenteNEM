# Proposal: PDF report output

## Why

AulaRaíz already calculates useful individual and group reports from attendance, project/activity delivery, formative achievement and student follow-up. Those reports currently exist only inside the desktop UI. Teachers need a portable, printable document that can be stored or shared through the channels authorized for their school context without copying data manually into another application.

The first PDF change should reuse the existing report models and calculations rather than creating a second reporting system. It must also treat generated PDFs as sensitive teacher-controlled outputs because an individual report can include pedagogical observations, supports and family agreements.

## What changes

- Add PDF output for the existing individual student report.
- Add PDF output for the existing group report.
- Keep all report calculations in `SistemaDocente.Reporting` and existing Application report orchestration.
- Add an Application-owned PDF exporter port and a concrete PDFsharp/MigraDoc adapter in `SistemaDocente.Interchange`.
- Use AulaRaíz branding and structured school/group context in the generated document.
- Include print-friendly sections for attendance, delivery/compliance, formative achievement and the existing report evidence.
- Add a `Guardar PDF` action to the Reports module for the currently selected individual/group report.
- Warn before saving that the PDF may contain personal or pedagogical information.
- Generate to a temporary sibling file and publish the requested destination only after successful rendering.
- Use deterministic Windows-safe suggested filenames with the `AulaRaiz` file-safe brand.

## Scope boundary

This change exports only report models that already exist today. It does not invent reporting periods, report-card grades, family-meeting reports, project-completion reports or a dedicated attendance-report layout before those workflows exist.

Print preview and direct printer integration remain separate follow-up work. The generated PDF itself is designed to be printable in standard PDF viewers.

## Privacy principles

- PDF creation is explicitly teacher initiated.
- The application shows a sensitive-information warning before the save dialog.
- PDF contents are never written to technical logs.
- No cloud upload or automatic sharing is introduced.
- A failed render must not leave a partial destination that looks like a complete report.

## Library choice

Use PDFsharp/MigraDoc 6.2.x. It supports .NET 10 and is distributed under the MIT license. The PDF layout adapter remains isolated from WPF and domain/report calculations.
