# UI and UX guidelines

**Status:** internal product standard  
**Scope:** `SistemaDocente.App.Wpf` and every future desktop workflow  
**Platform:** Windows Desktop / WPF / .NET  
**Goal:** keep the application modern, consistent, accessible, fast to operate and predictable during everyday teaching work.

## 1. Guiding principle

The application should feel like a purpose-built modern Windows tool, not an old administrative form and not a spreadsheet disguised as an application.

When priorities conflict, use this order:

1. Immediate comprehension.
2. Error prevention.
3. Operational speed.
4. Consistency.
5. Accessibility.
6. Visual refinement.
7. Information density.

If a visually attractive choice makes the workflow slower, less clear or less safe, choose clarity and safety.

## 2. Design references

The product draws from:

- Microsoft Fluent 2 principles;
- contemporary Windows desktop interaction patterns;
- Windows/WPF accessibility behavior;
- WCAG 2.2 as an additional accessibility reference;
- general usability principles: visible system state, consistency, error prevention, recognition over recall and user control.

WPF does not need to imitate WinUI component-for-component. Use the interaction and visual language that is practical without adding unnecessary UI dependencies.

## 3. Information architecture

- Global navigation appears only once.
- `Mis grupos` is the group-context entry point.
- A group is a workspace/context, not a secondary filter.
- Complex editing remains in focused windows rather than oversized master-detail screens.
- Reports are outputs/summaries; editable longitudinal evidence stays in `Expediente`.
- Domain hierarchy does not automatically become visual hierarchy.

## 4. Forms and configuration

Prefer structured controls when the value comes from a stable catalog or deterministic rule:

- entity/municipality → catalog selection;
- primary grade → structured grade selection;
- NEM phase → derived, read-only information;
- unigrade/multigrade → derived from grades served;
- school organization → explicit catalog;
- NEM methodology/formative field → future structured catalogs.

Keep teacher-authored content free enough for professional judgment. Do not turn pedagogical guidance into an arbitrary mandatory form.

### Field behavior

- Labels remain visible; do not use placeholder-only forms.
- Required fields are explicit.
- Validation appears next to or near the action/field and uses actionable language.
- Preserve the user's input after a correctable validation error.
- Use sensible defaults only when the result is deterministic.
- Never silently infer ambiguous pedagogical data.

## 5. Operational matrices

Attendance and Evaluation are high-density workflows and should behave as visual siblings.

- Freeze identifying columns when horizontal scrolling is required.
- Keep row/column headers visually distinct.
- Use hover, current-cell and focus affordances.
- Do not communicate state by color alone; include text/symbols.
- A normal cell click may open a compact action menu.
- Keyboard shortcuts remain available for high-speed capture.
- Full/detail editors are reserved for information that does not fit safely in the quick menu.
- Keep virtualized grid scrolling; never wrap a large operational `DataGrid` in an outer scrolling surface that defeats virtualization.

## 6. Keyboard behavior

Every frequent workflow should be usable without a mouse where practical.

- `Tab` order follows visual/logical order.
- `Esc` cancels/dismisses focused editing where safe.
- `Ctrl+S` saves when the window/module supports editable pending state.
- Contextual grid shortcuts run only while focus belongs to the intended grid and never while typing in a text editor.
- Visible keyboard focus is mandatory.

Existing domain-specific shortcuts include:

```text
Attendance: P / F / R / J
Evaluation: D / S / E / R / T / N / P
Evaluation detail: Enter / F2
Save: Ctrl+S
```

## 7. Themes and semantic resources

The supported themes are Light, Dark and High Contrast.

- Use semantic `DynamicResource` brushes/tokens.
- Avoid hardcoded foreground/background colors in views unless a documented technical reason requires them.
- Ensure contrast in every supported theme.
- Selected/hover/disabled/error/success states must remain distinguishable without relying only on hue.
- Shared popup patterns belong in shared style resources.

## 8. Typography and spacing

Use a small, deliberate hierarchy:

- page/window title;
- section title;
- form label;
- body/supporting text;
- compact metadata/eyebrow.

Do not make every container a card. Use grouping only when it clarifies structure. Prefer consistent spacing increments and align related controls.

## 9. Windows and dialogs

A dedicated window should have:

- clear title/purpose;
- concise supporting explanation where necessary;
- one visually dominant primary action;
- a predictable Cancel/Close action;
- reasonable minimum dimensions;
- scrolling only in the content region when needed;
- footer actions that remain reachable;
- accessible names for non-obvious controls.

Avoid opening a second dialog for a simple one-click state change when a contextual menu is safer and faster.

## 10. Accessibility

- Set `xml:lang="es-MX"` for Spanish UI surfaces.
- Provide `AutomationProperties.Name` when the visual label is insufficient for assistive technology.
- Ensure controls can be reached by keyboard.
- Maintain focus after operations where possible.
- Do not encode meaning only in color.
- Use plain, respectful language and avoid diagnostic labels not supported by the product's role.
- Piaget/developmental references are general pedagogical context, never an individual diagnosis.

## 11. Scaling and window sizes

Manual acceptance should cover at least:

- 100% scaling;
- 125% scaling;
- 150% scaling;
- Light theme;
- Dark theme;
- High Contrast.

Text must not clip at supported scaling. Operational content should reflow or scroll rather than overlap.

## 12. Feedback and errors

- Show saving/loading state for operations that are not effectively instantaneous.
- Success feedback should be quiet and non-blocking unless confirmation matters.
- Errors should explain what the user can do next.
- Never expose SQL, stack traces or local sensitive paths in user-facing messages.
- Unsaved-change guards must protect group switching, navigation and window close where edits could otherwise be lost.

## 13. Data privacy in UI

- Display only data needed for the current task.
- Avoid unnecessarily duplicating sensitive data across screens.
- Do not add fields merely because they exist on external institutional lists.
- Exports and future attachments must distinguish ordinary classroom information from sensitive information.
- Demo screenshots/test fixtures use fictitious data only.

## 14. Definition of UI done

A meaningful UI change is not ready to merge until applicable checks pass:

- behavior is covered by Presentation/WPF tests where stable structural assertions are useful;
- Windows Release build has zero warnings/errors;
- full tests pass;
- OpenSpec validates;
- whitespace check passes;
- no regression in keyboard shortcuts/unsaved-change protection;
- required Light/Dark/High Contrast and scaling checks are completed manually;
- the workflow is understandable without knowledge of the application's internal data model.