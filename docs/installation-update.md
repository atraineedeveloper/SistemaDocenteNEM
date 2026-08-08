# AulaRaíz Windows installation and update

AulaRaíz is distributed as a conventional Windows installer built from a self-contained .NET 10 `win-x64` publish output. The target teacher PC does not need the .NET SDK or a separately installed .NET 10 Desktop Runtime.

## Supported target

The version-1 installer targets x64-compatible Windows systems supported by the application's .NET 10 runtime. The primary supported desktop target is Windows 11 x64. An x64 application may also run under Windows 11 Arm64 x64 emulation, but native Arm64 packaging is not part of version 1.

The installer is built as a 64-bit Inno Setup 7 installer and uses the `x64compatible` architecture matcher.

## Default installation

The normal installation is per-user and does not request administrator privileges.

Default program location:

```text
%LOCALAPPDATA%\Programs\AulaRaiz
```

The installer creates:

- an AulaRaíz Start Menu shortcut;
- one normal Add/Remove Programs entry;
- an optional desktop shortcut only when the user explicitly selects it.

The installed application displays its semantic version next to the AulaRaíz product identity. Version 1 starts at `0.1.0`.

## Runtime strategy

AulaRaíz is published self-contained for `win-x64`. The installer packages the application runtime together with the app, including the .NET runtime and WPF runtime assets required by the published build.

Version 1 intentionally does **not** use trimming. It also keeps a normal publish directory rather than forcing all assets into one single executable. This favors reliability and inspectability for WPF, SQLite native assets and PDF/report dependencies over a smaller package.

## Classroom-data location is not the install location

Program files and teacher data have different ownership boundaries.

Installer-owned program files:

```text
%LOCALAPPDATA%\Programs\AulaRaiz\...
```

Historical application data remains:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\...
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\...
```

The following compatibility contracts remain unchanged:

- SQLite file name: `sistema-docente.db`;
- backup package identifier: `SistemaDocenteNEM.Backup`;
- Production/Demo data directories shown above.

Do not rename or move those identifiers as part of installer work. They are compatibility boundaries for existing local data and backup/restore.

## Updating AulaRaíz

Version 1 uses an explicit installer-based update model:

1. obtain the newer trusted AulaRaíz installer;
2. close AulaRaíz if it is running, or allow Setup to request closure;
3. run the newer installer;
4. the stable installer AppId identifies it as the same product;
5. Setup reuses the existing install directory and replaces application files;
6. classroom data remains untouched;
7. launch the updated application.

Automatic background update checks, installer downloads and forced updates are intentionally not implemented in version 1.

## SQLite during application updates

The installer never opens or edits SQLite. Database schema ownership remains in `SistemaDocente.Data`.

When an updated application opens a supported older database, the existing application initialization/migration paths prepare the schema. Current schema extensions are additive/idempotent. A database from an unsupported future version is rejected through the existing schema-incompatibility path rather than reset or overwritten.

A future destructive migration must define its own pre-migration safety backup and rollback behavior before it can be implemented. The installer must not become a second source of database migration rules.

## Uninstall behavior

Ordinary uninstall removes installer-owned application files, shortcuts and uninstall registration. It intentionally does **not** remove:

- Production classroom data;
- Demo data;
- `.sdocbackup` recovery packages;
- teacher-created XLSX/CSV/PDF exports stored elsewhere;
- any other file outside the installer-owned application directory.

This behavior is deliberate. Uninstalling an application should not silently destroy student/classroom records. A future explicit data-deletion/anonymization workflow belongs to the privacy/security module.

## Development installer build

The repository contains:

```text
installer/AulaRaiz.iss
scripts/build-installer.ps1
```

The build script expects Inno Setup 7 `ISCC.exe`, reads the product version from `Directory.Build.props`, publishes the WPF application self-contained and compiles the installer. `VersionOverride` exists only to create explicit test fixtures such as the older installer used by upgrade CI; normal builds use the repository version.

Example:

```powershell
.\scripts\build-installer.ps1 `
  -IsccPath "C:\Program Files\Inno Setup 7\ISCC.exe"
```

Expected output:

```text
artifacts\installer\AulaRaiz-Setup-0.1.0-win-x64.exe
```

## CI supply-chain and lifecycle checks

The installer workflow pins Inno Setup 7.0.2. CI downloads the official immutable GitHub release asset and verifies its GitHub release attestation before running the compiler.

CI then proves a real version transition rather than merely reinstalling the same version:

1. build an ephemeral `0.0.9` upgrade fixture from the current source with overridden assembly/installer metadata;
2. build the repository's real `0.1.0` installer;
3. silently install `0.0.9` for the current user;
4. verify the executable and Add/Remove Programs entry report `0.0.9`;
5. create a sentinel under the historical Production data directory;
6. run the `0.1.0` installer with the same stable AppId;
7. verify executable and uninstall metadata now report `0.1.0`;
8. verify exactly one normal AulaRaíz uninstall entry remains;
9. verify the user-data sentinel survived the version upgrade;
10. silently uninstall AulaRaíz;
11. verify program files are removed and the sentinel still survives;
12. upload the current development installer and a paired `0.0.9 → 0.1.0` manual-validation artifact.

The `0.0.9` package is a **test fixture only**, not a historical release and not a distribution candidate. Its purpose is to exercise Inno Setup's real version-upgrade path with the same product identity.

This lifecycle test protects the most important installer contract: installation and update operations must not accidentally take ownership of classroom data.

## Code signing and production distribution

The CI artifact is a **development installer** unless a trusted code-signing process has been applied.

Before broad real-world distribution, AulaRaíz should use Authenticode signing so Windows can identify the publisher and verify installer integrity. Signing certificates/private keys must never be committed to Git, stored in ordinary repository files or embedded in the installer script.

A production signing workflow should use an appropriate protected signing service or CI secret integration and should sign the deliverables after deterministic build steps. The public repository may continue to build unsigned artifacts for functional validation.

## Manual clean-machine acceptance

Before the installer is considered production-ready, validate on a clean/non-development Windows user or VM. The CI artifact `AulaRaiz-upgrade-validation` contains both installers required for an explicit manual update path.

1. confirm no .NET SDK is required;
2. install the `0.0.9` **test fixture** without administrator credentials using the default path;
3. launch AulaRaíz and confirm `v0.0.9` is visible;
4. run Demo mode and create/reopen fictitious data;
5. install `0.1.0` over the existing installation;
6. confirm `v0.1.0` is visible and the same Demo data reopens;
7. confirm Windows shows one AulaRaíz installation at version `0.1.0`;
8. uninstall AulaRaíz;
9. confirm program shortcuts/files are removed;
10. reinstall `0.1.0` and confirm the preserved local Demo data can still be opened.
