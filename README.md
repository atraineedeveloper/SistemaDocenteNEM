# Sistema Docente Local

Sistema Docente Local is an offline-first Windows desktop application for the everyday work of a Mexican public-primary-school teacher. The product is being designed around classroom workflows and the Nueva Escuela Mexicana (NEM), while keeping student data local to the teacher's computer.

## Current product scope

The application already includes working modules for:

- group and student management;
- monthly attendance with keyboard and one-click cell capture;
- projects and activities;
- formative evaluation with explicit delivery semantics and a matrix workflow;
- longitudinal student records (`Expediente`);
- individual and group reports;
- group/school context;
- safe student import from XLSX/CSV with preview, correction and atomic commit;
- group data export to multi-sheet XLSX or focused CSV on the current export feature branch;
- Demo mode with isolated fictitious data;
- Light, Dark and High Contrast themes;
- automated Windows CI for formatting, Release build, tests, OpenSpec and whitespace validation.

The current product includes structured primary grades, automatic NEM phases, multigrade support, school organization, student grade, offline Mexico entity/municipality catalogs and structured NEM project planning. Projects can carry a NEM methodology and target grades; activities can carry one formative field and an explicit grade scope while preserving their historical student roster. Safe XLSX/CSV student import is merged in `main`; the active export change reuses the isolated `SistemaDocente.Interchange` boundary to publish teacher-selected group data without modifying SQLite. See [`docs/nem-project-planning.md`](docs/nem-project-planning.md), [`docs/student-import.md`](docs/student-import.md) and [`docs/group-export.md`](docs/group-export.md).

## Product principles

- **Offline first:** core classroom work must not depend on Internet access.
- **Teacher workflow first:** the UI should model the task a teacher performs rather than expose storage or implementation details.
- **NEM-aware without unnecessary rigidity:** official/stable concepts are structured; teacher-authored pedagogical content remains flexible.
- **No invented educational data:** migrations do not guess pedagogical meaning for legacy records.
- **Privacy by design:** real student information must never be committed to the repository.
- **Accessible desktop UX:** keyboard operation, semantic themes, high contrast and common display scaling are part of acceptance criteria.
- **Spec-driven development:** meaningful changes are defined in OpenSpec before implementation and validated before merge.

## Technology

- C# / .NET 10
- WPF for Windows
- SQLite
- xUnit
- OpenSpec
- Open XML SDK for XLSX interchange
- Git / GitHub Actions

The solution is intentionally layered into Core, Application, Data, Presentation, Reporting and WPF projects so domain rules remain independent from SQLite and the desktop UI. The `SistemaDocente.Interchange` adapter project isolates XLSX/CSV syntax from Application and WPF for both import and export workflows.

## Repository workflow

Feature work follows this sequence:

1. explore the need and verify external requirements when necessary;
2. create an OpenSpec change;
3. define requirements, design decisions and implementation tasks;
4. implement on a dedicated branch;
5. add or update automated tests;
6. open a Draft pull request;
7. run Windows CI;
8. perform required manual UX validation;
9. use Squash and merge once the change is accepted.

New technical documentation, OpenSpec content, branch names, pull-request text and commit messages are written in English. Existing Spanish UI/domain identifiers are preserved where renaming them would add risk without product value.

## Running Demo mode

From the repository root on Windows:

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo-reset
```

`--demo-reset` recreates the isolated fictitious Demo dataset. Production and Demo SQLite/application-state paths are separate. The Demo dataset includes structured grades, representative NEM project/activity metadata and enough attendance/evaluation history to validate import/export workflows without real student data.

## Automated validation

The repository CI runs on Windows and verifies:

```powershell
dotnet restore SistemaDocente.sln
dotnet format SistemaDocente.sln --verify-no-changes --no-restore
dotnet build SistemaDocente.sln --configuration Release --no-restore
dotnet test SistemaDocente.sln --configuration Release --no-build
openspec validate --all
git diff --check
```

## Roadmap

The maintained roadmap is [`checklist_modulos_sistema_docente_nem.md`](checklist_modulos_sistema_docente_nem.md). Student import is merged. The active major change is XLSX/CSV group-data export; backup/restore remains the next separate recovery capability after export is accepted.

## Data policy

Do not commit real student names, identifiers, health information, family information or other personal data. Tests and Demo mode use fictitious records only. Export files containing pedagogical observations or follow-up data are intentionally opt-in and should be stored/shared according to applicable personal-data handling requirements.
