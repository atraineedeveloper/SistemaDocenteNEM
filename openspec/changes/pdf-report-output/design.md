# Design: PDF report output

## Architectural boundary

The PDF feature follows the same adapter direction used by spreadsheet interchange:

```text
Core / Reporting
       ↑
  Application
       ↑
  Presentation
       ↑
     WPF

Application → IExportadorReportesPdf
                 ↑
       Interchange adapter
```

`SistemaDocente.Reporting` remains the only owner of report calculations. PDF rendering receives already-calculated `ReporteIndividualAlumno` or `ReporteGrupal` values and only formats them.

## Application contract

Application owns `IExportadorReportesPdf` with overloads for the current individual and group report models. The port takes a destination path because the adapter owns destination-safe file publication. Infrastructure/render failures are translated to `ExportacionReportePdfException` so PDFsharp/MigraDoc details do not leak into Presentation.

## Concrete renderer

`SistemaDocente.Interchange` references `PDFsharp-MigraDoc` 6.2.4 and implements the port.

The renderer builds an A4 portrait MigraDoc document with:

- AulaRaíz identity and report type;
- school/group context;
- compact metric summaries;
- tables for monthly attendance and evidence summaries;
- individual pedagogical follow-up sections when present;
- page numbering and generation metadata.

The layout is deliberately print-oriented and does not mirror WPF theme colors. It uses restrained grayscale/accent styling that remains readable when printed in monochrome.

## Fonts

The application is Windows-only. The Core PDFsharp build is kept so `SistemaDocente.Interchange` can continue targeting `net10.0` and remain usable by existing non-WPF tests. A custom PDFsharp font resolver loads Segoe UI regular/bold faces from the Windows Fonts directory instead of redistributing Microsoft font files.

Font initialization is process-global, thread-safe and performed before the first document render.

## Destination safety

Rendering writes to a unique temporary sibling file in the requested destination directory. The final path is moved/replaced only after MigraDoc has rendered and PDFsharp has closed the output successfully. Temporary files are removed on failure when possible.

## Presentation workflow

`GestionReportesViewModel` accepts an optional PDF exporter dependency so existing tests/consumers remain compatible. It exposes whether the active report can be exported, creates a sanitized filename suggestion and dispatches the active individual/group model to the exporter.

WPF owns only:

- privacy confirmation;
- native `SaveFileDialog`;
- success/failure feedback.

No PDF document construction belongs in XAML/code-behind.

## Initial document contents

### Individual

- school, CCT, cycle, group, grades/phases and responsible teacher;
- student name/list number/status;
- attendance and compliance metrics;
- monthly attendance;
- formative achievement distribution;
- project/activity evidence;
- strengths, difficulties, supports, family agreements and recent observations.

### Group

- school/group context;
- historical and active enrollment;
- attendance/compliance metrics;
- monthly attendance;
- formative achievement distribution;
- per-student follow-up summary without competitive ranking.

## Deferred work

- direct printing and printer configuration;
- in-app print preview;
- dedicated attendance-only report;
- project-completion report;
- family-meeting summary;
- period/formative-field report cards;
- digital signing, PDF encryption or automatic distribution.
