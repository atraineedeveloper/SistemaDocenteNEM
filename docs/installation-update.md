# AulaRaíz Windows installation and update

AulaRaíz is distributed as a conventional Windows installer built from a self-contained .NET 10 `win-x64` publish output. The target teacher PC does not need the .NET SDK or a separately installed .NET 10 Desktop Runtime.

## Supported target

The installer targets x64-compatible Windows systems supported by the application's .NET 10 runtime. The primary supported desktop target is Windows 11 x64. An x64 application may also run under Windows 11 Arm64 x64 emulation, but native Arm64 packaging is not currently part of the product.

The installer is built as a 64-bit Inno Setup 7 package and uses the `x64compatible` architecture matcher.

## Default installation

The normal installation is per-user and does not request administrator privileges.

Default program location:

```text
%LOCALAPPDATA%\Programs\AulaRaiz
```

The installer creates a Start Menu shortcut, one normal Add/Remove Programs entry and an optional desktop shortcut only when selected by the user.

AulaRaíz displays its semantic version next to the product identity. The `0.2.5` line adds the consent-based in-app updater.

## Runtime strategy

The WPF application is self-contained for `win-x64`. The main WPF publish remains a normal directory rather than a single-file executable because this favors reliability for WPF, SQLite native assets and PDF dependencies.

The installer additionally includes two self-contained single-file utilities beside WPF:

- `aularaiz.exe` — local CLI/agent interface;
- `AulaRaiz.Updater.exe` — technical update coordinator used only after explicit user consent.

## Classroom-data location is not the install location

Installer-owned program files:

```text
%LOCALAPPDATA%\Programs\AulaRaiz\...
```

Historical application data remains:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\...
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\...
```

The SQLite file name, backup package identifier and Production/Demo data directories remain compatibility contracts. The installer and updater do not rename, delete or directly modify them.

## In-app update experience

AulaRaíz `0.2.5` uses GitHub Releases as its update source while preserving explicit teacher control.

After normal startup becomes usable, AulaRaíz performs a non-blocking Preview-channel check. The same check can be requested manually through **Actualizar** in the main header. If the network is unavailable, normal classroom work continues without interruption.

When a newer eligible Release is found, AulaRaíz shows the installed/new versions and Release notes. The teacher can choose:

- **Más tarde** — continue using the current version; nothing is installed;
- **Descargar e instalar** — download the update inside AulaRaíz.

The download does not immediately close the application. AulaRaíz first downloads `SHA256SUMS.txt` and the exact versioned installer to:

```text
%LOCALAPPDATA%\AulaRaiz\Updates\<version>\
```

The installer is streamed to a temporary sibling file and is not published as ready until its locally calculated SHA-256 exactly matches the checksum file entry.

After verification, AulaRaíz presents a second explicit action: **Cerrar y actualizar**. Existing pending-change protections are consulted first. If the teacher cancels a module's close/save decision, the application remains open and the update is not launched.

## Why a separate updater exists

The running WPF executable does not overwrite its own installed files.

After **Cerrar y actualizar**:

1. WPF copies the installed `AulaRaiz.Updater.exe` into the verified update cache so the installed helper itself is not locked during replacement;
2. WPF starts that temporary helper with only technical arguments (process id, installer path, expected SHA-256, app path, target version and Demo flag when applicable);
3. AulaRaíz closes normally;
4. the helper recalculates SHA-256 and refuses to proceed on mismatch;
5. the helper waits until the WPF process has exited;
6. it runs Inno Setup with `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-`;
7. it waits for a successful installer exit code;
8. it relaunches AulaRaíz and passes `--demo` only when the previous session was Demo;
9. WPF completes normal startup and then shows a concise update-success confirmation.

`--demo-reset` is never forwarded by the updater.

## Privacy boundary of update checks

Update discovery and downloads do not read SQLite and do not send student, teacher, group, school, attendance, evaluation, project or other classroom data.

The network request contains only normal HTTP metadata and a product User-Agent carrying the AulaRaíz version. GitHub credentials are not required because the repository and Releases are public.

Update failures use the same message-free safe diagnostic boundary as the rest of AulaRaíz. Release response bodies, arbitrary exception messages and classroom data are not written to diagnostics.

## Preview vs stable releases

Current `0.x` versions are GitHub prereleases, so the updater cannot rely on GitHub's `releases/latest` stable-only semantics. AulaRaíz lists published Releases and performs its own numeric `MAJOR.MINOR.PATCH` comparison.

The current application uses the **Preview** channel and accepts published prereleases. Draft releases are ignored. A candidate is eligible only when it contains both:

```text
AulaRaiz-Setup-<version>-win-x64.exe
SHA256SUMS.txt
```

The highest compatible version greater than the installed version is selected.

A future stable channel can reject prereleases while reusing the same contract.

## SQLite during application updates

Neither Inno Setup nor `AulaRaiz.Updater.exe` opens or edits SQLite. Database schema ownership remains in `SistemaDocente.Data`.

When an updated application opens a supported older database, normal application initialization/migration paths prepare the schema. A future destructive migration must define its own pre-migration safety backup and rollback behavior before implementation.

## Uninstall behavior

Ordinary uninstall removes installer-owned application files, shortcuts and uninstall registration. It intentionally does **not** remove Production data, Demo data, recovery packages or teacher-created exports/reports stored elsewhere.

## Development installer build

The repository contains:

```text
installer/AulaRaiz.iss
scripts/build-installer.ps1
```

The build script publishes WPF, CLI and updater, copies the two single-file helpers into the WPF publish directory and then compiles the Inno Setup package.

```powershell
.\scripts\build-installer.ps1 `
  -IsccPath "C:\Program Files\Inno Setup 7\ISCC.exe"
```

For the current product line the expected installer is:

```text
artifacts\installer\AulaRaiz-Setup-0.2.5-win-x64.exe
```

## CI supply-chain and lifecycle checks

Installer CI acquires the pinned Inno Setup compiler from its official GitHub Release and verifies the release asset before use.

The lifecycle test downloads the real published `v0.1.0` AulaRaíz installer and its checksum, verifies that baseline, then proves an update to the current source-built installer. After the current installation it requires:

- WPF at the repository version;
- `aularaiz.exe` at the repository version;
- `AulaRaiz.Updater.exe` at the repository version;
- exactly one AulaRaíz uninstall registration;
- preservation of a sentinel under the historical classroom-data directory.

Ordinary uninstall must remove all three program executables while the data sentinel survives.

## Code signing and production distribution

Current development/prerelease installers are still unsigned unless a trusted signing process is applied. SHA-256 verification prevents AulaRaíz from executing a downloaded installer whose bytes do not match the published checksum, but Authenticode remains the next distribution-hardening layer before broad institutional deployment.

A future production signing workflow should sign the installer (and preferably the updater/application executables) using protected signing infrastructure. Private signing keys must never be committed to the repository.

## Manual acceptance for the in-app updater

Before merging a change that modifies update coordination, validate on Demo or another non-production fixture:

1. install the current accepted AulaRaíz baseline;
2. provide a controlled newer Release/fixture with the exact installer/checksum asset names;
3. confirm automatic/manual discovery does not block startup;
4. confirm **Más tarde** makes no change;
5. confirm **Descargar e instalar** remains inside AulaRaíz and reaches 100% only after checksum verification;
6. create an unsaved change and confirm **Cerrar y actualizar** respects the normal pending-change warning;
7. complete the close/update path and confirm AulaRaíz reopens at the target version;
8. when testing Demo, confirm the process reopens in Demo without resetting its data;
9. confirm classroom data survives the upgrade;
10. confirm a deliberately incorrect checksum is rejected and the installer is never launched.
