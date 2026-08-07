# Design: Group workspace and direct matrix interactions

## Design goals

1. Make the active group an explicit workspace context rather than a form field.
2. Preserve fast keyboard entry while making pointer interaction equally efficient.
3. Keep the student record as editable longitudinal evidence and reports as generated summaries.
4. Hide technical delivery-state mechanics from the normal Evaluation workflow without weakening domain correctness.
5. Reuse the current WPF/MVVM architecture and avoid persistence migrations.

## Group workspace

### Entry behavior

`MainWindowViewModel` gains an explicit start state. The application may load the last-used group in the background for continuity, but navigation is hidden until the user opens a group from `Mis grupos`.

The landing workspace displays every available group as a card. A card contains the group name and lightweight information already available from `GrupoDetalle`, such as student count. The last-used group may appear selected, but opening a group remains an explicit action.

### Context switcher

The header replaces the wide `ComboBox` with a compact menu-style context switcher. It displays the current group name and exposes:

- each available group;
- `Mis grupos`;
- `Crear grupo`.

Changing group must first ask the shell whether the current module can be left, preserving unsaved-change guards from Attendance, Projects and Evaluation.

## Student record versus reports

Expediente remains a primary student workflow because it stores editable longitudinal evidence: strengths, difficulties, applied supports, chronological observations and family agreements.

Reports remain generated read models that combine attendance, delivery/evaluation and Expediente information. The redesign therefore improves Expediente instead of removing it.

The WPF window should emphasize:

- student identity and compact summary metrics;
- pedagogical summary;
- chronological follow-up;
- activity/evaluation history;
- family agreements;
- one clear action for adding a new follow-up item where possible.

This change does not alter Expediente persistence.

## Attendance matrix

The monthly grid keeps frozen identity columns and dynamic day columns, but adopts the interaction language used by Evaluation:

- compact semantic cells;
- stronger selected-cell affordance;
- consistent header and row sizing;
- semantic state colors using theme resources;
- one-click cell action menu;
- right-click and keyboard remain valid alternatives.

The compact menu exposes `Presente`, `Falta`, `Retardo` and `Justificada`. Choosing an option updates only the selected cell and keeps the existing unsaved-day semantics.

## Evaluation matrix

### Teacher-facing result

The UI exposes one result concept:

| Teacher-facing result | Internal delivery state | Internal achievement level |
| --- | --- | --- |
| Pendiente | `Pendiente` | `Pendiente` |
| Entregada · evaluar después | `Entregada` | `Pendiente` |
| Domina | `Entregada` | `Domina` |
| Suficiente | `Entregada` | `Suficiente` |
| En proceso | `Entregada` | `EnProceso` |
| Requiere apoyo | `Entregada` | `RequiereApoyo` |
| No entregó | `NoEntregada` | `Pendiente` |

`EvaluacionCeldaVisual` owns this projection. Existing `EstadoEntrega` and `NivelLogro` properties remain available for compatibility and persistence.

### Cell actions

A single click on an applicable editable cell opens a compact menu with the seven results above plus `Más opciones…`.

`Más opciones…` opens `EditarEvaluacionCeldaWindow`, which shows a single `Resultado` selector and the observation field. The technical delivery-state selector is removed from the teacher-facing editor.

Existing `D/S/E/R/T/N/P` keyboard shortcuts remain supported. `T` maps to `Entregada · evaluar después`.

## Scope boundaries

- No SQLite schema changes.
- No changes to report formulas.
- No removal of Expediente.
- No replacement of the existing domain enums.
- No new diagnosis or student-classification fields.

## Validation

Automated validation must include:

- mapping every teacher-facing Evaluation result to the correct internal pair;
- shell start-state and group switching behavior;
- structural WPF checks for the new workspace and compact switcher;
- structural checks for one-click menus and `Más opciones…`;
- existing full CI: format, Release build, tests, OpenSpec and whitespace.