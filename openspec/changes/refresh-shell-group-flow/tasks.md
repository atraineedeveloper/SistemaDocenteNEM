# Tasks: refresh shell navigation and group creation

## 1. Navigation state

- [x] 1.1 Add a shell-level `MostrarCreacionGrupo` route and cancel command.
- [x] 1.2 Entering create-group must preserve the confirmed group and current module state until submit.
- [x] 1.3 Successful creation must close the route and open the new group's Resumen.
- [x] 1.4 Back/Cancel must clear the draft and return to the exact previous shell context.

## 2. Group creation experience

- [x] 2.1 Add a dedicated `CrearGrupoView` and keep stale-reference recovery out of normal creation.
- [ ] 2.2 Convert the create surface into five steps: Grupo, Grados, Escuela, Ubicación and Confirmar.
- [ ] 2.3 Require only the group display name and provide `Omitir por ahora` for optional steps.
- [ ] 2.4 Preserve draft values when navigating backward; Cancel discards the full draft.
- [ ] 2.5 Reuse existing grade, school, shift and Mexico geographic catalogs/bindings.
- [ ] 2.6 Persist supplied optional values through existing `ContextoGrupo` storage after group creation.
- [x] 2.7 Preserve the existing `Mis grupos` empty state as the no-group landing surface.
- [ ] 2.8 Correct the student editor to label date of birth as optional.

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
- [ ] 4.4 Cover first-step required-name validation and optional-step navigation/skip behavior.
- [ ] 4.5 Cover successful wizard creation selecting the new group, routing to Resumen and persisting optional context.
- [ ] 4.6 Cover creation with only a name and no grades/geography.
- [x] 4.7 Keep WPF structure coverage for primary navigation and secondary recovery utility placement.
- [ ] 4.8 Cover wizard structure/labels and optional birth-date wording.

## 5. Validation

- [ ] 5.1 `dotnet restore SistemaDocente.sln -p:AuditPipeline=true` on the wizard HEAD.
- [ ] 5.2 `dotnet format SistemaDocente.sln --verify-no-changes --no-restore`.
- [ ] 5.3 Release build with zero warnings/errors.
- [ ] 5.4 Full test suite with coverage.
- [ ] 5.5 `openspec validate --all`.
- [ ] 5.6 `git diff --check`.
- [ ] 5.7 Manual Demo validation: wizard cancel/back/skip/create from Resumen, another module and Mis grupos; group switcher; backup/update/theme utility menus.
- [ ] 5.8 Manual Light/Dark/High Contrast and common scaling smoke check for the refreshed shell/wizard.

## Previous automated validation record

- Windows CI #381 and Installer #111 passed for the pre-wizard shell-refresh HEAD `79cb967`.
- The wizard extension reopens automated validation items 5.1–5.6 and keeps PR #40 Draft until the new UX is manually accepted.
