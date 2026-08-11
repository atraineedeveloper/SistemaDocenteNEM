# Tasks: refresh shell navigation and group creation

## 1. Navigation state

- [x] 1.1 Add a shell-level `MostrarCreacionGrupo` route and cancel command.
- [x] 1.2 Entering create-group must preserve the confirmed group and current module state until submit.
- [x] 1.3 Successful creation must close the route and open the new group's Resumen.
- [x] 1.4 Back/Cancel must clear the draft and return to the exact previous shell context.

## 2. Group creation experience

- [x] 2.1 Add a dedicated `CrearGrupoView` using the current group-name business operation/validation path.
- [x] 2.2 Provide explicit `Volver`, `Cancelar` and `Crear grupo` controls.
- [x] 2.3 Keep `Olvidar referencia` out of normal creation.
- [x] 2.4 Preserve the existing `Mis grupos` empty state as the no-group landing surface.

## 3. Shell hierarchy

- [x] 3.1 Split `MainNavigationHeader` into context/utilities and teacher-navigation rows.
- [x] 3.2 Present primary navigation as Resumen, Asistencia, Proyectos, Evaluación and Reportes.
- [x] 3.3 Move Respaldo and Actualizar under a compact secondary utility menu.
- [x] 3.4 Keep appearance/theme controls secondary rather than primary teacher navigation.
- [x] 3.5 Update the group picker footer to `Mis grupos` and `Crear nuevo grupo…`.
- [x] 3.6 Adjust window/toast layout for the taller shell.

## 4. Regression coverage

- [x] 4.1 Cover create-group launch from an existing group without clearing `GrupoIdActual`.
- [~] 4.2 Automated coverage proves Cancel returns to the prior Resumen/group; another-module return remains in manual acceptance.
- [x] 4.3 Cover Cancel from `Mis grupos` returning to `Mis grupos`.
- [x] 4.4 Cover successful creation selecting the new group and routing to Resumen.
- [x] 4.5 Add WPF structure coverage for the new primary label and secondary recovery utility placement.

## 5. Validation

- [x] 5.1 `dotnet restore SistemaDocente.sln -p:AuditPipeline=true`.
- [x] 5.2 `dotnet format SistemaDocente.sln --verify-no-changes --no-restore`.
- [x] 5.3 Release build with zero warnings/errors.
- [x] 5.4 Full test suite with coverage.
- [x] 5.5 `openspec validate --all`.
- [x] 5.6 `git diff --check`.
- [ ] 5.7 Manual Demo validation: create/cancel from Resumen and another module; create from Mis grupos; group switcher; backup/update/theme utility menus.
- [ ] 5.8 Manual Light/Dark/High Contrast and common scaling smoke check for the refreshed shell.

## Automated validation record

- Windows CI #380 on commit `134d8d7`: NuGet audit/restore, formatting, Release build, full tests with coverage, OpenSpec and whitespace all passed before this checklist-only update.
- Installer #110 on commit `134d8d7`: self-contained app/CLI/updater/installer build plus install/upgrade/uninstall lifecycle validation passed before this checklist-only update.
- PR #40 remains Draft until manual Demo UX/visual acceptance is completed.
