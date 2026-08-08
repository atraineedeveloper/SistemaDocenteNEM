# Deployment architecture

AulaRaíz separates **program delivery** from **teacher data ownership**. This boundary is part of the system architecture, not merely an installer preference.

## Program delivery boundary

The WPF application is published self-contained for `win-x64`. `scripts/build-installer.ps1` creates the publish output and feeds it to `installer/AulaRaiz.iss`, which produces a conventional Inno Setup 7 installer.

```text
source repository
    │
    ├── dotnet publish (Release, win-x64, self-contained)
    │       ↓
    │   artifacts/publish/win-x64
    │       ↓
    └── Inno Setup 7
            ↓
        AulaRaiz-Setup-<version>-win-x64.exe
            ↓
        %LOCALAPPDATA%\Programs\AulaRaiz
```

The installed directory contains executable/runtime/application dependencies only. It is replaceable during an update and removable during uninstall.

## Persistent data boundary

Runtime classroom data remains outside the installer-owned directory:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\...
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\...
```

This preserves the existing storage contract used by development builds, installed builds, backup/restore and schema migration code. The installer has no `[Files]`, `[Dirs]`, `[InstallDelete]`, `[UninstallDelete]` or SQL operation that claims those paths.

Consequences:

- reinstalling/updating AulaRaíz replaces program files without replacing the database;
- uninstalling AulaRaíz does not silently erase classroom records;
- reinstalling after uninstall can reopen preserved data;
- installer versioning can evolve independently from SQLite schema versioning;
- any future explicit user-data deletion belongs to privacy/security workflows, not the uninstaller.

## Version source

`Directory.Build.props` is the repository source for the semantic product version. The same version flows into managed assembly/file/informational metadata and into the Inno Setup build.

The visible UI obtains its value from assembly informational metadata through `IdentidadProducto.VersionVisible`. Installer CI checks the installed executable and uninstall registration against the same `VersionPrefix`.

`VersionOverride` in the installer build script is a validation-only escape hatch used to produce an older synthetic upgrade fixture. Normal product builds always use the repository version source.

## Update boundary

Version 1 uses installer-over-installer updates. A stable Inno Setup AppId and the same default application directory identify subsequent packages as the same installed product.

The update path is intentionally:

```text
new trusted installer
    ↓
replace installer-owned program files
    ↓
launch new AulaRaíz build
    ↓
application Data layer initializes/migrates supported SQLite schema
```

The installer does not execute schema SQL. This avoids two independent migration engines and keeps the existing Data-layer compatibility checks authoritative.

Automatic update discovery/downloading is not part of this architecture yet. Adding it later must preserve offline operation, user control, package authenticity and rollback considerations.

## Build-supply-chain boundary

The installer workflow pins Inno Setup 7.0.2. The compiler executable is downloaded from the official immutable GitHub release and verified with GitHub release-asset attestation before execution.

Development CI produces unsigned installer artifacts. Production code signing is a separate trust boundary: Authenticode credentials/private keys must come from protected release infrastructure and must never be stored in repository source.

## Automated lifecycle proof

`.github/workflows/installer.yml` validates the deployment boundary on Windows with a real version transition:

1. obtain and verify the pinned Inno compiler;
2. build an ephemeral `0.0.9` installer fixture using the same stable AppId;
3. build the repository's actual `0.1.0` installer;
4. install `0.0.9` for the current user and validate installed version metadata;
5. create a sentinel under the legacy Production data path;
6. install `0.1.0` over the older installation;
7. verify executable/uninstall metadata changed to `0.1.0` and exactly one AulaRaíz uninstall entry remains;
8. verify the legacy data sentinel survived the update;
9. uninstall;
10. verify program removal and verify the legacy data sentinel still remains;
11. upload the current installer plus the paired upgrade-validation installers for manual acceptance.

The synthetic `0.0.9` package is not a product release; it exists only to prove version-to-version installer behavior. This lifecycle test complements, rather than replaces, the normal solution CI and Data-layer migration tests.
