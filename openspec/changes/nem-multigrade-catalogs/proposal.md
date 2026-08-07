# Change: Structure school context, NEM phases and multigrade groups

## Why

The current application stores grade, group key, state, municipality and shift as free text. It also stores one manually selected Piaget stage for the whole group. This permits inconsistent data and does not represent multigrade primary classrooms correctly.

The product needs a structured school-context model before planning, NEM project metadata, import/export and backup features are expanded. The model must support one or several primary grades, derive NEM phases automatically, assign an individual grade to each student when needed, and distinguish classroom modality from whole-school organization.

## What Changes

- Add structured primary-grade values for grades 1 through 6.
- Derive NEM Phase 3, 4 or 5 from the selected primary grades instead of asking the teacher to choose a phase.
- Treat a group with one served grade as `Unigrado` and a group with multiple served grades as `Multigrado`.
- Store an individual primary grade for each student so a multigrade roster can distinguish students by grade.
- Replace free-text state selection with the 32 Mexican federal entities and filter municipalities by the selected entity using an offline catalog.
- Keep locality as free text.
- Add school-organization options: unitaria/unidocente, bidocente, tridocente, tetradocente, pentadocente and organización completa.
- Keep school organization separate from the derived unigrade/multigrade classroom modality.
- Replace the manually selected Piaget stage in the normal configuration workflow with a derived, explicitly non-diagnostic pedagogical reference based on the grades served.
- Preserve compatibility with existing SQLite v6 data through an additive schema extension and deterministic legacy-grade migration.
- Translate current engineering Markdown documentation to English while leaving `openspec/changes/archive/**` unchanged.

## Capabilities

### New Capabilities

- `school-context-nem-multigrade`: structured school geography, school organization, grades served, automatic NEM phases, student grade and developmental reference.

## Impact

- **Core:** structured grade/NEM/school-organization values and student grade.
- **Application:** student projections and use cases carry grade information.
- **Data:** additive extension tables preserve SQLite base schema version 6 and migrate unambiguous legacy grade values.
- **Presentation/WPF:** group configuration uses catalogs, grade selection and derived context; student editing exposes grade where appropriate.
- **Reporting:** continues to consume `ContextoGrupo`; legacy textual projections remain available during this compatibility stage.
- **Documentation:** all current Markdown outside archived OpenSpec changes is normalized to English.
- **Future work:** project methodology and activity formative-field metadata are intentionally deferred to the next NEM planning change.