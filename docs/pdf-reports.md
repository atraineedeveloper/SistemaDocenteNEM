# PDF report output

AulaRaíz can export its existing individual and group reports to print-oriented PDF files. PDF output is a presentation of report values already calculated by `SistemaDocente.Reporting`; it is not a second reporting engine and does not recalculate attendance, compliance or achievement semantics.

## Initial scope

Version 1 PDF output covers:

- the existing individual student report;
- the existing group report.

The individual PDF can contain school/group context, student identity, attendance, delivery/compliance, formative achievement, project/activity evidence, strengths, difficulties, applied supports, family agreements and recent observations.

The group PDF contains school/group context, enrollment counts, attendance, delivery/compliance, formative achievement, monthly attendance and the existing non-ranked per-student follow-up summary.

The first version intentionally does not add dedicated attendance-only reports, project-completion reports, family-meeting summaries, reporting-period/formative-field report cards, direct printer selection or in-app print preview.

## Architecture

The boundaries are:

```text
SistemaDocente.Reporting
        ↓ existing calculated report models
SistemaDocente.Application
        ↓ IExportadorReportesPdf
SistemaDocente.Interchange
        ↓ PDFsharp / MigraDoc renderer
local .pdf file
```

Presentation chooses the active report and creates the suggested filename. WPF owns only the privacy confirmation, native save dialog and user feedback.

No PDFsharp or MigraDoc types belong in Core, Reporting, Presentation or WPF view logic.

## PDF library

The renderer uses PDFsharp/MigraDoc 6.2.4. The dependency is isolated in `SistemaDocente.Interchange` alongside other teacher-controlled file interchange adapters.

AulaRaíz remains a Windows application. The Core PDFsharp package is used so Interchange can keep targeting `net10.0`; a custom font resolver reads Segoe UI regular/bold faces from the Windows Fonts directory at runtime. Font files are not copied into the repository or redistributed by this feature.

## File publication safety

PDF generation follows the same destination-safety principle as XLSX/CSV export:

1. resolve the requested destination path;
2. render the complete document to a unique temporary sibling PDF;
3. close the rendered PDF successfully;
4. publish/replace the requested destination only after success;
5. remove temporary output on failure when possible.

A renderer failure therefore does not intentionally leave a new partial destination that looks complete.

## Privacy

PDF reports may contain personal and pedagogical information. Before the Windows save dialog is shown, AulaRaíz asks the teacher to confirm a warning that the file should only be stored or shared through appropriate school-authorized locations/channels.

The application does not automatically upload, email or synchronize generated reports. PDF body contents must not be written to technical logs.

## Suggested filenames

User-facing filenames use the ASCII-safe brand `AulaRaiz` and include report type, report subject/group and date, for example:

```text
AulaRaiz_Reporte_Individual_1_Ana_Perez_2026-08-08.pdf
AulaRaiz_Reporte_Grupal_4.º_A_2026-08-08.pdf
```

Windows-invalid filename characters are sanitized before the suggestion is shown.

## Manual acceptance

Before merge, validate with fictitious Demo data only:

1. open an individual report and save PDF;
2. confirm the privacy warning appears before file selection;
3. open the generated PDF in a standard viewer and inspect every page;
4. verify school/student context, percentages, tables and follow-up sections;
5. switch to the group report and repeat;
6. verify a null/undefined metric displays as `—`, not `0%`;
7. verify long names/content wrap without clipping or overlap;
8. verify page numbers and multi-page tables;
9. print or use print preview from the PDF viewer to confirm A4 readability;
10. keep all validation data fictitious.
