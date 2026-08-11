## ADDED Requirements

### Requirement: Password protection is optional and does not replace version-1 backup by default
The system SHALL keep the existing unprotected version-1 manual backup behavior as the default.

A version-2 `.sdocbackup` SHALL be created only when the teacher explicitly selects password protection.

#### Scenario: Teacher leaves protection disabled
- **WHEN** the teacher creates a manual backup without selecting password protection
- **THEN** AulaRaíz creates the existing version-1 unencrypted package and the current restore behavior remains applicable

#### Scenario: Teacher selects password protection
- **WHEN** the teacher enables password protection and completes valid password confirmation
- **THEN** AulaRaíz creates a version-2 protected `.sdocbackup`

### Requirement: Protected version 2 encapsulates the existing recovery payload
A protected version-2 package SHALL encrypt/authenticate the complete logical version-1 recovery payload rather than define a second SQLite/application-state recovery model.

After successful authenticated decryption, the system SHALL run the existing version-1 manifest, checksum, mode, SQLite-integrity, schema-compatibility and restore-preparation validation against the decrypted payload.

#### Scenario: Protected backup decrypts successfully
- **WHEN** every version-2 chunk authenticates with the supplied password
- **THEN** the decrypted version-1 payload is inspected through the existing recovery validation path before destructive confirmation is available

#### Scenario: Inner payload is incompatible
- **WHEN** version-2 authentication succeeds but the inner version-1 database is wrong-mode, corrupt or incompatible
- **THEN** restore remains blocked by the existing version-1 validation rules

### Requirement: Version-2 outer metadata exposes no classroom contents
The unencrypted version-2 outer header SHALL contain only the stable product/format identifier and bounded cryptographic/framing parameters required to decrypt the payload.

Student, group, school, pedagogical, source-mode, backup-creation and inner manifest metadata SHALL remain inside the encrypted payload.

#### Scenario: Protected file is examined without the password
- **WHEN** a person reads `protection.json` from a valid version-2 package
- **THEN** the header identifies the backup/protection profile but does not reveal classroom records or inner backup metadata

### Requirement: Password-derived encryption uses authenticated .NET cryptographic primitives
The version-2 writer SHALL derive a 256-bit key from the password using one-shot PBKDF2-HMAC-SHA256 with a fresh random salt and a stored iteration count.

The payload SHALL be encrypted with AES-256-GCM using a fresh package nonce prefix and a unique 96-bit nonce per chunk. Authentication tags SHALL be 128 bits.

The exact header bytes and per-chunk framing values SHALL be authenticated associated data so cryptographic-header, chunk-index and chunk-length changes are detected.

#### Scenario: Two protected backups use the same password
- **WHEN** two protected backups are independently created with the same password
- **THEN** they use different random salts/nonce prefixes and do not intentionally reuse the same AES-GCM nonce/key combination

#### Scenario: Header is modified
- **WHEN** an attacker changes an authenticated cryptographic header field after creation
- **THEN** protected-backup decryption fails before the inner payload is trusted

### Requirement: Protected payload processing is bounded and streaming
The version-2 reader/writer SHALL process the encrypted payload in bounded chunks and SHALL NOT require the complete backup to be held in memory.

The reader SHALL bound header size, algorithm identifiers, decoded salt/nonce lengths, KDF work parameters, chunk size/count and declared payload size before expensive work or large allocation.

#### Scenario: Large backup spans several chunks
- **WHEN** a protected backup exceeds one encryption chunk
- **THEN** creation and restore process it sequentially with independently authenticated chunks and reproduce the exact inner payload

#### Scenario: Malicious header requests unreasonable KDF work
- **WHEN** an untrusted version-2 header contains unsupported or out-of-range KDF/framing parameters
- **THEN** the package is rejected before unbounded allocation or cryptographic work is attempted

### Requirement: Wrong password and protected-package tampering are non-destructive
A wrong password, modified ciphertext, modified authentication tag, missing/reordered/duplicate/truncated chunk or inconsistent declared length SHALL cause protected-package inspection to fail before any live storage change.

The teacher-facing error SHALL NOT claim whether the failure was definitely a wrong password or file tampering.

#### Scenario: Teacher enters wrong password
- **WHEN** the password cannot authenticate the protected payload
- **THEN** inspection reports that the password is incorrect or the protected backup is damaged and live storage remains unchanged

#### Scenario: Ciphertext is modified
- **WHEN** any encrypted chunk no longer verifies its authentication tag
- **THEN** the temporary decrypted output is rejected/removed when possible and live storage remains unchanged

### Requirement: Backup passwords and derived keys are ephemeral secrets
The backup password and derived encryption key SHALL NOT be stored in `.sdocbackup` metadata, SQLite, application state, diagnostics, CLI arguments, logs or configuration.

Mutable secret buffers SHALL be cleared as soon as practical after the create/decrypt operation completes or fails.

#### Scenario: Protected backup operation fails
- **WHEN** protected backup creation or decryption fails after receiving a password
- **THEN** diagnostics describe only the operational failure and do not serialize the password, derived key or decrypted classroom contents

### Requirement: Protected-backup creation requires clear password confirmation and recovery warning
The protected-backup UI SHALL require the teacher to enter and confirm a password of at least 12 characters. Spaces SHALL be permitted and no character-class composition rule SHALL be required.

The UI SHALL state before creation that AulaRaíz does not store the password and cannot recover the protected backup if the password is forgotten.

#### Scenario: Password confirmation differs
- **WHEN** the entered password and confirmation do not match
- **THEN** protected backup creation remains unavailable

#### Scenario: Password is too short
- **WHEN** the teacher enters fewer than 12 characters
- **THEN** the UI explains the minimum and protected backup creation remains unavailable

#### Scenario: Teacher cancels password entry
- **WHEN** the teacher cancels before confirming the protected backup
- **THEN** no protected destination is reported as created and live classroom data is unchanged

### Requirement: Protected restore asks for password before inner metadata is shown
The system SHALL identify a supported version-2 package from bounded outer structure without decrypting classroom metadata.

The teacher SHALL provide the password before the system displays the normal backup date, source mode, application/database version or included-component metadata.

#### Scenario: Teacher selects protected backup
- **WHEN** the candidate is recognized as supported version 2
- **THEN** the UI first identifies it as password protected and requests the password

#### Scenario: Password succeeds
- **WHEN** protected payload authentication/decryption and inner version-1 inspection succeed
- **THEN** the normal recovery metadata and typed `RESTAURAR` destructive confirmation become available

### Requirement: Version-1 restore remains backward compatible
Adding version-2 support SHALL NOT remove or weaken the ability to inspect and restore existing supported version-1 backups.

#### Scenario: Existing version-1 backup is selected
- **WHEN** a valid pre-version-2 `.sdocbackup` is selected
- **THEN** the existing version-1 inspection and restore flow runs without requesting a password

### Requirement: Protected backups remain portable across Windows computers
Version-2 password protection SHALL NOT depend on a machine-local or Windows-account-local secret required for decryption.

#### Scenario: Teacher moves protected backup to another supported computer
- **WHEN** the same supported AulaRaíz version is installed on another Windows computer and the teacher supplies the correct password
- **THEN** the backup can be decrypted and evaluated for normal mode/schema compatibility without access to the original computer's DPAPI profile

### Requirement: Existing restore safety boundary remains mandatory after decryption
Successful version-2 decryption SHALL NOT itself authorize live replacement.

The existing pending-change handling, typed `RESTAURAR` confirmation, mandatory safety backup, staged publication/rollback and post-restore shutdown requirements SHALL remain in force.

#### Scenario: Correct password is supplied
- **WHEN** the protected package decrypts and validates successfully
- **THEN** no live file is replaced until all existing destructive-restore protections are satisfied

### Requirement: Managed safety backups are not silently changed by version 2
This change SHALL NOT make the existing automatic pre-restore safety backup password protected or dependent on the selected backup password.

#### Scenario: Version-2 restore reaches destructive boundary
- **WHEN** the existing service creates its mandatory safety backup before live replacement
- **THEN** it uses the current application-managed safety-backup behavior and failure semantics
