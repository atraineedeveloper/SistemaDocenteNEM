# UX architecture addendum

## Scope

This addendum documents shell and presentation boundaries introduced by the group-workspace UX change. It does not replace `docs/architecture.md`; it records the new decisions in English without translating historical documentation in the same change.

## Shell state

`MainWindowViewModel` now distinguishes the landing workspace from module navigation.

```text
MainWindowViewModel
├── MostrarInicio
├── MostrarGrupo
├── MostrarAsistencia
├── MostrarProyectos
├── MostrarEvaluacion
└── MostrarReportes
```

`MostrarInicio` is mutually exclusive with the module surfaces. A group may already be loaded by `GestionGrupoViewModel` while the landing workspace is visible; this allows last-used-group continuity without entering a module implicitly.

The shell owns group-context switching because it already owns unsaved-change navigation guards. WPF controls request a switch through `MainWindowViewModel.CambiarGrupo(GrupoId)` instead of calling `GestionGrupoViewModel.CargarGrupoPorId` directly.

## WPF boundaries

### `InicioGruposView`

Presentation-only landing surface. It reads `Grupo.GruposDisponibles` and asks the shell to open the chosen group. It does not query Data or SQLite.

### `MainNavigationHeader`

Presentation-only context switcher and navigation control. It dynamically projects the available groups into menu items and delegates switching to the shell.

### Attendance and Evaluation grids

Cell menus remain WPF behavior because they are pointer/focus interactions. State mutation continues through Presentation view models:

- Attendance → `GestionAsistenciaMensualViewModel.AsignarEstado`.
- Evaluation → existing `EvaluacionActividadesViewModel` commands and `EvaluacionCeldaVisual`.

No persistence operations are performed by code-behind.

## Evaluation projection

`ResultadoEvaluacionVisual` is a Presentation concept. It intentionally does not enter Core or Data.

```text
teacher-facing Resultado
        ↓
EvaluacionCeldaVisual
        ↓
EstadoEntregaActividad + NivelLogro
        ↓
Application / Data
```

This keeps UI language task-oriented while preserving domain semantics and existing persistence compatibility.

## Expediente and Reporting boundary

Expediente remains editable state coordinated by Application and persisted by Data. Reporting remains a pure read-model/calculation boundary.

The WPF redesign changes information hierarchy only; it does not introduce a dependency from Expediente to Reporting or from Reporting to WPF.

## Dependency rule

The existing dependency direction remains unchanged:

```text
Core
↑       ↑          ↑
Application  Data  Reporting
↑       ↑          ↑
Presentation       │
↑                  │
App.Wpf ───────────┘
```

The UX change adds no new productive project reference and no schema migration.