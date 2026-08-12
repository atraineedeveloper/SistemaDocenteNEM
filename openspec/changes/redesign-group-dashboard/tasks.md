# Tasks: redesign group dashboard

## 1. Presentation state

- [x] 1.1 Keep status-filter and ordering state local to the WPF student-list view so no persisted/domain state is introduced.
- [x] 1.2 Compose existing search, status filtering and ordering through the DataGrid `ICollectionView` without mutating source data.
- [x] 1.3 Preserve existing student selection and command rules.

## 2. Dashboard layout

- [x] 2.1 Rebuild the normal `GrupoView` hierarchy to match the approved reference language.
- [x] 2.2 Keep only Total and Activos metric cards; do not add average-age KPI.
- [x] 2.3 Integrate search, filter, ordering, Add and secondary data actions into the table card toolbar.
- [x] 2.4 Remove the permanent bottom action bar.
- [x] 2.5 Preserve group configuration and rename actions in the heading.

## 3. Student row interactions

- [x] 3.1 Add a visible `⋮` affordance to every row.
- [x] 3.2 Right-click selects the interacted row before opening student actions.
- [x] 3.3 `⋮` and right-click use the same menu-construction path.
- [x] 3.4 Contextual actions expose expediente, edit and the applicable activate/deactivate action.
- [x] 3.5 Double-click selects the row and opens expediente.

## 4. Accessibility and regression coverage

- [x] 4.1 Add WPF structure coverage for the new toolbar, two metric cards and removed footer.
- [x] 4.2 Cover search/filter/order wiring without adding a second persisted source of truth.
- [x] 4.3 Cover contextual-selection wiring and state-specific action labels structurally.
- [x] 4.4 Preserve Light/Dark/High Contrast semantic resource usage and automation labels.

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