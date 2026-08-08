# Design: local terminal and agent interface

## Architecture

`SistemaDocente.Cli` is a console host, not a second business layer. Command handlers compose the existing SQLite adapters and Application use cases. They do not open SQLite connections directly and do not duplicate attendance/student business rules.

```text
Terminal / agent
    ↓
aularaiz.exe
    ↓
CLI command + privacy projection
    ↓
Application use cases
    ↓
Core rules / Reporting analysis
    ↓
Data SQLite adapters
```

WPF and CLI share one Data-owned LocalApplicationData path contract so Production/Demo locations cannot drift.

## Command contract V1

Read/discovery:

```text
aularaiz capabilities --json
aularaiz status [--demo] --json
aularaiz groups list [--demo] [--include-personal-data] --json
aularaiz students list --group <guid> [--demo] [--include-personal-data] --json
aularaiz attendance show --group <guid> --date <yyyy-MM-dd> [--demo] [--include-personal-data] --json
aularaiz agent context --group <guid> [--demo] [--include-personal-data] --json
aularaiz agent recommend --group <guid> [--demo] --json
```

Reversible writes:

```text
aularaiz attendance set --group <guid> --date <yyyy-MM-dd> --student <guid> --state P|F|R|J [--demo] [--apply] --json
aularaiz students deactivate --group <guid> --student <guid> [--demo] [--apply] --json
aularaiz students reactivate --group <guid> --student <guid> [--demo] [--apply] --json
```

Without `--apply`, mutation commands return an explicit dry-run result and do not persist. No V1 command deletes history.

## JSON envelope

Agent-oriented output uses a versioned envelope:

```json
{
  "schemaVersion": "1",
  "command": "agent.context",
  "mode": "production",
  "success": true,
  "data": {},
  "privacy": {
    "classification": "D1",
    "includesPersonalData": false,
    "includesFreeText": false,
    "networkAccess": false
  },
  "warnings": []
}
```

Command names and the envelope schema are compatibility contracts. Human-readable output can evolve independently, but `--json` must stay deterministic and machine parseable.

## Privacy defaults

- Internal GUID ids are returned by default because agents need stable referents across calls.
- Student/group names are omitted unless `--include-personal-data` is explicit.
- D3 free-form expediente/evaluation observations are not exposed by V1 agent commands, even with the personal-data switch.
- Raw database paths are not part of normal command output.
- No command transmits data to a network service.
- Sensitive free-form content is not accepted in argv.

The local process may read the data needed to compute a projection, but the externalized terminal/agent payload is minimized according to `docs/privacy-data-inventory.md`.

## Write safety

Attendance `set` loads/prepares the complete daily roster, changes exactly one requested student state in memory, then calls `GestionAsistenciaCasosUso.Guardar` with the full roster. This preserves the existing exact-roster and historical-roster validations.

Student deactivate/reactivate calls `GestionGrupoCasosUso`; it does not edit database rows directly.

The `--apply` flag is intentionally mandatory for every V1 mutation. Dry-run results identify the requested target/action but never claim persistence occurred.

## Recommendation model

Recommendations are generated locally and deterministically from established report evidence. Each recommendation contains:

- stable recommendation code;
- scope (`group` in V1);
- short recommendation;
- evidence summary using counts/percentages;
- coverage/caveat text.

Rules describe observed patterns only. They do not infer motivation, disability, family causes or diagnoses. They do not rank students. Examples include incomplete evaluation evidence, non-delivery patterns, students with `Requiere apoyo`, attendance patterns and opportunities to maintain strengths when evidence is adequate.

An external AI agent can consume `agent context` plus these transparent recommendations and add pedagogical reasoning, but the CLI itself remains offline.

## Error boundary

Expected input/domain/storage failures map to stable error codes and generic safe messages. Unexpected exceptions are written through the shared safe diagnostic contract with a terminal-command category. Raw exception messages and stack traces are never serialized in JSON output.

## Installation

The CLI is published self-contained for `win-x64` and packaged in the same Inno Setup installation. The executable is named `aularaiz.exe`. V1 does not mutate the user's global PATH automatically; documentation uses the stable installed path under `%LOCALAPPDATA%\Programs\AulaRaiz` and users/agents can create their own shell alias if desired.