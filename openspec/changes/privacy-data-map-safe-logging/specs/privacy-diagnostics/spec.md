# Privacy data inventory and safe diagnostics

## ADDED Requirements

### Requirement: Maintained data inventory
AulaRaíz SHALL maintain a product data inventory that identifies current primary data, derived copies, storage locations and an engineering sensitivity classification for each category.

#### Scenario: Student and pedagogical data are inventoried
- **WHEN** the maintained privacy inventory is reviewed
- **THEN** it identifies student identity/context, attendance, formative evaluation, expediente/family agreements and school context
- **AND** it identifies SQLite, app-state, backup, export, PDF and diagnostic surfaces

#### Scenario: Classification is not presented as a legal conclusion
- **WHEN** the inventory describes D0–D3 classifications
- **THEN** it states that the levels are product-engineering controls rather than statutory legal determinations

### Requirement: Derived copies inherit sensitivity
A derived file or package SHALL be handled according to the highest engineering sensitivity classification of the data it contains.

#### Scenario: Complete backup contains high-sensitivity educational data
- **WHEN** a `.sdocbackup` contains the current classroom database
- **THEN** the inventory classifies the package as D3 even if its manifest contains only technical metadata

### Requirement: Persisted diagnostics exclude classroom content
AulaRaíz SHALL NOT persist raw exception messages, stack traces, file-system paths, student/group identifiers, classroom values, pedagogical text or arbitrary caller-provided metadata in technical diagnostics.

#### Scenario: Exception contains sensitive text
- **WHEN** an exception message or stack context contains a student sentinel or local file path
- **THEN** the persisted diagnostic event does not contain that message, path or stack trace
- **AND** the event can still be grouped through predefined category, exception type and technical fingerprint

### Requirement: Diagnostics use a closed technical schema
A persisted diagnostic event SHALL contain only explicitly defined D0 technical fields: UTC timestamp, random event id, predefined category, exception type chain, technical fingerprint, application version and Production/Demo mode.

#### Scenario: Unhandled WPF exception is recorded
- **WHEN** WPF observes an unhandled exception after diagnostic initialization
- **THEN** it records a safe diagnostic event through the shared diagnostic contract
- **AND** it does not write `Exception.ToString()` to `crash.log`

### Requirement: Production and Demo diagnostics are isolated
Production and Demo diagnostics SHALL be written under their corresponding historical LocalApplicationData profile roots.

#### Scenario: Demo failure is recorded
- **WHEN** AulaRaíz runs in Demo mode and records a diagnostic event
- **THEN** the event is written under `SistemaDocenteNEM-Demo`
- **AND** the Production diagnostic stream is not used

### Requirement: Diagnostic failure is non-fatal
Failure to create or append the local diagnostic file SHALL NOT interrupt normal application error handling.

#### Scenario: Diagnostic path cannot be written
- **WHEN** the diagnostic adapter cannot create or append its JSONL file
- **THEN** the adapter does not throw a secondary failure to the application host

### Requirement: Legacy crash logs are treated as potentially sensitive
The maintained inventory SHALL state that older `crash.log` files may contain raw exception details and SHALL NOT require automatic deletion of existing user files.

#### Scenario: Existing installation is upgraded
- **WHEN** an installation already contains `crash.log`
- **THEN** new builds stop appending raw exception representations
- **AND** the existing file is left untouched unless the user deliberately removes it

### Requirement: Future terminal/agent access follows the inventory
Any future local terminal/agent interface SHALL use Application use cases rather than direct SQLite access and SHALL default to minimized/pseudonymous projections.

#### Scenario: Agent context is requested by default
- **WHEN** a future agent command requests classroom context without an explicit personal-data option
- **THEN** the projection prefers stable internal ids and aggregate pedagogical evidence over D2/D3 identity/free-text fields
- **AND** the command itself does not upload data to a network service