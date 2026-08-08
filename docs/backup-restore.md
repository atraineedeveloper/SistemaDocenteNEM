# Local backup and restore

Sistema Docente Local keeps classroom information on the teacher's computer. XLSX/CSV export produces teacher-readable subsets of that information, but an export is not a recovery image of the application. The local backup/restore workflow exists to recover the complete application state after accidental deletion, storage failure, computer migration or another local-data incident.

## Version 1 scope

Version 1 provides:

- teacher-initiated manual backup;
- complete SQLite snapshot of the current Production or Demo storage profile;
- optional valid application reopen state;
- a versioned `.sdocbackup` package;
- package inspection before restore;
- SHA-256 corruption checks;
- SQLite integrity and foreign-key validation;
- isolated compatibility/migration preparation;
- mandatory safety backup immediately before restore;
- explicit destructive confirmation;
- staged replacement with rollback attempt;
- application shutdown after a successful restore.

Version 1 intentionally does **not** provide scheduled backups, encryption/password protection, cloud synchronization, selective restore, merge restore, evidence-file backup or automatic retention/deletion of old safety backups.

## Backup file format

The user-facing extension is:

```text
.sdocbackup
```

The physical container is a ZIP archive managed only through the recovery service. Version 1 contains:

```text
manifest.json
data/sistema-docente.db
data/app-state.json       # optional
```

`manifest.json` identifies the product and package format independently from the SQLite database version. It stores the creation timestamp, application version, Production/Demo source mode, database `PRAGMA user_version`, component sizes and SHA-256 checksums.

Checksums detect accidental corruption. They are **not** a digital signature and do not prove that a package was not deliberately modified by someone who can rewrite both the component and the manifest.

## Privacy and security limit

A `.sdocbackup` file can contain names, attendance, projects, evaluations, observations and student follow-up. Version 1 packages are **not encrypted**.

Treat each backup as sensitive personal/pedagogical information. Store it only on a device, external drive or folder that is appropriately protected for the teacher's context. Do not upload real backup files to the repository or use them as test fixtures.

Encryption/key management is a separate future security feature rather than something the first recovery implementation pretends to provide.

## Production and Demo isolation

A backup records one source mode:

- `Production`; or
- `Demo`.

Version 1 does not allow cross-mode restore. A Demo package cannot replace Production data, and a Production package cannot replace Demo data. This prevents a manual test from accidentally becoming a production-data replacement operation.

## How manual backup works

The backup workflow is global to the application, not tied to the currently open group.

1. The teacher opens **Respaldo y restauración…** from the global shell.
2. The teacher chooses **Crear respaldo…** and a destination `.sdocbackup` file.
3. The Data layer creates a consistent temporary SQLite snapshot with `SqliteConnection.BackupDatabase`.
4. The snapshot passes `PRAGMA integrity_check` and `PRAGMA foreign_key_check`.
5. A valid `app-state.json` is included when available. Missing/invalid state does not block the database backup; it is omitted with a warning.
6. SHA-256 checksums and manifest metadata are calculated.
7. The package is written to a temporary sibling file.
8. Only after the archive closes successfully is the requested destination published/replaced.

The live SQLite file, WAL and SHM files are never copied directly as the backup representation.

## Inspecting a backup

Selecting a package for restore does not change live data.

Inspection performs the following outside the live storage paths:

1. opens the ZIP and validates the bounded manifest;
2. rejects duplicate, unexpected or path-traversal entries;
3. verifies the product identifier and supported package version;
4. verifies Production/Demo mode;
5. verifies component sizes and SHA-256 checksums;
6. extracts components only to application-chosen temporary paths;
7. checks SQLite integrity and foreign keys;
8. copies the extracted database and runs the application's current base-schema and additive-extension initialization/migration paths on that copy;
9. rejects unsupported future/incompatible schema or extension versions;
10. validates optional application-state JSON;
11. presents backup date, application/database version, mode, size, components and warnings to the teacher.

Supported older database versions may therefore be prepared on a temporary copy without rewriting the package or the current live database.

## Restore safety boundary

Restore is destructive because it replaces current local state. The workflow requires the teacher to type exactly:

```text
RESTAURAR
```

Before any live database/state file is moved, the recovery service creates a normal safety `.sdocbackup` of the current state under the application profile:

```text
Production
%LOCALAPPDATA%\SistemaDocenteNEM\backups\safety\

Demo
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\backups\safety\
```

If that mandatory safety backup cannot be created, restore does not begin.

## How live replacement works

After the selected package is fully validated/prepared and the safety backup exists:

1. prepared files are staged in the live-storage directory;
2. SQLite connection pools are cleared;
3. current database, application state and any WAL/SHM sidecars are moved to unique rollback names;
4. the validated/prepared database is installed;
5. valid packaged application state is installed when present;
6. if the package has no state file, the old state is intentionally not retained;
7. if publication fails, the service attempts to restore the moved-aside originals;
8. the safety backup is retained whether restore succeeds or a later publication failure occurs;
9. after success, the application reports the safety-backup path and exits.

The application exits because ViewModels and aggregates in the current process may still represent the pre-restore database. Continuing to edit after replacing the database could otherwise write stale state back into the restored storage.

## Troubleshooting

### The package is reported as damaged

Possible causes include a truncated ZIP, missing component, checksum mismatch or invalid SQLite file. The current live storage remains unchanged during inspection failure.

### The package is incompatible

The package may come from the wrong Production/Demo mode, an unsupported backup-format version, or a future/incompatible SQLite/schema-extension version. No live replacement occurs.

### Restore says the safety backup could not be created

The restore is blocked intentionally. Check available disk space and write permissions for the application profile. Do not work around this guard by manually deleting the current database.

### Restore fails during publication

The service attempts file-level rollback and retains the previously created safety backup. If it cannot prove the old live files were restored, the error includes the safety-backup location and normal editing should not continue until recovery is resolved.

### Backup contains no application state

The database is still recoverable. On restore, stale current reopen state is removed, so the teacher may need to select the intended group again after reopening the application.

## Testing policy

Automated tests use only temporary files and fictitious data. Coverage includes active-WAL snapshotting, manifest/checksum round trips, hostile ZIP paths, duplicate entries, checksum tampering, Production/Demo isolation, future-version rejection, older-version preparation, mandatory safety backup, rollback behavior and full Demo backup/mutate/restore/reopen recovery.

Manual acceptance must also use Demo mode before merge. Never use real student data to validate a feature branch.
