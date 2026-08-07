# expedientes-estudiantes-ampliados Specification

## Purpose

Define the structured student profile used by the student record and the WPF typography requirement introduced with the extended-record change.

## Requirements

### Requirement: Structured student record
The student entity MUST store first surname, second surname, given names, birth date, sex/gender, admission date and individual pedagogical observations.

#### Scenario: Create a student with structured data
- **WHEN** a teacher registers a new student and enters surnames, given names, birth date and gender
- **THEN** the system stores the information in structured fields and validates that pedagogical observations do not contain clinical diagnoses.

#### Scenario: Present age and data in the student record
- **WHEN** the teacher views a student's record
- **THEN** the system calculates current age in years from the birth date and displays the complete structured profile.

### Requirement: Global Montserrat typography
The WPF visual interface MUST use the **Montserrat** font family, with Segoe UI / sans-serif fallback.

#### Scenario: Apply Montserrat consistently
- **WHEN** the user navigates through WPF windows and controls
- **THEN** typographic controls are rendered using the Montserrat font family.
