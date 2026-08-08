# Proposal: privacy data map and safe logging baseline

## Why

AulaRaíz stores real classroom information ranging from student identity to attendance, formative evaluation and free-form pedagogical follow-up. The product already warns around several export/recovery boundaries, but it does not yet have one maintained classification map for those data flows.

The current WPF crash logger also persists the complete `.NET Exception.ToString()` value. Exception messages and stack traces can contain local paths or application values and therefore create an unnecessary privacy risk.

This change deliberately addresses only the first two privacy-hardening items:

1. inventory/classification of current data and secondary copies;
2. safe local diagnostics that never persist raw exception content.

Encryption, local application locking, retention/deletion policy and backup V2 are out of scope.

## What changes

- Add a maintained privacy data inventory covering SQLite, app state, backup/restore, export, PDF and diagnostic surfaces.
- Introduce product-engineering classifications D0–D3 and the rule that derived files inherit the highest classification they contain.
- Replace raw `crash.log` exception serialization with structured JSONL diagnostic events containing only predefined technical metadata.
- Keep Production and Demo diagnostics separated under their existing compatibility roots.
- Document the legacy `crash.log` risk without silently deleting existing user files.
- Add regression tests proving exception messages, stack traces and sensitive sentinel text are not persisted.
- Define privacy rules that a subsequent local CLI/agent interface must follow.

## Out of scope

- encryption of SQLite or `.sdocbackup`;
- PIN/password/local lock;
- automatic deletion/retention rules;
- external AI/network integrations;
- terminal/agent commands themselves (separate follow-up change);
- legal conclusions about the statutory classification of individual fields.