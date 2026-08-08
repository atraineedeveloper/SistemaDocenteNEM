# Proposal: AulaRaíz product branding

## Why

The current user-facing product name, `Sistema Docente Local`, is generic and does not provide a distinctive product identity. The application now has enough stable classroom workflows that a coherent brand should appear consistently in the shell, window titles, native file dialogs, product metadata and maintained documentation.

At the same time, the existing `SistemaDocente*` technical identity is already embedded in namespaces, project names, local storage directories and version-1 backup compatibility. A cosmetic rename must not make existing teacher data appear missing or invalidate previously created recovery packages.

## What changes

- Adopt **AulaRaíz** as the visible product name.
- Adopt **Gestión docente para la Nueva Escuela Mexicana** as the primary descriptor.
- Use a compact `AR` monogram in the application header without introducing a raster-logo dependency.
- Centralize visible brand strings in an Application-level product identity contract.
- Use `AulaRaiz` as the ASCII-safe form for new suggested filenames.
- Update WPF window titles, header branding, startup error titles and recovery file dialogs.
- Set the WPF assembly Product/Title/Description metadata to AulaRaíz.
- Update maintained README/recovery/branding documentation.
- Add regressions covering the visible brand and compatibility boundary.

## Compatibility boundary

The change intentionally does **not** rename:

- `SistemaDocente.*` C# namespaces or project names;
- the solution or repository technical name;
- `%LOCALAPPDATA%\SistemaDocenteNEM` or `%LOCALAPPDATA%\SistemaDocenteNEM-Demo`;
- `sistema-docente.db`;
- backup package format id `SistemaDocenteNEM.Backup`.

Those values require an explicit installation/update migration with backward-compatibility and rollback coverage rather than a cosmetic search-and-replace.

## Out of scope

- Creating a final graphic logo/icon asset.
- Renaming the GitHub repository or solution/projects.
- Moving existing local application data.
- Changing the version-1 backup product identifier.
- Rewriting archived/historical OpenSpec content solely for terminology.
