# AulaRaíz

**Gestión docente para la Nueva Escuela Mexicana**

AulaRaíz is an offline-first Windows desktop application for the everyday work of a Mexican public-primary-school teacher. The product is designed around classroom workflows and the Nueva Escuela Mexicana (NEM), while keeping student data local to the teacher's computer.

The commercial product name is **AulaRaíz**. Existing technical solution/project names, local-storage folders and backup-format identifiers still use the historical `SistemaDocente*` / `SistemaDocenteNEM` identity where changing them would risk data or backup compatibility. See [`docs/branding.md`](docs/branding.md).

## Current product scope

The application already includes working modules for:

- group and student management;
- monthly attendance with keyboard and one-click cell capture;
- projects and activities;
- formative evaluation with explicit delivery semantics and a matrix workflow;
- longitudinal student records (`Expediente`);
- individual and group reports with teacher-initiated PDF output;
- group/school context;
- safe student import from XLSX/CSV with preview, correction and atomic commit;
- teacher-controlled group export to multi-sheet XLSX or focused CSV;
- safe local backup/restore with versioned recovery packages, pre-restore inspection and mandatory safety backup;
- Demo mode with isolated fictitious data;
- Light, Dark and High Contrast themes;
- automated Windows CI for formatting, Release build, tests, OpenSpec and whitespace validation.

The current product includes structured primary grades, automatic NEM phases, multigrade support, school organization, student grade, offline Mexico entity/municipality catalogs and structured NEM project planning. Projects can carry a NEM methodology and target grades; activities can carry one formative field and an explicit grade scope while preserving their historical student roster. Safe XLSX/CSV student import, group-data export, local backup/restore and PDF output for the existing individual/group reports are merged in `main`. Windows installation/update packaging is the current hardening change. See [`docs/nem-project-planning.md`](docs/nem-project-planning.md), [`docs/student-import.md`](docs/student-import.md), [`docs/group-export.md`](docs/group-export.md), [`docs/backup-restore.md`](docs/backup-restore.md), [`docs/pdf-reports.md`](docs/pdf-reports.md) and [`docs/installation-update.md`](docs/installation-update.md).

## Product principles

- **Offline first:** core classroom work must not depend on Internet access.
- **Teacher workflow first:** the UI should model the task a teacher performs rather than expose storage or implementation details.
- **NEM-aware without unnecessary rigidity:** official/stable concepts are structured; teacher-authored pedagogical content remains flexible.
- **No invented educational data:** migrations do not guess pedagogical meaning for legacy records.
- **Privacy by design:** real student information must never be committed to the repository.
- **Accessible desktop UX:** keyboard operation, semantic themes, high contrast and common display scaling are part of acceptance criteria.
- **Spec-driven development:** meaningful changes are defined in OpenSpec before implementation and validated before merge.

## Branding

The visible product identity is:

```text
AulaRaíz
Gestión docente para la Nueva Escuela Mexicana
```

The UI uses the compact `AR` monogram where an image asset is unnecessary. File-system-safe user-facing names use `AulaRaiz` without the accent, for example `AulaRaiz_Respaldo_Demo_...sdocbackup`.

The historical technical identity remains intentionally stable for compatibility:

- solution/project namespaces: `SistemaDocente.*`;
- production data folder: `%LOCALAPPDATA%\SistemaDocenteNEM\...`;
- Demo data folder: `%LOCALAPPDATA%\SistemaDocenteNEM-Demo\...`;
- backup package format id: `SistemaDocenteNEM.Backup`.

Those identifiers are compatibility contracts, not user-facing branding. Any later rename must include an explicit migration and backward-compatibility plan.

## Technology

- C# / .NET 10
- WPF for Windows
- SQLite
- xUnit
- OpenSpec
- Open XML SDK for XLSX interchange
- PDFsharp / MigraDoc for PDF report rendering
- Inno Setup 7 for the Windows installer
- Git / GitHub Actions

The solution is intentionally layered into Core, Application, Data, Presentation, Reporting and WPF projects so domain rules remain independent from SQLite and the desktop UI. The `SistemaDocente.Interchange` adapter project isolates XLSX/CSV syntax and PDF rendering from Application and WPF, while local recovery remains a separate SQLite/storage concern behind an Application recovery port.

## Windows delivery

The current installer work publishes AulaRaíz as a self-contained .NET 10 `win-x64` application and packages it with Inno Setup 7. The default installation is per-user under:

```text
%LOCALAPPDATA%\Programs\AulaRaiz
```

The installer owns program files and shortcuts only. Existing classroom data continues to live in the historical Production/Demo folders and is intentionally preserved during update and ordinary uninstall. SQLite migration remains application-owned; the installer does not execute database SQL.

The first installable semantic product version is `0.1.0`, and the same version metadata is shown in the AulaRaíz UI and consumed by the installer build. See [`docs/installation-update.md`](docs/installation-update.md) for the update, uninstall, signing and clean-machine validation contract.

## Repository workflow

Feature work follows this sequence:

1. explore the need and verify external requirements when necessary;
2. create an OpenSpec change;
3. define requirements, design decisions and implementation tasks;
4. implement on a dedicated branch;
5. add or update automated tests;
6. open a Draft pull request;
7. run Windows CI;
8. perform required manual UX/operational validation;
9. use Squash and merge once the change is accepted.

New technical documentation, OpenSpec content, branch names, pull-request text and commit messages are written in English. Existing Spanish UI/domain identifiers and historical technical names are preserved where renaming them would add risk without product value.

## Running Demo mode

From the repository root on Windows:

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo-reset
```

`--demo-reset` recreates the isolated fictitious Demo dataset. Production and Demo SQLite/application-state paths are separate. The Demo dataset includes structured grades, representative NEM project/activity metadata and enough attendance/evaluation history to validate import/export, PDF reporting and recovery workflows without real student data.

## Automated validation

The normal repository CI runs on Windows and verifies:

```powershell
dotnet restore SistemaDocente.sln
dotnet format SistemaDocente.sln --verify-no-changes --no-restore
dotnet build SistemaDocente.sln --configuration Release --no-restore
dotnet test SistemaDocente.sln --configuration Release --no-build
openspec validate --all
git diff --check
```

The installer workflow additionally publishes the self-contained WPF application, verifies/acquires the pinned Inno Setup compiler, builds an ephemeral older-version fixture plus the current development installer, performs a real version-to-version install/update/uninstall lifecycle check, proves that a sentinel in the historical user-data directory survives update and uninstall, and uploads both the normal installer artifact and a paired manual-upgrade validation artifact.

## Roadmap

The maintained roadmap is [`checklist_modulos_sistema_docente_nem.md`](checklist_modulos_sistema_docente_nem.md). PDF output for the existing reports is merged. The active hardening change is Windows installation/update packaging; privacy/local-security work remains pending as a separate module before broader production use.

## Data policy

Do not commit real student names, identifiers, health information, family information or other personal data. Tests and Demo mode use fictitious records only. Export files and PDF reports containing pedagogical observations or follow-up data must be stored/shared according to applicable personal-data handling requirements. `.sdocbackup` version 1 files can contain the complete local dataset and are not encrypted, so they must also be treated as sensitive files and stored only in appropriately protected locations.
