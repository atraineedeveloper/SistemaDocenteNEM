# mejora-ui-ux-global Specification

## Purpose

Define the behavior contract for the WPF application's global UI/UX improvement program, covering accessibility, design-system consistency, form validation, state feedback, theming, localization and overall product polish.

## Requirements

### Requirement: FOCUS_VISUAL_GLOBAL
The system SHALL define a global `FocusVisualStyle` in `App.xaml` that provides a visible, consistent focus indicator for interactive WPF controls.

#### Scenario: Visible focus on a button
- **WHEN** the user reaches a button with `Tab`
- **THEN** the button displays the global focus indicator

#### Scenario: Visible focus on a text field
- **WHEN** the user reaches a `TextBox` with `Tab`
- **THEN** the field displays a visible focus border/indicator consistent with the design system

### Requirement: CONTRASTE_WCAG_AA
The system SHALL ensure text and interactive elements meet a minimum 4.5:1 contrast ratio where applicable under WCAG AA guidance, replacing low-contrast colors identified during UX review.

#### Scenario: Text on a light background
- **WHEN** primary text is displayed on a light background
- **THEN** text/background contrast is at least 4.5:1

#### Scenario: Readable attendance states
- **WHEN** attendance states use semantic color
- **THEN** the associated text/icon remains readable with sufficient contrast

### Requirement: AUTOMATION_PROPERTIES_NAME
The system SHALL configure `AutomationProperties.Name` for non-textual elements that communicate information or actions, including icons, custom cards/cells, combo boxes and graphic buttons where the visible content is insufficient.

#### Scenario: Accessible icon name
- **WHEN** an icon represents an action or state
- **THEN** assistive technology can announce its descriptive automation name

#### Scenario: Accessible card name
- **WHEN** a screen reader reaches a meaningful content card
- **THEN** the card exposes a descriptive name when necessary

### Requirement: KEYBOARD_NAVIGATION
The system SHALL support keyboard operation for principal workflows with logical tab order and shortcuts for frequent actions such as save, cancel, add and close.

#### Scenario: Logical Tab navigation
- **WHEN** the user repeatedly presses `Tab` in an edit window
- **THEN** focus moves in logical visual/task order

#### Scenario: Save with Enter
- **WHEN** the user presses `Enter` in a valid form whose default action is Save
- **THEN** the default save action executes

#### Scenario: Cancel with Escape
- **WHEN** the user presses `Escape` in an editable dialog
- **THEN** the operation is cancelled/dismissed without saving when safe

### Requirement: LIVE_REGIONS
The system SHALL mark dynamic user messages such as toasts, edit messages and error banners as polite live regions when appropriate.

#### Scenario: Screen reader announces toast
- **WHEN** a success/error toast appears
- **THEN** assistive technology announces its message

#### Scenario: Screen reader announces contextual edit message
- **WHEN** the contextual edit message changes
- **THEN** assistive technology announces the new content without stealing focus

### Requirement: DESIGN_TOKENS
The system SHALL provide a centralized design-token `ResourceDictionary` for semantic colors, spacing, typography and related reusable visual values.

#### Scenario: Reuse a color token
- **WHEN** a style needs the primary application color
- **THEN** it references a design token instead of duplicating a hardcoded color

#### Scenario: Reuse a spacing token
- **WHEN** a reusable style defines standard spacing
- **THEN** it uses the design-system spacing value rather than introducing an arbitrary duplicate

### Requirement: DYNAMIC_RESOURCES
Theme-sensitive colors and other runtime theme resources SHALL use `DynamicResource` references rather than view-specific hardcoded theme values.

#### Scenario: Replace hardcoded theme color
- **WHEN** a view uses a color that belongs to the design system
- **THEN** it references a semantic resource

#### Scenario: Dynamic dialog theme
- **WHEN** a modal dialog loads
- **THEN** its theme-sensitive resources resolve from the active semantic theme

### Requirement: SEMANTIC_TYPOGRAPHY
The system SHALL use semantic typography styles such as `Heading1`, `Heading2`, `Heading3`, `FormLabel`, `Caption` and section-subtitle styles for recurring hierarchy rather than unrelated inline values.

#### Scenario: Semantic heading
- **WHEN** a section title is displayed
- **THEN** it uses the appropriate semantic heading style

#### Scenario: Semantic form label
- **WHEN** a field label is displayed
- **THEN** it uses the shared form-label style

### Requirement: FLUENT_ICONS
Functional iconography SHOULD use stable vector paths or appropriate Fluent/Segoe icon glyphs instead of emoji where emoji rendering would be inconsistent, and functional icons SHALL have an accessible name when necessary.

#### Scenario: Stable action icon
- **WHEN** a critical action requires iconography
- **THEN** the chosen icon renders consistently on supported Windows environments

#### Scenario: Accessible icon
- **WHEN** assistive technology reaches a functional icon control
- **THEN** it can announce a descriptive accessible name

### Requirement: UNIFIED_MARGENS_VENTANA
Primary windows and dialogs SHALL use a consistent content-spacing system, with 24 logical units as the normal root spacing where the composition allows it.

#### Scenario: Consistent main-window spacing
- **WHEN** a standard page is displayed
- **THEN** its content follows the shared root-spacing pattern

#### Scenario: Consistent modal spacing
- **WHEN** a dedicated dialog is displayed
- **THEN** its header/content/footer spacing remains consistent with shared popup patterns

### Requirement: REUSABLE_COMPONENTS
The system SHALL reuse components for recurring interface structures, including `FormField`, metric-card patterns and `EmptyState`, rather than duplicating complex XAML unnecessarily.

#### Scenario: Reusable form field
- **WHEN** a field requires label/content/validation structure
- **THEN** `FormField` or an equivalent shared pattern is used where appropriate

#### Scenario: Reusable empty state
- **WHEN** a list/grid has no data
- **THEN** a consistent empty-state component/pattern communicates what the user can do next

### Requirement: DATA_ERROR_INFO
Presentation SHALL support property-level validation notification through `INotifyDataErrorInfo` or an equivalent reusable validation mechanism where asynchronous or multi-property validation requires it.

#### Scenario: Validation error
- **WHEN** an invalid field value is detected through that mechanism
- **THEN** the ViewModel reports the error for the relevant property

#### Scenario: Error cleared
- **WHEN** the user corrects the invalid value
- **THEN** the ViewModel clears the property error and notifies the UI

### Requirement: VALIDATION_ERROR_TEMPLATE
The system SHALL provide a consistent validation-error visual treatment with a distinctive field state and contextual error text where validation is field-specific.

#### Scenario: Field with visual error
- **WHEN** a ViewModel property reports a validation error
- **THEN** the corresponding field presents a visible error state

#### Scenario: Contextual error message
- **WHEN** validation fails
- **THEN** the message explains which input is invalid and why without relying only on a generic dialog

### Requirement: DATE_PICKER_FECHAS
User-facing forms SHALL use `DatePicker` for normal date selection instead of plain free-text date fields where WPF date selection is appropriate.

#### Scenario: Select a date
- **WHEN** the user must enter a normal calendar date
- **THEN** the form provides a localized `DatePicker`

#### Scenario: Invalid date
- **WHEN** a date is outside an allowed range or otherwise invalid
- **THEN** the form shows contextual validation feedback

### Requirement: EMPTY_STATES_DATAGRID
Lists and grids SHALL communicate meaningful empty states rather than leaving an unexplained blank surface.

#### Scenario: Group with no students
- **WHEN** a group has no registered students
- **THEN** the student area explains that the first student can be added

#### Scenario: Project with no activities
- **WHEN** a project has no activities
- **THEN** the activity area presents an explanatory empty state

### Requirement: NOTIFICATION_SERVICE
The system SHOULD centralize transient success/warning/error notifications in a consistent notification service/pattern that is accessible to assistive technology.

#### Scenario: Success notification
- **WHEN** an operation benefits from explicit success feedback
- **THEN** the application presents a quiet, consistent success notification

#### Scenario: Error notification
- **WHEN** a recoverable domain/infrastructure error occurs
- **THEN** the application presents an actionable error message and retry action when applicable

#### Scenario: Accessible notification
- **WHEN** a transient notification appears
- **THEN** assistive technology can announce it without inappropriate focus theft

### Requirement: PROGRESS_BUSY
The UI SHALL show a clear busy/progress indication when a ViewModel reports an operation long enough for the user to perceive waiting.

#### Scenario: Data-loading operation
- **WHEN** a prolonged loading operation starts
- **THEN** the affected surface shows a progress/busy indicator and prevents conflicting actions

#### Scenario: Operation completes
- **WHEN** the operation finishes
- **THEN** the busy indicator disappears and normal interaction is restored

### Requirement: CUSTOM_DIALOGS
Destructive confirmations and important application messages SHOULD use product-consistent dialogs when the native `MessageBox` would provide insufficient styling, context or action clarity.

#### Scenario: Destructive confirmation
- **WHEN** a destructive action requires confirmation
- **THEN** the dialog clearly identifies the action and presents primary/secondary choices

#### Scenario: Critical error
- **WHEN** a blocking error requires acknowledgement
- **THEN** the dialog presents an understandable message without technical internals

### Requirement: SUBTLE_ANIMATIONS
Animations, when used, SHALL be subtle, short and non-essential to understanding. Typical interactive transitions SHOULD remain approximately 150–250 ms and MUST NOT block keyboard operation.

#### Scenario: Hover transition
- **WHEN** a pointer enters an animated interactive surface
- **THEN** visual transition is brief and unobtrusive

#### Scenario: Dialog transition
- **WHEN** a dialog uses an entrance animation
- **THEN** the animation is short and the dialog becomes usable promptly

### Requirement: THEME_DICTIONARY
The system SHALL support at least Light, Dark and High Contrast visual themes through semantic resources.

#### Scenario: Change to Dark theme
- **WHEN** Dark theme is activated
- **THEN** theme-sensitive controls update consistently without requiring application restart where runtime switching is supported

#### Scenario: High Contrast
- **WHEN** High Contrast is used
- **THEN** the application provides readable, distinguishable semantic states

### Requirement: LOCALIZED_RESOURCES
User-visible strings SHOULD be structured so localization can be introduced without rewriting workflow logic. Strings that require reuse or future localization SHOULD live in resources instead of being duplicated across code.

#### Scenario: Shared static text
- **WHEN** a shared user-visible phrase is reused across surfaces
- **THEN** it can be sourced from a common resource rather than independently duplicated

#### Scenario: Shared error text
- **WHEN** a reusable user-facing error is presented
- **THEN** the message can be centralized without exposing technical exception text

### Requirement: XML_LANG
Each principal WPF window/view SHALL declare an appropriate `xml:lang`, normally `es-MX`, so assistive technology interprets the Spanish UI correctly.

#### Scenario: Language declared on MainWindow
- **WHEN** `MainWindow.xaml` loads
- **THEN** its root declares `xml:lang="es-MX"` or the active UI language

#### Scenario: Language declared on dialog
- **WHEN** a modal dialog loads
- **THEN** its root declares the active UI language

### Requirement: SORT_COLUMNS
Data grids SHALL enable user sorting only where sorting adds value and does not break the operational meaning of the matrix/list.

#### Scenario: Sort by name
- **WHEN** a sortable student-name header is activated
- **THEN** the rows are ordered by that field

#### Scenario: Sort by list number
- **WHEN** a sortable list-number header is activated
- **THEN** rows are numerically sorted

### Requirement: SEARCH_STUDENTS
The student roster SHALL provide search/filtering by relevant identifiers such as name, list number and, when structured multigrade data is available, grade.

#### Scenario: Filter by name
- **WHEN** the user types a student name fragment
- **THEN** the roster shows matching students

#### Scenario: No results
- **WHEN** no students match the query
- **THEN** the interface communicates that no matches were found

### Requirement: DYNAMIC_TITLE
The main-window title SHOULD provide useful current context such as active group or Demo state when doing so improves orientation.

#### Scenario: Active-group title
- **WHEN** a group is active
- **THEN** the window title may include that group context

#### Scenario: Evaluation context
- **WHEN** the user works in Evaluation
- **THEN** the title/context still makes the active application/group understandable

### Requirement: BREADCRUMB_DIALOGS
Nested workflows MAY use breadcrumbs when they materially improve orientation; breadcrumb navigation SHALL NOT be introduced when a dedicated-window title and parent context are clearer or safer.

#### Scenario: Activity detail context
- **WHEN** an activity detail window needs parent-project context
- **THEN** the UI communicates the project/activity relationship clearly

#### Scenario: Navigate to a parent level
- **WHEN** an explicit parent-navigation affordance is provided
- **THEN** it returns safely without discarding unsaved changes

### Requirement: CLEAN_DEAD_CODE
The system SHALL remove obsolete unused converters, event handlers and commands when identified, and SHALL correct naming/logic defects rather than preserving dead compatibility code without a reason.

#### Scenario: Remove unused converter
- **WHEN** a converter is proven to have no references
- **THEN** it is removed rather than kept as dead application code

#### Scenario: Correct a command defect
- **WHEN** a command's name/implementation does not match its intended action
- **THEN** the implementation is corrected and covered by regression tests where practical

### Requirement: TOOLTIP_HEADERS
The system SHALL provide descriptive tooltips for compact/ambiguous headers whose meaning cannot be understood reliably from the visible label alone.

#### Scenario: Compact matrix header
- **WHEN** the user hovers a compact activity/month header
- **THEN** a tooltip provides the fuller context such as name/date/period
