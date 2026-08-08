# Tasks

## Data inventory and classification
- [x] 1. Define D0–D3 product-engineering classifications without presenting them as statutory legal labels.
- [x] 2. Inventory current student, attendance, evaluation, expediente and school-context data.
- [x] 3. Map local SQLite/app-state, backup, export, PDF, safety-backup and diagnostic copies.
- [x] 4. Document the legacy `crash.log` risk and the rule that it is not silently deleted.
- [x] 5. Add future terminal/agent data-minimization rules to the maintained inventory.

## Safe diagnostics
- [x] 6. Add a closed Application diagnostic-event schema and predefined categories.
- [x] 7. Generate a message-free technical SHA-256 fingerprint from exception type/target metadata.
- [x] 8. Add a Data JSONL diagnostic writer isolated by Production/Demo profile.
- [x] 9. Replace WPF raw `Exception.ToString()`/`Debug.WriteLine` logging with the safe diagnostic contract.
- [x] 10. Ensure diagnostic write failures cannot interrupt the application.

## Regression validation
- [x] 11. Add Application tests for message-free diagnostic projection and stable fingerprints.
- [x] 12. Add Data tests proving sensitive sentinel text, paths and stack methods are not persisted.
- [x] 13. Add a WPF structural regression test preventing restoration of raw crash logging.
- [x] 14. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.

## Acceptance
- [x] 15. Review the data inventory against the implemented product surfaces.
- [ ] 16. Squash-merge after CI is green.
- [ ] 17. Clean the feature branch after merge.