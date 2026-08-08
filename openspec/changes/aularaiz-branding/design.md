# Design: AulaRaíz product branding

## Goals

- Make AulaRaíz the consistent user-facing product identity.
- Keep branding easy to change without scattering literal strings across multiple layers.
- Preserve existing local data, crash logs and backup compatibility.
- Keep the rebrand independent from pedagogical/domain semantics.

## Product identity contract

`SistemaDocente.Application.IdentidadProducto` owns the small set of stable commercial strings needed by multiple upper layers:

- `Nombre = AulaRaíz`;
- `NombreSeguroArchivo = AulaRaiz`;
- `Subtitulo = Gestión docente para la Nueva Escuela Mexicana`;
- `IdentificadorTecnicoLegado = SistemaDocenteNEM`.

Application is a suitable location because Presentation and WPF already depend on it, while Core remains independent from product/marketing concerns.

## WPF shell

The main header uses the `AR` text monogram inside the existing semantic primary-color surface. This avoids adding an unreviewed raster/vector logo asset and remains compatible with Light, Dark and High Contrast themes.

The brand name and descriptor are resolved with `x:Static` from `IdentidadProducto`. Main-window title composition also uses the same contract.

## File names and dialogs

Native recovery dialogs use the visible `AulaRaíz` name. Suggested new backup files use ASCII-safe `AulaRaiz` to avoid avoidable cross-tool/path encoding friction.

The package contents and product id are not renamed. The existing `SistemaDocenteNEM.Backup` identifier is a compatibility contract for version-1 `.sdocbackup` files.

## Technical identity and migration

The current local data path and namespace/project names are not user branding. They remain stable in this change. The future installation/update work can decide whether selected technical identities should migrate, but that migration must detect legacy data, move/copy it safely, preserve rollback and continue accepting old backup packages.

## Documentation strategy

Maintained product documentation uses AulaRaíz. Historical OpenSpec/archive content may keep the terminology used when those changes were originally developed; rewriting history adds noise without improving the product.

## Testing

- Application tests lock the central product identity values and safe filename form.
- WPF structural tests verify that the header and shell use the central identity instead of the old literal brand.
- Recovery tests retain the old package identifier to prove branding does not silently break compatibility.
- Windows CI remains the authoritative build/XAML validation gate.
