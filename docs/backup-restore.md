# Local backup and restore

AulaRaíz keeps classroom information on the teacher's computer. XLSX/CSV export produces teacher-readable subsets of that information, but an export is not a recovery image of the application. The local backup/restore workflow exists to recover the complete application state after accidental deletion, storage failure, computer migration or another local-data incident.

## Supported backup formats

AulaRaíz keeps the proven version-1 recovery format as the default and adds optional password protection as version 2.

### Version 1 — standard backup

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

Version 1 remains intentionally **unencrypted**. It also does not provide scheduled backups, cloud synchronization, selective restore, merge restore, evidence-file backup or automatic retention/deletion of old safety backups.

### Version 2 — optional password-protected backup

Version 2 is created only when the teacher explicitly enables **Proteger con contraseña (v2)**. Ordinary manual backup continues to create version 1.

Version 2 does not introduce a second SQLite recovery model. AulaRaíz first creates the same logical version-1 recovery payload, then encrypts/authenticates that complete payload inside a version-2 envelope. After successful decryption, the existing version-1 manifest, checksums, Production/Demo checks, SQLite integrity, schema preparation and destructive-restore protections remain the source of truth.

The password must contain at least 12 characters. Spaces are allowed and there are no character-class composition rules. AulaRaíz does not store the password and cannot recover a protected backup if the password is forgotten.

Version 2 is portable across supported Windows computers because it does not depend on DPAPI or another machine/account-local secret.

## Backup file format

The user-facing extension remains:

```text
.sdocbackup
```

### Version-1 physical package

The physical container is a ZIP archive managed only through the recovery service. Version 1 contains:

```text
manifest.json
data/sistema-docente.db
data/app-state.json       # optional
```

`manifest.json` identifies the product and package format independently from the SQLite database version. It stores the creation timestamp, application version, Production/Demo source mode, database `PRAGMA user_version`, component sizes and SHA-256 checksums.

The version-1 package identifier remains `SistemaDocenteNEM.Backup` for backward compatibility with backups created before the AulaRaíz branding change. The product rename does not invalidate existing packages.

Checksums detect accidental corruption. They are **not** a digital signature and do not prove that a package was not deliberately modified by someone who can rewrite both the component and the manifest.

### Version-2 physical envelope

A protected package is an outer ZIP with exactly:

```text
protection.json
payload.bin
```

`payload.bin` is the authenticated ciphertext of the complete logical version-1 package. Normal backup metadata — including source mode, creation time, application/database versions and classroom components — remains inside that ciphertext.

`protection.json` contains only bounded non-classroom parameters needed to derive the key and decrypt the payload:

- historical format id `SistemaDocenteNEM.Backup`;
- backup format version `2`;
- password-protection mode;
- PBKDF2-HMAC-SHA256 identifier and iteration count;
- random salt;
- AES-256-GCM chunked profile identifier;
- chunk size;
- random nonce prefix;
- plaintext payload size and chunk count.

The exact UTF-8 header bytes are authenticated as associated data for every chunk. A valid change to a cryptographic header field therefore causes authentication to fail unless the complete package was created consistently with that header.

## Version-2 cryptographic profile

The first version-2 writer uses only .NET platform cryptography:

- fresh 16-byte random salt per protected backup;
- password encoded after Unicode NFC normalization;
- PBKDF2-HMAC-SHA256;
- 600,000 PBKDF2 iterations for the initial writer profile;
- a 32-byte derived key;
- AES-256-GCM;
- 16-byte authentication tags;
- 1 MiB plaintext chunks;
- a fresh four-byte random nonce prefix plus an eight-byte monotonically increasing chunk index to form each 96-bit GCM nonce.

The iteration count is stored in the package so a future writer can increase it without changing format version 2. Readers currently accept only bounded values from 100,000 through 5,000,000 iterations and reject unreasonable work parameters before performing the KDF. Chunk sizes are similarly bounded before allocation.

The 600,000 writer count is an AulaRaíz product baseline rather than a claim that one iteration count is universally optimal. It should be re-benchmarked on supported Windows hardware before a broad distribution milestone; changing the writer count within the accepted v2 range does not require a new format version.

Password and derived-key material is never written to the package, application state, SQLite, logs, diagnostics or CLI arguments. Mutable buffers are cleared when practical after each operation. WPF keeps password entry in `PasswordBox` controls rather than ViewModel properties.

## Privacy and security limits

A `.sdocbackup` file can contain names, attendance, projects, evaluations, observations and student follow-up.

A standard version-1 backup is **not encrypted**. Store it only on a device, external drive or folder that is appropriately protected for the teacher's context.

A password-protected version-2 backup provides confidentiality and authenticated integrity for the portable backup artifact, but it does not:

- make a weak password strong;
- recover a forgotten password;
- protect an already-unlocked Windows session;
- replace full-disk/device security;
- digitally sign the backup or prove who created it;
- guarantee secure deletion of temporary plaintext after a process or operating-system crash.

During v2 create/decrypt operations, a plaintext logical v1 package may exist temporarily in an application-chosen temporary directory. AulaRaíz removes it on success/failure when possible. This is a bounded working-file tradeoff that lets version 2 reuse the already-tested v1 recovery semantics rather than duplicate them.

Do not upload real backup files to the repository or use them as test fixtures.

## Production and Demo isolation

A backup records one source mode inside its logical version-1 payload:

- `Production`; or
- `Demo`.

Neither v1 nor v2 allows cross-mode restore. A Demo package cannot replace Production data, and a Production package cannot replace Demo data. In v2 the mode is not visible until successful password authentication/decryption.

## How manual backup works

The backup workflow is global to the application, not tied to the currently open group.

### Standard v1

1. The teacher opens **Respaldo y restauración…** from the global shell.
2. The teacher leaves password protection disabled, chooses **Crear respaldo…** and a destination `.sdocbackup` file.
3. The Data layer creates a consistent temporary SQLite snapshot with `SqliteConnection.BackupDatabase`.
4. The snapshot passes `PRAGMA integrity_check` and `PRAGMA foreign_key_check`.
5. A valid `app-state.json` is included when available. Missing/invalid state does not block the database backup; it is omitted with a warning.
6. SHA-256 checksums and manifest metadata are calculated.
7. The package is written to a temporary sibling file.
8. Only after the archive closes successfully is the requested destination published/replaced.

### Protected v2

1. The teacher enables **Proteger con contraseña (v2)**.
2. The teacher enters and confirms a password of at least 12 characters and accepts the warning that AulaRaíz cannot recover a forgotten password.
3. AulaRaíz creates the logical v1 package in an application-controlled temporary location using the normal snapshot/validation path.
4. A fresh salt and nonce prefix are generated, and the v2 header is serialized.
5. The v1 payload is streamed through chunked AES-256-GCM into a temporary outer v2 package; the complete package is never required in memory at once.
6. Only the completed outer archive is published to the requested destination.
7. Plaintext temporary package and partial ciphertext are removed when possible; mutable password/key buffers are cleared when practical.

New suggested filenames use the ASCII-safe AulaRaíz brand form, for example `AulaRaiz_Respaldo_Produccion_...sdocbackup`. Existing backup filenames do not need to be renamed.

The live SQLite file, WAL and SHM files are never copied directly as the backup representation.

## Inspecting a backup

Selecting a package for restore does not change live data.

### Version 1

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

### Version 2

Before a password is requested, AulaRaíz validates only the bounded outer structure and cryptographic profile. It does not expose inner backup metadata.

After the teacher supplies a password:

1. PBKDF2 derives the candidate key using the stored bounded parameters;
2. each encrypted chunk is read in strict sequence and authenticated with AES-GCM;
3. wrong password, ciphertext/tag tampering, truncation or inconsistent framing fails before any plaintext package is trusted;
4. the teacher-facing failure deliberately says only that the password is incorrect **or** the protected backup is damaged;
5. after every chunk authenticates, the temporary decrypted v1 payload enters the normal version-1 inspection path above.

No live storage or mandatory safety backup is touched during v1 inspection or v2 decryption/inspection.

Supported older database versions may therefore be prepared on a temporary copy without rewriting the package or the current live database.

## Restore safety boundary

Restore is destructive because it replaces current local state. A successful v2 password does not bypass any existing safety control. The workflow still requires the teacher to type exactly:

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

These application-managed safety backups remain version 1 in the current v2 change and do not reuse the selected backup password. Encrypting/retaining managed safety backups is a separate privacy decision.

These directories intentionally keep the historical technical identifier during the branding change. Renaming/migrating them belongs to future installation/update work because a cosmetic path change could make existing data appear missing.

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

### The protected backup asks for a password

That package is version 2. AulaRaíz must authenticate/decrypt it before showing normal backup metadata. If the password was forgotten, there is intentionally no recovery or escrow mechanism.

### The password is reported as incorrect or the protected backup as damaged

The same message covers both cases intentionally. A wrong password and a modified/truncated authenticated package are not distinguished. The current live storage remains unchanged.

### The package is reported as damaged

For v1, possible causes include a truncated ZIP, missing component, checksum mismatch or invalid SQLite file. For v2, invalid outer framing/parameters may also be rejected. The current live storage remains unchanged during inspection failure.

### The package is incompatible

After successful v1 inspection or v2 decryption, the logical package may come from the wrong Production/Demo mode, an unsupported backup-format version, or a future/incompatible SQLite/schema-extension version. No live replacement occurs.

### Restore says the safety backup could not be created

The restore is blocked intentionally. Check available disk space and write permissions for the application profile. Do not work around this guard by manually deleting the current database.

### Restore fails during publication

The service attempts file-level rollback and retains the previously created safety backup. If it cannot prove the old live files were restored, the error includes the safety-backup location and normal editing should not continue until recovery is resolved.

### Backup contains no application state

The database is still recoverable. On restore, stale current reopen state is removed, so the teacher may need to select the intended group again after reopening the application.

## Testing policy

Automated tests use only temporary files and fictitious data. Version-1 coverage continues to include active-WAL snapshotting, manifest/checksum round trips, hostile ZIP paths, duplicate entries, checksum tampering, Production/Demo isolation, future-version rejection, older-version preparation, mandatory safety backup, rollback behavior and full Demo backup/mutate/restore/reopen recovery.

Version-2 coverage includes protected round trip, non-personal outer metadata, v1 backward compatibility, wrong-password behavior, per-package randomization and protected restore through the existing safety-backup boundary. Additional hostile framing/tamper and large multi-chunk cases remain acceptance items until their dedicated regressions are complete.

Manual acceptance must use Demo mode before merge. It must create/open an ordinary v1 backup and a protected v2 backup, reject a wrong password, restore v2 with the correct password, reopen with `--demo` (not `--demo-reset`) and verify the original fictitious state. Never use real student data to validate a feature branch.
