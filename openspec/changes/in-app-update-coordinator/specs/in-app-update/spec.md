# In-app update specification

## ADDED Requirements

### Requirement: AulaRaíz discovers compatible GitHub Releases without blocking normal use
AulaRaíz SHALL be able to check the public repository Releases asynchronously after the application becomes usable. The check SHALL NOT require GitHub credentials and SHALL NOT read or transmit classroom data.

#### Scenario: Preview update is available
- **GIVEN** AulaRaíz `0.2.5` is installed on the Preview channel
- **AND** a published non-draft prerelease with a greater semantic version exists
- **AND** that Release contains the expected versioned installer and `SHA256SUMS.txt`
- **WHEN** update discovery completes
- **THEN** AulaRaíz SHALL identify the highest eligible semantic version
- **AND** SHALL make that update available to the teacher.

#### Scenario: Network is unavailable
- **WHEN** the GitHub Release request cannot be completed
- **THEN** AulaRaíz SHALL continue normal local operation
- **AND** SHALL NOT block application startup.

### Requirement: Downloaded installers are verified before installation is offered
AulaRaíz SHALL download the checksum file and expected installer into a local update cache and SHALL verify SHA-256 before treating the installer as ready.

#### Scenario: Installer hash matches
- **WHEN** the downloaded installer hash equals the exact `SHA256SUMS.txt` entry for its filename
- **THEN** AulaRaíz SHALL atomically publish the verified installer into the update cache
- **AND** SHALL allow the teacher to proceed to the close-and-update confirmation.

#### Scenario: Installer hash does not match
- **WHEN** the downloaded installer hash differs from the expected checksum
- **THEN** AulaRaíz SHALL reject the update
- **AND** SHALL NOT execute the installer
- **AND** SHALL remove or leave unpublished the unverified temporary installer.

### Requirement: Update installation requires explicit teacher consent
AulaRaíz SHALL NOT silently install an update while the teacher is using the application.

#### Scenario: Teacher postpones an available update
- **WHEN** AulaRaíz reports a newer version
- **AND** the teacher chooses `Más tarde`
- **THEN** no installer SHALL be executed
- **AND** the current application session SHALL continue.

#### Scenario: Teacher confirms installation
- **GIVEN** an installer has been downloaded and verified
- **WHEN** the teacher chooses `Cerrar y actualizar`
- **THEN** AulaRaíz SHALL launch the updater helper with technical restart parameters
- **AND** SHALL close the WPF process normally.

### Requirement: A separate updater performs the file replacement and restart
The running WPF process SHALL NOT overwrite its own installed binaries. `AulaRaiz.Updater.exe` SHALL coordinate installation after WPF exits.

#### Scenario: Verified update installs successfully
- **GIVEN** the helper receives the parent process id, installer path, expected SHA-256, target application path and target version
- **WHEN** the helper re-verifies the installer hash and the parent WPF process exits
- **THEN** the helper SHALL execute the existing Inno Setup installer silently
- **AND** SHALL wait for a successful exit code
- **AND** SHALL relaunch the installed AulaRaíz executable.

#### Scenario: Hash changes before helper execution
- **WHEN** the helper computes a SHA-256 value different from the expected hash
- **THEN** it SHALL refuse to run the installer
- **AND** SHALL NOT modify installed application files.

### Requirement: Update preserves local data and launch mode
The update coordinator and helper SHALL NOT directly modify SQLite, backups, exports, reports or application-state data.

#### Scenario: Demo session updates
- **GIVEN** the currently running session is Demo
- **WHEN** a verified update is installed and AulaRaíz is relaunched
- **THEN** the helper SHALL relaunch with `--demo`
- **AND** SHALL NOT pass `--demo-reset`.

#### Scenario: Production session updates
- **GIVEN** the currently running session is Production
- **WHEN** the update completes
- **THEN** AulaRaíz SHALL relaunch without a Demo argument.

### Requirement: Successful restart gives visible feedback
AulaRaíz SHALL be able to distinguish a normal start from a post-update restart using technical startup arguments that contain no classroom data.

#### Scenario: Post-update restart succeeds
- **WHEN** AulaRaíz starts with the updater success argument for version `0.2.5`
- **THEN** the application SHALL complete normal storage initialization first
- **AND** SHALL then show a concise success confirmation identifying the installed version.

### Requirement: Version 0.2.5 packages the updater helper
The Windows installer for AulaRaíz `0.2.5` SHALL contain `AulaRaiz.Updater.exe` beside the primary WPF executable and CLI.

#### Scenario: Installer lifecycle validation
- **WHEN** installer CI installs AulaRaíz `0.2.5`
- **THEN** WPF, `aularaiz.exe` and `AulaRaiz.Updater.exe` SHALL exist in the install directory
- **AND** normal uninstall SHALL remove all three program executables without deleting the legacy classroom-data sentinel.
