# AulaRaíz

**Gestión docente para la Nueva Escuela Mexicana**

AulaRaíz is an offline-first Windows desktop application for the everyday work of a Mexican public-primary-school teacher. The product is designed around classroom workflows and the Nueva Escuela Mexicana (NEM), while keeping student data local to the teacher's computer.

The commercial product name is **AulaRaíz**. Existing technical solution/project names, local-storage folders and backup-format identifiers still use the historical `SistemaDocente*` / `SistemaDocenteNEM` identity where changing them would risk data or backup compatibility. See [`docs/branding.md`](docs/branding.md).

## Current product scope

The application includes working modules for:

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
- self-contained Windows installation/update packaging with version-to-version lifecycle validation;
- automated GitHub Releases for accepted version tags;
- consent-based in-app update discovery/download/verified restart in the `0.2.5` line;
- a maintained privacy data inventory and message-free local diagnostic stream;
- a local terminal/agent interface with dry-run-by-default mutations and transparent pedagogical recommendations;
- automated Windows CI for formatting, Release build, tests, OpenSpec and whitespace validation.

The NEM foundation includes structured primary grades, automatic phases, multigrade support, school organization, student grade, offline Mexico entity/municipality catalogs and structured project planning. Projects can carry a NEM methodology and target grades; activities can carry one formative field and an explicit grade scope while preserving their historical student roster.

See [`docs/nem-project-planning.md`](docs/nem-project-planning.md), [`docs/student-import.md`](docs/student-import.md), [`docs/group-export.md`](docs/group-export.md), [`docs/backup-restore.md`](docs/backup-restore.md), [`docs/pdf-reports.md`](docs/pdf-reports.md), [`docs/installation-update.md`](docs/installation-update.md), [`docs/releases.md`](docs/releases.md), [`docs/privacy-data-inventory.md`](docs/privacy-data-inventory.md) and [`docs/terminal-agent-cli.md`](docs/terminal-agent-cli.md).

## Product principles

- **Offline first:** core classroom work must not depend on Internet access.
- **Teacher workflow first:** the UI and automation surfaces model teacher tasks rather than storage implementation details.
- **NEM-aware without unnecessary rigidity:** official/stable concepts are structured; teacher-authored pedagogical content remains flexible.
- **No invented educational data:** migrations and recommendations do not guess unsupported pedagogical meaning.
- **Privacy by design:** real student information must never be committed to the repository, and automation output is minimized by default.
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
- Git / GitHub Actions / GitHub Releases

The product is layered into Core, Application, Data, Presentation, Reporting, Interchange, WPF and CLI hosts, plus the small standalone updater helper. Domain/application rules remain independent from SQLite and presentation hosts. `SistemaDocente.Cli` composes the same Application/Data contracts as WPF; its command handlers do not issue domain SQL directly. `AulaRaiz.Updater.exe` never opens SQLite.

## Windows delivery and in-app updates

AulaRaíz is published as a self-contained .NET 10 `win-x64` application and packaged with Inno Setup 7. The default installation is per-user under:

```text
%LOCALAPPDATA%\Programs\AulaRaiz
```

The installer owns program files and shortcuts only. Existing classroom data continues to live in the historical Production/Demo folders and is intentionally preserved during update and ordinary uninstall. SQLite migration remains application-owned; the installer does not execute database SQL.

GitHub Releases are the durable versioned download surface. A matching `vMAJOR.MINOR.PATCH` tag triggers quality gates, builds the installer, generates `SHA256SUMS.txt` and publishes both assets. Versions with major version `0` are prereleases.

The `0.2.5` line adds a teacher-controlled update experience. After the app is usable, AulaRaíz can check published Preview releases without blocking local work. A manual **Actualizar** action is also available in the header. When a newer version is found, the teacher can postpone it or download it inside AulaRaíz. The installer is not considered ready until its SHA-256 matches the published checksum.

After verification, **Cerrar y actualizar** invokes the normal pending-change protections, copies `AulaRaiz.Updater.exe` outside the install directory, closes WPF, re-verifies SHA-256, runs Inno Setup silently and reopens AulaRaíz in the same Production/Demo mode. Update checks never read or transmit classroom data. See [`docs/installation-update.md`](docs/installation-update.md).

## Terminal and agent interface

The installed CLI lives beside the desktop executable:

```powershell
$AulaRaiz = "$env:LOCALAPPDATA\Programs\AulaRaiz\aularaiz.exe"
& $AulaRaiz capabilities --json
& $AulaRaiz status --json
```

Agents should use `--json`. Personal names are omitted by default; supported read commands require `--include-personal-data` to include them. V1 agent context does not expose expediente notes, family agreements or other D3 free-form pedagogical text.

Mutations such as attendance changes and student deactivate/reactivate are dry runs unless `--apply` is explicit. There are no delete commands in V1. The CLI itself has no network integration: an external AI agent may consume its minimized JSON, but forwarding that JSON to a cloud service is a separate privacy boundary.

Transparent local recommendations are available through:

```powershell
& $AulaRaiz agent context --group <group-guid> --json
& $AulaRaiz agent recommend --group <group-guid> --json
```

Recommendations include the evidence and coverage/caveat that produced them; they do not diagnose, infer unsupported causes or rank students. See [`docs/terminal-agent-cli.md`](docs/terminal-agent-cli.md).

## Privacy and diagnostics

The maintained privacy map in [`docs/privacy-data-inventory.md`](docs/privacy-data-inventory.md) classifies current product data using D0–D3 engineering controls (not statutory legal labels) and maps SQLite, app-state, backups, exports, PDFs and diagnostic copies.

New diagnostics no longer persist raw `Exception.ToString()`. They write only a closed D0 technical schema (time/id/category/exception type chain/message-free fingerprint/app version/mode) to the corresponding Production/Demo `diagnostics/events.jsonl`. Existing legacy `crash.log` files are left untouched and should be treated as potentially sensitive.

The update client is a separate optional network surface: it requests only public GitHub Release metadata/assets and sends no student/group/school data. Update failures use the same D0-only safe diagnostics contract.

## Code signing policy

AulaRaíz is preparing an application to SignPath Foundation's free service for open-source projects. Releases are currently unsigned; the trusted-build submission step will be enabled only after acceptance and configuration with real SignPath identifiers.

**Free code signing provided by SignPath.io, certificate by SignPath Foundation**

See the [code signing policy](docs/code-signing-policy.md) for scope, roles, privacy, manual approval and incident handling. The [SignPath readiness review](docs/signpath-readiness.md) records verified dependencies and the provenance checks that must be closed before applying.

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

Then the CLI can read the same isolated fictitious dataset:

```powershell
dotnet run --project .\src\SistemaDocente.Cli\SistemaDocente.Cli.csproj -- groups list --demo --json
```

Production and Demo SQLite/application-state/diagnostic paths remain separate.

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

`SistemaDocente.App.Wpf.Tests` references the CLI and updater projects, so the solution quality gate restores/builds/tests both auxiliary hosts through the existing project graph. Installer CI publishes WPF plus both single-file helpers, upgrades from the verified published baseline, checks the installed versions, and proves ordinary uninstall removes program files while preserving user data.

The Release workflow starts only from a version tag, requires the tag to match `Directory.Build.props`, repeats the tagged-source quality gates, reuses the verified installer path, writes SHA-256 integrity metadata and publishes the installer/checksum pair through GitHub Releases.

## Roadmap

The maintained roadmap is [`checklist_modulos_sistema_docente_nem.md`](checklist_modulos_sistema_docente_nem.md). The privacy data-map/safe-logging baseline, installer/Release pipeline and local terminal/agent interface are already in `main`. The `0.2.5` feature line adds consent-based in-app update coordination. Backup encryption, local application lock and lifecycle/retention remain separate future privacy changes.

## License

Except where a file states otherwise, AulaRaíz source code and original project materials are licensed under the [GNU General Public License version 3.0 only](LICENSE) (`GPL-3.0-only`).

You may use, study, modify and redistribute the covered work under that license. A redistributed modified version must preserve the applicable GPL notices and make its corresponding source available under GPL v3. Third-party packages, frameworks, fonts, catalogs and other incorporated materials retain their own copyright and license terms.

This software is provided without warranty as described by the GPL. The software license applies to the program and repository materials; it does not grant rights over teacher, student, family or school data created or managed with AulaRaíz.

## Data policy

Do not commit real student names, identifiers, health information, family information or other personal data. Tests and Demo mode use fictitious records only. Export files, PDFs and `.sdocbackup` files can contain D3 educational/personal information and must be handled accordingly. A CLI response that omits names can still contain sensitive student-level educational evidence tied to stable internal ids; pseudonymization is minimization, not anonymization.