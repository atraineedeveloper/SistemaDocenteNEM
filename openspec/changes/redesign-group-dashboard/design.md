# Design: group dashboard visual refresh

## Page hierarchy

`GrupoView` remains the Resumen module and keeps the current shell above it. The page is reorganized into four vertical areas:

1. group heading with eyebrow, group name, student counts and group-level actions;
2. compact Total and Activos metric cards;
3. one large student-list card whose toolbar contains search, filters, ordering, add and secondary data actions;
4. the existing inline group-name editor only when that edit flow is active.

The old permanent bottom action card is removed so the table can consume the available height.

## Student-list toolbar

The toolbar keeps `Agregar estudiante` visible as the primary action. `Importar alumnos…` and `Exportar datos…` move to a compact `⋯` table actions menu because they are group-level but less frequent.

Status filtering is explicit and simple: Todos, Activos, Inactivos. Ordering supports Nombre A–Z, Nombre Z–A and Número de lista. Search continues matching name, list number and grade. These controls compose in `GestionGrupoViewModel.EstudiantesFiltrados` without changing persisted data.

## Row actions

Every row exposes a visible `⋮` affordance. Right-click and `⋮` use one code-behind menu-construction path after explicitly assigning `EstudianteSeleccionado` to the row under interaction. This prevents a context action from targeting a previously selected student.

The contextual menu contains:

- Ver expediente
- Editar estudiante
- Desactivar estudiante, when active
- Reactivar estudiante, when inactive

Double-click opens expediente for the double-clicked row after selecting it.

## Metrics and age

Only Total and Activos are page-level metrics. Age may remain a row-level informational column because it already exists and gracefully represents missing birth dates; no average-age calculation or KPI is introduced.

## Visual language

The layout follows the approved reference using existing semantic theme resources:

- more generous outer whitespace;
- compact bordered metric cards;
- a single rounded table card;
- toolbar integrated into the table card;
- 44–48 px student rows;
- circular initials avatar;
- small semantic pills for gender/status;
- quiet borders and section backgrounds;
- primary burgundy action preserved through `PrimaryButton`.

No reference image is embedded into the product and no fixed screenshot-specific background is used.

## Accessibility and themes

All interactive controls remain keyboard-focusable and receive automation names. Row actions are available through both visible overflow and context menu; right-click is therefore an accelerator, not the only discoverable path. Light, Dark and High Contrast must use existing semantic brushes. The redesign must remain usable at common Windows scaling values and with narrow-but-supported desktop widths.