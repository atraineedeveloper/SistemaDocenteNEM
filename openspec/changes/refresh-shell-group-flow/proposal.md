# Proposal: refresh shell navigation and group creation

## Why

AulaRaíz's current header places product identity, group context, teacher modules, backup, updates and appearance controls on one horizontal level. That hierarchy is becoming crowded and makes software-maintenance actions compete visually with daily teacher work.

The current **Crear grupo** route also reuses the historical welcome state in `GestionGrupoViewModel`. Opening it from an existing group clears the active group presentation context before the teacher has committed a new group and exposes the legacy `Olvidar referencia` recovery action without a normal Back/Cancel path.

The first refreshed create-group screen solved that navigation problem but still asked only for a display name. AulaRaíz already stores school/group context such as grades, school, CCT, cycle, shift and geographic location, so initial setup should offer those fields without forcing a teacher to know or enter them immediately.

The student editor also labels date of birth as required even though the presentation/application path already accepts a missing date. The UI should match the actual optional data contract.

## What changes

- Split the horizontal shell into two visual levels: product/group context and utilities above; teacher navigation below.
- Keep primary navigation limited to **Resumen**, **Asistencia**, **Proyectos**, **Evaluación** and **Reportes**.
- Move backup, update and appearance actions into secondary utility menus.
- Keep the active-group picker in the shell and expose **Mis grupos** plus **Crear nuevo grupo…** there.
- Replace the single-field create-group form with a five-step wizard: **Grupo**, **Grados**, **Escuela**, **Ubicación** and **Confirmar**.
- Require only the group display name; grades, school, CCT, school cycle, shift, state, municipality and locality remain optional during creation and can be skipped.
- Persist supplied optional setup data through the existing `ContextoGrupo` model/storage rather than introduce duplicate group metadata.
- Add **Volver**, **Cancelar** and **Omitir por ahora** where appropriate while preserving the current group/module until creation is committed.
- Route a successful creation to the new group's **Resumen**.
- Keep stale-reference recovery separate from normal group creation; `Olvidar referencia` is not part of the normal create flow.
- Label student date of birth as optional, matching the existing nullable model/behavior.

## Non-goals

- No sidebar navigation in this change.
- No SQLite/domain schema changes.
- No change that makes date of birth mandatory.
- No requirement to complete school/group context during initial creation.
- No redesign of the full group-configuration window beyond reusing its existing context model.
- No new cloud/account concept.

## Compatibility and risk

Existing group ids, persisted selected-group references, `ContextoGrupo` records and module data remain compatible. A group may be created with only its display name; any wizard fields left blank remain unspecified and can be completed later through normal group configuration.

The main regression risks are losing pending module work while entering/exiting the wizard, accidentally making optional context mandatory, or creating a group but failing to associate supplied context. The wizard must therefore remain reversible before commit and receive automated coverage for skip/optional behavior and context persistence.
