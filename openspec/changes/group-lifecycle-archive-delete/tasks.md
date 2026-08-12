# Tasks: group lifecycle, archive and safe deletion

## 1. Core and application contracts

- [x] 1.1 Add reversible active/archived state to `Grupo`, defaulting new groups to active.
- [x] 1.2 Preserve lifecycle state during rehydration and prevent ordinary mutation while archived.
- [x] 1.3 Extend `GrupoDetalle`, group storage and group use cases with archive/restore and active/archived queries.
- [x] 1.4 Add a deletion-impact summary and permanent-delete operation to the application storage contract.

## 2. SQLite compatibility and deletion

- [x] 2.1 Add an independently versioned `group-lifecycle` SQLite extension while keeping base `PRAGMA user_version` at v6.
- [x] 2.2 Default existing groups to active and persist/read archive state through the lifecycle storage wrapper.
- [x] 2.3 Compute deletion-impact counts from current group-related data, excluding empty compatibility context rows.
- [x] 2.4 Delete known restrictive group dependents and the group in one SQLite transaction without disabling foreign keys.
- [x] 2.5 Cover NEM/context/report/lifecycle extension rows through existing cascade relationships.

## 3. Safety backup

- [x] 3.1 Reuse the existing local recovery service to create a managed v1 backup before deleting any data-bearing group.
- [x] 3.2 Abort deletion when safety-backup creation fails.
- [x] 3.3 Skip automatic safety backup for a truly empty group created by mistake.

## 4. Presentation and WPF

- [x] 4.1 Keep normal group queries active-only and expose a separate archived collection for management.
- [x] 4.2 Keep archived groups out of normal group switching and teacher modules until restored.
- [x] 4.3 Add Archive and Delete actions to active group cards with accessible labels.
- [x] 4.4 Add an `Archivados` section with Restore and Delete actions and no Open action.
- [x] 4.5 Add simple archive/empty-delete confirmation.
- [x] 4.6 Add typed exact-name confirmation for data-bearing permanent deletion with impact summary.
- [x] 4.7 Clear stale current-group selection after archive/delete and return safely to `Mis grupos`.

## 5. Automated validation

- [x] 5.1 Core tests cover active default, idempotent archive/restore, rehydration and archived mutation rejection.
- [x] 5.2 Application/data tests cover active/archived filtering, additive extension initialization, persisted lifecycle state, impact counts and complete relational deletion.
- [x] 5.3 Data tests prove backup failure prevents deletion, populated deletion creates a safety backup first and empty deletion skips the backup.
- [x] 5.4 Application/WPF structure tests cover active/archived separation and lifecycle composition.
- [x] 5.5 WPF tests verify archive/restore/delete affordances and typed exact-name confirmation semantics.
- [x] 5.6 Run formatting, Release build, automated tests, coverage, OpenSpec validation and whitespace checks on the exact PR head.

## 6. Manual acceptance

- [ ] 6.1 In Demo, archive an active group with data and verify it disappears from active workspaces without data loss.
- [ ] 6.2 Restore the group and verify students, attendance, projects/evaluation and context remain available.
- [ ] 6.3 Delete an empty test group through simple confirmation.
- [ ] 6.4 Attempt deletion of a populated group with a wrong typed name and verify deletion remains disabled.
- [ ] 6.5 Delete a populated group with exact-name confirmation, verify a safety backup is created and the group disappears completely.
- [ ] 6.6 Recheck Light, Dark and High Contrast plus keyboard focus for lifecycle controls/dialog.
