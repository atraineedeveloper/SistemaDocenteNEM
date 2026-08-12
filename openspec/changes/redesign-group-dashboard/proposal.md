# Proposal: redesign the group dashboard

## Why

The current AulaRaíz group summary already exposes the needed student operations, but the page still feels like a utility screen rather than a polished daily-work dashboard. A permanent bottom action bar consumes vertical space, mixes global actions with row-specific actions and duplicates operations that naturally belong to the selected student.

The approved visual direction is the supplied commercial-dashboard reference adapted to AulaRaíz: preserve the two-level application shell, strengthen the group heading and student table, keep only Total and Activos metrics, move frequent global actions to the table toolbar, and expose student-specific operations through a row overflow menu and right-click context menu.

## What changes

- Redesign `GrupoView` to closely match the approved reference in spacing, hierarchy, cards, toolbar, table density, badges and row actions.
- Keep only **Total** and **Activos** summary metrics; no average-age KPI is added.
- Remove the permanent bottom action bar.
- Keep **Agregar estudiante** as the visible primary table action.
- Move **Importar alumnos** and **Exportar datos** to a compact table-level actions menu.
- Add useful status filtering and ordering controls to the table toolbar.
- Add a visible `⋮` action at the end of every student row.
- Right-clicking a student SHALL select that student and open the same contextual actions as `⋮`.
- Double-clicking a student SHALL open their expediente.
- Student contextual actions SHALL expose expediente, edit, and the applicable activate/deactivate action.
- Preserve existing group configuration, rename, import/export, editor, expediente and domain behavior.

## Non-goals

- No SQLite or domain schema changes.
- No average-age card.
- No always-visible mass-actions footer.
- No new bulk student operations in this change.
- No redesign of Asistencia, Proyectos, Evaluación or Reportes.
- No dependency on an online icon/font service.

## Compatibility and risk

The change is primarily Presentation/WPF plus small view-model state for filtering and ordering. Existing commands and windows remain the source of truth for student CRUD, expediente, import/export and group configuration. Main risks are acting on the wrong row from a context menu, making actions undiscoverable after removing the footer, or regressing theme/accessibility behavior; automated tests and manual Demo validation will cover those paths.