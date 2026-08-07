# expedientes-alumnos Specification

## ADDED Requirements

### Requirement: The student record MUST remain distinct from generated reports

Expediente MUST remain the editable longitudinal source of pedagogical follow-up, while Reports remain generated summaries that may consume Expediente data.

#### Scenario: Opening a student record

- **WHEN** the user opens Expediente for a student
- **THEN** the interface exposes editable follow-up evidence such as strengths, difficulties, applied supports, chronological observations and family agreements
- **AND** the interface does not present Expediente as a generated report.

### Requirement: The student record MUST prioritize follow-up information hierarchy

The Expediente UI MUST present student identity and compact summary information first, followed by clearly separated follow-up, activity/evaluation and family-agreement areas.

#### Scenario: Reviewing pedagogical follow-up

- **WHEN** the student record opens
- **THEN** the user can identify the student, attendance summary and follow-up areas without scanning unrelated controls
- **AND** chronological observations are presented as a follow-up history rather than an undifferentiated input list.