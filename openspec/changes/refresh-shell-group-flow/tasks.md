# Tasks: refresh shell navigation and group creation

## 1. Navigation state

- [ ] 1.1 Add a shell-level `MostrarCreacionGrupo` route and cancel command.
- [ ] 1.2 Entering create-group must preserve the confirmed group and current module state until submit.
- [ ] 1.3 Successful creation must close the route and open the new group's Resumen.
- [ ] 1.4 Back/Cancel must clear the draft and return to the exact previous shell context.

## 2. Group creation experience

- [ ] 2.1 Add a dedicated `CrearGrupoView` using the current group-name business rule/command.
- [ ] 2.2 Provide explicit `Volver`, `Cancelar` and `Crear grupo` controls.
- [ ] 2.3 Keep `Olvidar referencia` out of normal creation.
- [ ] 2.4 Preserve the existing `Mis grupos` empty state as the no-group landing surface.

## 3. Shell hierarchy

- [ ] 3.1 Split `MainNavigationHeader` into context/utilities and teacher-navigation rows.
- [ ] 3.2 Present primary navigation as Resumen, Asistencia, Proyectos, Evaluación and Reportes.
- [ ] 3.3 Move Respaldo and Actualizar under a compact secondary utility menu.
- [ ] 3.4 Keep appearance/theme controls secondary rather than primary teacher navigation.
- [ ] 3.5 Update the group picker footer to `Mis grupos` and `Crear nuevo grupo…`.
- [ ] 3.6 Adjust window/toast layout for the taller shell.

## 4. Regression coverage

- [ ] 4.1 Cover create-group launch from an existing group without clearing `GrupoIdActual`.
- [ ] 4.2 Cover Cancel returning to the exact prior module/group.
- [ ] 4.3 Cover Cancel from `Mis grupos` returning to `Mis grupos`.
- [ ] 4.4 Cover successful creation selecting the new group and routing to Resumen.
- [ ] 4.5 Add WPF structure coverage for the new primary labels and secondary utility placement where practical.

## 5. Validation

- [ ] 5.1 `dotnet restore SistemaDocente.sln -p:AuditPipeline=true`.
- [ ] 5.2 `dotnet format SistemaDocente.sln --verify-no-changes --no-restore`.
- [ ] 5.3 Release build with zero warnings/errors.
- [ ] 5.4 Full test suite with coverage.
- [ ] 5.5 `openspec validate --all`.
- [ ] 5.6 `git diff --check`.
- [ ] 5.7 Manual Demo validation: create/cancel from Resumen and another module; create from Mis grupos; group switcher; backup/update/theme utility menus.
- [ ] 5.8 Manual Light/Dark/High Contrast and common scaling smoke check for the refreshed shell.
