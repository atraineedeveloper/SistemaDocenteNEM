# AulaRaíz branding

## Product identity

The user-facing product name is **AulaRaíz**.

Primary descriptor:

> Gestión docente para la Nueva Escuela Mexicana

The name is intended to communicate classroom work (`Aula`) plus growth, continuity and educational foundations (`Raíz`) without presenting the application as an official SEP product.

## Visible brand rules

Use **AulaRaíz** in:

- application window titles;
- the global navigation header;
- native file-dialog titles and filters;
- user-facing recovery/export/import language when a product name is useful;
- product/readme documentation;
- future installer, shortcut and installed-application metadata.

Use the compact **AR** monogram when a small text-based brand mark is appropriate. It avoids depending on emoji rendering and works across Light, Dark and High Contrast themes.

Use **AulaRaiz** without the accent only where a filesystem-safe ASCII name is preferable, such as suggested backup filenames.

## Compatibility boundary

Branding is deliberately separate from historical technical identity. The following identifiers remain unchanged until a dedicated migration is designed and tested:

- C# namespaces and project names under `SistemaDocente.*`;
- solution/repository technical names;
- `%LOCALAPPDATA%\SistemaDocenteNEM\...`;
- `%LOCALAPPDATA%\SistemaDocenteNEM-Demo\...`;
- SQLite filename `sistema-docente.db`;
- backup package format identifier `SistemaDocenteNEM.Backup`.

Changing those values as a cosmetic rename could make existing data appear missing, split crash logs, or make already-created `.sdocbackup` files incompatible. A later installation/update change may migrate selected identifiers only with explicit backward compatibility and rollback coverage.

## Product metadata

The WPF executable exposes:

- Assembly title: `AulaRaíz`;
- Product: `AulaRaíz`;
- Description: `Gestión docente para la Nueva Escuela Mexicana`.

The assembly/project name remains technical for now.

## Naming guidance

Prefer `AulaRaíz` over generic historical labels such as `Sistema Docente Local` in maintained user-facing surfaces.

Do not use names such as `SEP AulaRaíz`, `AulaRaíz SEP` or similar wording that could imply official sponsorship or endorsement.

Historical OpenSpec/archive content does not need cosmetic rewrites. It remains an engineering record of the terminology in use when those changes were designed.
