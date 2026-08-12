# Tasks: group lifecycle, archive and safe deletion

## 1. Core and application contracts

- [ ] 1.1 Add reversible active/archived state to `Grupo`, defaulting new groups to active.
- [ ] 1.2 Preserve lifecycle state during rehydration and prevent ordinary mutation while archived.
- [ ] 1.3 Extend `GrupoDetalle`, group storage and group use cases with archive/restore and active/archived queries.
- [ ] 1.4 Add a deletion-impact summary and permanent-delete operation to the application storage contract.

## 2. SQLite compatibility and deletion

- [ ] 2.1 Add v6-to-v7 migration for `grupos.archivado` with existing rows active by default.
- [ ] 2.2 Persist/read archive state and keep fresh v7 schema validation strict.
- [ ] 2.3 Compute deletion-impact counts from current group-related data.
- [ ] 2.4 Delete known restrictive group dependents and the group in one SQLite transaction without disabling foreign keys.
- [ ] 2.5 Cover NEM/context/report extension rows through existing cascade relationships.

## 3. Safety backup

- [ ] 3.1 Reuse the existing local recovery service to create a managed v1 backup before deleting any data-bearing group.
- [ ] 3.2 Abort deletion when safety-backup creation fails.
- [ ] 3.3 Skip automatic safety backup for a truly empty group created by mistake.

## 4. Presentation and WPF

- [ ] 4.1 Separate active and archived group collections in presentation state.
- [ ] 4.2 Keep archived groups out of normal group switching and teacher modules until restored.
- [ ] 4.3 Add Archive and Delete actions to active group cards with accessible labels.
- [ ] 4.4 Add an `Archivados` section with Restore and Delete actions and no Open action.
- [ ] 4.5 Add simple archive/empty-delete confirmation.
- [ ] 4.6 Add typed exact-name confirmation for data-bearing permanent deletion with impact summary.
- [ ] 4.7 Clear stale current-group selection after archive/delete and return safely to `Mis grupos`.

## 5. Automated validation

- [ ] 5.1 Core tests cover active default, idempotent archive/restore, rehydration and archived mutation rejection.
- [ ] 5.2 Data tests cover v6 migration, persisted lifecycle state, impact counts and complete relational deletion.
- [ ] 5.3 Data tests prove backup failure prevents deletion and successful data-bearing deletion creates a safety backup.
- [ ] 5.4 Presentation tests cover active/archived list movement and selected-group reset behavior.
- [ ] 5.5 WPF tests verify archive/restore/delete affordances and typed confirmation semantics.
- [ ] 5.6 Run formatting, Release build, automated tests, coverage, OpenSpec validation and whitespace checks.

## 6. Manual acceptance

- [ ] 6.1 In Demo, archive an active group with data and verify it disappears from active workspaces without data loss.
- [ ] 6.2 Restore the group and verify students, attendance, projects/evaluation and context remain available.
- [ ] 6.3 Delete an empty test group through simple confirmation.
- [ ] 6.4 Attempt deletion of a populated group with a wrong typed name and verify deletion remains disabled.
- [ ] 6.5 Delete a populated group with exact-name confirmation, verify a safety backup is created and the group disappears completely.
- [ ] 6.6 Recheck Light, Dark and High Contrast plus keyboard focus for lifecycle controls/dialog.
