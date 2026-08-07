# expedientes-alumnos Specification

## Purpose

Consolidate each student's individual follow-up through a pedagogical record that combines existing information with its own qualitative entries, without turning alerts into clinical diagnoses.

## Requirements

### Requirement: Consolidated individual student record
The system SHALL provide one individual record per student that consolidates available attendance, activity delivery, formative evaluation and qualitative pedagogical notes.

#### Scenario: View a student's consolidated record
- **WHEN** the user selects a student from the group and opens the individual record
- **THEN** the system displays the available consolidated student information, including attendance, activity/evaluation and pedagogical follow-up

### Requirement: Pedagogical note records
The student record SHALL allow strengths, difficulties, applied supports and chronological observations to be recorded for the student, including record date and validated pedagogical content.

#### Scenario: Record an applied support
- **WHEN** the teacher records an applied pedagogical support
- **THEN** the system preserves the note linked to the student and presents it in the student record

### Requirement: Family or tutor agreements
The student record SHALL allow agreements with family members or tutors to be recorded, including reason, commitments and follow-up dates when applicable.

#### Scenario: Record a tutor agreement
- **WHEN** the teacher records the result of a meeting with a tutor or family member
- **THEN** the system preserves the agreement and its commitments in the student's record

### Requirement: Pedagogical alerts without diagnoses
Alerts derived from attendance, activity follow-up or pedagogical notes SHALL be advisory and SHALL avoid issuing clinical, medical or psychological diagnoses.

#### Scenario: Student with follow-up incidents
- **WHEN** the student record detects information that requires pedagogical attention
- **THEN** the interface may highlight it as a follow-up alert without labeling the student with a diagnosis