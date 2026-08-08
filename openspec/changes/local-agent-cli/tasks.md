# Tasks

## Shared storage and CLI host
- [ ] 1. Add a Data-owned Production/Demo local-storage path contract and make WPF reuse it.
- [ ] 2. Add `SistemaDocente.Cli` as a .NET 10 console host named `aularaiz`.
- [ ] 3. Add a stable JSON envelope with schema version, privacy metadata and safe errors.
- [ ] 4. Add `capabilities` and `status` commands.

## Read and management commands
- [ ] 5. Add group listing with ids-only/minimized output by default and explicit personal-data opt-in.
- [ ] 6. Add student listing with ids/structured fields by default and explicit name opt-in.
- [ ] 7. Add attendance read by group/date.
- [ ] 8. Add attendance single-student dry-run/apply using the complete-roster Application use case.
- [ ] 9. Add student deactivate/reactivate dry-run/apply using `GestionGrupoCasosUso`.
- [ ] 10. Keep deletion and sensitive free-form argv writes out of V1.

## Agent context and recommendations
- [ ] 11. Add minimized group agent context with aggregate attendance/completion/achievement and pseudonymous student evidence.
- [ ] 12. Add deterministic local pedagogical recommendations with evidence and coverage/caveats.
- [ ] 13. Ensure recommendations do not diagnose, infer causes or rank students.
- [ ] 14. Keep D3 free-form notes/agreements/observations out of V1 agent output.

## Privacy and diagnostics
- [ ] 15. Add a predefined safe diagnostic category for terminal failures.
- [ ] 16. Map CLI errors to stable generic codes without raw exception messages/stack traces.
- [ ] 17. Prove default JSON output does not contain student names or sensitive free-text sentinel data.
- [ ] 18. Prove `--include-personal-data` is explicit and reflected in privacy metadata.
- [ ] 19. Prove the CLI has no network dependency/action.

## Packaging and validation
- [ ] 20. Publish `aularaiz.exe` self-contained and include it in the existing installer.
- [ ] 21. Add installed CLI smoke tests to the installer workflow.
- [ ] 22. Add CLI/Application/Reporting regression tests for dry-run, apply and recommendation behavior.
- [ ] 23. Document terminal usage, agent workflow and privacy boundary.
- [ ] 24. Update README/roadmap and bump the installable product line to `0.2.0`.
- [ ] 25. Run Windows CI, installer lifecycle CI and OpenSpec.

## Acceptance
- [ ] 26. Manually validate CLI commands against Demo data.
- [ ] 27. Squash-merge after automated/manual acceptance.
- [ ] 28. Clean stale feature/maintenance branches after merge.