# Proposal: NEM project planning catalogs

## Why

The project and activity modules already support lifecycle, historical rosters, delivery status and formative achievement, but their pedagogical metadata is still generic. A NEM-aware classroom needs structured project methodology, formative field and grade targeting, especially for multigrade groups.

The change must preserve existing projects and activities without inventing historical pedagogical metadata.

## What changes

- Add a structured project methodology catalog with the four SEP-suggested project approaches plus `No especificada` for legacy and intentionally unspecified records.
- Add the four NEM formative fields as a structured catalog for activities plus `No especificado` for legacy records.
- Add explicit target primary grades to projects and activities.
- Restrict new activity rosters to active students whose grade is targeted by the activity.
- Require activity target grades to be compatible with the containing project's target grades when those are explicitly configured.
- Preserve historical projects/activities by allowing legacy records to remain pedagogically unspecified until the teacher edits them.
- Persist the new metadata through an additive SQLite extension without changing `PRAGMA user_version = 6`.
- Expose the new metadata in project/activity editors and summaries without turning SEP methodological suggestions into mandatory field-to-method mappings.

## SEP alignment

The supported project methodologies are:

1. Aprendizaje Basado en Proyectos Comunitarios.
2. Aprendizaje Basado en Indagación (STEAM como enfoque).
3. Aprendizaje Basado en Problemas (ABP).
4. Aprendizaje Servicio (AS).

The supported formative fields are:

1. Lenguajes.
2. Saberes y Pensamiento Científico.
3. Ética, Naturaleza y Sociedades.
4. De lo Humano y lo Comunitario.

The application treats the four methodologies as structured suggestions, not as a closed recipe that limits teacher autonomy.

## Compatibility

- Existing project rows migrate to methodology `No especificada` and no explicit grade targeting.
- Existing activity rows migrate to formative field `No especificado` and no explicit grade targeting.
- Empty legacy target-grade sets mean `all students in the preserved historical roster`; they do not retroactively invent grade intent.
- New records created through the updated UI use explicit pedagogical metadata and grade targeting.

## Out of scope

- Content/PDA catalogs.
- Seven articulating axes.
- Rubrics or numeric grades.
- XLSX/CSV import/export.
- Backup/restore.
- Automatic selection of a methodology based on formative field.