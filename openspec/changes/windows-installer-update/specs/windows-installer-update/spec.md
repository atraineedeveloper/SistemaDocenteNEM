## ADDED Requirements

### Requirement: Self-contained Windows delivery
AulaRaíz SHALL provide an installable `win-x64` Windows package whose application runtime is self-contained and does not require the target teacher PC to have the .NET 10 Desktop Runtime installed separately.

#### Scenario: Install on a supported clean Windows account
- **GIVEN** a supported x64-compatible Windows environment without the .NET SDK or .NET 10 Desktop Runtime
- **WHEN** the user installs AulaRaíz
- **THEN** the installed application can start using only the files delivered by the installer and operating-system components

### Requirement: No default administrator requirement
The installer SHALL default to a per-user installation and SHALL NOT require elevation for the normal installation path.

#### Scenario: Standard-user installation
- **GIVEN** a normal Windows user account
- **WHEN** the user starts the installer
- **THEN** AulaRaíz is installed under the current user's LocalAppData Programs area without requiring administrator credentials

### Requirement: Stable product upgrade identity
The installer SHALL use one stable AppId across releases and SHALL treat a newer installer as an upgrade to the existing AulaRaíz installation rather than as a second independent product.

#### Scenario: Install newer version over older version
- **GIVEN** AulaRaíz version A is installed
- **WHEN** the user runs an installer for version B where B is newer and the installer has the same AppId
- **THEN** the existing installation is updated in place and only one normal uninstall entry remains for AulaRaíz

### Requirement: Preserved classroom-data identity
Installation, upgrade and uninstall SHALL preserve the historical local-data contracts `%LOCALAPPDATA%\SistemaDocenteNEM`, `%LOCALAPPDATA%\SistemaDocenteNEM-Demo`, `sistema-docente.db` and `SistemaDocenteNEM.Backup`.

#### Scenario: Upgrade existing teacher data
- **GIVEN** the teacher has Production or Demo data in the existing local-data folders
- **WHEN** a newer AulaRaíz installer is installed
- **THEN** the installer does not move, rename, recreate or delete those data folders
- **AND** the newer application opens them through the existing application storage logic

#### Scenario: Uninstall application
- **GIVEN** AulaRaíz has local classroom data and backups
- **WHEN** the user uninstalls AulaRaíz
- **THEN** installer-owned binaries and shortcuts are removed
- **AND** classroom data, backups and exports outside the install directory are left intact

### Requirement: Application-owned SQLite migration
The installer SHALL NOT directly mutate SQLite. Schema initialization and supported migrations SHALL remain owned by the application's Data layer and SHALL reject incompatible future schema versions instead of resetting data.

#### Scenario: Application opens a supported older database after update
- **GIVEN** a database version supported by the current migration paths
- **WHEN** the newly installed AulaRaíz version opens it
- **THEN** existing additive/idempotent application migration logic prepares it successfully without installer SQL

#### Scenario: Application encounters an unsupported future database
- **GIVEN** a database schema newer than the application supports
- **WHEN** the installed application attempts to open it
- **THEN** the application rejects it through the existing incompatibility path and does not overwrite or reset it

### Requirement: Installed version visibility
The installed application SHALL expose a human-readable product version derived from the same version source used by the installer.

#### Scenario: Support identifies installed build
- **WHEN** the user opens the version/about surface
- **THEN** AulaRaíz displays its product name and semantic installed version

### Requirement: Normal Windows integration
The installer SHALL create a Start Menu entry for AulaRaíz and MAY create a desktop shortcut only when the user explicitly selects that option.

#### Scenario: Default install
- **WHEN** the user completes the installer using default options
- **THEN** AulaRaíz is available from the Start Menu
- **AND** no desktop shortcut is created unless the user opted into it

### Requirement: Build and lifecycle verification
The repository SHALL contain automated Windows validation for self-contained publish, installer compilation, installation/upgrade/uninstall behavior and user-data preservation.

#### Scenario: CI installer smoke test
- **WHEN** the installer validation workflow runs
- **THEN** it publishes the WPF app for `win-x64`
- **AND** compiles the installer
- **AND** performs a silent current-user install and upgrade smoke test
- **AND** performs a silent uninstall
- **AND** verifies a sentinel placed in the legacy local-data folder survives uninstall

### Requirement: Production signing boundary
The installer documentation SHALL distinguish an unsigned development build from a production-distribution build and SHALL require Authenticode signing for broad trusted distribution without storing signing secrets in the repository.

#### Scenario: Repository build without signing secret
- **WHEN** CI builds the development installer without signing credentials
- **THEN** installer compilation still succeeds
- **AND** documentation clearly states that production distribution must add trusted signing outside source control