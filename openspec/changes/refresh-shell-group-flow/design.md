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

`MainWindowViewModel` owns a new `MostrarCreacionGrupo` route state. Entering the route:

- first asks the current module whether it can be left;
- clears only the new-group draft input;
- does **not** call `GestionGrupoViewModel.AbrirNuevoGrupoCommand`;
- does **not** mutate the confirmed/current group;
- does **not** alter the underlying module-selection flags.

Because the underlying module state is preserved, **Volver** and **Cancelar** can simply close the create route and return the teacher to the exact prior context.

If creation succeeds, `GestionGrupoViewModel.CrearGrupoCommand` loads the newly created group and raises `GrupoIdActual`. While the create route is active, `MainWindowViewModel` observes that change, closes the route and navigates to the existing group module, presented as **Resumen**.

When entered from `Mis grupos` with no active group, Cancel returns to `Mis grupos` rather than the historical welcome state.

## Legacy welcome/recovery state

The historical `MostrarBienvenida` state remains available only as a compatibility/recovery fallback inside `GestionGrupoViewModel` for absent/invalid stored references. The normal shell create route no longer exposes that view, so `Olvidar referencia` is not presented as a normal group-creation action.

A later cleanup may remove or redesign the legacy recovery surface once startup/reference recovery has its own explicit shell state.

## Presentation composition

A new WPF `CrearGrupoView` is rendered by `MainWindow.xaml` when `MostrarCreacionGrupo` is true. It uses existing theme tokens, `PrimaryButton`, standard controls and semantic automation labels.

While creation is active, primary shell navigation is hidden to avoid silently abandoning the draft through another route; the form itself provides Back/Cancel.

The shell header height increases to accommodate the two-row hierarchy. Toast placement must move below the new header.

## Accessibility

- Back, Cancel and Create are keyboard-focusable controls.
- The group name input receives a clear label and automation name.
- Primary navigation state remains represented by text plus the existing active indicator, not color alone.
- The change reuses semantic Light/Dark/High Contrast brushes rather than fixed colors.
