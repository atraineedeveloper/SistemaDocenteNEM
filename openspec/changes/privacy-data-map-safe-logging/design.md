# Design: privacy data map and safe logging baseline

## Scope boundary

This change is intentionally narrow. It provides the data map and a safe diagnostic primitive that later privacy and agent-access work can rely on. It does not attempt to solve encryption, user authentication or lifecycle policy in the same PR.

## Data classification

The maintained inventory uses four engineering levels:

- **D0** public/technical;
- **D1** operational/pseudonymous;
- **D2** personal/contextual;
- **D3** high-sensitivity educational.

These labels are product design controls, not legal classifications. Copies inherit the highest level of their contents. This makes it possible to reason consistently about SQLite, backups, exports, PDFs and future CLI projections.

## Diagnostic event shape

Persisted diagnostics use a closed schema rather than arbitrary logging properties. Each event contains:

- timestamp in UTC;
- random event id;
- predefined category;
- outer exception type;
- exception type chain;
- technical fingerprint;
- application version;
- Production/Demo mode.

The fingerprint is SHA-256 over exception type and target-method metadata. It intentionally excludes exception messages and stack traces. The fingerprint is for grouping technically similar failures, not for reconstructing exception content.

## Why no arbitrary metadata dictionary

A common logging design is `Log(category, properties)` with unrestricted key/value fields. AulaRaíz rejects that design for this baseline because future callers could accidentally add student names, observations, file paths or imported rows. New diagnostic fields require an explicit schema/code review.

## Storage location

Diagnostics stay inside the historical compatibility roots but outside SQLite:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\diagnostics\events.jsonl
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\diagnostics\events.jsonl
```

JSON Lines keeps append behavior simple and makes individual events independently inspectable. Diagnostic failure must never prevent AulaRaíz from running; file-write failures are therefore swallowed at the diagnostic adapter boundary.

## Legacy crash log

Existing `crash.log` files are not automatically deleted or migrated. Previous versions could have stored raw exception messages, stack traces and local paths. New code stops writing to that format. Documentation treats an existing file as potentially sensitive.

## Layering

- `SistemaDocente.Application` owns the safe diagnostic schema, categories and projection/fingerprint rules.
- `SistemaDocente.Data` owns the local JSONL file adapter.
- WPF selects Production/Demo mode and sends predefined event categories.

This split is intentional because the upcoming local CLI can reuse the same Application/Data diagnostic boundary without depending on WPF.

## Agent-access dependency

The next CLI/agent change will use this data map as a contract. Its default projections will favor D1 ids/aggregates, require explicit opt-in for D2/D3 fields, reuse Application use cases for writes, and never upload data by itself.