# Proposal: local backup and restore

## Why

The application now stores irreplaceable classroom information locally: groups, students, attendance, projects, activities, formative evaluation, student follow-up, school context and lightweight application state. XLSX/CSV export is intentionally a teacher-facing representation of selected data and is not sufficient to recover the application after disk failure, accidental deletion, corruption or a failed computer migration.

A recovery workflow must therefore preserve the complete local application state in a versioned package and must treat restore as a destructive operation. The most important requirement is not convenience: it is that a failed or incompatible restore never silently destroys the current working data.

## What changes

- Add teacher-initiated manual backup of the current local application storage.
- Create one versioned `.sdocbackup` package containing a consistent SQLite snapshot plus valid application state when available.
- Store a manifest with package format/version, creation time, application version, source mode, database schema metadata, component sizes and SHA-256 checksums.
- Build the SQLite snapshot through SQLite's backup API rather than copying a live database file directly.
- Inspect and validate a selected backup before restore without touching live storage.
- Reject damaged, structurally invalid, unsupported, future/incompatible or wrong-mode backup packages.
- Validate the restored database in an isolated temporary location, including SQLite integrity/foreign-key checks and the application's current schema compatibility/migration path.
- Require explicit destructive confirmation before restore.
- Create an automatic safety backup of the current live state immediately before any restore; if the safety backup cannot be created, restore does not begin.
- Stage replacement files and roll back the live file swap when possible if publication fails.
- Require the application to exit after a successful restore so stale in-memory ViewModels cannot overwrite restored data.
- Surface backup date, source mode, application version, database version, included components and size before confirmation.
- Keep backup/restore global to the application rather than tied to one current group.

## Package and privacy principles

- Backup files contain personal and pedagogical data and are sensitive.
- Version 1 packages are local files and are **not encrypted**; the UI must state this clearly and advise secure storage.
- SHA-256 checksums detect accidental package corruption but are not a digital signature and do not establish authenticity against a maliciously modified package.
- Restore validation must therefore distrust package paths and contents even when checksums match the manifest.
- Technical logs must not dump database contents, application-state JSON or student records.
- Demo and production backups are mode-bound in version 1; a Demo backup cannot be restored into Production and vice versa.

## Compatibility

- The package format starts at version 1 and is explicitly versioned independently from `PRAGMA user_version`.
- The current application may restore an older database version only when its existing SQLite migration/extension initialization paths can prepare an isolated copy successfully.
- A database or package created by an unsupported future version is rejected before live data is changed.
- Restore installs the validated/prepared temporary database, not an unvalidated raw ZIP entry.

## Out of scope

- Scheduled/automatic backup policy.
- Cloud synchronization or cloud backup upload.
- Backup encryption/password protection or key management.
- Digital signatures/authenticity infrastructure.
- Digital evidence attachments, because the product does not yet persist them.
- Selective restore of one group, student or module.
- Merging backup contents into current data.
- Recovery from arbitrary XLSX/CSV exports.
- Database salvage/repair of already-corrupt source data.
- Automatic deletion/retention policy for safety backups.
