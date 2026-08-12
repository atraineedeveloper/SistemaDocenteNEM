# Design: group lifecycle, archive and safe deletion

## Lifecycle model

`Grupo` gains a persisted `EstaArchivado` flag. New groups are active. `Archivar` and `Restaurar` are idempotent and preserve `GrupoId`, visible name and every child record.

Archived groups are historical, read-only workspaces. Normal group/student mutation operations reject changes while archived; restoration is the explicit transition back to an editable workspace.

`Grupo.Rehidratar` receives the persisted lifecycle state together with the existing complete student snapshot so Data does not bypass Core semantics.

## Application queries

The group use case exposes all groups as well as convenience active/archived queries. The normal shell and group picker consume only active groups. `Mis grupos` additionally exposes a secondary archived collection.

Loading an archived group as the active working context is rejected. A teacher restores it first, after which the existing modules can use it normally.

## SQLite schema compatibility

The base SQLite schema advances from v6 to v7 by adding:

```sql
archivado INTEGER NOT NULL DEFAULT 0 CHECK (archivado IN (0,1))
```

to `grupos`. Migration from v6 uses `ALTER TABLE`; prior migration chains finish v6 first and then apply v7. Fresh databases create the v7 shape directly. Existing rows therefore remain active.

## Deletion preflight

Persistence exposes a small immutable deletion summary containing counts for students, attendance days, projects, activities and deliveries. `TieneDatos` is true when any count is non-zero. The summary is used only to choose the confirmation strength and explain impact; it is not an authorization token.

## Safety backup boundary

Permanent deletion is wrapped by a storage decorator at composition time. Before deletion it reads the current summary. If the group contains data, the decorator creates a full unprotected v1 `.sdocbackup` in the existing application-managed safety-backup directory, using the current application version.

Backup failure aborts deletion. Empty mistaken groups do not create a safety file.

The backup is intentionally the existing application-managed v1 format: this change does not invent another recovery format or ask for the teacher's manual-backup password.

## Transactional relational deletion

SQLite foreign keys remain enabled. Permanent deletion executes in one explicit transaction and removes known restrictive dependents in child-to-parent order. Cascade-owned extension rows are allowed to cascade from their parent records.

The base restrictive order is:

1. activity deliveries;
2. project activities;
3. projects;
4. attendance registrations;
5. attendance days;
6. students (pedagogical notes, tutor agreements and student-grade extension rows cascade);
7. group (group context, configured grades and compatible extension rows cascade).

NEM project/activity extension rows cascade from projects/activities. The implementation does not disable foreign keys. Any unexpected dependency or SQL failure rolls the complete operation back.

## Selected-group reference

Archiving or deleting the currently selected group clears its local selected-group reference and resets the presentation to `Mis grupos`. If active groups remain, the teacher can select one normally; the system does not silently choose a historical group.

Startup treats an archived stored reference similarly to an unavailable working group: it does not open archived data into daily modules.

## UX

### Active group card

An active card keeps `Abrir grupo` as the primary action and adds secondary `Archivar` and `Eliminar…` actions that are visually separated from opening the workspace.

Archiving uses a simple confirmation explaining that no information is deleted and the group can be restored later.

### Archived groups

`Mis grupos` contains a secondary `Archivados (N)` expander/list. Archived cards identify their state and expose `Restaurar` plus `Eliminar…`; they do not expose `Abrir`.

### Permanent deletion

Empty group:

- explain that the group will be permanently removed;
- `Cancelar` is the safe/default choice;
- destructive action is `Eliminar grupo`.

Data-bearing group:

- show the visible name and impact counts;
- explain that associated information is removed and a safety backup is created first;
- require the exact current visible group name in a text field;
- enable `Eliminar definitivamente` only on ordinal exact match.

The typed confirmation uses a dedicated WPF dialog so `PasswordBox`/secret semantics are not involved. The group name is not sensitive data and may be bound as ordinary text.

## Accessibility and theme behavior

Archive, restore and delete controls remain keyboard-focusable and have explicit automation names. Destructive meaning is communicated with text/iconography rather than color alone. New surfaces reuse semantic theme resources and must remain legible in Light, Dark and High Contrast themes.
