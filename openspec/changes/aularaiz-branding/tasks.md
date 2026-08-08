# Tasks

## Product identity
- [x] 1. Add a central product identity contract for `AulaRaíz`, its descriptor, ASCII-safe filename form and legacy technical identifier.
- [x] 2. Keep Core/domain rules independent from commercial branding.

## WPF branding
- [x] 3. Replace the old shell brand with `AulaRaíz` and the NEM descriptor.
- [x] 4. Add the compact `AR` header monogram using semantic theme resources.
- [x] 5. Use AulaRaíz in main-window title composition and startup error chrome.
- [x] 6. Use AulaRaíz in recovery window/file-dialog user-facing text.
- [x] 7. Add AulaRaíz Product/AssemblyTitle/Description metadata to the WPF executable.

## Compatibility
- [x] 8. Keep `%LOCALAPPDATA%\SistemaDocenteNEM` and Demo paths unchanged.
- [x] 9. Keep `SistemaDocenteNEM.Backup` unchanged so version-1 backups remain compatible.
- [x] 10. Keep namespaces, project names, solution name and SQLite filename unchanged.
- [x] 11. Change only new suggested backup filenames to the ASCII-safe `AulaRaiz` brand.

## Documentation and tests
- [x] 12. Update README and maintained backup documentation for AulaRaíz.
- [x] 13. Add a maintained branding/compatibility document.
- [x] 14. Add Application tests for the branding contract and backup filename.
- [x] 15. Add WPF structural regressions for visible branding and legacy recovery identity.
- [x] 16. Run Windows CI: format, Release build, full tests, OpenSpec and whitespace.
- [x] 17. Manually open Demo mode and confirm header, title, recovery dialogs, Light/Dark/High Contrast rendering and no obvious old visible brand remains.