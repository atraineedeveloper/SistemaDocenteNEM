# Proposal: group lifecycle, archive and safe deletion

## Why

AulaRaíz currently lets a teacher create and switch groups, but it has no lifecycle operation for a group that was created by mistake or that belongs to a completed school cycle. The only safe workaround is to leave the group in the normal picker forever.

Permanent deletion also cannot be treated as a plain `DELETE`: groups own or anchor students, attendance, projects, evaluation/delivery data, pedagogical records and NEM/context metadata. A mistaken destructive action therefore has a much larger impact than removing one row.

## What changes

- Add an explicit reversible group lifecycle state: **active** or **archived**.
- Keep archived groups out of the normal active-group picker and daily modules until they are restored.
- Show archived groups in a secondary `Archivados` section under `Mis grupos` with restore and delete actions.
- Allow active groups to be archived after a clear reversible confirmation.
- Add permanent deletion with risk-adaptive confirmation:
  - empty groups use a simple confirmation;
  - groups with associated data require typing the exact visible group name before deletion is enabled.
- Create an application-managed local safety backup before permanently deleting a group that contains data. If backup creation fails, deletion MUST NOT proceed.
- Delete the complete known relational group graph atomically in one SQLite transaction; failure rolls the transaction back.
- Migrate existing SQLite databases so every existing group starts as active.
- Clear or replace the locally selected group reference when the selected group is archived or deleted.

## Non-goals

- No cloud trash/recycle bin.
- No automatic archive at the end of a school year.
- No background retention policy for archived groups.
- No partial deletion of individual group modules.
- No recovery key or new backup format; the safety copy reuses the existing unprotected v1 application-managed backup behavior.

## Compatibility and risk

Existing group identities and associated classroom data remain unchanged by migration. The new SQLite column defaults legacy groups to active. Archive/restore changes only lifecycle state. Permanent deletion is intentionally irreversible in live storage, but data-bearing deletion is gated by a successful full local safety backup and typed confirmation.

The highest regression risks are accidentally exposing archived groups to normal editing, leaving relational rows orphaned, invalidating the current-group reference, or reporting a deletion as successful when only part of the graph was removed. Automated coverage must therefore include migration, archive/restore, active/archived queries, current-reference handling, complete relational deletion and rollback/safety-backup failure behavior.
