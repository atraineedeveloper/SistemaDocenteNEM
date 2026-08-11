# SignPath Foundation readiness review

Review date: 2026-08-11

This document records the repository evidence and remaining work for an application to the free SignPath Foundation code-signing service. It is a readiness record, not proof of acceptance.

## Eligibility evidence

| Requirement | Repository evidence | Status |
| --- | --- | --- |
| OSI-approved open-source license | AulaRaíz uses `GPL-3.0-only`; the root `LICENSE` and README identify the license. | Ready |
| Existing executable release | GitHub Releases already distributes the Windows installer in the same form intended for signing. | Ready |
| Maintained project | Changes, tests, CI and release automation are active in the public repository. | Ready |
| Documented functionality | The README describes product scope, installation/update behavior, privacy and validation. | Ready |
| Reproducible provenance | Release tags are checked against `main` and `Directory.Build.props`; GitHub-hosted Actions rebuild and test the tagged source. | Ready for SignPath integration |
| Product metadata | WPF, CLI and updater projects define product/title metadata; centralized assembly/file/informational versions come from `Directory.Build.props`. | Ready |
| Uninstall and disclosed system changes | Inno Setup owns program files and shortcuts, includes uninstall and intentionally preserves classroom data. | Ready |
| Privacy/network disclosure | The privacy inventory documents local data and the optional GitHub release update surface. The required network statement is in the code-signing policy. | Ready |
| Team roles and signing approval | Committer, reviewer and signing approver are identified in the code-signing policy; every future signing request is required to receive manual approval. | Documented; MFA confirmation pending |
| No proprietary packaged components | Direct NuGet dependencies are open source. Montserrat binaries match the official 9.000 upstream files under OFL-1.1, and the INEGI-derived geographic catalog now has source, transformation and attribution records. License/notices are packaged with the installer. | Ready for final installed-file inventory |
| Verifiable project reputation | The project is public and has an existing release, but SignPath evaluates reputation and may reject a new or insufficiently established project. | External decision |

## Direct application dependencies

The direct production package references found in the project files are compatible open-source components:

| Component | Version | License | Primary package record |
| --- | ---: | --- | --- |
| Microsoft.Data.Sqlite | 10.0.10 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Data.Sqlite/10.0.10) |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.12 | Apache-2.0 | [NuGet](https://www.nuget.org/packages/SQLitePCLRaw.bundle_e_sqlite3/2.1.12) |
| DocumentFormat.OpenXml | 3.3.0 | MIT | [NuGet](https://www.nuget.org/packages/DocumentFormat.OpenXml/3.3.0) |
| PDFsharp-MigraDoc | 6.2.4 | MIT | [NuGet](https://www.nuget.org/packages/PDFsharp-MigraDoc/6.2.4) |

The self-contained Windows package also contains the applicable .NET runtime files and third-party notices. The release workflow installs Inno Setup from its official GitHub release and verifies the downloaded archive. Before application submission, the final installed-file inventory must be checked against the corresponding .NET and Inno Setup license/notice files.

Test-only packages are not distributed as application binaries, but remain subject to their own open-source license terms.

## Verified source-material provenance

### Montserrat fonts

The three embedded files are Montserrat 9.000 Regular, SemiBold and Bold. Their
byte lengths, SHA-256 hashes and Git blob ids match the corresponding files in
the official [JulietaUla/Montserrat](https://github.com/JulietaUla/Montserrat)
repository. Their internal name tables also identify Montserrat 9.000 and SIL
Open Font License 1.1.

The upstream OFL text is retained at `third-party/montserrat/OFL.txt`.
`THIRD-PARTY-NOTICES.txt`, the root project license and a copy named
`Montserrat-OFL.txt` are installed beside the application.

### INEGI geographic catalog

`src/SistemaDocente.Presentation/Data/estados-municipios.json` is documented
as an AulaRaíz transformation of the official INEGI Catálogo Único de Claves
de Áreas Geoestadísticas Estatales, Municipales y Localidades. The sidecar
`estados-municipios.SOURCE.md` records the official catalog/service links,
INEGI terms, required attribution, transformation, 2025-06-17 reference date,
reviewed counts and update procedure.

The reviewed JSON contains 32 entities and 2,478 municipalities or Mexico City
territorial demarcations, matching INEGI's 2025 national count. The installed
`THIRD-PARTY-NOTICES.txt` credits INEGI, identifies AulaRaíz's transformation
and disclaims endorsement.

## Integration after acceptance

Do not add placeholder identifiers to the release workflow. Once SignPath accepts the project:

1. enable MFA for every listed team member on GitHub and SignPath;
2. install the SignPath GitHub App with access limited to this repository;
3. create the SignPath organization/project/signing policy;
4. add `SIGNPATH_API_TOKEN` as a GitHub Actions secret;
5. upload the unsigned installer as the trusted-build artifact;
6. submit it with `signpath/github-action-submit-signing-request@v2` using the real organization id, project slug, signing policy slug and artifact id;
7. require manual signing approval;
8. publish the signed installer returned by SignPath, not the unsigned input;
9. verify Authenticode and recalculate `SHA256SUMS.txt` from the signed installer;
10. run the full installation, update and uninstall lifecycle validation against that signed artifact.

The SHA-256 checksum must be generated after signing because Authenticode changes the executable bytes.

## Application package

The application should link to:

- the repository and existing GitHub Release;
- `LICENSE`;
- the README product/download documentation;
- [the code-signing policy](code-signing-policy.md);
- [the privacy data inventory](privacy-data-inventory.md);
- the release workflow and its trusted-build controls;
- this readiness review after all blocking items are closed.

No application should claim that releases are signed until an accepted policy has produced and verified a signed artifact.
