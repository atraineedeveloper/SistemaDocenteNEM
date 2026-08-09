# AulaRaíz terminal and agent interface

AulaRaíz 0.2 adds a local command-line interface (`aularaiz.exe`) for teachers, scripts and agents. The CLI is an automation surface over the same Application/Core rules used by the desktop application; it is **not** a direct SQLite API.

## Installation and invocation

The normal Windows installer places the CLI beside the WPF application:

```powershell
$AulaRaiz = "$env:LOCALAPPDATA\Programs\AulaRaiz\aularaiz.exe"
& $AulaRaiz status --json
```

The CLI is self-contained. A separately installed .NET runtime is not required. Version 1 does not modify the user's global `PATH`; this avoids silently changing the shell environment. A teacher or automation host can define its own PowerShell variable/alias.

Production is the default storage profile. Add `--demo` to use the isolated Demo profile.

## Machine-readable contract

Agents should always use `--json`. Successful and failed commands use schema version `1` and include privacy metadata:

```json
{
  "schemaVersion": "1",
  "command": "status",
  "mode": "production",
  "success": true,
  "data": {},
  "privacy": {
    "classification": "D0",
    "includesPersonalData": false,
    "includesFreeText": false,
    "networkAccess": false
  },
  "warnings": []
}
```

Unexpected exceptions are not serialized. Errors use stable generic codes such as `invalid_arguments`, `not_found`, `validation_error`, `conflict`, `storage_error` or `internal_error`. Unexpected failures are recorded through AulaRaíz's privacy-safe local diagnostic stream.

## Discover available capabilities

```powershell
& $AulaRaiz capabilities --json
```

This is the preferred first call for an automation agent. V1 advertises no delete commands, reports `dry-run-unless-apply`, does not accept sensitive free-form mutation text, and reports no network access.

## Groups and students

List groups without names:

```powershell
& $AulaRaiz groups list --json
```

Explicitly include display names when a human/agent genuinely needs them:

```powershell
& $AulaRaiz groups list --include-personal-data --json
```

List students using a group GUID:

```powershell
& $AulaRaiz students list --group <group-guid> --json
```

Default output uses internal student ids plus structured operational fields (number, active state, grade). Names are omitted. Add `--include-personal-data` only when necessary.

### Reversible active-state changes

Preview a deactivation without changing data:

```powershell
& $AulaRaiz students deactivate --group <group-guid> --student <student-guid> --json
```

Apply it explicitly:

```powershell
& $AulaRaiz students deactivate --group <group-guid> --student <student-guid> --apply --json
```

Reactivation follows the same dry-run/apply rule.

## Attendance

Read an already-persisted day:

```powershell
& $AulaRaiz attendance show --group <group-guid> --date 2026-09-03 --json
```

Names remain omitted unless `--include-personal-data` is explicit.

Preview one student's attendance-state change:

```powershell
& $AulaRaiz attendance set `
  --group <group-guid> `
  --student <student-guid> `
  --date 2026-09-03 `
  --state F `
  --json
```

Apply the same change:

```powershell
& $AulaRaiz attendance set `
  --group <group-guid> `
  --student <student-guid> `
  --date 2026-09-03 `
  --state F `
  --apply `
  --json
```

Supported shortcuts match the desktop workflow:

- `P` — Presente
- `F` — Falta
- `R` — Retardo
- `J` — Justificada

The CLI does not patch a SQLite cell. It loads/prepares the complete daily roster, changes the target student in memory and sends the complete roster through `GestionAsistenciaCasosUso.Guardar`, preserving the application's roster/history rules.

## Agent context

```powershell
& $AulaRaiz agent context --group <group-guid> --json
```

The default context intentionally contains no student names or D3 free-form notes. It provides:

- group id;
- group modality, served grades, NEM phases and school-organization type;
- active/historical student counts;
- group attendance/completion/achievement summaries;
- monthly attendance aggregates;
- per-student internal ids with attendance/completion and `Requiere apoyo` indicators.

This output is designed to give an agent evidence for pedagogical reasoning while minimizing directly identifying information. It is still classroom information and must be handled according to `docs/privacy-data-inventory.md`.

Names can be requested explicitly with `--include-personal-data`. V1 still does **not** expose expediente notes, family agreements or free-form pedagogical/evaluation observations through agent output.

## Local recommendations

```powershell
& $AulaRaiz agent recommend --group <group-guid> --json
```

Recommendations are deterministic and generated locally. Each item includes a stable code, priority, recommendation, evidence and a coverage/caveat statement.

The analyzer can surface patterns such as:

- incomplete/pending evaluation evidence;
- non-delivery that merits a barrier/access review rather than an assumption about effort;
- structured `Requiere apoyo` evidence suggesting targeted scaffolding and another evidence opportunity;
- `En proceso` evidence suggesting a feedback/retry cycle;
- attendance records that should be considered when planning recovery opportunities;
- insufficient evidence, when AulaRaíz should explicitly avoid a stronger interpretation.

The analyzer does not diagnose students, infer disability/family/motivation causes, rank students or claim causality from attendance/evaluation data.

## Using an external AI agent

A local/external agent can execute `aularaiz.exe`, parse the JSON and provide additional recommendations. The CLI itself never calls a network service and never stores API credentials.

If the agent is cloud-hosted, **the tool that forwards CLI output to that service is a separate privacy boundary**. Prefer minimized `agent context` output; do not add `--include-personal-data` unless it is genuinely necessary and institutionally authorized. A future first-party network integration must be designed separately with explicit consent/data-minimization/credential controls.

## Why V1 does not accept names or notes as mutation arguments

Command lines can be retained in PowerShell history, terminal logs and process listings. For that reason V1 deliberately omits commands such as:

```text
students add --name "..."
expediente note --text "..."
family agreement --text "..."
```

A later secure narrative-input channel can use stdin or a protected local file/IPC protocol so sensitive text is not placed in argv.

## Demo validation

Create/reset fictitious Demo data from the desktop app first:

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo-reset
```

Then point the CLI to the same isolated profile:

```powershell
dotnet run --project .\src\SistemaDocente.Cli\SistemaDocente.Cli.csproj -- groups list --demo --json
```

Installed builds use the same `--demo` switch and historical Demo storage root.