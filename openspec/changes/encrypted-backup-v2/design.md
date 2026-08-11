# Design: optional encrypted backup v2

## Goals

- Add portable confidentiality to manual backups without changing the default version-1 recovery path.
- Reuse the already-tested version-1 package semantics after decryption.
- Use authenticated encryption so password/ciphertext/header tampering is detected before restore preparation reaches live storage.
- Process protected backups in bounded chunks rather than buffering a complete package in memory.
- Keep passwords and derived keys ephemeral and out of persistence, logs and diagnostics.
- Preserve cross-machine restore; protection must depend only on the teacher password and package metadata, not on one Windows account or device.

## Non-goals

- Making encryption mandatory.
- Replacing the existing SQLite snapshot, manifest, checksum, compatibility or rollback design.
- Password recovery or escrow.
- Device-bound DPAPI protection.
- Encrypting managed safety backups in this change.
- Cloud backup or automatic scheduling.
- Digital signatures/authenticity.

## Compatibility strategy

The existing version-1 package remains the default and is not rewritten.

When the teacher selects password protection, Data first produces the same logical version-1 recovery payload used today, then wraps those bytes in a version-2 encrypted envelope. Restore performs the inverse: authenticate/decrypt the v2 envelope into an application-chosen temporary v1 payload and hand that payload to the existing version-1 inspection/preparation path.

This deliberately creates only one recovery truth for SQLite/application-state semantics. Version 2 owns confidentiality and authentication; version 1 continues to own manifest validation, checksums, source-mode isolation, SQLite checks, schema preparation and destructive restore safety.

## Architecture

### Application

Application extends the recovery use case with an explicit protection choice, for example:

- unprotected/default;
- password protected.

The password is an ephemeral secret supplied only to the protected create/decrypt operation. Application contracts must not add the password to ordinary result/summary models, app state, diagnostics or serialization.

### Data

Data remains the recovery implementation because it already owns the backup package and local temporary-file workflow. It adds a small version-2 envelope codec around the existing v1 package implementation.

The codec uses only `System.Security.Cryptography` primitives available in .NET 10:

- `RandomNumberGenerator` for salt and nonce-prefix generation;
- one-shot `Rfc2898DeriveBytes.Pbkdf2` with HMAC-SHA256 for password key derivation;
- `AesGcm` with a 256-bit key and 128-bit authentication tags.

The writer stores the PBKDF2 iteration count in the envelope. The initial writer count is selected and documented during implementation using a measured usability/security budget on supported Windows hardware rather than an arbitrary hidden constant. Readers enforce sane lower/upper parameter bounds before running expensive KDF work, and future writers may increase the count without changing format version 2.

### Presentation/WPF

The recovery UI adds a clearly optional protection control during manual backup creation.

When protection is selected:

- the teacher enters a password and confirmation;
- passwords shorter than 12 characters are rejected;
- spaces are allowed and no character-class composition rules are imposed;
- the UI explains that AulaRaíz cannot recover a forgotten password;
- the password is not retained after the operation completes/cancels.

During restore, a v2 package is identified from its bounded outer header before any payload metadata is available. WPF requests the password, then the normal inspection metadata is shown only after successful authenticated decryption and v1 inspection.

## Version-2 physical envelope

File extension remains `.sdocbackup`.

The outer container is a ZIP archive with exactly two expected entries:

```text
protection.json
payload.bin
```

Unexpected or duplicate outer entries and unsafe paths are rejected before decryption.

### `protection.json`

The outer header contains no classroom data. It contains only fields needed to identify and decrypt the protected payload, for example:

```json
{
  "format": "SistemaDocenteNEM.Backup",
  "formatVersion": 2,
  "protection": {
    "mode": "Password",
    "kdf": "PBKDF2-HMAC-SHA256",
    "iterations": 600000,
    "salt": "<base64>",
    "cipher": "AES-256-GCM-CHUNKED",
    "chunkSizeBytes": 1048576,
    "noncePrefix": "<base64>",
    "plaintextSizeBytes": 1234567,
    "chunkCount": 2
  }
}
```

The numeric values above illustrate the schema; the implementation-selected PBKDF2 writer count is documented/tested rather than inferred from this example.

The exact UTF-8 bytes of `protection.json` are included as authenticated associated data for every encrypted chunk. Any cryptographic-header modification therefore causes authentication failure.

The header size is bounded (target maximum 16 KiB), strings/enums must match the supported v2 profile exactly, Base64 fields have exact decoded lengths, numeric parameters are range-checked before allocation/KDF work, and declared payload sizes/counts are cross-checked against the actual stream.

## Password-to-key derivation

For every protected backup:

1. generate a fresh cryptographically random salt;
2. encode the password deterministically as UTF-8 after Unicode NFC normalization;
3. derive exactly 32 bytes with one-shot PBKDF2-HMAC-SHA256;
4. use the derived bytes only as the AES-256-GCM key for that package;
5. clear mutable password/key buffers as soon as practical after use.

The salt and iteration count are public parameters and are stored in the outer header. The password and derived key are never stored.

## Chunked authenticated encryption

`payload.bin` stores the encrypted bytes of the complete logical v1 package in fixed-size chunks (writer target: 1 MiB plaintext per chunk, with a smaller final chunk).

A fresh random 32-bit nonce prefix is generated per package. Each AES-GCM nonce is 96 bits:

```text
nonce = noncePrefix[4 bytes] || chunkIndex[8 bytes, big endian]
```

Because every chunk index is unique within one package and the prefix is freshly random for each newly derived package key, the writer never intentionally reuses a nonce with the same key.

Each chunk record contains:

```text
plaintextLength (bounded integer)
ciphertext[plaintextLength]
tag[16 bytes]
```

Associated data binds:

- the exact `protection.json` bytes;
- the zero-based chunk index;
- the declared plaintext length.

The reader requires chunks in strict sequential order and requires the number/combined plaintext length to match the authenticated header declarations. Missing, reordered, duplicated, truncated or modified chunks fail authentication/structure validation.

## Protected backup creation flow

1. Teacher chooses the normal backup destination.
2. If protection is off, execute the existing version-1 flow unchanged.
3. If protection is on, WPF validates password/confirmation and displays the non-recoverability warning.
4. Data creates the ordinary v1 logical package in an application-controlled temporary location using the current consistent snapshot and package validation path.
5. Data generates the v2 cryptographic header and derives the key.
6. Data streams the temporary v1 package through chunked AES-256-GCM into an outer temporary sibling `.sdocbackup`.
7. The outer ZIP is closed; cryptographic/header structure is validated as practical before publication.
8. Only the completed protected package replaces/moves to the requested destination.
9. Plaintext temporary package, mutable password material, derived key and partial ciphertext are removed/cleared when possible on success or failure.

A crash can prevent best-effort cleanup of temporary plaintext, so v2 is confidentiality protection for the portable backup artifact, not full-disk/temp-directory encryption.

## Protected backup inspection flow

1. Open candidate without touching live storage.
2. Detect exact supported outer v2 structure and parse bounded `protection.json`.
3. Report only that the package is password protected; do not expose inner backup metadata yet.
4. Request password.
5. Derive the candidate key from the stored bounded KDF parameters.
6. Stream-decrypt/authenticate all chunks into an application-chosen temporary v1 package.
7. If any authentication/structure check fails, delete temporary output and report the generic `contraseña incorrecta o respaldo protegido dañado` class of error.
8. Only after full authenticated decryption succeeds, run the existing v1 inspection/preparation workflow on the decrypted package.
9. Display the existing backup metadata and allow the existing typed `RESTAURAR` destructive confirmation.

No live storage or safety backup is touched during steps 1-8.

## Failure and trust boundaries

### Wrong password or modified protected file

The system does not try to distinguish these cases for the teacher. Both fail before the v1 payload is trusted and before live storage changes.

### Malicious KDF/header parameters

Outer metadata is untrusted. The reader validates algorithm identifiers, exact salt/nonce lengths, header size, iteration range, chunk size/count and total size before allocating buffers or performing expensive work.

### Temporary plaintext

Decrypted/v1 temporary files are sensitive. They stay only in application-chosen temporary working directories, are never published as successful destinations and are deleted on completion/failure when possible. Diagnostics never record their contents or the password/key.

### Safety backups

The existing mandatory pre-restore safety backup remains version 1 and application-managed in this change. This does not weaken the current restore boundary, but encrypting/retaining managed safety backups is a separate privacy change.

## Testing strategy

Automated coverage includes:

- unprotected creation still produces/restores the current v1 package;
- v1 backups remain readable after v2 support is added;
- protected v2 round trip with Unicode password and representative Demo data;
- different backups from the same password use different salt/nonce prefixes and ciphertext;
- wrong password rejection;
- tampered header/ciphertext/tag rejection;
- missing/duplicate/reordered/truncated chunk rejection;
- bounded malicious KDF/header parameters rejected before expensive work/allocation;
- large multi-chunk streaming round trip;
- inner v1 checksum/mode/schema validation still runs after successful decryption;
- Demo/Production mismatch still blocks restore;
- password/derived-key sentinel absent from package metadata, app state and safe diagnostics;
- cancellation/mismatch/short-password UI behavior;
- Light/Dark/High Contrast construction and keyboard access for the protection controls.

Manual validation uses fictitious Demo data to create both v1 and password-protected v2 backups, open/restart the app, verify wrong-password behavior, restore v2 with the correct password, and confirm original Demo state is recovered.
