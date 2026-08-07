# Design: NEM project planning catalogs

## Context

`ProyectoDidactico` and `ActividadProyecto` are separate aggregates. SQLite stores their base data in `proyectos_didacticos` and `actividades_proyecto`; activity delivery history is preserved independently. The preceding multigrade change introduces structured `GradoPrimaria` values for students and group context.

This change adds pedagogical metadata without rebuilding the base project/activity tables or changing the existing aggregate boundaries.

## Decisions

### 1. Project methodology is structured but not prescriptive

Add `MetodologiaProyectoNem` with stable values:

- `NoEspecificada = 0`
- `ProyectosComunitarios = 1`
- `IndagacionSteam = 2`
- `AprendizajeBasadoEnProblemas = 3`
- `AprendizajeServicio = 4`

`NoEspecificada` is valid for rehydrated legacy data and may remain available when the teacher intentionally has not selected a methodology. The UI labels use the full Spanish SEP terminology.

The system does not enforce a formative-field-to-methodology mapping. It may present non-blocking guidance later, but professional teacher autonomy remains intact.

### 2. Every current activity can identify one formative field

Add `CampoFormativoNem` with stable values:

- `NoEspecificado = 0`
- `Lenguajes = 1`
- `SaberesPensamientoCientifico = 2`
- `EticaNaturalezaSociedades = 3`
- `DeLoHumanoYLoComunitario = 4`

New activity editing requires a real field before saving through the updated user flow. Rehydration accepts `NoEspecificado` so existing data is never guessed retroactively.

### 3. Grade targeting is explicit for new planning

Both project and activity aggregates expose a normalized, unique, ordered set of `GradoPrimaria` target grades.

- Project grades express the intended primary-grade scope of the project.
- Activity grades express the intended scope of that activity.
- For an explicitly targeted project, an activity cannot target a grade outside the project set.
- Empty target sets are accepted only as a legacy/unspecified compatibility state.

Application validates new/edited explicit target grades against the known grades represented in the classroom context or, when context metadata is unavailable, against real grades represented by current students. Presentation limits choices to configured group grades.

### 4. Activity roster follows target grades at creation time

For a newly prepared or created activity with explicit target grades, Application includes only active students whose `Estudiante.Grado` belongs to the activity target set.

Once the activity exists, its delivery roster remains historical and immutable in membership exactly as before. Later student grade changes or group reconfiguration do not rewrite old activity rosters.

Legacy activities with no explicit target grades keep their existing historical roster and are treated as applying to that roster.

### 5. Additive SQLite extension

Keep `PRAGMA user_version = 6` and add extension `nem-planeacion-proyectos`, version 1.

Tables:

- `proyectos_nem(proyecto_id PK, metodologia)`
- `grados_proyecto(proyecto_id, grado, PK(proyecto_id, grado))`
- `actividades_nem(actividad_id PK, campo_formativo)`
- `grados_actividad(actividad_id, grado, PK(actividad_id, grado))`

Foreign keys reference existing project/activity rows and use cascade only for metadata owned exclusively by the deleted project/activity record. This does not affect student history or delivery history.

Migration inserts one `NoEspecificada` project metadata row and one `NoEspecificado` activity metadata row for existing records. It does not populate target-grade tables because historical intent cannot be inferred safely.

### 6. Persistence keeps existing ports

`PersistenciaProyectosSqlite` continues implementing the existing project/activity storage ports. Reads join or query the side metadata tables; writes update the base aggregate and its NEM metadata in the same transaction.

Optimistic concurrency remains based on the existing base-row version. Metadata changes are part of the same aggregate save and therefore share the same expected version.

### 7. Application records remain backwards compatible

Extend `EntradaProyecto`, `ProyectoResumen`, `ProyectoDetalle`, `EntradaActividad`, `ActividadProyectoResumen` and `ActividadProyectoDetalle` with optional/defaulted pedagogical fields so existing call sites can compile during migration.

New UI flows always populate explicit values. Legacy call sites remain valid until migrated.

### 8. UX

Project editor adds:

- `Metodología de proyecto`
- `Grados objetivo`

Activity editor adds:

- `Campo formativo`
- `Grados objetivo`

For a unigrade group the only grade is selected automatically and grade targeting is visually quiet. For a multigrade group the grade selector is explicit.

Project and activity summaries display compact pedagogical metadata. Evaluation continues to derive applicability from the historical activity roster, so existing matrix behavior remains stable.

## Risks and mitigations

- **Historical metadata could be falsely inferred.** Keep migrated methodology/field unspecified and target-grade sets empty.
- **A student's grade may be missing in legacy data.** Explicit grade-targeted activity creation excludes students without a matching real grade and Presentation warns before save; legacy no-target flows remain readable.
- **Metadata could drift from base rows.** Store/update it in the same transaction as aggregate persistence.
- **Methodology may be mistaken for a mandatory SEP rule.** Documentation and UI describe the catalog as suggested project methodologies and do not auto-lock choices by formative field.

## Validation

- Core: enum validation, normalization and grade-set invariants.
- Application: target-grade roster construction and project/activity compatibility.
- Data: additive extension migration, round-trip metadata and rollback/concurrency.
- Presentation/WPF: catalog labels, unigrade default, multigrade selection and editor persistence.
- Full Windows CI and manual Demo validation before merge.