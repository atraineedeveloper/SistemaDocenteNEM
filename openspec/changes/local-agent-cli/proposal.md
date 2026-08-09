# Proposal: local terminal and agent interface

## Why

AulaRaíz already centralizes classroom data and domain rules locally, but today all interaction is mediated by WPF. A local CLI gives teachers, scripts and AI agents a stable automation surface without bypassing the application's validation rules or exposing SQLite as an API.

The interface must be designed as a privacy boundary, not merely a convenience wrapper. Terminal command lines can be retained in shell history and process listings, and agent output can be copied into external systems. Therefore the first CLI defaults to minimized structured data, uses internal ids for people, and does not accept sensitive free-form pedagogical text in command-line arguments.

## What changes

- Add an installed `aularaiz.exe` CLI that uses the same Production/Demo storage contract as WPF.
- Reuse Core/Application/Data use cases instead of issuing SQL from command handlers.
- Add stable JSON envelopes intended for agent/tool consumption.
- Add discovery/read commands for groups, students and attendance.
- Add reversible student activation/deactivation and attendance-state changes, all as dry-run unless `--apply` is present.
- Add a minimized `agent context` projection with aggregate/group evidence and pseudonymous student ids by default.
- Add deterministic local pedagogical recommendations that state the evidence used and never diagnose, rank students or invent causes.
- Keep all CLI behavior offline; the CLI itself never sends classroom data to a network service.
- Package the CLI with the normal Windows installer.

## Out of scope

- deleting groups/students/projects/history;
- accepting names, observations, agreements or other D2/D3 free-form text through argv;
- autonomous background agents;
- direct integration with an external LLM/API;
- giving an external service permission to receive classroom data;
- replacing the WPF application.

A future external AI connector requires a separate consent, minimization and credential-security design.