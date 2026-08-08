# Design: Windows installer and update delivery

## Packaging choice

Version 1 uses Inno Setup 7 to produce a conventional Windows installer executable. The default installation is per-user (`PrivilegesRequired=lowest`) under LocalAppData Programs, so a normal teacher account does not need administrator credentials. The installer is x64-compatible and packages a self-contained .NET 10 `win-x64` publish output.

MSIX was considered because Windows provides strong package identity and App Installer update support, but unsigned/sideloaded distribution introduces certificate trust and deployment friction that is disproportionate for the current offline-first teacher workflow. MSI/WiX was also considered, but its servicing model is heavier than required for the first installer. The chosen installer keeps the repository architecture unchanged and can later be replaced without changing classroom-data contracts.

## Runtime strategy

The application is published self-contained. The installer therefore does not bootstrap or modify a machine-wide .NET runtime. We intentionally do not trim the WPF application in version 1 because WPF/BAML and reflection-heavy dependencies make aggressive trimming an unnecessary deployment risk. Version 1 also prefers a normal publish directory over single-file bundling so native SQLite/runtime assets remain explicit and easy to inspect in installation smoke tests.

## Product and installer identity

- Visible product: `AulaRaíz`.
- File-safe product form: `AulaRaiz`.
- Installer AppId: a repository-stable GUID that must not change between releases.
- Default install directory: `{localappdata}\Programs\AulaRaiz`.
- Start Menu shortcut: `AulaRaíz`.
- Optional desktop shortcut: user-selected, off by default.

The installer identity is independent from the historical storage identity. `%LOCALAPPDATA%\SistemaDocenteNEM`, `%LOCALAPPDATA%\SistemaDocenteNEM-Demo`, `sistema-docente.db` and `SistemaDocenteNEM.Backup` remain unchanged.

## Update semantics

An update is a newer installer with the same AppId and install directory. Installing it over an older version replaces program files and updates the uninstall registration. It must not remove or rewrite classroom data.

The installer never opens SQLite. Database initialization/migration remains an application responsibility and continues to use the existing additive/idempotent migration paths. Unsupported future schemas remain rejected rather than overwritten. Upgrade tests cover installation preservation plus the existing database migration test suite; any future destructive migration must introduce its own safety-backup/rollback design before implementation.

Automatic update discovery/download is intentionally deferred. The first release model is explicit: obtain a newer trusted installer and run it over the current installation.

## Versioning

The first installable product line uses semantic product version `0.1.0`. WPF assembly metadata and the installer consume the same version source. The application exposes a human-readable installed version in a lightweight About/version surface so support can identify the running build.

## Uninstall and user data

Uninstall removes installed binaries, shortcuts and installer-owned registration only. It must not delete Production/Demo data, backups, exports or other teacher-created files. The uninstaller/installer should make this preservation boundary explicit in documentation; a future controlled data-deletion workflow belongs to privacy/security rather than uninstall.

## Signing and trust

Development CI may build an unsigned installer. Production distribution should Authenticode-sign the application/installer using a trusted code-signing certificate so Windows can identify the publisher and avoid an Unknown Publisher experience. Signing secrets/certificates must never be committed to the repository.

## Validation

CI should:

1. run the normal solution quality gate;
2. publish `SistemaDocente.App.Wpf` self-contained for `win-x64`;
3. compile the Inno Setup script with a pinned supported compiler;
4. silently install the package for the current user into an isolated/default profile;
5. verify executable presence and installed version metadata;
6. place a sentinel in the legacy local-data directory;
7. run the installer again as an upgrade smoke test;
8. uninstall silently;
9. verify installer-owned binaries are removed and the user-data sentinel remains.

Manual validation uses a clean/non-development Windows user or VM: install, launch Demo, create/reopen data, install a newer test build over it, reopen, uninstall, and confirm user data remains.