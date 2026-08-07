# Reports, group context and delivery semantics

This guide summarizes the functional and technical decisions around Reports, group context and the separation between delivery state and achievement level.

## 1. Group context

Group configuration belongs to `GrupoId`, not to an individual student. It can be completed progressively and is shared by Group and Reports.

Current structured context includes:

- school year;
- school name and CCT;
- federal entity and municipality/alcaldía from the offline Mexico catalog;
- free-text locality;
- school organization;
- one or more served primary grades;
- group key and shift;
- teacher responsibility period;
- entry/exit schedule.

### Grades and NEM phases

Primary grade is a structured value from first through sixth grade. NEM phase is derived and cannot be edited independently:

```text
1.º–2.º → Phase 3
3.º–4.º → Phase 4
5.º–6.º → Phase 5
```

One configured grade derives `Unigrado`; two or more derive `Multigrado`. A multigrade classroom may therefore expose more than one NEM phase.

School organization is a different concept and is selected independently:

- No especificada;
- Unitaria / unidocente;
- Bidocente;
- Tridocente;
- Tetradocente;
- Pentadocente;
- Organización completa.

### Developmental reference

Piaget stages are not modeled as NEM catalog values. The current UI no longer asks the teacher to classify the whole group manually. Instead it derives a general pedagogical reference from served grades and explicitly labels the information as non-diagnostic.

The same `ConfiguracionGrupoWindow` is available from Group and Reports. Both surfaces use the shared `ConfiguracionGrupoViewModel` instance composed by the WPF shell.

## 2. Delivery and achievement are separate dimensions

### Delivery state

```text
Pendiente
Entregada
NoEntregada
```

### Achievement level

```text
Pendiente
Domina
Suficiente
EnProceso
RequiereApoyo
```

`NivelLogro.NoEntrego` remains only for compatibility with legacy data/code. New workflows must not produce it.

Relevant valid combinations:

| Delivery state | Achievement | Meaning |
| --- | --- | --- |
| Pendiente | Pendiente | delivery has not been decided yet |
| Entregada | Pendiente | work was received but has not been evaluated yet |
| Entregada | Domina/Suficiente/EnProceso/RequiereApoyo | work was received and evaluated |
| NoEntregada | Pendiente | the work was recorded as not delivered |

Automatic rules:

- `NoEntregada` forces achievement to `Pendiente`;
- assigning D/S/E/R forces delivery to `Entregada`;
- non-delivery is never converted into zero or into an achievement level.

## 3. Evaluation matrix

The main view is student × activity; there is no separate activity selector.

Compact representation:

```text
P  pending delivery decision
N  not delivered
✓  delivered, awaiting evaluation
D  Domina
S  Suficiente
E  En proceso
R  Requiere apoyo
—  activity not applicable to the historical roster
```

### Teacher-facing unified result

The UI exposes one result instead of requiring the teacher to manipulate delivery state and achievement separately:

| Visible result | Internal delivery | Internal achievement |
| --- | --- | --- |
| Pendiente | Pendiente | Pendiente |
| Entregada · evaluar después | Entregada | Pendiente |
| Domina | Entregada | Domina |
| Suficiente | Entregada | Suficiente |
| En proceso | Entregada | EnProceso |
| Requiere apoyo | Entregada | RequiereApoyo |
| No entregó | NoEntregada | Pendiente |

Clicking a cell opens the quick result menu. `Más opciones…` opens the full editor, which uses the same unified result plus the pedagogical observation field.

### Keyboard shortcuts

When focus belongs to the matrix:

| Key | Action |
| --- | --- |
| `T` | Delivered, awaiting evaluation |
| `N` | Not delivered |
| `P` | Pending delivery decision |
| `D` | Domina + Delivered |
| `S` | Suficiente + Delivered |
| `E` | En proceso + Delivered |
| `R` | Requiere apoyo + Delivered |
| `Enter` / `F2` | Open full cell editor |
| `Ctrl+S` | Save pending changes |

## 4. Filters and metrics

The matrix can filter by delivered, not delivered, pending delivery, delivered awaiting evaluation, each D/S/E/R level, incidents and active/historical roster scope.

Selected-activity metrics include applicable total, pending delivery, delivered, not delivered, delivered awaiting evaluation and requires-support counts.

## 5. Reports

There is one global `Reportes` module with individual and group modes.

### Individual report

Includes:

- identity and structured group context;
- monthly/average attendance;
- delivery compliance;
- achievement distribution;
- applicable projects and activities;
- strengths;
- difficulties;
- supports;
- observations;
- tutor/family agreements.

### Group report

Includes:

- historical and active enrollment;
- aggregate attendance;
- delivery compliance;
- achievement distribution;
- monthly evolution;
- individual follow-up without competitive ranking.

### Delivery compliance

```text
Delivered / (Delivered + NotDelivered) × 100
```

Pending decisions are excluded from the denominator. If no delivery decision exists yet, UI shows `—`, not 0%.

## 6. SQLite compatibility

The validated base schema remains:

```text
PRAGMA user_version = 6
```

### Reporting/context/delivery extension

```text
esquema_extensiones
name: reportes-contexto-entregas
version: 1
```

Tables:

- `configuracion_grupo`;
- `estados_entrega_actividad`.

The historical `entregas_actividad.estado_entrega` column temporarily stores `NivelLogro`; the adapter performs combined reads and compatible writes.

Legacy delivery conversion:

```text
NoEntrego       -> NoEntregada + Pendiente
Pendiente       -> Pendiente + Pendiente
Domina/Suf/etc. -> Entregada + same achievement
```

### Structured NEM/multigrade extension

```text
esquema_extensiones
name: nem-contexto-multigrado
version: 1
```

Tables:

- `contexto_nem_grupo`;
- `grados_grupo`;
- `grados_estudiante`.

The original `configuracion_grupo` table remains a compatibility/reporting projection. Structured saves update it together with the additive extension.

Deterministic legacy grade values may be migrated to structured grades; ambiguous text is never guessed.

## 7. Legacy delivery compatibility

`EntradaEntregaActividad` distinguishes new calls that intentionally express `EstadoEntregaActividad` from older calls that supplied only `NivelLogro`.

When a legacy edit supplies `Pendiente` without an explicit delivery state, Application preserves the existing historical delivery value. Editing metadata therefore cannot accidentally erase a state such as `Entregada + Pendiente`.

## 8. Demo mode

Demo context is isolated from production and includes a structured fourth-grade group in Tabasco/Centro with morning shift, `Organización completa`, derived Phase 4, teacher responsibility and schedule.

## 9. Validation before merge

Windows CI must pass:

```powershell
dotnet restore SistemaDocente.sln
dotnet format SistemaDocente.sln --verify-no-changes --no-restore
dotnet build SistemaDocente.sln --configuration Release --no-restore
dotnet test SistemaDocente.sln --configuration Release --no-build
openspec validate --all
git diff --check
```

Manual validation should cover saving/reopening structured group context, unigrade/multigrade derived values, dependent municipality selection, student grade, Evaluation shortcuts/quick menus, Report consistency, Light/Dark/High Contrast and 100/125/150% scaling.