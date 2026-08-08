# Windows installer and update requirements

## Requirement: self-contained Windows delivery

AulaRaíz SHALL provide an installable `win-x64` Windows package whose application runtime is self-contained and does not require the target teacher PC to have the .NET 10 Desktop Runtime installed separately.

### Scenario: install on a supported clean Windows account
- **Given** a supported x64-compatible Windows environment without the .NET SDK or .NET 10 Desktop Runtime
- **When** the user installs AulaRaíz
- **Then** the installed application can start using only the files delivered by the installer and operating-system components.

## Requirement: no default administrator requirement

The installer SHALL default to a per-user installation and SHALL NOT require elevation for the normal installation path.

### Scenario: standard-user installation
- **Given** a normal Windows user account
- **When** the user starts the installer
- **Then** AulaRaíz is installed under the current user's LocalAppData Programs area without requiring administrator credentials.

## Requirement: stable product upgrade identity

The installer SHALL use one stable AppId across releases and SHALL treat a newer installer as an upgrade to the existing AulaRaíz installation rather than as a second independent product.

### Scenario: install newer version over older version
- **Given** AulaRaíz version A is installed
- **When** the user runs an installer for version B where B is newer and the installer has the same AppId
- **Then** the existing installation is updated in place and only one normal uninstall entry remains for AulaRaíz.

## Requirement: preserved classroom-data identity

Installation, upgrade and uninstall SHALL preserve the historical local-data contracts `%LOCALAPPDATA%\SistemaDocenteNEM`, `%LOCALAPPDATA%\SistemaDocenteNEM-Demo`, `sistema-docente.db` and `SistemaDocenteNEM.Backup`.

### Scenario: upgrade existing teacher data
- **Given** the teacher has Production or Demo data in the existing local-data folders
- **When** a newer AulaRaíz installer is installed
- **Then** the installer does not move, rename, recreate or delete those data folders and the newer application opens them through the existing application storage logic.

### Scenario: uninstall application
- **Given** AulaRaíz has local classroom data and backups
- **When** the user uninstalls AulaRaíz
- **Then** installer-owned binaries and shortcuts are removed
- **And** classroom data, backups and exports outside the install directory are left intact.

## Requirement: application-owned SQLite migration

The installer SHALL NOT directly mutate SQLite. Schema initialization and supported migrations SHALL remain owned by the application's Data layer and SHALL reject incompatible future schema versions instead of resetting data.

### Scenario: application opens a supported older database after update
- **Given** a database version supported by the current migration paths
- **When** the newly installed AulaRaíz version opens it
- **Then** existing additive/idempotent application migration logic prepares it successfully without installer SQL.

### Scenario: application encounters an unsupported future database
- **Given** a database schema newer than the application supports
- **When** the installed application attempts to open it
- **Then** the application rejects it through the existing incompatibility path and does not overwrite or reset it.

## Requirement: installed version visibility

The installed application SHALL expose a human-readable product version derived from the same version source used by the installer.

### Scenario: support identifies installed build
- **When** the user opens the version/about surface
- **Then** AulaRaíz displays its product name and semantic installed version.

## Requirement: normal Windows integration

The installer SHALL create a Start Menu entry for AulaRaíz and MAY create a desktop shortcut only when the user explicitly selects that option.

### Scenario: default install
- **When** the user completes the installer using default options
- **Then** AulaRaíz is available from the Start Menu
- **And** no desktop shortcut is created unless the user opted into it.

## Requirement: build and lifecycle verification

The repository SHALL contain automated Windows validation for self-contained publish, installer compilation, installation/upgrade/uninstall behavior and user-data preservation.

### Scenario: CI installer smoke test
- **When** the installer validation workflow runs
- **Then** it publishes the WPF app for `win-x64`
- **And** compiles the installer
- **And** performs a silent current-user install and upgrade smoke test
- **And** performs a silent uninstall
- **And** verifies a sentinel placed in the legacy local-data folder survives uninstall.

## Requirement: production signing boundary

The installer documentation SHALL distinguish an unsigned development build from a production-distribution build and SHALL require Authenticode signing for broad trusted distribution without storing signing secrets in the repository.

### Scenario: repository build without signing secret
- **When** CI builds the development installer without signing credentials
- **Then** installer compilation still succeeds
- **And** documentation clearly states that production distribution must add trusted signing outside source control.