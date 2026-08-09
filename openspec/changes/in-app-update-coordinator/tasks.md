# Tasks

## Update contracts and discovery
- [ ] 1. Bump the product line to `0.2.5`.
- [ ] 2. Add Application records/contracts for release metadata, verified downloads and update channels.
- [ ] 3. Add GitHub Release discovery for Preview releases with semantic-version selection.
- [ ] 4. Ensure discovery sends no classroom data and does not block startup when offline.

## Secure download
- [ ] 5. Download `SHA256SUMS.txt` and the exact versioned installer into a local update cache.
- [ ] 6. Stream to a temporary file and atomically publish only after SHA-256 verification.
- [ ] 7. Reject malformed/missing checksum entries and hash mismatches.

## WPF experience
- [ ] 8. Check for updates asynchronously after normal startup.
- [ ] 9. Show installed/new version and release notes with `Más tarde` / `Descargar e instalar`.
- [ ] 10. Show a second `Cerrar y actualizar` confirmation after verification.
- [ ] 11. Preserve Production/Demo mode and show update-success feedback after restart.

## Updater helper
- [ ] 12. Add self-contained `AulaRaiz.Updater.exe`.
- [ ] 13. Re-verify SHA-256 in the helper before executing the installer.
- [ ] 14. Wait for WPF to exit, install silently and relaunch AulaRaíz.
- [ ] 15. Keep the helper isolated from SQLite/classroom storage.

## Packaging and tests
- [ ] 16. Include the updater helper in the existing installer build.
- [ ] 17. Extend installer CI to verify the helper's install/uninstall lifecycle.
- [ ] 18. Add regression tests for semantic release selection, checksum parsing and mismatch rejection.
- [ ] 19. Add structural WPF/updater tests for explicit consent and safe restart arguments.
- [ ] 20. Document the update workflow and privacy boundary.
- [ ] 21. Run format/build/tests/OpenSpec/installer CI.

## Acceptance
- [ ] 22. Manually validate discovery/download/restart against a controlled test release or local fixture without risking classroom data.
- [ ] 23. Squash-merge after automated/manual acceptance.
- [ ] 24. Publish `v0.2.5` only after the merged `main` version matches `0.2.5`.
