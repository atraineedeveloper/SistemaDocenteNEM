# Updater and release trust specification

## ADDED Requirements

### Requirement: Update assets are bound to the AulaRaíz release identity
AulaRaíz SHALL accept update assets only when their HTTPS URLs identify the exact AulaRaíz repository, selected semantic tag and expected filename.

#### Scenario: Asset belongs to another GitHub repository
- **GIVEN** otherwise valid release metadata points the installer or checksum to another repository
- **WHEN** AulaRaíz evaluates or downloads the candidate
- **THEN** it SHALL reject the asset before requesting it
- **AND** SHALL NOT execute an installer.

#### Scenario: Asset path matches the selected release
- **GIVEN** both asset URLs use `github.com/atraineedeveloper/SistemaDocenteNEM/releases/download/<tag>/`
- **AND** their filenames exactly match the selected versioned installer and `SHA256SUMS.txt`
- **WHEN** discovery evaluates the Release
- **THEN** the candidate MAY proceed to checksum verification.

### Requirement: Installer downloads are resource bounded
AulaRaíz SHALL reject an installer whose declared or streamed content exceeds 512 MiB.

#### Scenario: Declared length exceeds the ceiling
- **WHEN** the installer response declares more than 512 MiB
- **THEN** AulaRaíz SHALL stop before streaming the body
- **AND** SHALL NOT publish the installer as verified.

#### Scenario: Stream exceeds the ceiling without a trustworthy length
- **WHEN** cumulative installer bytes exceed 512 MiB
- **THEN** AulaRaíz SHALL abort the download
- **AND** SHALL remove or leave unpublished the temporary file.

### Requirement: Tagged releases originate from accepted main history
The release workflow SHALL publish only when the tagged commit is reachable from `origin/main` and its semantic version equals the repository version.

#### Scenario: Tag points to an unmerged commit
- **WHEN** a valid-looking version tag points outside `origin/main` history
- **THEN** the release workflow SHALL fail before packaging or publication.

### Requirement: Release hardening preserves privacy and offline operation
Updater/release validation SHALL NOT read or transmit classroom storage and a rejected update SHALL NOT prevent normal local use.

#### Scenario: Trust validation fails
- **WHEN** an asset URL, checksum, size or tag ancestry check fails
- **THEN** no installer SHALL execute
- **AND** SQLite and classroom files SHALL remain outside updater ownership
- **AND** normal offline application work SHALL remain available.

### Requirement: Unsigned releases disclose residual publisher risk
Until production Authenticode is implemented, release documentation SHALL state that SHA-256 verifies matching bytes but does not independently authenticate the Windows publisher.

#### Scenario: Development release is published unsigned
- **WHEN** the release workflow publishes an unsigned installer
- **THEN** the Release SHALL warn that Windows may show an unknown publisher
- **AND** broad institutional distribution SHALL remain gated on production signing.
