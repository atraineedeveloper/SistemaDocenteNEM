# Design: shell navigation and reversible group creation

## Shell hierarchy

The desktop shell remains horizontal but uses two rows.

The first row owns product identity, current-group context and infrequent application utilities. The second row owns only teacher-work navigation. This keeps software maintenance actions out of the same visual hierarchy as attendance/evaluation work.

The primary teacher labels are **Resumen**, **Asistencia**, **Proyectos**, **Evaluación** and **Reportes**. `Resumen` maps to the existing group module; internal command/type names do not need a risky rename.

Backup/update live under a compact **Más** menu. Appearance remains a secondary utility menu. The installed version is removed from the high-emphasis brand cluster and remains available through application/about surfaces rather than competing with navigation.

## Group picker

The existing dynamic group picker remains the context switcher. Its footer contains:

1. `Mis grupos`
2. `Crear nuevo grupo…`

Switching an existing group continues through `MainWindowViewModel.CambiarGrupo`, preserving existing pending-change guards.

## Dedicated create-group route

`MainWindowViewModel` owns `MostrarCreacionGrupo` plus wizard-step state. Entering the route:

- first asks the current module whether it can be left;
- clears only the new-group wizard draft;
- does **not** call the historical welcome route;
- does **not** mutate the confirmed/current group;
- does **not** alter the underlying module-selection flags.

Because the underlying module state is preserved, cancellation can return the teacher to the exact prior context.

## Five-step setup wizard

The wizard is intentionally progressive and skippable after its first step:

1. **Grupo** — display name; this is the only required value.
2. **Grados** — optional 1.º–6.º selections. Zero selections are valid during initial setup.
3. **Escuela** — optional school name, CCT, school cycle and shift.
4. **Ubicación** — optional state, municipality and locality. Municipality choices depend on state when a state is selected.
5. **Confirmar** — summarize the entered values and explain that unspecified fields can be completed later.

Steps 2–4 expose `Omitir por ahora`. Back moves to the previous step; on step 1 Back behaves like Cancel and returns to the prior shell context. Cancel is always available and discards the entire draft.

The full group-configuration surface may retain stronger completeness validation for teachers who explicitly choose to configure a group. Initial creation uses a dedicated optional-save path so an empty grade/geographic setup does not become an accidental creation requirement.

## Reusing existing context persistence

The wizard does not add columns or a parallel metadata object. After the display-name group is created, supplied optional setup values are persisted as the existing `ContextoGrupo` associated with the new `GrupoId`.

`ConfiguracionGrupoViewModel` owns draft/reset and optional initial-save behavior so it can reuse:

- the existing Mexico state/municipality catalog;
- grade normalization and NEM projections;
- existing field-length/domain validation;
- `GestionContextoGrupoCasosUso` and the current SQLite context storage.

If all optional values are omitted, saving an otherwise empty `ContextoGrupo` remains valid and the teacher can complete it later.

## Successful creation

The shell owns the final-step command and invokes the existing group-creation operation only after the wizard reaches confirmation; the wizard button is not bound directly to the historical welcome surface. A successful create:

1. creates/selects the new group through the existing group business operation;
2. persists the wizard context for that new `GrupoId`;
3. closes the wizard;
4. navigates to the new group's **Resumen**.

The historical `MostrarBienvenida` state remains only as a compatibility/recovery fallback for absent/invalid stored references. Normal create-group navigation never exposes `Olvidar referencia`.

## Student optional birth date UX

`GestionGrupoViewModel.FechaNacimientoEdicion` and the underlying student data path already use nullable dates. The student editor therefore labels **Fecha de nacimiento** as optional and does not add a new validation rule. This is a presentation correction, not a data-contract change.

## Presentation composition

`CrearGrupoView` remains a shell-level WPF view but renders one wizard step at a time, a visible `Paso X de 5` indicator, Back/Cancel navigation and a final confirmation action. It uses existing theme tokens, `PrimaryButton`, standard controls and semantic automation labels.

While creation is active, primary shell navigation is hidden to avoid silently abandoning the draft through another route.

## Accessibility

- Back, Cancel, Skip, Next and Create are keyboard-focusable controls.
- Required versus optional fields are explicit in text, not color alone.
- The wizard exposes a textual step indicator and descriptive headings.
- Primary navigation state remains represented by text plus the existing active indicator, not color alone.
- The change reuses semantic Light/Dark/High Contrast brushes rather than fixed colors.
