# Packaged component license inventory

Reviewed: 2026-08-11

This inventory covers the production components intentionally included by the
AulaRaíz Windows release process. Test-only and build-only dependencies are not
distributed to end users.

## Project and data

| Component | Version or snapshot | Terms | Distribution treatment |
| --- | --- | --- | --- |
| AulaRaíz project code and original materials | 0.2.5 | GPL-3.0-only | Root `LICENSE` is installed as `LICENSE.txt`. |
| Montserrat | 9.000 | SIL Open Font License 1.1 | Exact provenance, hashes and full license are recorded in `THIRD-PARTY-NOTICES.txt` and `third-party/montserrat/OFL.txt`. |
| INEGI geographic catalog transformation | classification through 2025-06-17 | INEGI terms of free use | Source, attribution and transformation are documented in `src/SistemaDocente.Presentation/Data/estados-municipios.SOURCE.md`. |

## Direct production NuGet dependencies

| Package | Pinned version | Primary license record | Result |
| --- | --- | --- | --- |
| Microsoft.Data.Sqlite | 10.0.10 | [MIT, dotnet/efcore](https://github.com/dotnet/efcore/blob/main/LICENSE.txt) | Compatible |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.12 | [Apache-2.0, ericsink/SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw/blob/v2.1.12/LICENSE) | Compatible |
| DocumentFormat.OpenXml | 3.3.0 | [MIT, dotnet/Open-XML-SDK](https://github.com/dotnet/Open-XML-SDK/blob/v3.3.0/LICENSE) | Compatible |
| PDFsharp-MigraDoc | 6.2.4 | [MIT, empira/PDFsharp](https://github.com/empira/PDFsharp/blob/v6.2.4/LICENSE) | Compatible |

The SQLite native engine distributed by the selected SQLitePCLRaw bundle is
[public domain](https://www.sqlite.org/copyright.html). Transitive assemblies
belonging to the package families above retain the same upstream license and
notice obligations. Dependency versions are pinned in the project files and are
audited during restore.

## Self-contained .NET 10 Windows runtime

AulaRaíz publishes WPF, CLI and updater artifacts for `win-x64` with
`--self-contained true`. Microsoft documents the Windows distribution as
follows:

- most .NET binaries are under the MIT license;
- `coreclr.dll`, the runtime embedded in single-file binaries and specified WPF
  native files are under the Microsoft .NET Library License;
- `D3DCompiler_47_cor3.dll` is under the Windows SDK License.

Primary record:
[License information for .NET on Windows](https://github.com/dotnet/core/blob/main/license-information-windows.md).

The non-OSI runtime files are treated as **System Libraries**, not as AulaRaíz
source or project-owned binaries. SignPath Foundation explicitly permits System
Libraries in signed packages under its
[conditions for open-source projects](https://signpath.org/terms.html), using
the definition in section 1 of GPLv3. The official .NET Library License permits
object-code redistribution as part of an application, subject to its
distribution conditions.

AulaRaíz must not submit Microsoft runtime files as project-owned binaries for
individual signing. They may remain unsigned upstream components inside the
signed installer. The installed third-party notice must continue to identify
the applicable Microsoft terms.

## Installer toolchain

| Component | Pinned version | Terms | Treatment |
| --- | --- | --- | --- |
| Inno Setup | 7.0.2 | [Inno Setup License](https://github.com/jrsoftware/issrc/blob/is-7_0_2/license.txt) | Permits use and binary redistribution subject to preserving its notices. The upstream compiler, setup engine and uninstaller are not represented as AulaRaíz-authored binaries. |

The release workflow downloads the pinned Inno Setup release from the official
repository and verifies its GitHub release provenance before use.

## SignPath conclusion

The reviewed application code, font, data and direct production dependencies
are distributable under their recorded terms. The specially licensed Microsoft
runtime files fall under SignPath's express System Libraries exception. No
proprietary application component authored by or affiliated with the AulaRaíz
maintainer is included.

This inventory supports eligibility; SignPath Foundation retains sole authority
to accept the project and configure the final signing scope.

Review this file whenever a production dependency, target runtime, publish mode,
font, data source or installer version changes.
