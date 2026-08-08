# Tasks

## Version and publish contract
- [x] 1. Add one repository version source for AulaRaíz, starting at `0.1.0`.
- [x] 2. Feed WPF assembly/file/informational version metadata from that source.
- [x] 3. Add a self-contained `win-x64` publish profile with trimming disabled.
- [x] 4. Add automated checks that published output is self-contained and runnable without a repository-local build layout.

## Installer
- [x] 5. Add an Inno Setup 7 installer script with one stable AppId.
- [x] 6. Install per user under LocalAppData Programs without default elevation.
- [x] 7. Create a Start Menu shortcut and optional desktop shortcut.
- [x] 8. Preserve the historical Production/Demo data folders by keeping them outside installer ownership.
- [x] 9. Support installing newer versions over the same AppId/install directory.
- [x] 10. Ensure ordinary uninstall removes program files/shortcuts but not classroom data/backups/exports.

## Application integration
- [x] 11. Add a lightweight version/about surface that displays AulaRaíz and the semantic installed version.
- [x] 12. Keep SQLite initialization/migration in Data; do not add installer-owned SQL.
- [x] 13. Add regression coverage for stable legacy storage identities and version projection.

## CI and lifecycle validation
- [x] 14. Add a Windows installer workflow that runs after/alongside the normal quality gate.
- [x] 15. Pin/verify the Inno Setup compiler acquisition used by CI.
- [x] 16. Publish the self-contained app and compile an unsigned development installer artifact.
- [x] 17. Smoke-test silent current-user install and verify the installed executable/version.
- [x] 18. Smoke-test a real upgrade from an older `0.0.9` validation fixture to the current `0.1.0` installer using the same AppId.
- [x] 19. Smoke-test uninstall and prove a sentinel under the historical user-data directory survives.
- [x] 20. Upload the development installer and paired upgrade-validation installers as CI artifacts for manual testing.

## Documentation and acceptance
- [x] 21. Add maintained installation/update documentation including system requirements and data-preservation behavior.
- [x] 22. Document production Authenticode signing and the rule that signing material never enters source control.
- [x] 23. Update README, deployment architecture and roadmap; mark merged PDF output complete and module 15 in progress.
- [x] 24. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [x] 25. Manually install on a normal Windows user account, launch Demo, upgrade from the `0.0.9` validation fixture to `0.1.0`, reopen data, uninstall/reinstall and confirm data preservation.
