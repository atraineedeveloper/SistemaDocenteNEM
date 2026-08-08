# Tasks

## Recovery contracts and package foundation
- [x] 1. Add Application backup/inspection/restore contracts, result models and a recovery service port.
- [x] 2. Extend application storage paths with an isolated managed safety-backup directory for Production and Demo.
- [x] 3. Implement version 1 `.sdocbackup` manifest models with explicit product id, package version, source mode, component metadata and SHA-256 checksums.
- [x] 4. Implement safe ZIP package writing/reading with duplicate/unexpected/path-traversal rejection and bounded manifest handling.

## Manual backup
- [x] 5. Create a consistent SQLite snapshot with `SqliteConnection.BackupDatabase` instead of raw live-file copying.
- [x] 6. Validate the temporary snapshot with SQLite integrity and foreign-key checks before packaging.
- [x] 7. Include valid `app-state.json` when available and continue database backup with a warning when state is missing/invalid.
- [x] 8. Publish manual backups through a temporary sibling file so failed package generation cannot masquerade as a successful destination.
- [x] 9. Return backup metadata including destination, creation time, size, database version, included components and warnings.

## Inspection and compatibility
- [x] 10. Inspect candidate packages without touching live storage.
- [x] 11. Validate manifest/product/package version, mode, required entries, hashes and safe archive structure.
- [x] 12. Extract only to application-chosen temporary paths and validate SQLite integrity/foreign keys on the extracted database.
- [x] 13. Prepare an isolated database copy through all current base-schema and additive-extension initialization/migration paths.
- [x] 14. Reject future/incompatible database or extension versions before destructive confirmation.
- [x] 15. Validate optional application-state JSON and expose human-readable package metadata for review.

## Safe restore
- [x] 16. Add typed `RESTAURAR` confirmation gating after successful inspection.
- [x] 17. Create a standard safety `.sdocbackup` in the managed safety directory immediately before live replacement.
- [x] 18. Abort restore without moving/deleting live files when the safety backup cannot be created.
- [x] 19. Stage current database/state files under rollback names and clear SQLite pools/WAL/SHM at the controlled swap boundary.
- [x] 20. Install only the validated/prepared database copy and restore valid application state or clear stale state when absent.
- [x] 21. Attempt file-level rollback if publication fails and preserve/surface the safety-backup path.
- [x] 22. Return a restart-required result and prevent normal in-process editing after successful restore.

## Presentation and WPF
- [x] 23. Add a portable recovery ViewModel for backup, inspection metadata, warnings, confirmation and result states.
- [x] 24. Add a global `Respaldo y restauración…` entry separate from group XLSX/CSV export.
- [x] 25. Add native save/open dialogs for `.sdocbackup` without ZIP/SQLite logic in code-behind.
- [x] 26. Display that version 1 backups contain sensitive personal/pedagogical data and are not encrypted.
- [x] 27. Display backup date, source mode, app/database version, included components, size and compatibility before restore confirmation.
- [x] 28. Shut down the WPF application after successful restore acknowledgement.

## Documentation and roadmap
- [x] 29. Add maintained recovery documentation covering package purpose, security limits, manual backup, inspection, restore, safety backups and troubleshooting.
- [x] 30. Update README/architecture/roadmap to mark group export merged and local manual backup/restore as the active change.
- [x] 31. Keep automatic backup policy, encryption, evidence-file backup and cloud integration explicitly deferred.

## Quality
- [x] 32. Add package round-trip tests for manifest/checksums and Unicode paths/metadata.
- [x] 33. Add malicious/invalid ZIP tests for traversal, duplicate entries, missing components and checksum mismatch.
- [x] 34. Add real SQLite online-backup tests including WAL activity and integrity checks.
- [x] 35. Add compatibility tests for supported older schema migration plus future/incompatible schema rejection.
- [x] 36. Add state-file tests for valid, missing and invalid `app-state.json`.
- [x] 37. Add restore tests proving safety backup happens before any live move and blocks restore when it fails.
- [x] 38. Add forced publication-failure tests proving rollback attempts and retained safety-backup reporting.
- [x] 39. Add Presentation/WPF regressions for confirmation gating, global entry point, warning text and real window construction.
- [x] 40. Add an end-to-end Demo recovery test: backup, mutate Demo data, restore, reopen storage and verify the original snapshot.
- [ ] 41. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [ ] 42. Manually validate Demo backup creation, metadata inspection, destructive confirmation, restore/reopen and recovered data before merge.