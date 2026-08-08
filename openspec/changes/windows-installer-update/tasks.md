# Tasks

## Version and publish contract
- [ ] 1. Add one repository version source for AulaRaíz, starting at `0.1.0`.
- [ ] 2. Feed WPF assembly/file/informational version metadata from that source.
- [ ] 3. Add a self-contained `win-x64` publish profile with trimming disabled.
- [ ] 4. Add automated checks that published output is self-contained and runnable without a repository-local build layout.

## Installer
- [ ] 5. Add an Inno Setup 7 installer script with one stable AppId.
- [ ] 6. Install per user under LocalAppData Programs without default elevation.
- [ ] 7. Create a Start Menu shortcut and optional desktop shortcut.
- [ ] 8. Preserve the historical Production/Demo data folders by keeping them outside installer ownership.
- [ ] 9. Support installing newer versions over the same AppId/install directory.
- [ ] 10. Ensure ordinary uninstall removes program files/shortcuts but not classroom data/backups/exports.

## Application integration
- [ ] 11. Add a lightweight version/about surface that displays AulaRaíz and the semantic installed version.
- [ ] 12. Keep SQLite initialization/migration in Data; do not add installer-owned SQL.
- [ ] 13. Add regression coverage for stable legacy storage identities and version projection.

## CI and lifecycle validation
- [ ] 14. Add a Windows installer workflow that runs after/alongside the normal quality gate.
- [ ] 15. Pin/verify the Inno Setup compiler acquisition used by CI.
- [ ] 16. Publish the self-contained app and compile an unsigned development installer artifact.
- [ ] 17. Smoke-test silent current-user install and verify the installed executable/version.
- [ ] 18. Smoke-test reinstall/upgrade with the same AppId.
- [ ] 19. Smoke-test uninstall and prove a sentinel under the historical user-data directory survives.
- [ ] 20. Upload the development installer as a CI artifact for manual clean-machine testing.

## Documentation and acceptance
- [ ] 21. Add maintained installation/update documentation including system requirements and data-preservation behavior.
- [ ] 22. Document production Authenticode signing and the rule that signing material never enters source control.
- [ ] 23. Update README, architecture and roadmap; mark merged PDF output complete and module 15 in progress.
- [ ] 24. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [ ] 25. Manually install on a clean/non-development Windows user or VM, launch Demo, upgrade over the installation, reopen data, uninstall and confirm data preservation.
