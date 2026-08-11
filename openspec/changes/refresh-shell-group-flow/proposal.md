# Proposal: refresh shell navigation and group creation

## Why

AulaRaíz's current header places product identity, group context, teacher modules, backup, updates and appearance controls on one horizontal level. That hierarchy is becoming crowded and makes software-maintenance actions compete visually with daily teacher work.

The current **Crear grupo** route also reuses the historical welcome state in `GestionGrupoViewModel`. Opening it from an existing group clears the active group presentation context before the teacher has committed a new group and exposes the legacy `Olvidar referencia` recovery action without a normal Back/Cancel path.

## What changes

- Split the horizontal shell into two visual levels: product/group context and utilities above; teacher navigation below.
- Keep primary navigation limited to **Resumen**, **Asistencia**, **Proyectos**, **Evaluación** and **Reportes**.
- Move backup, update and appearance actions into secondary utility menus.
- Keep the active-group picker in the shell and expose **Mis grupos** plus **Crear nuevo grupo…** there.
- Add a dedicated shell-level create-group route with **Volver** and **Cancelar**.
- Preserve the current group and current module while the create-group form is only a draft.
- Route a successful creation to the new group's **Resumen**.
- Keep stale-reference recovery separate from normal group creation; `Olvidar referencia` is not part of the normal create flow.

## Non-goals

- No sidebar navigation in this change.
- No SQLite/domain schema changes.
- No changes to group/student business rules.
- No redesign of the individual teacher modules beyond the shell labels and creation entry point.
- No new cloud/account concept.

## Compatibility and risk

The change is presentation/navigation-only. Existing group ids, persisted selected-group references and module data remain unchanged. The main regression risk is losing pending module work or active context while entering/exiting group creation, so the create route must be reversible and covered by navigation regression tests.
