# System architecture

## Overview

Sistema Docente Local is an offline-first WPF desktop application for the everyday work of a primary-school teacher. The solution uses a layered architecture with manual composition. Domain rules and use cases remain independent from WPF and SQLite; concrete adapters are created in the application composition root.

The solution targets .NET 10. Portable production projects use `net10.0`; the WPF application uses `net10.0-windows`.

## Projects and responsibilities

| Project | Responsibility |
| --- | --- |
| `SistemaDocente.Core` | Domain entities, identities, invariants and domain exceptions. Includes groups/students, attendance, projects/activities, delivery/achievement, school context, NEM primary-grade/planning rules and student-record concepts. |
| `SistemaDocente.Application` | Use cases, persistence ports, snapshots and orchestration between aggregates. |
| `SistemaDocente.Data` | SQLite adapters, base-schema initialization, additive versioned extensions, queries and transactions. Technical exceptions are translated at the infrastructure boundary. |
| `SistemaDocente.Presentation` | Portable MVVM: ViewModels, commands, visual models, editable snapshots, filters, module boundaries and local catalog services. It does not reference WPF or SQLite. |
| `SistemaDocente.Reporting` | Pure report models and calculations for individual/group reports, attendance, delivery compliance and achievement distribution. |
| `SistemaDocente.App.Wpf` | WPF shell, module views, dedicated windows, themes, WPF services, application composition and isolated Demo mode. |

Test projects mirror the production layers: Core, Application, Data, Presentation and WPF. Reporting tests currently live in Application tests.

## Dependency direction

```text
Core
 ↑  ↑  ↑
 │  │  └── Reporting
 │  └───── Data
 └──────── Application
              ↑
         Presentation
              ↑
            App.Wpf
```

Relevant production references:

```text
Application  → Core + Reporting
Data         → Application + Core
Presentation → Application + Reporting
Reporting    → Core
App.Wpf      → Presentation + Application + Data
Core         → no other production project
```

There are no dependency cycles. Presentation never knows SQLite adapters.

## Domain model

### Group and student

`Grupo` is the aggregate responsible for its display name, roster, list numbers, active/inactive state and student consistency.

Each student can carry a structured `GradoPrimaria` value. The grade is especially important for multigrade classrooms. Legacy construction paths remain compatible with `NoEspecificado` while migration/configuration is completed.

### Structured school context and NEM primary phases

`ContextoGrupo` remains a 1:1 context associated with `GrupoId`, but structured values are the source of truth for stable concepts.

The context includes school year, school/CCT, federal entity, municipality/alcaldía, locality, school organization, one or more served primary grades, group key, shift, teacher responsibility and schedule.

NEM phase is derived and never independently editable:

```text
1.º–2.º → Phase 3
3.º–4.º → Phase 4
5.º–6.º → Phase 5
```

One served grade derives `Unigrado`; two or more derive `Multigrado`. This classroom modality is separate from whole-school `OrganizacionEscolar` (unitaria/unidocente, bidocente, tridocente, tetradocente, pentadocente or organización completa).

Piaget stages are not NEM catalog values. The application derives only a general pedagogical developmental reference from served grades and explicitly presents it as non-diagnostic.

### Attendance

`AsistenciaDiaria` is the attendance aggregate. Its natural identity is `GrupoId + DateOnly`. Each date is persisted independently and atomically.

The month is a projection, not an aggregate. Application builds an immutable monthly snapshot; Presentation keeps the confirmed snapshot plus an editable copy. There is no monthly database transaction.

### Projects and activities

`ProyectoDidactico` is an independent aggregate with group, date range, lifecycle state, description, observations, optimistic-concurrency version, one structured `MetodologiaProyectoNem` value and an ordered unique target-grade set.

`ActividadProyecto` belongs to a project/group and owns its historical student applicability plus delivery/evaluation entries. Activity + full applicable roster is the atomic save unit. It also stores one structured `CampoFormativoNem` and an ordered unique target-grade set.

A project's methodology is an explicit teacher choice and is not inferred from formative field. A new activity may target all or a subset of the project's explicit target grades. Its initial roster contains active students whose individual grade belongs to that scope. After creation, activity grade scope and roster are historical: later student/group changes do not rewrite applicability.

Legacy project/activity rows remain conservative: unspecified methodology/field and empty target-grade sets. No historical pedagogical intent is guessed. See [`nem-project-planning.md`](nem-project-planning.md) for the detailed contract.

### Delivery and achievement

`EntregaActividad` keeps two internal dimensions:

```text
EstadoEntregaActividad
├── Pendiente
├── Entregada
└── NoEntregada

NivelLogro
├── Pendiente
├── Domina
├── Suficiente
├── EnProceso
├── RequiereApoyo
└── NoEntrego  (legacy compatibility only)
```

Rules:

- new activity → `Pendiente + Pendiente`;
- `Entregada + Pendiente` means received but not yet evaluated;
- `NoEntregada` forces achievement to `Pendiente`;
- D/S/E/R forces `Entregada`;
- legacy `NivelLogro.NoEntrego` is normalized to `NoEntregada + Pendiente`.

Presentation exposes one teacher-facing evaluation result while preserving these two internal dimensions.

### Evaluation matrix

Evaluation reuses the historical `ActividadProyecto` roster. Rows are students present in at least one applicable activity; columns are activities; each cell contains delivery state, achievement and optional observation.

A historically non-applicable cell is `—` and cannot be edited. Compact states are `P`, `N`, `✓`, `D`, `S`, `E`, `R` and `—`.

Saving the matrix persists each changed activity sequentially; every activity keeps its own transaction and optimistic-concurrency semantics.

### Student record (`Expediente`)

The student record combines attendance, activities/evaluation and dedicated pedagogical entries. It stores strengths, difficulties, applied supports, chronological observations and tutor/family agreements. The workflow is formative and must not present pedagogical alerts as clinical diagnoses.

## Reporting

`SistemaDocente.Reporting` contains pure calculations. Application collects data through ports, builds report-source models and delegates calculations to Reporting.

Individual reports include identity/context, attendance, delivery compliance, achievement distribution, applicable project/activity history and student-record evidence.

Group reports include historical/active enrollment, aggregate attendance, delivery compliance, achievement distribution, monthly evolution and individual follow-up without competitive ranking.

Delivery compliance is:

```text
Delivered / (Delivered + NotDelivered) × 100
```

Pending delivery decisions are excluded from the denominator. No decided deliveries produces an undefined value (`—` in UI), not 0%.

## SQLite persistence

The base schema remains:

```text
PRAGMA user_version = 6
```

New capabilities use additive versioned extensions instead of destructively rebuilding a validated v6 database.

### Report/context/delivery extension

```text
esquema_extensiones
└── reportes-contexto-entregas = 1

configuracion_grupo
estados_entrega_actividad
```

The historical `entregas_actividad.estado_entrega` column temporarily stores `NivelLogro` for compatibility; explicit delivery state lives in the side table.

### NEM/multigrade context extension

```text
esquema_extensiones
└── nem-contexto-multigrado = 1

contexto_nem_grupo
grados_grupo
grados_estudiante
```

The existing `configuracion_grupo` table remains a compatibility/reporting projection. Structured saves write both the compatibility representation and the new extension atomically.

Legacy grade migration is conservative: deterministic values such as `4`, `4.º` or `Cuarto` may become structured fourth grade; ambiguous text is not guessed. A configured one-grade group can deterministically assign that grade to its students.

### NEM project-planning extension

```text
esquema_extensiones
└── nem-planeacion-proyectos = 1

proyectos_nem
actividades_nem
grados_proyecto
grados_actividad
```

Project methodology/grades and activity formative-field/grades are stored in side tables and written in the same transaction as their base aggregate. Legacy rows are initialized with unspecified metadata and empty grade sets; later legacy inserts are self-healed without guessing grades.

### SQLite principles

- `PRAGMA foreign_keys = ON`;
- parameterized queries;
- canonical persisted dates/times;
- explicit transactions for compound operations;
- optimistic concurrency where versions are exposed;
- relational group/project ownership constraints;
- no accidental deletion of pedagogical history;
- SQLite exceptions do not leak into Presentation/UI.

`app-state.json` stores only minimal reopen state; domain data lives in SQLite.

## Offline geographic catalog

Presentation embeds a local Mexico entity/municipality catalog. Entity selection filters municipality/alcaldía choices without Internet access. Locality remains free text. See [`geography-catalog.md`](geography-catalog.md) for provenance and maintenance guidance.

## Presentation and WPF composition

Presentation uses portable MVVM. ViewModels own selection, filters, editable state, confirmation boundaries and unsaved-change logic without WPF dependencies.

`MainWindowViewModel` coordinates global navigation. `App.xaml.cs` is the composition root: it creates SQLite adapters, use cases, ViewModels and WPF services; interprets Demo arguments; and wires shared group context across Group, Projects and Reports.

The shell uses one top navigation surface. Current modules are Group, Attendance, Projects, Evaluation and Reports. `Mis grupos` is the explicit group workspace; after opening a group, a compact context switcher allows fast changes while preserving unsaved-change guards.

Complex tasks remain in dedicated windows, including student editing, project/activity detail, student record, evaluation-cell detail and structured group configuration. Project/activity editors expose structured NEM planning catalogs and grade scope while project/activity summaries surface compact planning metadata.

## Themes, accessibility and density

The UI consumes semantic resources from `DesignTokens.xaml` and supports Light, Dark and High Contrast modes. Operational grids retain virtualization and own their scrolling. WPF views use `xml:lang="es-MX"`, automation names where useful and visible keyboard focus.

Attendance and Evaluation support both mouse and keyboard workflows. Direct cell menus optimize ordinary use while shortcuts preserve high-speed entry.

Manual acceptance includes common Windows scaling levels (100%, 125%, 150%) and supported themes.

## Demo isolation

Production and Demo data never share files:

```text
Production
%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json

Demo
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\app-state.json
```

`--demo-reset` deletes/recreates only Demo storage. The Demo dataset contains structured student grades plus representative project methodologies, formative fields and target grades.

## Engineering invariants

Every significant change should preserve these boundaries:

- Core does not know SQLite, WPF or file-system UI details.
- Application orchestrates use cases through ports.
- Data owns SQLite implementation details.
- Presentation stays portable.
- Reporting remains pure and infrastructure-independent.
- WPF owns desktop-specific interaction and composition.
- Database upgrades are conservative, tested and versioned.
- Pedagogical references do not become diagnoses.
- Official/stable NEM concepts may be structured; teacher-authored pedagogical content should not be over-constrained.