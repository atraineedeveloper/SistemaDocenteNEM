## ADDED Requirements

### Requirement: The visible product brand is AulaRaíz
The application SHALL present **AulaRaíz** as its primary user-facing product name and SHALL use **Gestión docente para la Nueva Escuela Mexicana** as its primary descriptor where a descriptor is appropriate.

#### Scenario: Teacher opens the application
- **WHEN** the main shell is displayed
- **THEN** the global header identifies the product as AulaRaíz rather than `Sistema Docente Local`

#### Scenario: Demo mode is running
- **WHEN** the application runs in Demo mode
- **THEN** the AulaRaíz brand remains visible and Demo is presented as a mode/status rather than as a different product name

### Requirement: Visible branding has one shared identity contract
User-facing brand strings used across Application, Presentation and WPF SHALL come from one stable product identity contract where practical rather than unrelated repeated literals.

Core SHALL remain independent from commercial branding.

#### Scenario: Main window title is composed
- **WHEN** the active module or group changes
- **THEN** the window title keeps AulaRaíz as its product prefix while module/group context is appended separately

### Requirement: The global header has a theme-safe compact brand mark
The main navigation header SHALL display a compact `AR` monogram using existing semantic theme resources and SHALL NOT require a new raster-logo dependency for this change.

#### Scenario: Theme changes
- **WHEN** the teacher switches between supported Light, Dark or High Contrast themes
- **THEN** the brand mark continues to use semantic resources rather than a hardcoded physical color palette

### Requirement: User-facing recovery surfaces use AulaRaíz
Native backup/restore file dialogs and maintained recovery copy SHALL identify the product as AulaRaíz when a product name is shown.

New suggested backup filenames SHALL use the ASCII-safe brand form `AulaRaiz`.

#### Scenario: Teacher creates a new backup
- **WHEN** the save dialog opens for a manual backup
- **THEN** the dialog identifies the backup as an AulaRaíz backup and proposes a filename beginning with `AulaRaiz_Respaldo_`

### Requirement: Branding does not break existing local data
The branding change SHALL NOT silently rename or move the historical Production/Demo storage paths, SQLite filename or technical namespace/project identities.

#### Scenario: Existing installation starts after rebranding
- **WHEN** AulaRaíz starts on a computer that already has data under `%LOCALAPPDATA%\SistemaDocenteNEM`
- **THEN** it continues to use that existing data instead of creating an empty parallel profile solely because the visible product name changed

### Requirement: Version-1 backup compatibility is preserved
The branding change SHALL keep the existing `SistemaDocenteNEM.Backup` version-1 package identifier so backups created before the AulaRaíz rename remain inspectable/restorable subject to the existing compatibility rules.

#### Scenario: Older version-1 backup is selected
- **WHEN** a valid pre-branding `.sdocbackup` package with product id `SistemaDocenteNEM.Backup` is inspected
- **THEN** it is not rejected merely because the current visible product name is AulaRaíz

### Requirement: WPF executable metadata reflects AulaRaíz
The WPF executable SHALL expose AulaRaíz as its Product and AssemblyTitle and SHALL expose the NEM descriptor as its Description without requiring a technical assembly rename in this change.

#### Scenario: Installed application metadata is inspected
- **WHEN** Windows or a future installer reads the executable product metadata
- **THEN** the user-facing product identity is AulaRaíz while the technical project/assembly naming migration remains separately controlled
