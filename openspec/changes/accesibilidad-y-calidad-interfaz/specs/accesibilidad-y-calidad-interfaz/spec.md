# accesibilidad-y-calidad-interfaz Specification

## ADDED Requirements

### Requirement: Navegación Completa por Teclado y Mnemónicos
Every interactive control in the WPF user interface SHALL have a defined, logical `TabIndex` sequence and support keyboard access via mnemionics (Alt+Key) or shortcuts (Enter/Escape).

#### Scenario: Guardar o cancelar formulario con teclado
- **Given** the student editor panel is visible
- **When** the teacher presses `Enter` inside a single-line input field or `Alt+G`
- **Then** the student data is saved
- **When** the teacher presses `Escape` or `Alt+C`
- **Then** the edition panel is closed without saving changes

### Requirement: Comunicación Visual Multimodal y Accesibilidad de Contraste
The system SHALL NOT rely solely on color to communicate state (such as student status or attendance markers) and SHALL maintain a text contrast ratio of at least 4.5:1.

#### Scenario: Visualización de estados con texto explícito e icono
- **Given** a list of students with active and inactive states or attendance records
- **When** the list is displayed in the DataGrid
- **Then** each row displays a textual status indicator alongside color coding (e.g. "Activo", "Inactivo", "Presente", "Falta")

### Requirement: Adaptabilidad a Resoluciones Reducidas y Escalado de Windows
The user interface SHALL remain fully usable without content clipping or overlapping when executed on 1024x768 / 800x600 display resolutions or under Windows DPI scaling (125%, 150%).

#### Scenario: Visualización en pantallas pequeñas o escaladas
- **Given** a window resized to minimum dimensions (800x600) or under 150% DPI scaling
- **When** a group with 40 students is loaded
- **Then** scrollbars allow complete navigation to all controls and records without visual distortion

### Requirement: Rendimiento en Listas de 40 Estudiantes
The UI controls SHALL virtualize student rows to maintain smooth scrolling and immediate responsiveness for groups of 40 or more students.

#### Scenario: Carga de grupo numeroso
- **Given** a group containing 40 enrolled students
- **When** the group is displayed in the DataGrid
- **Then** the UI renders instantly and scrolling maintains steady frame rates without UI freezing
