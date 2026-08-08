## ADDED Requirements

### Requirement: releases are tag-driven and version-consistent
The repository SHALL publish AulaRaíz releases only from semantic-version tags shaped as `vMAJOR.MINOR.PATCH`, and the tag version SHALL exactly match the repository `VersionPrefix`.

#### Scenario: tag matches repository version
- **WHEN** tag `v0.1.0` points to source whose `VersionPrefix` is `0.1.0`
- **THEN** the release workflow may continue to validation and packaging

#### Scenario: tag does not match repository version
- **WHEN** a release workflow receives a tag whose semantic version differs from `VersionPrefix`
- **THEN** the workflow fails before creating or modifying a GitHub Release

### Requirement: tagged source passes quality gates before publication
A GitHub Release SHALL NOT be published until the tagged source passes formatting verification, Release build, the complete test suite, OpenSpec validation and whitespace validation.

#### Scenario: a quality gate fails
- **WHEN** any required quality gate fails for the tagged source
- **THEN** no release is published for that workflow run

### Requirement: release packaging reuses validated Windows delivery
The release workflow SHALL build the existing self-contained `win-x64` AulaRaíz installer using the repository's established Inno Setup and installer build contracts rather than introducing a second packaging implementation.

#### Scenario: release installer is built
- **WHEN** tagged source has passed quality gates
- **THEN** the workflow verifies the pinned Inno Setup compiler and produces `AulaRaiz-Setup-<version>-win-x64.exe` through the existing build script

### Requirement: releases include integrity metadata
Each AulaRaíz GitHub Release SHALL attach the Windows installer and a `SHA256SUMS.txt` file containing the SHA-256 digest for that installer.

#### Scenario: release assets are prepared
- **WHEN** the installer has been built successfully
- **THEN** the workflow calculates SHA-256 from that exact installer and uploads both files to the same release

### Requirement: pre-1.0 versions are pre-releases
AulaRaíz versions whose semantic major version is `0` SHALL be published as GitHub pre-releases rather than stable releases.

#### Scenario: version is 0.1.0
- **WHEN** release tag `v0.1.0` is published
- **THEN** GitHub marks the resulting release as a pre-release

### Requirement: release notes disclose unsigned development status
Until a production Authenticode signing workflow exists, release notes SHALL clearly state that the installer is unsigned and Windows may identify the publisher as unknown.

#### Scenario: unsigned release is published
- **WHEN** the automated workflow creates a release without Authenticode signing
- **THEN** the release notes prepend the unsigned-development warning before generated change notes

### Requirement: release publication requires an existing tag
The automated release workflow SHALL require the Git tag to exist before creating the GitHub Release and SHALL NOT silently invent a missing release tag from the default branch.

#### Scenario: expected tag is missing
- **WHEN** release creation is attempted for a tag that does not exist remotely
- **THEN** release creation fails rather than creating a new tag implicitly

### Requirement: GitHub Packages remain outside current desktop distribution
The release implementation SHALL NOT publish AulaRaíz application projects as GitHub Packages merely to distribute the desktop installer.

#### Scenario: desktop release is published
- **WHEN** a new AulaRaíz desktop version is released
- **THEN** the installer is distributed through GitHub Releases and no NuGet/npm/container package is required by this change
