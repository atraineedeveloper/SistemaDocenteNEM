# NEM project planning model

## Purpose

This document describes the structured New Mexican School (`Nueva Escuela Mexicana`, NEM) metadata used by Projects and Activities. The goal is to make stable planning concepts queryable without turning teacher-authored planning into a rigid template.

UI and domain labels remain in Spanish because they are teacher-facing concepts. Engineering documentation, specifications, branch metadata and code comments remain in English.

## Project methodology

Each `ProyectoDidactico` stores one `MetodologiaProyectoNem` value:

- `NoEspecificada` — compatibility value for historical records;
- `ProyectosComunitarios` — **Aprendizaje Basado en Proyectos Comunitarios**;
- `IndagacionSteam` — **Aprendizaje Basado en Indagación (STEAM como enfoque)**;
- `AprendizajeBasadoEnProblemas` — **Aprendizaje Basado en Problemas (ABP)**;
- `AprendizajeServicio` — **Aprendizaje Servicio (AS)**.

The methodology is an explicit teacher planning choice. The application does **not** infer or force a methodology from a formative field.

New project UI requires a specific methodology before save. Existing records migrated from older versions remain `NoEspecificada` until the teacher intentionally edits them.

## Formative field

Each `ActividadProyecto` stores one `CampoFormativoNem` value:

- `NoEspecificado` — compatibility value for historical records;
- `Lenguajes` — **Lenguajes**;
- `SaberesPensamientoCientifico` — **Saberes y Pensamiento Científico**;
- `EticaNaturalezaSociedades` — **Ética, Naturaleza y Sociedades**;
- `DeLoHumanoYLoComunitario` — **De lo Humano y lo Comunitario**.

A new activity requires a specific formative field. A historical activity may remain unspecified until edited; changing this metadata does not rewrite its historical roster.

## Target grades

Projects and activities can store an ordered unique set of `GradoPrimaria` target values.

### Project scope

A project may target one or more of the primary grades served by the active classroom. In a normal unigrade classroom, the only configured grade is preselected. In a multigrade classroom, the teacher explicitly chooses the intended subset.

### Activity scope

A new activity may target all or a subset of its project's explicit target grades. Application validates that an activity cannot introduce a grade outside the project's explicit scope.

Once an activity is created, its grade scope is treated as historical. The WPF editor therefore disables grade changes for existing activities. This prevents a later planning edit from silently adding or removing students from the activity's historical applicability.

## Historical roster semantics

For a new activity with explicit target grades, the initial roster contains exactly the students who are:

1. currently active in the group; and
2. assigned to one of the selected target grades.

After creation, the roster is historical. Later student activation/deactivation, grade changes or group-configuration changes do not rewrite it.

Evaluation continues to use the stored activity roster as the source of applicability. A student who was not part of an activity remains non-applicable (`—`) even if the student's current grade or group state changes later.

## Legacy compatibility

Historical project/activity rows created before this capability are migrated conservatively:

- project methodology → `NoEspecificada`;
- activity formative field → `NoEspecificado`;
- project target-grade set → empty;
- activity target-grade set → empty.

No historical pedagogical intent or grade scope is inferred from titles, descriptions or current classroom configuration.

An existing legacy activity with an empty target-grade set remains editable for normal metadata and evaluation work. Saving it does not require inventing historical target grades. New activities, by contrast, require an explicit target-grade selection.

## SQLite persistence

The base database stays at:

```text
PRAGMA user_version = 6
```

Planning metadata is stored in additive extension `nem-planeacion-proyectos = 1`:

```text
proyectos_nem
├── proyecto_id
└── metodologia

grados_proyecto
├── proyecto_id
└── grado

actividades_nem
├── actividad_id
└── campo_formativo

grados_actividad
├── actividad_id
└── grado
```

Metadata writes share the same SQLite transaction as the corresponding project or activity aggregate. A metadata failure therefore cannot leave the base aggregate partially updated.

The extension also self-heals metadata rows if a legacy build later inserts a base project/activity row after the extension was installed. Such rows still receive only unspecified metadata; grades are never guessed.

## Presentation behavior

`GestionProyectosViewModel` receives the active group's configured grades from the shared `ConfiguracionGrupoViewModel` context used by Group and Reports.

The project editor exposes:

- methodology;
- target grades;
- ordinary project text/date fields.

The activity editor exposes:

- formative field;
- target grades for new activities;
- ordinary activity text/date fields.

Project and activity lists show compact NEM metadata so the teacher can recognize scope without opening every editor.

## Demo mode

`--demo-reset` creates representative fourth-grade planning data with explicit student grades, project methodologies, formative fields and target grades. Demo remains isolated from production storage.