# Sistema Docente NEM module roadmap

**Product:** offline Windows desktop application for a Mexican public-primary-school teacher, built with C#/.NET 10, WPF and SQLite.

This checklist reflects the current product state rather than the original prototype plan. A checked item means the capability is implemented in `main` unless it is explicitly marked as part of the current feature branch.

## Implemented foundation and classroom workflows

- [x] **Technical foundation**
  - Layered Core / Application / Data / Presentation / Reporting / WPF solution.
  - Local SQLite persistence and additive migrations/extensions.
  - Automated tests by layer.
  - OpenSpec-driven changes.
  - Windows GitHub Actions CI: format, Release build, tests, OpenSpec and whitespace.

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

- [x] **Demo mode and UX foundation**
  - Isolated Demo/production data paths.
  - `--demo` and `--demo-reset`.
  - Fictitious students, attendance, projects, activities, evaluation and follow-up data.
  - Group workspace (`Mis grupos`).
  - Light, Dark and High Contrast themes.
  - Shared popup styles and accessible keyboard patterns.

## Current stacked changes: structured school context and NEM planning

- [~] **School context, NEM phase and multigrade foundation** — PR #11 / `feature/nem-multigrade-catalogs`
  - [x] Structured primary grades 1.º–6.º.
  - [x] Automatic NEM phase mapping: 1.º–2.º → Phase 3; 3.º–4.º → Phase 4; 5.º–6.º → Phase 5.
  - [x] Unigrade/multigrade modality derived from served grades.
  - [x] School-organization catalog: unitaria/unidocente, bidocente, tridocente, tetradocente, pentadocente, organización completa.
  - [x] Individual student grade in the domain/persistence model.
  - [x] Offline Mexico entity/municipality catalog; locality remains free text.
  - [x] Derived, non-diagnostic Piaget developmental reference.
  - [x] Additive SQLite extension while keeping `PRAGMA user_version = 6`.
  - [x] Automated regression coverage and Windows CI.
  - [ ] Complete manual unigrade/multigrade UX validation.

- [~] **Structured NEM project/activity metadata** — PR #12 / `feature/nem-project-planning-catalogs`
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
  - [ ] Complete manual Demo UX validation and normalize the stacked branch after PR #11 merges.

---

# Planned modules and extensions

## 1. Richer NEM pedagogical planning

The structured methodology/formative-field/grade foundation is implemented in the current PR #12. Remaining planning depth includes:

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

The report calculation/model foundation already exists. Remaining work includes:

- [ ] Printable attendance report.
- [ ] Project completion report.
- [ ] Family-meeting summary.
- [ ] Print preview.
- [ ] PDF output.
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

- [ ] Import `.xlsx` and `.csv`.
- [ ] Preview before writing.
- [ ] Map source columns to application fields.
- [ ] Validate headers/data types and required fields.
- [ ] Detect probable duplicates.
- [ ] Allow corrections or row exclusion before commit.
- [ ] Use one transaction for the confirmed import.
- [ ] Show imported / skipped / needs-review counts.
- [ ] For a unigrade group, default missing grade to the group's only grade.
- [ ] For a multigrade group, require or resolve student grade explicitly.
- [ ] Never overwrite existing students without explicit confirmation.

## 11. Data export

- [ ] Export students to XLSX/CSV.
- [ ] Export attendance to XLSX/CSV.
- [ ] Export projects and activities.
- [ ] Export delivery/evaluation data.
- [ ] Export student follow-up where appropriate and authorized.
- [ ] Select period/content before export.
- [ ] Support a multi-sheet workbook for a complete group export.
- [ ] Exclude sensitive fields when the selected output purpose does not require them.

## 12. Backup and restore

Backup is intentionally separate from export.

- [ ] Manual backup.
- [ ] Optional automatic backup policy.
- [ ] Include SQLite database, application configuration and later evidence files.
- [ ] Version/validate backup packages.
- [ ] Restore only after explicit confirmation.
- [ ] Create a safety backup before restore.
- [ ] Display backup date/version/size.
- [ ] Detect incompatible or damaged backups.
- [ ] Preserve data across application upgrades.

## 13. Application settings

- [ ] Teacher profile defaults.
- [ ] School defaults reusable across groups.
- [ ] School-year defaults.
- [ ] Date/format preferences where needed.
- [ ] Evidence and backup folders.
- [ ] Version/diagnostic information.
- [ ] Configurable rules that never rewrite history silently.

## 14. Privacy and local security

- [ ] Personal-data inventory and classification.
- [ ] Sensitive-information warnings in relevant workflows.
- [ ] Optional local application lock.
- [ ] Backup protection/encryption strategy.
- [ ] Safe error logging without leaking student data.
- [ ] Controlled deletion/anonymization strategy.
- [ ] Retention policy.
- [ ] Review of sensitive observations/evidence workflows.

## 15. Installation and update

- [ ] Windows installer/package.
- [ ] Runtime/dependency strategy.
- [ ] Shortcuts and application identity.
- [ ] Safe SQLite migrations during updates.
- [ ] Installed-version display.
- [ ] Uninstall without accidental user-data deletion.
- [ ] Clean-machine installation tests.

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

1. [~] Complete manual validation and merge the structured school-context/multigrade foundation (PR #11).
2. [~] Normalize, manually validate and merge structured NEM project/activity metadata (PR #12).
3. [ ] Implement student XLSX/CSV import.
4. [ ] Implement data XLSX/CSV export.
5. [ ] Implement backup/restore before production use with irreplaceable real data.
6. [ ] Extend richer NEM planning fields, evaluation criteria/rubrics and reporting periods.
7. [ ] Add PDF/print report outputs.
8. [ ] Add teacher journal, family workflow, evidence attachments and calendar as prioritized.
9. [ ] Harden privacy, installer/update and long-term support workflows.

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
- [ ] Required manual UX validation is completed.
- [ ] Architecture/documentation is updated when behavior changes.
- [ ] The pull request is reviewed/audited as appropriate.
- [ ] The feature branch is merged into `main` using the agreed merge strategy.