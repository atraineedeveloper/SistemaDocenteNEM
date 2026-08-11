# Tasks: optional encrypted backup v2

## 1. Recovery contracts and version dispatch

- [ ] 1.1 Extend Application recovery contracts with an explicit optional protection choice without changing the default v1 request path.
- [ ] 1.2 Define an ephemeral password/secret handoff that is never serialized into summaries, app state, diagnostics or CLI surfaces.
- [ ] 1.3 Add format detection/dispatch so v1 packages continue through the existing inspector and v2 packages enter the protected-envelope path.

## 2. Version-2 envelope and cryptography

- [ ] 2.1 Implement bounded `protection.json` parsing/writing with the historical product id, format version 2 and exact supported cryptographic profile identifiers.
- [ ] 2.2 Implement one-shot PBKDF2-HMAC-SHA256 key derivation with fresh random salt, stored iteration count and a 32-byte key.
- [ ] 2.3 Select/document the initial PBKDF2 writer iteration count using a measured usability/security budget on representative supported Windows hardware; enforce sane reader lower/upper bounds.
- [ ] 2.4 Implement streaming chunked AES-256-GCM framing with fresh package nonce prefix, unique nonce per chunk, 16-byte authentication tags and authenticated header/index/length associated data.
- [ ] 2.5 Reject unsupported algorithms, malformed Base64, unreasonable KDF parameters, unsafe outer ZIP structure, inconsistent sizes/counts, missing/reordered/truncated chunks and authentication failures before trusting plaintext.
- [ ] 2.6 Clear mutable password/key buffers and delete plaintext/partial temporary artifacts when possible on all success/failure paths.

## 3. Protected backup creation

- [ ] 3.1 Reuse the current v1 backup creation implementation to produce the logical recovery payload in an application-controlled temporary location.
- [ ] 3.2 Wrap that payload as v2 only when password protection is explicitly requested.
- [ ] 3.3 Preserve destination-safe temporary-sibling publication so incomplete protected output is never reported as a successful backup.
- [ ] 3.4 Keep ordinary unprotected manual backup output byte/semantic compatible with the existing v1 format.

## 4. Protected inspection and restore preparation

- [ ] 4.1 Detect supported v2 outer structure without exposing inner classroom metadata.
- [ ] 4.2 Request/accept the password only for v2 packages and stream authenticated plaintext into an application-chosen temporary v1 package.
- [ ] 4.3 Return one non-oracular operational error class for wrong password or protected-package authentication failure.
- [ ] 4.4 After full successful decryption, delegate to the existing v1 inspection/preparation flow and preserve Demo/Production, checksums, SQLite integrity, schema compatibility and application-state validation.
- [ ] 4.5 Preserve the existing typed `RESTAURAR`, safety-backup, staged rollback/publication and post-restore shutdown boundary unchanged.

## 5. Presentation and WPF experience

- [ ] 5.1 Add an optional `Proteger con contraseña` control to manual backup creation; leave it off by default.
- [ ] 5.2 Add password and confirmation inputs shown only when protection is enabled.
- [ ] 5.3 Require at least 12 characters, allow spaces, avoid composition rules and show a clear passphrase-oriented explanation.
- [ ] 5.4 Show a pre-creation warning that AulaRaíz does not store/recover forgotten backup passwords.
- [ ] 5.5 For v2 restore, request password before showing inner backup metadata; after successful inspection, return to the existing restore confirmation experience.
- [ ] 5.6 Ensure password fields do not persist in portable ViewModel/app-state models and are cleared after submit/cancel where practical.
- [ ] 5.7 Validate keyboard access, semantic labels, Light/Dark/High Contrast resources and 100/125/150% scaling for the new controls.

## 6. Automated regression coverage

- [ ] 6.1 Prove ordinary unprotected creation still produces a valid v1 package and old v1 backups remain restorable.
- [ ] 6.2 Add deterministic v2 protected round-trip tests with representative fictitious Demo data and Unicode/passphrase input.
- [ ] 6.3 Prove same password on two backups yields different salts/nonce prefixes/ciphertext.
- [ ] 6.4 Test wrong password plus header/ciphertext/tag tampering and missing/duplicate/reordered/truncated chunks.
- [ ] 6.5 Test malicious/out-of-range KDF/header parameters are rejected before unreasonable allocation/work.
- [ ] 6.6 Test a large multi-chunk payload round trip without full-package buffering assumptions.
- [ ] 6.7 Prove inner v1 corruption, wrong mode and incompatible schema still fail after correct v2 decryption.
- [ ] 6.8 Inject secret/password sentinels and prove they are absent from outer metadata, application state and safe diagnostics.
- [ ] 6.9 Cover WPF protection-option visibility, mismatch/short-password gating, cancel behavior and protected-restore password step.

## 7. Documentation and roadmap

- [ ] 7.1 Update `docs/backup-restore.md` with v1/v2 compatibility, optional protection, password-loss semantics and protected restore flow.
- [ ] 7.2 Update README privacy/recovery language without implying unprotected v1 files are encrypted.
- [ ] 7.3 Update the maintained roadmap to mark optional encrypted backup v2 implemented only after validation/merge.
- [ ] 7.4 Document the chosen PBKDF2 writer parameters and the rationale/bounds used by format v2.

## 8. Validation and acceptance

- [ ] 8.1 Run `dotnet restore SistemaDocente.sln`.
- [ ] 8.2 Run `dotnet format SistemaDocente.sln --verify-no-changes --no-restore`.
- [ ] 8.3 Run `dotnet build SistemaDocente.sln --configuration Release --no-restore`.
- [ ] 8.4 Run the full test suite in Release configuration.
- [ ] 8.5 Run `openspec validate --all`.
- [ ] 8.6 Run `git diff --check`.
- [ ] 8.7 Manually create/open an unprotected v1 Demo backup to prove no regression.
- [ ] 8.8 Manually create a password-protected v2 Demo backup, reject a wrong password, restore with the correct password, close/reopen with `--demo` (not `--demo-reset`) and verify the original fictitious state is recovered.
- [ ] 8.9 Recheck the protected workflow in Light, Dark and High Contrast and at 100/125/150% Windows scaling.
