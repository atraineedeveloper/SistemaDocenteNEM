# Change: Redesign group workspace, attendance and evaluation interactions

## Why

The current application treats the active group as a secondary header dropdown even though it defines the context for every teaching workflow. Attendance and evaluation also use different interaction patterns, and the student record competes visually with reports even though both serve different purposes.

The next UX iteration should make the group context explicit, reduce repetitive clicks during classroom capture, preserve the distinction between student record and reports, and give Attendance and Evaluation a coherent matrix interaction model.

## What Changes

- Add a `Mis grupos` workspace shown when entering the application, with clear group cards and an explicit action to open a group.
- Replace the large header group selector with a compact context switcher and a direct entry back to `Mis grupos`.
- Keep Expediente as the editable longitudinal student record and redesign its presentation around student summary, follow-up history, activities and family agreements.
- Bring the monthly Attendance matrix to the same visual hierarchy as Evaluation.
- Open a compact action menu from an attendance or evaluation matrix cell with one click while preserving keyboard shortcuts.
- In Evaluation, expose one teacher-facing result selector instead of separate technical controls for delivery state and achievement level.
- Infer the internal delivery state automatically from the selected evaluation result while preserving `EstadoEntregaActividad` and `NivelLogro` as separate domain data.
- Keep `Entregada + Pendiente` available as “Entregada · evaluar después”.
- Use `Más opciones…` from the Evaluation cell menu to open the full observation editor.
- Write new technical documentation, OpenSpec content, branch metadata, commits and PR text in English while preserving existing Spanish system component identifiers and visible product copy.

## Capabilities

### New Capabilities

- `group-workspace-navigation`: explicit group landing workspace and compact context switching.
- `matrix-cell-actions`: direct one-click actions for Attendance and Evaluation matrices.

### Modified Capabilities

- `interfaz-evaluacion-matriz`: teacher-facing result selection infers delivery state while preserving domain semantics.
- `expedientes-alumnos`: student record remains distinct from reports and gains a clearer follow-up-oriented presentation.

## Impact

- **Presentation:** shell navigation state and a unified visual evaluation result projection.
- **App.Wpf:** new group workspace, compact group context switcher, matrix cell menus and redesigned student record presentation.
- **Core/Application/Data/SQLite:** no schema or domain semantic changes are required.
- **Tests:** add Presentation and WPF regressions for group workspace, matrix interactions and evaluation result mapping.
- **Accessibility:** preserve keyboard capture and add equivalent pointer actions with clear accessible names.