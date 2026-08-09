# Local terminal and agent interface

## ADDED Requirements

### Requirement: CLI reuses application use cases
The AulaRaíz CLI SHALL use Core/Application/Data interfaces and use cases for classroom operations and SHALL NOT issue domain SQL directly from command handlers.

#### Scenario: Attendance is changed from terminal
- **WHEN** a user applies an attendance state change for one student
- **THEN** the CLI loads/prepares the complete daily roster
- **AND** calls the existing attendance Application use case with the complete roster

### Requirement: Machine-readable output is versioned
The CLI SHALL provide a deterministic `--json` envelope containing schema version, command, mode, success state, data, privacy metadata and warnings/errors.

#### Scenario: Agent requests group context
- **WHEN** `agent context` is executed with `--json`
- **THEN** stdout is a single valid JSON document
- **AND** its `schemaVersion` is `1`
- **AND** privacy metadata states whether personal data or free text is included

### Requirement: Personal data is minimized by default
CLI read/agent commands SHALL prefer stable internal ids and structured/aggregate evidence over names or free-form sensitive content.

#### Scenario: Student list uses default privacy mode
- **WHEN** `students list` is executed without `--include-personal-data`
- **THEN** student ids and necessary structured fields may be returned
- **AND** student names/surnames are not returned

#### Scenario: Personal names are explicitly requested
- **WHEN** a supported read command includes `--include-personal-data`
- **THEN** the command may include names
- **AND** its privacy metadata reports `includesPersonalData: true`

### Requirement: D3 free-form content stays out of V1 agent output
V1 agent commands SHALL NOT return expediente notes, family agreements, pedagogical observations or evaluation observation free text.

#### Scenario: Group recommendation context is produced
- **WHEN** `agent context` or `agent recommend` is executed
- **THEN** the output contains only structured/aggregate evidence defined for V1
- **AND** it does not contain free-form D3 notes or agreements

### Requirement: Mutations require explicit apply
Every V1 mutation SHALL be a dry run unless the exact `--apply` option is present.

#### Scenario: Attendance mutation omits apply
- **WHEN** `attendance set` is called without `--apply`
- **THEN** no attendance data is persisted
- **AND** the response clearly reports a dry run

#### Scenario: Attendance mutation includes apply
- **WHEN** `attendance set` is valid and includes `--apply`
- **THEN** exactly the requested student's state is changed through the Application use case
- **AND** existing roster/history validation remains authoritative

#### Scenario: Student activation mutation omits apply
- **WHEN** `students deactivate` or `students reactivate` omits `--apply`
- **THEN** no student active-state change is persisted

### Requirement: V1 has no destructive delete commands
The CLI SHALL NOT expose deletion of groups, students, projects, activities, attendance history or expediente history in V1.

#### Scenario: Capabilities are requested
- **WHEN** `capabilities --json` is executed
- **THEN** it enumerates supported commands
- **AND** no delete capability is advertised

### Requirement: Sensitive free text is not accepted through argv
V1 SHALL NOT accept student names, observations, family agreements, pedagogical notes or other sensitive free-form classroom content as command-line arguments for mutation.

#### Scenario: Agent needs to alter sensitive narrative data
- **WHEN** the desired operation requires free-form D2/D3 narrative content
- **THEN** V1 exposes no argv command for that operation

### Requirement: Agent context supports pedagogical reasoning
`agent context` SHALL provide structured evidence sufficient for local/external agent reasoning without default student names, including group coverage, attendance, activity completion, achievement distribution and pseudonymous student-level indicators.

#### Scenario: Group has report evidence
- **WHEN** group agent context is requested
- **THEN** aggregate evidence and per-student internal ids/metrics are returned
- **AND** no competitive ranking is generated

### Requirement: Recommendations are transparent and non-diagnostic
`agent recommend` SHALL generate deterministic local recommendations that include evidence and coverage/caveats and SHALL NOT diagnose students, infer unsupported causes or rank them.

#### Scenario: Evidence includes incomplete evaluation
- **WHEN** pending/incomplete activity evidence exists
- **THEN** a recommendation may advise completing evidence before interpreting progress
- **AND** it states the observed counts used

#### Scenario: Students require support
- **WHEN** structured achievement data contains `Requiere apoyo`
- **THEN** a recommendation may suggest targeted scaffolding/feedback
- **AND** it does not infer a disability, family cause or lack of effort

### Requirement: CLI is offline by itself
The CLI SHALL NOT send classroom data to network services.

#### Scenario: Agent context is generated
- **WHEN** any V1 CLI command runs
- **THEN** its own processing uses local storage only
- **AND** privacy metadata reports `networkAccess: false`

### Requirement: Errors do not leak raw exceptions
CLI error JSON SHALL use stable generic codes/messages and SHALL NOT serialize raw exception messages or stack traces. Unexpected failures SHALL use the shared safe diagnostic contract.

#### Scenario: Unexpected terminal failure occurs
- **WHEN** an unexpected exception reaches the CLI host
- **THEN** it is recorded with a predefined safe terminal diagnostic category
- **AND** stdout/stderr does not include raw exception content in JSON mode

### Requirement: CLI ships with AulaRaíz
The normal Windows installer SHALL include a self-contained `aularaiz.exe` terminal executable.

#### Scenario: AulaRaíz is installed
- **WHEN** installation completes
- **THEN** `aularaiz.exe` exists under the AulaRaíz program directory
- **AND** it can execute `status --json` without a separately installed .NET runtime