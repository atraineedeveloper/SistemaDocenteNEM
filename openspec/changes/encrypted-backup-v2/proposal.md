# Proposal: optional encrypted backup v2

## Why

AulaRaíz version-1 `.sdocbackup` recovery packages are intentionally portable and already protect recovery integrity through conservative snapshot, validation and rollback behavior, but their contents are not encrypted. That is acceptable for the current default workflow when the teacher stores the file in a trusted location, yet a teacher may reasonably copy a backup to removable media or another location where confidentiality matters.

Password protection should therefore be available without making recovery harder for every teacher or destabilizing the proven version-1 format. The safest compatibility strategy is to leave ordinary unprotected backups on format version 1 and introduce format version 2 only when the teacher explicitly opts into password protection.

## What changes

- Keep the existing unprotected version-1 backup creation and restore behavior unchanged by default.
- Add an explicit `Proteger con contraseña` option to manual backup creation.
- Create a version-2 `.sdocbackup` only when password protection is selected.
- Wrap the existing version-1 recovery payload inside an authenticated encrypted envelope rather than inventing a second database/manifest recovery model.
- Derive a 256-bit encryption key from the teacher password with PBKDF2-HMAC-SHA256 using a fresh random salt and a stored iteration count.
- Encrypt the payload with chunked AES-256-GCM so large backups can be processed without loading the complete package into memory.
- Keep only bounded, non-personal cryptographic metadata outside the encrypted payload.
- Require password confirmation when creating a protected backup and warn that AulaRaíz cannot recover a forgotten password.
- Ask for the password before protected-backup metadata is inspected; only after successful authenticated decryption does the existing version-1 inspection/compatibility path run.
- Treat a wrong password and authenticated-ciphertext failure as the same non-destructive operational error.
- Never store, log, serialize or transmit the backup password or derived key.

## Privacy impact

A protected version-2 package prevents ordinary disclosure of the contained student, family, school and pedagogical data when the `.sdocbackup` file is copied or lost. The plaintext outer header contains only the product/format identifier and bounded cryptographic parameters required to derive the key and decrypt the payload; classroom metadata such as student names, group data, source mode and backup creation details remains inside the encrypted payload.

Encryption does not make a weak password strong, does not protect an already-unlocked Windows session, and does not replace appropriate device/storage security. Temporary plaintext working files required by the existing recovery pipeline remain application-controlled and must be deleted on success/failure when possible.

## Compatibility

- Existing version-1 `.sdocbackup` files remain readable and restorable.
- Creating an unprotected backup continues to produce version 1.
- Version 2 uses the same `.sdocbackup` extension and the historical `SistemaDocenteNEM.Backup` product identifier.
- Version 2 decrypts to the existing version-1 logical payload, then reuses all current manifest, checksum, Demo/Production, SQLite integrity, schema-compatibility and restore-safety validation.
- No SQLite schema, classroom data model, export format, PDF, CLI, updater or installer data contract changes.
- An older AulaRaíz version that does not understand version 2 may reject a protected backup as unsupported; the protected-backup UI must make that forward-compatibility boundary clear.

## Non-goals

- Requiring passwords for every backup.
- Automatically converting existing version-1 backups.
- Password recovery, escrow, recovery questions or cloud key storage.
- DPAPI/device-bound encryption that would prevent restoration on another computer.
- Automatic/scheduled backup policy.
- Encrypting existing application-managed safety backups as part of this change.
- Cloud upload/synchronization.
- Digital signatures or backup publisher authenticity.
- Selective/merge restore.
