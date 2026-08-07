# Design: Structured school context and multigrade primary groups

## Goals

1. Remove avoidable free-text entry from stable school and NEM concepts.
2. Represent both unigrade and multigrade primary classrooms without creating artificial groups.
3. Make NEM phase a deterministic consequence of grade, never a separately editable field.
4. Preserve existing SQLite v6 data and avoid destructive table rebuilds.
5. Keep Piaget references explicitly pedagogical and non-diagnostic.
6. Keep the application usable offline.

## Domain model

### Primary grade

`GradoPrimaria` contains `NoEspecificado` plus Primero through Sexto. `NoEspecificado` is retained only for incomplete/legacy data; a fully configured group must select at least one real grade.

### NEM phase

`CatalogoNemPrimaria` owns the deterministic mapping:

| Primary grade | NEM phase |
| --- | --- |
| 1.º, 2.º | Phase 3 |
| 3.º, 4.º | Phase 4 |
| 5.º, 6.º | Phase 5 |

A multigrade group may therefore expose one or several phases. The phase collection is derived and is never persisted as an independently editable value.

### Group modality

Classroom modality is derived:

- one served grade → `Unigrado`;
- two or more served grades → `Multigrado`.

This is separate from `OrganizacionEscolar`, which describes the staffing/organization of the whole school.

### School organization

`OrganizacionEscolar` provides:

- No especificada;
- Unitaria / unidocente;
- Bidocente;
- Tridocente;
- Tetradocente;
- Pentadocente;
- Organización completa.

### Student grade

`Estudiante` carries `GradoPrimaria`. In a one-grade group, the UI may default new students to the only served grade. In multigrade groups, grade is explicit for each student. The domain must reject a configured student grade that is outside the group's served grades when the group-level operation has enough context to validate it.

## Developmental reference

Piaget stages are not NEM catalog values. They remain an optional pedagogical reference and MUST NOT be presented as a diagnosis.

The new workflow derives a reference set from primary grades rather than asking the teacher to classify the whole group manually:

- 1.º: transition between preoperational and concrete operations;
- 2.º–5.º: concrete operations as the principal general reference;
- 6.º: concrete operations with possible transition toward formal operations.

Multigrade groups display the union of applicable references. Existing `EtapaCognoscitiva` storage remains for compatibility, but new UI does not use it as a manual classification control.

## Geographic catalog

The application remains offline. Federal entities and municipalities are packaged as local catalog data. Entity selection filters municipality options. Locality remains free text because the complete locality catalog is much larger and changes more frequently.

The catalog provenance is INEGI's Catálogo Único de Claves de Áreas Geoestadísticas Estatales, Municipales y Localidades. A maintenance note documents how the packaged snapshot can be refreshed. Stable domain data stores the selected names; catalog implementation details remain outside Core.

## SQLite compatibility

Keep `PRAGMA user_version = 6`.

Add extension `nem-contexto-multigrado`, version 1, using the existing `esquema_extensiones` registry.

Tables:

```text
contexto_nem_grupo
- grupo_id PK/FK
- organizacion_escolar
- entidad_catalogo
- municipio_catalogo

grados_grupo
- grupo_id
- grado
- PK(grupo_id, grado)

grados_estudiante
- grupo_id
- estudiante_id
- grado
- PK(grupo_id, estudiante_id)
```

The existing `configuracion_grupo` table remains the compatibility projection for reports and older code. Structured saves write the textual `grado`, entity and municipality values there as well as the new extension tables.

### Legacy migration

When the extension is initialized:

1. inspect existing `configuracion_grupo.grado`;
2. parse only deterministic forms such as `4`, `4.º`, `Cuarto` and equivalent normalized variants;
3. when exactly one grade is resolved, add it to `grados_grupo` and assign it to students currently belonging to that group;
4. when the value is ambiguous or cannot be parsed, leave structured grades empty/unspecified and require user correction;
5. never guess a multigrade composition from an arbitrary group name.

Migration is transactional and idempotent.

## Presentation

`ConfiguracionGrupoWindow` is reorganized into:

- school identity and geography;
- school organization;
- grades served and group key;
- automatically derived classroom modality and NEM phases;
- developmental-reference summary;
- teacher responsibility and schedule.

State and municipality become ComboBoxes. Grades 1–6 use explicit multi-selection controls. NEM phase and modality are read-only derived information.

Student editing adds a grade selector. If the current group has exactly one configured grade, that value is preselected for new students.

## Documentation normalization

All current Markdown is written in English from this change forward, including README, root checklists, `docs/`, current specs and non-archived OpenSpec changes. Files under `openspec/changes/archive/` are historical artifacts and remain untouched.

## Explicitly deferred

This change does not yet add:

- NEM project methodology;
- formative field per activity;
- grade targeting per project/activity;
- Excel/CSV import;
- Excel/CSV export;
- backup/restore.

Those capabilities will build on the structured context introduced here.