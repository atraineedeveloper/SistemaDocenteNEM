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

The build script expects Inno Setup 7 `ISCC.exe`, reads the product version from `Directory.Build.props`, publishes the WPF application self-contained and compiles the installer.

Example:

```powershell
.\scripts\build-installer.ps1 `
  -IsccPath "C:\Program Files\Inno Setup 7\ISCC.exe"
```

Expected output:

```text
artifacts\installer\AulaRaiz-Setup-0.1.0-win-x64.exe
```

## CI supply-chain check

The installer workflow pins Inno Setup 7.0.2. CI downloads the official immutable GitHub release asset and verifies its GitHub release attestation before running the compiler.

After building AulaRaíz, CI performs a lifecycle smoke test:

1. silent current-user install;
2. verify installed executable and product version;
3. create a sentinel under the historical Production data directory;
4. run the same installer again as an upgrade/reinstall smoke test;
5. verify both program and sentinel remain;
6. silent uninstall;
7. verify program files are removed;
8. verify the user-data sentinel survives;
9. upload the development installer as a workflow artifact.

This test protects the most important installer contract: installation lifecycle operations must not accidentally take ownership of classroom data.

## Code signing and production distribution

The CI artifact is a **development installer** unless a trusted code-signing process has been applied.

Before broad real-world distribution, AulaRaíz should use Authenticode signing so Windows can identify the publisher and verify installer integrity. Signing certificates/private keys must never be committed to Git, stored in ordinary repository files or embedded in the installer script.

A production signing workflow should use an appropriate protected signing service or CI secret integration and should sign the deliverables after deterministic build steps. The public repository may continue to build unsigned artifacts for functional validation.

## Manual clean-machine acceptance

Before the installer is considered production-ready, validate on a clean/non-development Windows user or VM:

1. confirm no .NET SDK is required;
2. install without administrator credentials using the default path;
3. launch AulaRaíz and confirm `v0.1.0` is visible;
4. run Demo mode and create/reopen fictitious data;
5. install a newer test build over the existing installation;
6. confirm the same data reopens;
7. uninstall AulaRaíz;
8. confirm program shortcuts/files are removed;
9. reinstall AulaRaíz and confirm the preserved local data can still be opened.
