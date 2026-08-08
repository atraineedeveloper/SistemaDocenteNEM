# Privacy data inventory

This document is the maintained data map for AulaRaíz. It describes what the product currently stores or produces, how sensitive the information is **inside the product**, and where copies can exist.

The classifications below are product-engineering classifications. They are intentionally more operational than statutory labels and must not be read as a legal determination that every item in a class has the same legal status.

## Classification levels

| Level | Meaning | Examples |
| --- | --- | --- |
| D0 — public/technical | Product information that is not about a teacher or student. | AulaRaíz version, schema/package version, technical event category. |
| D1 — operational/pseudonymous | Classroom or application data that does not directly identify a person by itself but can become identifying when combined with local data. | Internal `GrupoId`/`EstudianteId`, list number, active-group state, counts. |
| D2 — personal/contextual | Data that identifies or describes a teacher, student, school or local context. | Student name, birth date, gender, teacher name, school/CCT/locality. |
| D3 — high-sensitivity educational | Content whose exposure could materially affect a student/family and therefore receives the strongest product safeguards. | Attendance history, achievement/evaluation, pedagogical observations, strengths/difficulties, support actions, tutor agreements and alerts. |

A file or output inherits the **highest** classification of the information it contains. A complete backup is therefore D3 even if its manifest also contains D0 metadata.

## Primary application data

### Group and student roster

Stored in SQLite under the active Production or Demo profile.

- `GrupoId`, `EstudianteId`: D1.
- group display name and student list number: D1/D2 depending on local naming practice.
- student display/full name, first/second surname and given names: D2.
- birth date and derived age: D2.
- gender: D2.
- admission date, active/inactive state and primary grade: D2.
- free-form student observations: D3.

AulaRaíz does **not** currently model CURP. Import does not map CURP into the product model.

### School/NEM context

Stored in SQLite by group.

- school year, served grades, NEM phases, organization, shift and schedule: D1/D2.
- school name and CCT: D2.
- state, municipality and locality: D2 because the combination can narrow the real-world school context.
- responsible teacher name and responsibility dates: D2.
- derived NEM/Piaget reference fields: D1 when detached from a person; they become D2 when attached to a real group context.

### Attendance

Stored in SQLite as historical daily rosters and states.

- date and internal roster identities: D1.
- present/absent/late/justified state tied to a student: D3.
- monthly counts and percentages tied to a student: D3.
- group-only aggregate counts without direct identifiers: normally D1, but should still be handled as internal classroom information.

### Projects, activities and formative evaluation

Stored in SQLite.

- project title, methodology, target grades, activity title/date and formative field: normally D1; teacher-authored text can contain D2/D3 content and must be treated accordingly.
- student delivery state and achievement level: D3.
- per-delivery/per-activity observations: D3.
- historical activity roster identities: D1, becoming D3 when joined to evaluation/delivery data.

### Student record (`Expediente`)

Stored in SQLite.

The following are D3:

- strengths;
- difficulties;
- applied support actions;
- chronological pedagogical observations;
- pedagogical alerts;
- tutor/family meeting reasons, agreements and follow-up context.

These fields are intentionally free-form enough that a teacher could enter additional personal information. They must never be copied into technical diagnostics.

## Local application state

`app-state.json` stores application reopen/navigation state such as the selected group identity. It is normally D1. It must not be expanded casually to contain names, observations or other D2/D3 content.

Production and Demo remain isolated under their historical compatibility roots:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\...
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\...
```

## Derived files and secondary copies

### `.sdocbackup`

Backup version 1 can contain the complete SQLite dataset plus application state. It is therefore D3 and currently **not encrypted**. The manifest contains D0/D1 metadata, but the package as a whole must be treated as D3.

### XLSX/CSV exports

Exports can contain roster, attendance, project/activity and evaluation data. Sensitive follow-up/observations require an explicit opt-in in the current export workflow. The resulting file inherits the highest classification of the selected datasets and can be D3.

AulaRaíz does not control the destination after the teacher saves an export.

### PDF reports

Individual and group PDF reports can contain direct identifiers, attendance/evaluation summaries and student-record evidence. They are D3. AulaRaíz warns before saving them; the destination is teacher-controlled.

### Safety backups created before restore

Managed restore-safety backups contain the current local dataset and are D3. They live under the corresponding Production/Demo application profile.

## Diagnostics and crash information

### Current safe diagnostics

New diagnostics are written as JSON Lines under:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\diagnostics\events.jsonl
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\diagnostics\events.jsonl
```

A diagnostic event may contain only D0 information:

- UTC timestamp;
- random event id;
- predefined event category;
- exception **type names only** (including the inner-exception type chain);
- a SHA-256 technical fingerprint derived from exception type/target-method metadata;
- AulaRaíz version;
- Production/Demo mode.

The persisted diagnostic format must **not** include:

- exception messages;
- stack traces;
- file-system paths;
- student/group ids;
- names;
- attendance/evaluation values;
- observations, notes, agreements or imported/exported rows;
- arbitrary caller-provided key/value metadata.

The last restriction is deliberate: an unrestricted logging dictionary would make it too easy for future code to leak classroom data.

### Legacy `crash.log`

Older AulaRaíz/SistemaDocente builds wrote the full `.NET Exception.ToString()` representation to `crash.log`. Existing installations may therefore already contain a legacy file with exception messages, stack traces or local paths. New builds stop appending raw exceptions to that file.

The application does not silently delete an existing legacy `crash.log`; automatic deletion could destroy information a user is intentionally retaining for troubleshooting. Treat an existing legacy file as potentially sensitive and remove/share it only deliberately.

## Installer, GitHub and release surfaces

The installed program directory under `%LOCALAPPDATA%\Programs\AulaRaiz` contains application/runtime binaries, not normal classroom storage.

GitHub source, CI, Actions artifacts and Releases must contain fictitious test/Demo data only. Real classroom databases, backups, exports, PDFs and diagnostics are never valid repository/release assets.

## Data-flow rules for future terminal/agent access

The planned local CLI/agent boundary must follow this inventory rather than bypass it:

1. agents call Application use cases; they do not query SQLite directly;
2. read-only/minimized output is the default;
3. stable internal ids and aggregates are preferred over names;
4. D2/D3 fields require an explicit command option when they are genuinely needed;
5. writes require an explicit apply/confirmation switch and reuse domain validation;
6. no CLI command sends data to a network service by itself;
7. diagnostics from CLI commands use the same D0-only safe logging contract;
8. recommendation outputs must identify their evidence/coverage and must not invent diagnoses or unsupported student facts.

A future external AI connector would require its own explicit data-minimization/consent boundary. The local CLI does not imply permission to upload classroom data anywhere.

## Review checklist when adding a field

For every new persisted/exported/report field, answer before implementation:

- What classification does the field have?
- Is it necessary for the teacher workflow?
- Where is the primary copy stored?
- Which backups/exports/reports inherit it?
- Could it enter diagnostics or error text?
- Does a terminal/agent projection need it, and can that projection be aggregated or pseudonymized instead?
- Does deletion/history behavior need to preserve it for an explicit pedagogical reason?

If those questions cannot be answered, the field is not ready to be added.