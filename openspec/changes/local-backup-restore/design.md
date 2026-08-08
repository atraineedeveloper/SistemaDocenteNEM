# Design: local backup and restore

## Goals

- Preserve the complete current local application state in one portable recovery package.
- Make backup safe while the SQLite database is in normal use.
- Make restore conservative: validate first, create a safety backup second, replace live files only after both succeed.
- Reuse the application's existing SQLite schema/migration behavior rather than inventing a second compatibility engine.
- Keep Presentation portable and keep WPF limited to desktop-specific file picking, confirmation and application shutdown.
- Ensure export remains separate from recovery.

## Non-goals

- Automatic backup schedules.
- Encryption, password-protected ZIP files or cloud key management.
- Cloud storage integration.
- Partial/merge restore.
- Evidence-file backup before the evidence module exists.
- Repairing a corrupt source database.

## Architecture

The first implementation keeps recovery infrastructure in `SistemaDocente.Data` because the operation is inseparable from the current SQLite persistence and local storage layout.

### Application

Application owns recovery use-case contracts and result models, for example:

- `IServicioRespaldoLocal`;
- `GestionRespaldoCasosUso`;
- `SolicitudRespaldoLocal`;
- `ResumenRespaldoLocal`;
- `InspeccionRespaldoLocal`;
- `SolicitudRestauracionLocal`;
- `ResultadoRestauracionLocal`.

Application never manipulates ZIP entries or calls SQLite APIs directly.

### Data

Data implements the recovery port using:

- SQLite online backup API for a consistent database snapshot;
- `ZipArchive` for the package container;
- SHA-256 component checksums;
- isolated temporary directories for inspection/preparation;
- existing SQLite initialization/migration paths to prove compatibility on a copy;
- staged same-volume file replacement/rollback for the live database and application-state file.

The implementation is constructed with the concrete production or Demo storage paths at the composition root. It does not discover `%LOCALAPPDATA%` itself.

### Presentation

Presentation owns the portable state machine for:

- creating a backup;
- inspecting a candidate backup;
- presenting metadata/warnings;
- destructive restore confirmation state;
- reporting success/failure/restart-required status.

### WPF

WPF owns:

- native open/save file dialogs;
- a global `Respaldo y restauración…` entry that is not tied to one group;
- the final typed confirmation UI;
- shutting down the process after a successful restore.

## Version 1 package format

File extension: `.sdocbackup`.

The physical container is a ZIP archive, but callers interact with it only through the recovery service.

Expected entries:

```text
manifest.json
data/sistema-docente.db
data/app-state.json       # optional
```

Unexpected paths, duplicate entry names and path traversal components are rejected.

### `manifest.json`

The manifest contains at least:

```json
{
  "format": "SistemaDocenteNEM.Backup",
  "formatVersion": 1,
  "createdUtc": "2026-08-08T02:00:00Z",
  "applicationVersion": "...",
  "sourceMode": "Production",
  "database": {
    "path": "data/sistema-docente.db",
    "userVersion": 6,
    "sizeBytes": 123456,
    "sha256": "..."
  },
  "applicationState": {
    "included": true,
    "path": "data/app-state.json",
    "sizeBytes": 123,
    "sha256": "..."
  }
}
```

`sourceMode` is `Production` or `Demo` and must match the currently running storage profile for restore.

Checksums detect accidental corruption. They do not provide authenticity because there is no digital signature in version 1.

## Manual backup flow

1. WPF asks the teacher for a destination `.sdocbackup` file.
2. Application asks the recovery service to create the backup.
3. Data creates a unique temporary working directory.
4. Data opens the live SQLite database and creates a consistent temporary snapshot using `SqliteConnection.BackupDatabase` rather than copying the live `.db`, `-wal` and `-shm` files.
5. The temporary snapshot is checked with `PRAGMA integrity_check` and `PRAGMA foreign_key_check`.
6. Data reads the snapshot `PRAGMA user_version` and package metadata.
7. If `app-state.json` exists and is valid JSON, it is included. If it is absent, the package remains valid without it. An invalid state file does not block the database backup; it is omitted and the result exposes a warning.
8. Component sizes and SHA-256 hashes are calculated.
9. Data writes the package to a temporary sibling file next to the requested destination.
10. Only after the ZIP closes successfully is the destination atomically moved/replaced where the platform permits.
11. Temporary working files are removed when possible.

A known-corrupt live database causes ordinary backup to fail. Version 1 does not pretend to be a salvage tool.

## Backup inspection flow

Selecting a backup for restore never changes live storage.

Inspection performs, in order:

1. Basic file/ZIP readability.
2. Manifest presence, bounded size and JSON parsing.
3. Exact product identifier and supported package format version.
4. Current-mode match (`Production` vs `Demo`).
5. Required entry presence and absence of duplicate/unexpected unsafe paths.
6. Component size and SHA-256 verification.
7. Extraction to a unique temporary directory using trusted destination paths rather than archive-provided paths.
8. SQLite `integrity_check` and `foreign_key_check` on the extracted database.
9. Database compatibility preparation on an isolated copy through the current schema/migration/extension initialization paths.
10. Validation of optional application state when present.

Inspection returns human-readable metadata and a compatibility status. A failed inspection leaves live storage untouched.

## Database compatibility preparation

Compatibility must not be reduced to `PRAGMA user_version == 6` because the application also uses additive extension tables.

The Data layer therefore prepares an isolated extracted database with the same initialization paths used by the application today. Existing supported base schema versions may migrate on the temporary copy. Current extension initializers may create/upgrade supported extension metadata on that copy.

If any current initializer reports an incompatible future schema/extension or data-integrity problem, restore is rejected.

The prepared temporary database is the file later installed. The original package entry remains unchanged.

## Restore flow

Restore is intentionally multi-stage.

1. Teacher chooses a backup.
2. The application completes full inspection/preparation.
3. The UI shows creation time, source mode, app version, database version, package size and included components.
4. The UI states that current local data will be replaced and that version 1 backups are not encrypted.
5. Teacher must explicitly confirm the destructive action by typing `RESTAURAR`.
6. **Before touching live files**, Data creates a normal safety `.sdocbackup` of the current live state in an application-managed safety-backup directory.
7. If the safety backup fails, restore aborts.
8. Data clears SQLite pools as an additional guard, stages rollback file names in the live storage directory and removes stale `-wal`/`-shm` sidecars only at the controlled swap boundary.
9. Current live database/state files are moved aside to rollback names where they exist.
10. The prepared database is moved into the live database path.
11. If the selected backup contains valid application state, that prepared state replaces the live state. If the backup has no application state, the current live `app-state.json` is removed so stale reopen state cannot point at unrelated restored identities.
12. If publication fails, Data attempts to restore the moved-aside originals.
13. On success, rollback staging files are removed when possible; the safety `.sdocbackup` is retained.
14. The result requires application shutdown before any further normal save can occur.

## Safety backup location

`RutasAplicacion` will gain an application-profile-specific managed backup directory, for example:

```text
Production
%LOCALAPPDATA%\SistemaDocenteNEM\backups\safety\

Demo
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\backups\safety\
```

Safety backups use deterministic timestamps plus a collision-safe suffix. Version 1 does not automatically delete them.

## Process lifecycle after restore

A successful restore must not leave old in-memory aggregates/ViewModels active against the newly replaced database.

Therefore `ResultadoRestauracionLocal` reports `ReinicioRequerido = true`, WPF informs the teacher that restoration succeeded, and the application exits immediately after acknowledgement. Automatic relaunch is not required in version 1; the teacher can reopen the application normally.

## Concurrency

Backup/inspection/restore operations are serialized inside the recovery service. Restore runs in a modal global workflow so ordinary classroom editing cannot continue during the destructive boundary.

Normal persistence adapters use short-lived connections and currently configure SQLite pooling off; the restore path additionally calls `SqliteConnection.ClearAllPools()` before file replacement.

## Privacy and logging

- `.sdocbackup` contains sensitive local data and is not encrypted in version 1.
- The UI must display a clear secure-storage warning.
- Logs may record operation type, package path, timestamps and high-level exception categories.
- Logs must not dump manifest component contents, SQLite rows, JSON contents or student/pedagogical data.
- Temporary extraction directories must be deleted on completion/failure when possible.

## Failure semantics

### Backup failure

- Live storage is unchanged.
- No successful destination is reported.
- Partial temporary output is removed when possible.

### Inspection failure

- Live storage is unchanged.
- The UI reports a concise reason such as damaged package, wrong mode or incompatible version.

### Restore preparation failure

- Live storage is unchanged.
- No safety backup is necessary until preparation has fully succeeded.

### Safety-backup failure

- Live storage is unchanged.
- Restore is aborted.

### Live swap failure

- The service attempts file-level rollback from the staged originals.
- The already-created safety backup is retained and its path is surfaced in the critical result/error.
- The application must not continue normal editing if the service cannot prove that the original live storage was restored.

## Testing strategy

Automated coverage will include:

- real SQLite online-backup snapshot with WAL activity;
- package manifest/checksum round trip;
- missing/duplicate/unsafe ZIP entry rejection;
- checksum mismatch;
- wrong Demo/Production mode;
- unsupported package format;
- future/incompatible database schema;
- `integrity_check`/foreign-key failure;
- valid older supported database migration on the isolated copy;
- missing and invalid `app-state.json` behavior;
- safety backup creation before file replacement;
- forced failure during staged publication and rollback;
- restore without app state clears stale current state;
- Presentation confirmation gating;
- actual WPF window construction;
- Demo end-to-end backup, mutate Demo data, restore, exit/reopen and verify the original snapshot is recovered.
