# Group workspace and matrix UX

## Purpose

This document describes the interaction model introduced by the `ux-group-workspace-grids` change. The product remains Spanish-facing, but engineering documentation is written in English from this point forward.

## Group as application context

A group is not a secondary filter. It determines the student roster, attendance, projects, evaluation and reports that the teacher is working with.

The application therefore opens into **Mis grupos**. Each group is represented as a workspace card. Opening a card activates that group and enables the main module navigation.

The application may remember and preload the last-used group, but the landing workspace remains explicit so the user can confirm the intended context before entering classroom data.

### Header context switcher

Once a group is open, the header shows a compact context switcher instead of the previous wide labeled ComboBox. The menu provides:

- the available groups;
- **Mis grupos**;
- **Crear grupo…**.

Switching groups delegates to the shell navigation guard. Unsaved changes in Attendance, Projects or Evaluation therefore remain protected.

## Expediente and Reports are complementary

Expediente is intentionally retained.

**Expediente** is the editable longitudinal source of pedagogical follow-up. It contains strengths, difficulties, applied supports, chronological observations, activity history and family agreements.

**Reports** are generated read models that summarize information from Attendance, Evaluation, Projects and Expediente.

Removing Expediente because Reports exist would remove the editable evidence that reports depend on. The UI instead presents Expediente as a student follow-up workspace with four areas:

1. **Resumen** — alerts, strengths and areas of follow-up.
2. **Seguimiento** — applied supports and chronological observations.
3. **Actividades** — project/activity evaluation history.
4. **Familia** — agreements and commitments with tutors or family members.

No Expediente persistence model changes are required by this redesign.

## Attendance matrix

The monthly Attendance grid uses the same interaction principles as Evaluation:

- frozen identity columns;
- compact semantic day cells;
- theme-based colors;
- stronger hover and selected-cell affordances;
- one-click state menu;
- keyboard capture remains available.

Clicking an editable day cell opens:

- **Presente (P)**
- **Falta (F)**
- **Retardo (R)**
- **Justificada (J)**

P/F/R/J keyboard shortcuts remain the fastest capture mode for keyboard-oriented users.

## Evaluation matrix

### Teacher-facing result

The teacher no longer needs to manipulate a technical delivery-state selector and an achievement selector separately. The UI exposes a single **Resultado** while the domain still preserves two independent values.

| Result shown to the teacher | `EstadoEntregaActividad` | `NivelLogro` |
| --- | --- | --- |
| Pendiente | Pendiente | Pendiente |
| Entregada · evaluar después | Entregada | Pendiente |
| Domina | Entregada | Domina |
| Suficiente | Entregada | Suficiente |
| En proceso | Entregada | EnProceso |
| Requiere apoyo | Entregada | RequiereApoyo |
| No entregó | NoEntregada | Pendiente |

This keeps the internal model correct without exposing implementation mechanics to the teacher.

### Direct cell menu

Clicking an applicable editable Evaluation cell opens the seven results above plus **Más opciones…**.

**Más opciones…** opens the full cell editor where the teacher can select the same unified result and add a pedagogical observation of up to 500 characters.

Existing keyboard shortcuts remain supported:

- D — Domina
- S — Suficiente
- E — En proceso
- R — Requiere apoyo
- T — Entregada · evaluar después
- N — No entregó
- P — Pendiente
- Enter/F2 — full editor

## Accessibility and themes

All new visual states use semantic `DynamicResource` tokens. The implementation must continue to work in Light, Dark and High Contrast themes.

Pointer actions supplement keyboard actions rather than replacing them. Every major context action includes an accessible name or textual label.

## Validation checklist

Before merging this UX change:

- run the full Windows CI workflow;
- verify `Mis grupos` with zero, one and multiple groups;
- verify switching groups with and without unsaved changes;
- verify Attendance pointer and keyboard capture;
- verify every Evaluation result mapping;
- verify `Más opciones…` and observation rollback on cancel;
- verify Expediente tabs and empty states;
- review Light/Dark/High Contrast at 100%, 125% and 150% scaling.