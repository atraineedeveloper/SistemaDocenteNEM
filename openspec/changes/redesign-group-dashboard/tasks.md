# Tasks: redesign group dashboard

## 1. Presentation state

- [ ] 1.1 Add status-filter and ordering state to `GestionGrupoViewModel`.
- [ ] 1.2 Compose search, status filter and ordering in `EstudiantesFiltrados` without mutating source data.
- [ ] 1.3 Preserve existing student selection/command rules.

## 2. Dashboard layout

- [ ] 2.1 Rebuild the normal `GrupoView` hierarchy to match the approved reference language.
- [ ] 2.2 Keep only Total and Activos metric cards; do not add average-age KPI.
- [ ] 2.3 Integrate search, filter, ordering, Add and secondary data actions into the table card toolbar.
- [ ] 2.4 Remove the permanent bottom action bar.
- [ ] 2.5 Preserve group configuration and rename actions in the heading.

## 3. Student row interactions

- [ ] 3.1 Add a visible `⋮` affordance to every row.
- [ ] 3.2 Right-click SHALL select the interacted row before opening student actions.
- [ ] 3.3 `⋮` and right-click SHALL use the same menu-construction path.
- [ ] 3.4 Contextual actions SHALL expose expediente, edit and the applicable activate/deactivate action.
- [ ] 3.5 Double-click SHALL select the row and open expediente.

## 4. Accessibility and regression coverage

- [ ] 4.1 Add WPF structure coverage for the new toolbar, two metric cards and removed footer.
- [ ] 4.2 Cover filter/order composition in Presentation tests.
- [ ] 4.3 Cover contextual-selection wiring and state-specific action labels structurally or behaviorally.
- [ ] 4.4 Preserve Light/Dark/High Contrast semantic resource usage and automation labels.

## 5. Validation

- [ ] 5.1 `dotnet restore SistemaDocente.sln -p:AuditPipeline=true`.
- [ ] 5.2 `dotnet format SistemaDocente.sln --verify-no-changes --no-restore`.
- [ ] 5.3 Release build with zero warnings/errors.
- [ ] 5.4 Full test suite with coverage.
- [ ] 5.5 `openspec validate --all`.
- [ ] 5.6 `git diff --check`.
- [ ] 5.7 Installer lifecycle validation.
- [ ] 5.8 Manual Demo review: populated and empty groups, search/filter/order, `⋮`, right-click, double-click, add/import/export, activate/deactivate.
- [ ] 5.9 Manual Light/Dark/High Contrast and common Windows scaling smoke check.