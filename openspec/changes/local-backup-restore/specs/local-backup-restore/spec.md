## ADDED Requirements

### Requirement: Manual backup creates a complete local recovery package
The system SHALL allow the teacher to create one manual `.sdocbackup` file representing the current local application storage profile.

The package SHALL include a consistent SQLite snapshot and SHALL include valid application state when available. Backup SHALL be global to the application rather than scoped to only the currently selected group.

#### Scenario: Teacher creates a production backup
- **WHEN** a production backup is created successfully
- **THEN** the package contains a recoverable snapshot of the complete production SQLite data rather than only the current group

#### Scenario: Teacher cancels the save dialog
- **WHEN** the teacher cancels before a destination is chosen
- **THEN** no backup file is reported as created and live application data is unchanged

### Requirement: Live SQLite is snapshotted consistently
The backup implementation SHALL use SQLite's supported backup/snapshot mechanism rather than copying a potentially active database file and WAL sidecars directly.

The temporary snapshot SHALL pass SQLite integrity and foreign-key validation before it is packaged.

#### Scenario: Database has normal WAL activity
- **WHEN** backup begins while the application database has ordinary SQLite WAL state
- **THEN** the created package contains one self-consistent database snapshot without requiring raw `-wal` or `-shm` files in the package

#### Scenario: Source database fails integrity validation
- **WHEN** the temporary backup snapshot fails SQLite integrity or foreign-key validation
- **THEN** ordinary backup fails and no successful package is published

### Requirement: Backup packages are explicitly versioned and self-describing
Every backup SHALL contain `manifest.json` with a stable product identifier, independent backup format version, creation timestamp, application version, source mode, database metadata and component sizes/checksums.

Backup format version 1 SHALL be distinguishable from SQLite `PRAGMA user_version`.

#### Scenario: Package metadata is inspected
- **WHEN** the teacher selects a valid backup for restore
- **THEN** the system can display creation time, source mode, application version, database version, package size and included components before confirmation

### Requirement: Backup component corruption is detected
The system SHALL store SHA-256 checksums for packaged data components and SHALL verify them before restore preparation.

Checksums SHALL be described as corruption detection, not as a digital signature or authenticity guarantee.

#### Scenario: Database entry bytes are changed after backup
- **WHEN** the packaged database no longer matches its manifest checksum
- **THEN** inspection rejects the backup before live storage is touched

### Requirement: Archive paths are treated as untrusted input
Restore inspection SHALL reject duplicate entry names, path traversal, unsafe/unexpected archive paths and missing required components. Extraction SHALL write only to application-chosen temporary paths.

#### Scenario: Archive contains `../` traversal
- **WHEN** a candidate backup contains an entry that could escape the temporary extraction directory
- **THEN** the backup is rejected without extracting that entry or changing live storage

#### Scenario: Archive contains duplicate database entries
- **WHEN** more than one archive entry claims the required database path
- **THEN** the backup is rejected as structurally invalid

### Requirement: Demo and production backup modes are isolated
Version 1 restore SHALL require the backup `sourceMode` to match the currently running application storage profile.

#### Scenario: Demo backup is selected in Production
- **WHEN** a Demo backup is inspected while the application is using Production storage
- **THEN** restore is blocked before any live file is changed

#### Scenario: Production backup is selected in Demo
- **WHEN** a Production backup is inspected while the application is using Demo storage
- **THEN** restore is blocked before any Demo file is changed

### Requirement: Restore compatibility is proven on an isolated database copy
The system SHALL NOT decide database compatibility only from `PRAGMA user_version`.

Before destructive confirmation, the system SHALL validate and prepare an extracted database copy through the application's current SQLite base-schema and additive-extension initialization/migration paths. Supported older schemas MAY migrate on that isolated copy. Unsupported future/incompatible schemas or extensions SHALL be rejected.

#### Scenario: Older supported database is selected
- **WHEN** the package contains a database version supported by the application's existing migration paths
- **THEN** the isolated copy is migrated/prepared successfully and may proceed to confirmation

#### Scenario: Future unsupported database is selected
- **WHEN** the package contains a future/incompatible database or extension version
- **THEN** restore is rejected before the current live database is replaced

### Requirement: Backup inspection never mutates live storage
Selecting or inspecting a candidate backup SHALL perform all parsing, checksum validation, extraction, SQLite checks and compatibility preparation outside the live storage paths.

#### Scenario: Inspection fails halfway through validation
- **WHEN** any candidate package validation step fails
- **THEN** the current live database and application state remain byte-for-byte untouched by the inspection workflow

### Requirement: Application state is optional but never allowed to remain stale after restore
A valid `app-state.json` SHALL be included in backup when available. If live application state is absent or invalid during backup, the database backup MAY still succeed with a warning and without that component.

If a restored package contains valid application state, it SHALL replace the live state. If the restored package contains no application state, the previous live state SHALL be removed rather than retained.

#### Scenario: Current state JSON is invalid during backup
- **WHEN** `app-state.json` exists but cannot be validated as JSON
- **THEN** database backup may complete, the invalid state is omitted and the result reports a warning

#### Scenario: Backup has no state file
- **WHEN** a valid database-only backup is restored
- **THEN** stale current `app-state.json` is removed so it cannot reference identities from the replaced database

### Requirement: Restore requires explicit destructive confirmation
The restore workflow SHALL clearly state that current local data will be replaced and SHALL require the teacher to type `RESTAURAR` after successful inspection before destructive work can begin.

#### Scenario: Confirmation text does not match
- **WHEN** the teacher has not entered the required confirmation text exactly
- **THEN** the restore command remains unavailable

#### Scenario: Teacher cancels after inspection
- **WHEN** the teacher closes/cancels the workflow before destructive confirmation
- **THEN** live storage is unchanged

### Requirement: A safety backup is mandatory before live replacement
Immediately before restoring prepared files, the system SHALL create a standard safety `.sdocbackup` of the current live state in an application-managed safety-backup directory.

If the safety backup cannot be created successfully, restore SHALL abort before moving or deleting any live database/state file.

#### Scenario: Safety backup fails
- **WHEN** destination disk or another infrastructure error prevents creation of the safety backup
- **THEN** restore does not begin and current live files remain unchanged

#### Scenario: Safety backup succeeds
- **WHEN** restore proceeds to file replacement
- **THEN** the result/failure context can identify the retained safety backup path

### Requirement: Live replacement is staged and rollback-aware
The restore implementation SHALL install only the validated/prepared database copy. It SHALL stage existing live files under rollback names before replacement and SHALL attempt to put them back if publication fails.

SQLite pools SHALL be cleared before the swap boundary, and stale live `-wal`/`-shm` sidecars SHALL NOT be allowed to survive as sidecars for the restored database.

#### Scenario: Database move fails after current files were staged
- **WHEN** the prepared database cannot be installed after the original live files were moved aside
- **THEN** the service attempts to restore the original files and retains the safety backup

#### Scenario: Restore publication succeeds
- **WHEN** all prepared components are installed successfully
- **THEN** the live database is the previously validated prepared database and no stale WAL/SHM sidecar is used with it

### Requirement: Successful restore requires application shutdown
After successful restore, the system SHALL not permit continued normal editing in the same process because in-memory aggregates/ViewModels may represent the pre-restore database.

The restore result SHALL require application shutdown after the teacher acknowledges success.

#### Scenario: Restore succeeds
- **WHEN** the prepared files have replaced live storage successfully
- **THEN** the UI reports success and the application exits before returning to ordinary classroom editing

### Requirement: Backup publication is destination-safe
Manual backup SHALL serialize to a temporary sibling file and SHALL publish/replace the requested destination only after the package is complete and closed successfully.

#### Scenario: ZIP writing fails
- **WHEN** package serialization fails after a temporary output has been created
- **THEN** no incomplete destination is presented as a successful backup and the temporary artifact is removed when possible

### Requirement: Backup and restore are application-global workflows
The recovery entry point SHALL be visually separate from group-specific XLSX/CSV export and SHALL explain that backup preserves application recovery state rather than producing a spreadsheet.

#### Scenario: Teacher opens recovery while a group is selected
- **WHEN** `Respaldo y restauración…` is opened from the global shell
- **THEN** the workflow describes recovery of the complete local application rather than only that group

### Requirement: Version 1 backup files are explicitly identified as unencrypted sensitive files
The UI SHALL warn that `.sdocbackup` version 1 files contain sensitive local data and are not encrypted. The workflow SHALL recommend storing them only in a secure location.

#### Scenario: Teacher prepares a manual backup
- **WHEN** the backup workflow shows the destination step
- **THEN** a visible warning states that the backup may contain personal/pedagogical information and is not encrypted

### Requirement: Recovery diagnostics do not leak student contents
Technical logging and user-facing infrastructure errors SHALL avoid dumping SQLite rows, backup component contents, application-state JSON or student/pedagogical records.

#### Scenario: Backup package is damaged
- **WHEN** inspection fails because a checksum or archive structure is invalid
- **THEN** the diagnostic explains the operational problem without emitting the contained student data

### Requirement: Automatic backups, encryption and selective restore remain outside version 1
Version 1 SHALL NOT silently schedule backups, upload them, encrypt them, merge them into current data or restore selected groups/modules independently.

#### Scenario: Teacher completes a manual backup
- **WHEN** the backup succeeds
- **THEN** no recurring backup policy is enabled as a side effect
