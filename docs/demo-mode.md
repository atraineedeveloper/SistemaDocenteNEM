# Demo mode

Demo mode allows the UI and classroom workflows to be reviewed with a rich fictitious dataset without writing to the teacher's production database.

## Run

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo
```

The first execution creates Demo data. Changes made during the session are retained so editing, saving and historical behavior can be tested.

## Reset fictitious data

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo-reset
```

`--demo-reset` deletes only Demo storage and seeds it again. The reset routine cannot target production paths.

## Storage isolation

Production:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json
```

Demo:

```text
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\app-state.json
```

The shell displays a `DEMO` badge and the window title also indicates Demo mode.

## Dataset

The fictitious dataset includes:

- `4.º A · Demostración` with 30 currently active students, one historical inactive student and one student admitted after a project began;
- `5.º B · Muestra` for group-switching behavior;
- fictitious school context for the main group: school year, school/CCT, Tabasco/Centro/Villahermosa, fourth grade, group A, morning shift, teacher and schedule;
- structured fourth-grade context, derived NEM Phase 4 and `Organización completa` school organization;
- the general developmental reference derived from fourth grade rather than a manually assigned diagnostic classification;
- July and August 2026 attendance with present, absent, late and justified-absence examples;
- one completed historical project;
- one in-progress project with nine activities and different historical rosters;
- one draft project;
- varied delivery states: Pending, Delivered and Not delivered;
- `Delivered + Pending evaluation` cases;
- varied achievement levels over delivered work: Domina, Suficiente, En proceso and Requiere apoyo;
- evaluation observations;
- pedagogical notes and one fictitious tutor agreement.

Names, observations and agreements are fictitious and exist only to test the application.

## Key Evaluation-matrix scenario

The first activities of `Periódico mural: voces de nuestra escuela` are created before `Ximena Torres Vidal` joins the group. Later activities include her. Evaluation should therefore show `—` in her first columns and editable cells in later columns.

The matrix uses:

```text
P  pending delivery decision
N  not delivered
✓  delivered, awaiting evaluation
D  Domina
S  Suficiente
E  En proceso
R  Requiere apoyo
—  not applicable to the historical roster
```

This scenario verifies that the matrix respects each activity's historical roster, does not add students retroactively and keeps delivery separate from achievement.

## Reports and group configuration

The main group ships with fictitious context so Reports has meaningful data from first launch. `Configurar grupo` can be opened from Group and Reports.

The structured configuration should show:

- Tabasco as the federal entity;
- Centro as a municipality from the dependent offline catalog;
- fourth grade selected;
- `Unigrado` derived automatically;
- `Fase 4` derived automatically;
- `Organización completa`;
- a non-diagnostic concrete-operations developmental reference.

After changing Evaluation results and saving, Reports should reflect delivered, not-delivered, pending-delivery, delivered-awaiting-evaluation and achievement-distribution data.

Run `--demo-reset` again to restore the original fictitious dataset.