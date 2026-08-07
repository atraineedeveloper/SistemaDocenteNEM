# group-workspace-navigation Specification

## ADDED Requirements

### Requirement: The application MUST expose group selection as a workspace context

The application MUST present available groups in a dedicated `Mis grupos` workspace instead of treating group selection only as a header form control.

#### Scenario: Opening the application with existing groups

- **GIVEN** one or more groups exist
- **WHEN** the main window is opened
- **THEN** the user sees the `Mis grupos` workspace
- **AND** each available group is represented as a selectable card
- **AND** the main module navigation is not presented as active until a group workspace is opened.

#### Scenario: Opening a group card

- **GIVEN** the `Mis grupos` workspace is visible
- **WHEN** the user opens a group card
- **THEN** that group becomes the active application context
- **AND** the Group module is shown
- **AND** the main module navigation becomes available.

### Requirement: The header MUST provide a compact group context switcher

The active group MUST be shown as compact contextual information rather than a wide labeled ComboBox.

#### Scenario: Switching to another group

- **GIVEN** a group is active
- **WHEN** the user chooses another group from the context switcher
- **THEN** the shell first evaluates whether the current module can be left
- **AND** the new group becomes active only if navigation is allowed.

#### Scenario: Returning to group selection

- **GIVEN** a group is active
- **WHEN** the user chooses `Mis grupos`
- **THEN** the landing workspace is shown
- **AND** the current group data remains intact.