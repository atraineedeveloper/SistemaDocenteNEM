# Proposal: Windows installer and update delivery

## Why

AulaRaíz is currently run from a development checkout. Teachers need a normal Windows installation experience that does not require the .NET SDK, does not require administrative rights for the default installation, preserves the existing local classroom data profile across upgrades and uninstall, and provides an explicit installed product version for support and diagnostics.

## What changes

- Publish the WPF application as a self-contained .NET 10 `win-x64` deployment so the target PC does not need a separately installed .NET runtime.
- Add a versioned Inno Setup 7 installer for a per-user installation under LocalAppData Programs.
- Give the installer a stable application identity so installing a newer AulaRaíz installer upgrades the existing installation instead of creating a second product entry.
- Create Start Menu integration and an optional desktop shortcut.
- Keep `%LOCALAPPDATA%\SistemaDocenteNEM` and `%LOCALAPPDATA%\SistemaDocenteNEM-Demo` outside the install directory and never remove them during ordinary uninstall.
- Keep SQLite schema evolution owned by the application. Installer upgrades replace program files only; they do not directly edit the database.
- Add installed-version display backed by application assembly metadata.
- Add Windows CI coverage that publishes the self-contained application, compiles the installer, performs a silent per-user install/upgrade/uninstall smoke test and proves that a user-data sentinel survives uninstall.
- Document signing as a production distribution requirement. The repository can build unsigned development installers, but production releases should be Authenticode-signed before broad distribution.

## Non-goals

- Automatic background update checks or downloading installers from inside AulaRaíz.
- Microsoft Store publication.
- MSIX packaging.
- Deleting local classroom data during uninstall.
- Renaming the legacy storage folders, SQLite filename, namespaces or backup package identifier.
- Implementing privacy/security module 14; that remains a separate change.

## Compatibility

The visible product name remains **AulaRaíz**. Existing technical storage contracts remain unchanged so an installed build opens the same Production/Demo data as the development build. Installer identity is new and stable, while local storage and `.sdocbackup` compatibility remain tied to the existing historical technical identifiers.