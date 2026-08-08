# AulaRaíz module roadmap

**Product:** AulaRaíz — offline Windows desktop application for a Mexican public-primary-school teacher, built with C#/.NET 10, WPF and SQLite.

This checklist reflects the current product state rather than the original prototype plan. A checked item means the capability is implemented in `main` unless it is explicitly marked as part of the current feature branch.

## Implemented foundation and classroom workflows

- [x] **Technical foundation**
  - Layered Core / Application / Data / Presentation / Reporting / WPF solution.
  - Local SQLite persistence and additive migrations/extensions.
  - Automated tests by layer.
  - OpenSpec-driven changes.
  - Windows GitHub Actions CI: format, Release build, tests, OpenSpec and whitespace.
  - AulaRaíz visible product identity with legacy technical identifiers preserved for compatibility.

- [x] **Group and students**
  - Multiple groups.
  - Student create/edit/activate/deactivate.
  - Structured names, birth date, calculated age, gender, admission date and pedagogical observations.
  - Historical inactive students retained.
  - Student-record (`Expediente`) workflow.
  - Explicit exclusion of CURP from the current product model.

- [x] **Attendance**
  - Atomic daily attendance.
  - Monthly matrix.
  - Present, absent, late and justified-absence states.
  - P/F/R/J keyboard capture.
  - One-click cell state menu.
  - Counts, percentages and historical roster behavior.

- [x] **Projects and activities — operational foundation**
  - Project lifecycle: draft, in progress and completed.
  - Activities within projects.
  - Historical applicability by roster.
  - Student delivery/evaluation data.
  - Dedicated project/activity windows.

- [x] **Formative evaluation — operational foundation**
  - Student × activity matrix.
  - Delivery separated internally from achievement level.
  - Teacher-facing unified result interaction.
  - Pending, delivered-awaiting-evaluation, no-delivery, Domina, Suficiente, En proceso and Requiere apoyo.
  - Cell observations and keyboard shortcuts.
  - Filters and per-activity metrics.

- [x] **Student record (`Expediente`)**
  - Individual profile.
  - Strengths, difficulties, support actions and chronological observations.
  - Family/tutor agreements.
  - Attendance, activity/evaluation context and pedagogical follow-up.
  - No diagnostic labeling.

- [x] **Reports — first operational version**
  - Individual report.
  - Group report.
  - Attendance, delivery/compliance and achievement summaries.
  - Group context and student-record evidence.
  - No competitive student ranking.
  - Teacher-initiated PDF output for existing individual/group reports, merged via PR #21 after automated and manual validation.

- [x] **Demo mode and UX foundation**
  - Isolated Demo/production data paths.
  - `--demo` and `--demo-reset`.
  - Fictitious students, attendance, projects, activities, evaluation and follow-up data.
  - Group workspace (`Mis grupos`).
  - Light, Dark and High Contrast themes.
  - Shared popup styles and accessible keyboard patterns.

## Structured school context and NEM planning

- [x] **School context, NEM phase and multigrade foundation**
  - [x] Structured primary grades 1.º–6.º.
  - [x] Automatic NEM phase mapping: 1.º–2.º → Phase 3; 3.º–4.º → Phase 4; 5.º–6.º → Phase 5.
  - [x] Unigrade/multigrade modality derived from served grades.
  - [x] School-organization catalog: unitaria/unidocente, bidocente, tridocente, tetradocente, pentadocente, organización completa.
  - [x] Individual student grade in the domain/persistence model.
  - [x] Offline Mexico entity/municipality catalog; locality remains free text.
  - [x] Derived, non-diagnostic Piaget developmental reference.
  - [x] Additive SQLite extension while keeping `PRAGMA user_version = 6`.
  - [x] Automated regression coverage and Windows CI.
  - [x] Manual unigrade/multigrade UX validation completed before merge.

- [x] **Structured NEM project/activity metadata**
  - [x] Four structured project methodologies plus legacy `No especificada`.
  - [x] Four structured formative fields plus legacy `No especificado`.
  - [x] Project target grades.
  - [x] Activity target grades constrained by explicit project scope.
  - [x] New activity roster filtered by active students in target grades.
  - [x] Historical activity roster preserved after creation.
  - [x] Unigrade grade preselection and explicit multigrade selection.
  - [x] Additive SQLite extension `nem-planeacion-proyectos` while keeping `PRAGMA user_version = 6`.
  - [x] Demo data with explicit NEM planning metadata.
  - [x] Executable Demo seeder integration test against temporary SQLite storage.
  - [x] Automated regression coverage and Windows CI.
  - [x] Manual Demo UX validation and normalized merge completed.

---

# Planned modules and extensions

## 1. Richer NEM pedagogical planning

The structured methodology/formative-field/grade foundation is implemented in `main`. Remaining planning depth includes:

- [ ] Register project purpose and expected final product.
- [ ] Register curriculum content and learning-development processes (PDA).
- [ ] Register articulating axes.
- [ ] Register resources/materials and support/adaptation notes.
- [ ] Decide whether additional activity-level planning fields are useful without duplicating the project plan.
- [ ] Keep teacher-authored planning flexible rather than turning NEM guidance into a rigid recipe.

## 2. Evaluation criteria, rubrics and richer formative evidence

- [ ] Define criteria per project or activity.
- [ ] Add lightweight qualitative rubrics where useful.
- [ ] Link feedback to criteria/evidence.
- [ ] Preserve the distinction between non-delivery and not-yet-evaluated work.
- [ ] Expand progress views without reducing formative assessment to one opaque score.

## 3. Reporting periods and school grades

- [ ] Configure evaluation/reporting periods.
- [ ] Record or derive results by formative field.
- [ ] Make every calculated result traceable to its evidence.
- [ ] Allow justified teacher adjustments when policy/workflow requires them.
- [ ] Avoid misleading averages when evidence is incomplete.

## 4. Teacher journal / classroom log

- [ ] Daily entries.
- [ ] Link entries to projects, activities and students when appropriate.
- [ ] Record progress, difficulties, incidents and planning adjustments.
- [ ] Search/filter by date, project or student.
- [ ] Promote relevant observations into student follow-up.

## 5. Family communication and agreements

The current `Expediente` already stores tutor agreements. A future dedicated workflow may add:

- [ ] Meeting records and reasons.
- [ ] Commitments, responsible parties and follow-up dates.
- [ ] Pending-agreement view.
- [ ] Printable/exportable summary.
- [ ] Strong privacy controls for sensitive content.

## 6. School incidents and coexistence

- [ ] Objective incident record.
- [ ] Separate facts, interpretations and actions taken.
- [ ] People involved and follow-up.
- [ ] Optional links to applicable school protocols.
- [ ] Privacy controls.
- [ ] No automatic labels, sanctions or diagnoses.

## 7. Reports and output formats

The report calculation/model foundation already exists. PDF output for the established individual/group report models is merged; remaining outputs should add explicit report semantics rather than infer unsupported data.

- [ ] Printable attendance-only report.
- [ ] Project completion report.
- [ ] Family-meeting summary.
- [ ] In-app print preview / direct print workflow.
- [x] PDF output for the existing individual and group reports (PR #21).
- [ ] Period/formative-field reports once those modules exist.

## 8. Digital evidence attachments

- [ ] Attach photos/documents/student products.
- [ ] Link files to projects, activities and students.
- [ ] Store metadata in SQLite while keeping large files outside the database.
- [ ] Detect missing files and avoid unnecessary duplicates.
- [ ] Define size/storage limits.
- [ ] Apply personal-data safeguards.

## 9. School calendar and agenda

- [ ] Instructional days.
- [ ] Suspensions and School Technical Council dates.
- [ ] Events and project/activity dates.
- [ ] Upcoming tasks.
- [ ] Planning links.
- [ ] Never rewrite historical attendance automatically when the calendar changes.

## 10. Student import

**Status:** merged in `main` via PR #15 after manual functional validation.

- [x] Import `.xlsx` and UTF-8 `.csv`.
- [x] Preview before any SQLite write.
- [x] Map source columns only to supported student fields; CURP is excluded.
- [x] Validate headers/data types and required fields.
- [x] Detect hard list-number conflicts and deterministic probable duplicates.
- [x] Allow in-memory corrections, row exclusion and explicit `import as new` review decisions.
- [x] Resolve ambiguous CSV delimiters explicitly (comma / semicolon / tab).
- [x] Revalidate against a fresh group/context snapshot before commit.
- [x] Use one aggregate save / SQLite transaction for the confirmed import.
- [x] Prove rollback when a later SQLite insert fails.
- [x] Show ready / needs-review / invalid / excluded counts plus final imported/excluded counts.
- [x] For a unigrade group, default a missing grade to the group's only configured grade.
- [x] For a multigrade or unconfigured group, require explicit grade resolution.
- [x] Never overwrite, reactivate or deactivate existing students implicitly.
- [x] Keep imported students out of historical attendance/activity/evaluation rosters.
- [x] Keep raw workbook rows out of technical logs.
- [x] Manual functional validation completed with normal XLSX/CSV import, duplicate conflicts, invalid/review rows, multigrade resolution and ambiguous CSV delimiter selection.
- [~] Theme/scaling rechecks remain part of continuous UI quality rather than an import-specific merge blocker.

## 11. Data export

**Status:** merged in `main` via PR #17 after automated validation and manual opening of generated XLSX/CSV files.

- [x] Export students to XLSX/CSV.
- [x] Export attendance to XLSX/CSV.
- [x] Export projects and activities.
- [x] Export delivery/evaluation data.
- [x] Export student follow-up only through an explicit sensitive-content opt-in.
- [x] Select attendance period and project/content scope before export.
- [x] Support a multi-sheet workbook for a complete group export.
- [x] Exclude sensitive observations/follow-up by default and display a warning when enabled.
- [x] Write macro-free/value-only XLSX and formula-safe UTF-8 CSV.
- [x] Publish through a temporary sibling file so failed exports do not leave misleading partial destinations.
- [x] Reuse `SistemaDocente.Interchange` rather than putting XLSX/CSV syntax in WPF or SQLite.
- [x] Generate deterministic Windows-safe file-name suggestions from structured group context.
- [x] Validate representative Demo exports automatically and manually open generated XLSX/CSV before merge.

## 12. Backup and restore

**Status:** manual recovery version 1 merged in `main` via PR #18 after automated and manual Demo validation. Automatic scheduling, encryption, cloud integration and future evidence-file recovery remain separate work.

- [x] Create a manual `.sdocbackup` for the complete current Production or Demo storage profile.
- [x] Snapshot SQLite through its online backup API rather than copying a live DB/WAL directly.
- [x] Include valid application reopen state when available and warn/omit it when invalid or absent.
- [x] Version and validate backup packages with manifest metadata, bounded components and SHA-256 corruption checks.
- [x] Inspect/extract/prepare a selected package entirely outside live storage before restore.
- [x] Reject unsafe ZIP paths, duplicates, checksum mismatch, wrong Production/Demo mode and future/incompatible schema versions.
- [x] Prepare supported older database versions through current schema/additive-extension migration paths on an isolated copy.
- [x] Require the typed confirmation `RESTAURAR` before destructive work.
- [x] Create a mandatory safety backup of current live state before moving or deleting live files.
- [x] Stage live database/state/WAL/SHM under rollback names and attempt rollback on publication failure.
- [x] Display backup date, source mode, application/database version, included components and size before restore.
- [x] Shut down the application after successful restore so stale in-memory state cannot overwrite restored data.
- [x] Keep managed safety backups under the active Production/Demo application profile.
- [x] Warn that version 1 backups contain sensitive personal/pedagogical data and are not encrypted.
- [ ] Add an optional automatic backup policy in a later change.
- [ ] Add backup encryption/key-management strategy in a later privacy/security change.
- [ ] Include future external evidence files once the evidence module exists.
- [ ] Define an automatic retention/deletion policy for safety backups.

## 13. Application settings

- [ ] Teacher profile defaults.
- [ ] School defaults reusable across groups.
- [ ] School-year defaults.
- [ ] Date/format preferences where needed.
- [ ] Evidence and backup folders.
- [ ] Version/diagnostic information beyond the installed-version display.
- [ ] Configurable rules that never rewrite history silently.

## 14. Privacy and local security

- [ ] Personal-data inventory and classification.
- [~] Sensitive-information warnings in relevant workflows, including sensitive export/PDF/recovery boundaries.
- [ ] Optional local application lock.
- [ ] Backup protection/encryption strategy.
- [ ] Safe error logging without leaking student data.
- [ ] Controlled deletion/anonymization strategy.
- [ ] Retention policy.
- [ ] Review of sensitive observations/evidence workflows.

## 15. Installation and update

**Current feature branch:** `feature/windows-installer-update` / PR #23. Version 1 uses a per-user Inno Setup package and an explicit run-a-newer-installer update model; automatic update discovery/download remains separate future work.

- [~] Windows installer/package.
- [~] Self-contained .NET 10 `win-x64` runtime/dependency strategy.
- [~] Start Menu shortcut, optional desktop shortcut and stable installer identity.
- [~] Safe update boundary: installer replaces program files while SQLite migration remains application-owned.
- [~] Installed semantic-version display (`0.1.0` for the first installable line).
- [~] Ordinary uninstall without accidental user-data deletion.
- [~] Automated install/reinstall/uninstall lifecycle test with user-data preservation sentinel.
- [ ] Manual clean/non-development-machine installation and update validation.
- [ ] Production Authenticode signing workflow/certificate strategy before broad distribution.

## 16. Accessibility and UI quality — continuous work

- [~] Keyboard navigation and shortcuts.
- [~] Accessible labels/names.
- [x] Semantic Light/Dark/High Contrast resources.
- [~] Do not communicate state by color alone.
- [~] Windows scaling validation (100/125/150%).
- [ ] Small-resolution stress testing.
- [~] Clear, actionable error messages.
- [ ] Explicit loading/save-progress treatment for longer operations.
- [~] 30–40 student list/matrix stress testing.

---

# Recommended development sequence from the current state

1. [x] Merge structured school-context/multigrade foundation.
2. [x] Merge structured NEM project/activity metadata.
3. [x] Merge safe student XLSX/CSV import after functional manual validation (PR #15).
4. [x] Merge group data XLSX/CSV export after manual file-opening validation (PR #17).
5. [x] Merge safe local backup/restore after manual recovery validation (PR #18).
6. [x] Merge PDF output for the existing individual/group reports after manual rendering validation (PR #21).
7. [~] Complete Windows installation/update packaging and clean-machine validation (PR #23).
8. [ ] Harden privacy and local security (module 14; deliberately deferred while module 15 is completed).
9. [ ] Return to richer NEM planning fields, evaluation criteria/rubrics and reporting periods.
10. [ ] Add teacher journal, family workflow, evidence attachments and calendar as prioritized.

# Definition of done for a significant module/change

A change is considered complete when applicable items are satisfied:

- [ ] OpenSpec requirements/design/tasks accurately describe the final behavior.
- [ ] Core contains domain rules only.
- [ ] Application contains use cases/contracts rather than UI/SQLite details.
- [ ] Data does not leak SQLite details into upper layers.
- [ ] Presentation remains independent from WPF and Data.
- [ ] WPF workflow is usable with keyboard and supported themes/scaling.
- [ ] Automated regression coverage is sufficient for the risk introduced.
- [ ] `dotnet format --verify-no-changes` passes.
- [ ] Release build passes with zero warnings and zero errors.
- [ ] Full test suite passes.
- [ ] `openspec validate --all` passes.
- [ ] `git diff --check` passes.
- [ ] Required manual UX/operational validation is completed.
- [ ] Architecture/documentation is updated when behavior changes.
- [ ] The pull request is reviewed/audited as appropriate.
- [ ] The feature branch is merged into `main` using the agreed merge strategy.
