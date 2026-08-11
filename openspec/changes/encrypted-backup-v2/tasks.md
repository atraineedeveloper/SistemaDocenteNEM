# Tasks: optional encrypted backup v2

## 1. Recovery contracts and version dispatch

- [x] 1.1 Extend Application recovery contracts with an explicit optional protection choice without changing the default v1 request path.
- [x] 1.2 Define an ephemeral password/secret handoff that is never serialized into summaries, app state, diagnostics or CLI surfaces.
- [x] 1.3 Add format detection/dispatch so v1 packages continue through the existing inspector and v2 packages enter the protected-envelope path.

## 2. Version-2 envelope and cryptography

- [x] 2.1 Implement bounded `protection.json` parsing/writing with the historical product id, format version 2 and exact supported cryptographic profile identifiers.
- [x] 2.2 Implement one-shot PBKDF2-HMAC-SHA256 key derivation with fresh random salt, stored iteration count and a 32-byte key.
- [~] 2.3 Use/document an initial 600,000-iteration PBKDF2 writer profile and enforce reader bounds; representative end-user Windows performance measurement remains part of distribution hardening before broad adoption.
- [x] 2.4 Implement streaming chunked AES-256-GCM framing with fresh package nonce prefix, unique nonce per chunk, 16-byte authentication tags and authenticated header/index/length associated data.
- [x] 2.5 Reject unsupported algorithms, malformed/bounded framing, unreasonable KDF parameters, unsafe outer ZIP structure, inconsistent sizes/counts, truncated content and authentication failures before trusting plaintext.
- [x] 2.6 Clear mutable password/key buffers and delete plaintext/partial temporary artifacts when possible on all success/failure paths.

## 3. Protected backup creation

- [x] 3.1 Reuse the current v1 backup creation implementation to produce the logical recovery payload in an application-controlled temporary location.
- [x] 3.2 Wrap that payload as v2 only when password protection is explicitly requested.
- [x] 3.3 Preserve destination-safe temporary-sibling publication so incomplete protected output is never reported as a successful backup.
- [x] 3.4 Keep ordinary unprotected manual backup output semantically compatible with the existing v1 format.

## 4. Protected inspection and restore preparation

- [x] 4.1 Detect supported v2 outer structure without exposing inner classroom metadata.
- [x] 4.2 Request/accept the password only for v2 packages and stream authenticated plaintext into an application-chosen temporary v1 package.
- [x] 4.3 Return one non-oracular operational error class for wrong password or protected-package authentication failure.
- [x] 4.4 After full successful decryption, delegate to the existing v1 inspection/preparation flow and preserve Demo/Production, checksums, SQLite integrity, schema compatibility and application-state validation.
- [x] 4.5 Preserve the existing typed `RESTAURAR`, safety-backup, staged rollback/publication and post-restore shutdown boundary unchanged.

## 5. Presentation and WPF experience

- [x] 5.1 Add an optional `Proteger con contraseña` control to manual backup creation; leave it off by default.
- [x] 5.2 Add password and confirmation inputs shown only when protection is enabled.
- [x] 5.3 Require at least 12 characters, allow spaces, avoid composition rules and show a clear passphrase-oriented explanation.
- [x] 5.4 Show a pre-creation warning that AulaRaíz does not store/recover forgotten backup passwords.
- [x] 5.5 For v2 restore, request password before showing inner backup metadata; after successful inspection, return to the existing restore confirmation experience.
- [x] 5.6 Ensure password fields do not persist in portable ViewModel/app-state models and are cleared after submit/cancel where practical.
- [~] 5.7 Functional password/restore UX was manually accepted; Light/Dark/High Contrast and 100/125/150% rechecks remain part of continuous UI quality.

## 6. Automated regression coverage

- [x] 6.1 Prove ordinary unprotected creation still produces a valid v1 package and old v1 backups remain restorable.
- [x] 6.2 Add v2 protected round-trip/restore tests with fictitious data and a Unicode passphrase.
- [x] 6.3 Prove same password on two backups yields different salts/nonce prefixes/ciphertext.
- [~] 6.4 Wrong-password authentication failure is covered; dedicated hostile header/ciphertext/tag/duplicate/reordered/truncated fixture expansion remains desirable hardening.
- [~] 6.5 Reader bounds are implemented; dedicated malicious-parameter regression fixtures remain desirable hardening.
- [ ] 6.6 Add a dedicated large multi-chunk payload regression that proves streaming behavior across several chunks.
- [~] 6.7 Protected restore demonstrably delegates through v1 validation/safety behavior; dedicated inner wrong-mode/corrupt/future-schema v2 fixtures remain desirable hardening.
- [~] 6.8 Outer metadata excludes representative classroom/password sentinels and secrets are absent from ViewModel binding; dedicated safe-diagnostics secret-sentinel coverage remains desirable hardening.
- [~] 6.9 WPF structural coverage plus manual functional acceptance cover the primary password workflow; broader interactive edge-case/theme/scaling checks remain continuous quality work.

## 7. Documentation and roadmap

- [x] 7.1 Update `docs/backup-restore.md` with v1/v2 compatibility, optional protection, password-loss semantics and protected restore flow.
- [x] 7.2 Update README privacy/recovery language without implying unprotected v1 files are encrypted.
- [x] 7.3 Update the maintained roadmap to record optional encrypted backup v2 as accepted for PR #39.
- [x] 7.4 Document the current PBKDF2 writer parameters, reader bounds and the need to re-benchmark before broad distribution.

## 8. Validation and acceptance

- [x] 8.1 Run `dotnet restore SistemaDocente.sln` with the repository NuGet audit gate.
- [x] 8.2 Run `dotnet format SistemaDocente.sln --verify-no-changes --no-restore`.
- [x] 8.3 Run `dotnet build SistemaDocente.sln --configuration Release --no-restore` with zero warnings/errors.
- [x] 8.4 Run the full test suite in Release configuration with coverage collection.
- [x] 8.5 Run `openspec validate --all`.
- [x] 8.6 Run `git diff --check`.
- [x] 8.7 Manually create/open an unprotected v1 Demo backup to prove no regression.
- [x] 8.8 Manually exercise the password-protected v2 workflow and accept correct-password creation/inspection/restore behavior after automated wrong-password coverage.
- [~] 8.9 Theme/high-contrast/scaling rechecks remain part of continuous UI quality rather than a backup-v2-specific merge blocker.

## Automated validation record

- Windows CI run #368 on commit `a7dd617`: restore/NuGet audit, format, Release build, full tests with coverage, OpenSpec and whitespace all passed.
- Installer run #98 on commit `a7dd617`: packaging/lifecycle validation passed.
- Manual functional acceptance of the optional password workflow was confirmed on 2026-08-11 using a local development environment.
- Documentation/acceptance commits require one final green CI/Installer pass before squash merge.
