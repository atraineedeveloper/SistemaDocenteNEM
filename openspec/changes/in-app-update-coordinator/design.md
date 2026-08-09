# Design: in-app update coordinator

## Overview

The update flow is split into three responsibilities so the running WPF process never overwrites itself:

1. **Application contracts** describe update metadata and verification state.
2. **Interchange/network implementation** discovers GitHub Releases and downloads assets.
3. **WPF + updater helper** collect explicit consent, close the app, run the existing installer and restart AulaRaíz.

The existing Inno Setup package remains the only component that replaces installed program files. SQLite and the historical `%LOCALAPPDATA%\SistemaDocenteNEM*` data roots remain application-owned.

## Release discovery

AulaRaíz `0.x` uses the Preview channel. The client lists published releases rather than relying on GitHub's `/releases/latest` endpoint because current AulaRaíz releases are prereleases.

A candidate is eligible when all of the following are true:

- not a draft;
- semantic tag matches `vMAJOR.MINOR.PATCH`;
- version is greater than the installed version;
- Preview channel accepts prereleases;
- it contains exactly the expected installer asset `AulaRaiz-Setup-<version>-win-x64.exe`;
- it contains `SHA256SUMS.txt`.

The highest eligible semantic version wins.

## Privacy boundary

The HTTP client sends only normal protocol metadata plus a product User-Agent containing the AulaRaíz version. It does not read SQLite or include teacher/student/group/school identifiers. Release discovery/download does not require GitHub credentials because the repository and Releases are public.

Failures use the existing safe diagnostics category model and must not persist response bodies or arbitrary exception text.

## Download and verification

The selected installer and checksum file are downloaded to a non-classroom cache under `%LOCALAPPDATA%\AulaRaiz\Updates\<version>\`.

The download workflow:

1. download `SHA256SUMS.txt`;
2. parse the exact expected installer filename;
3. validate that the checksum is exactly 64 hexadecimal characters;
4. stream the installer to a temporary sibling file;
5. calculate SHA-256 locally;
6. reject and delete the temporary file on mismatch;
7. atomically publish the verified installer into the update cache.

A verified update record contains version, installer path and expected hash.

## User experience

Release discovery runs asynchronously after the main window is usable; lack of network must never block startup.

When an update is available, WPF presents the installed/new versions and release notes with two actions:

- **Más tarde** — no download/install occurs;
- **Descargar e instalar** — download and verify while AulaRaíz stays open.

After verification, WPF displays a second explicit consent boundary explaining that AulaRaíz will close and reopen. Only **Cerrar y actualizar** launches the helper.

## Updater helper

`AulaRaiz.Updater.exe` is a separate self-contained Windows executable installed beside WPF. It receives only technical values: parent process id, verified installer path, expected SHA-256, application executable path, target version and whether Demo mode should be restored.

The helper:

1. validates arguments;
2. re-computes SHA-256 and refuses to continue on mismatch;
3. waits for the parent WPF process to exit;
4. starts the Inno installer with `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-`;
5. waits for a successful installer exit code;
6. starts the installed WPF executable with `--updated-to <version>` and `--demo` when applicable;
7. shows a generic local error dialog if installation/restart fails.

The helper never opens SQLite or application data files.

## Restart and success feedback

WPF accepts `--updated-to <version>` as a technical startup argument. After normal startup succeeds, AulaRaíz shows a short confirmation that the update completed. Demo mode is preserved by forwarding `--demo` only; `--demo-reset` is never forwarded by an update.

## Packaging

The existing installer build script publishes `AulaRaiz.Updater.exe` self-contained/single-file and copies it into the WPF publish directory before Inno Setup runs. Installer CI verifies the helper is installed and removed with the other program files.

## Versioning

`Directory.Build.props` is bumped from `0.2.0` to `0.2.5`. A future `v0.2.5` tag will use the existing Release workflow after this change is merged and accepted.
