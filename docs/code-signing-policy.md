# Code signing policy

## Status

AulaRaíz is preparing an application for the free code-signing service for open-source projects. Official releases remain unsigned until the application is accepted and the trusted build integration described below is enabled.

Planned signing service: **Free code signing provided by [SignPath.io](https://signpath.io/), certificate by [SignPath Foundation](https://signpath.org/).**

The Authenticode publisher shown by Windows will therefore be **SignPath Foundation**, not AulaRaíz or the maintainer.

## Scope

Only release artifacts built from this repository's reviewed source and published through the official GitHub Release workflow are eligible for signing:

- the AulaRaíz Inno Setup installer;
- project-owned executable files included in that installer, when required by the accepted SignPath policy.

Third-party binaries are never submitted as if they were authored by AulaRaíz. An unsigned upstream open-source component may only be included inside the signed package when its license and provenance have been verified. System Libraries may be included under SignPath Foundation's express exception and must remain identified under their applicable terms. See the maintained [packaged component license inventory](packaged-component-license-inventory.md).

## Source and build integrity

The authoritative source repository is [atraineedeveloper/SistemaDocenteNEM](https://github.com/atraineedeveloper/SistemaDocenteNEM).

The release process must:

1. start from a version tag whose commit belongs to `main`;
2. require the tag version to match the version in `Directory.Build.props`;
3. restore, format-check, build and test the tagged source;
4. use a GitHub-hosted Windows runner;
5. build the unsigned installer before submitting that exact artifact to SignPath;
6. let SignPath validate the trusted GitHub Actions origin;
7. require manual approval of every signing request;
8. publish only the returned signed artifact and its SHA-256 checksum.

Signing credentials and API tokens must be stored as GitHub Actions secrets. No private signing key is stored in the repository or workflow.

The SignPath submission step will not be added until the project has been accepted and the real organization id, project slug, signing policy slug and API token are available.

## Roles

All people in these roles must enable multi-factor authentication on both GitHub and SignPath.

| Role | Members | Responsibility |
| --- | --- | --- |
| Committer / author | [@atraineedeveloper](https://github.com/atraineedeveloper) | Maintains project source and release automation. |
| Reviewer | [@atraineedeveloper](https://github.com/atraineedeveloper) | Reviews contributions from people without commit access before merge. |
| Signing approver | [@atraineedeveloper](https://github.com/atraineedeveloper) | Manually verifies and approves every signing request. |

A future team member must be listed here before receiving a SignPath role. A contribution from a person without commit access requires review by a listed reviewer.

## Privacy and network behavior

**This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.**

AulaRaíz stores classroom data locally. Its optional update feature contacts GitHub only to request public release metadata or download a release after the teacher initiates or permits that operation. It does not send student, teacher, group or school data. See the maintained [privacy data inventory](privacy-data-inventory.md).

## Product behavior

The installer announces the program being installed, supports ordinary uninstall and preserves classroom data intentionally so an update or uninstall does not destroy user records. Network update behavior is documented in [installation and update](installation-update.md).

## Verification before activation

The maintainer must complete all of the following before submitting the SignPath application or enabling signing:

- [x] confirm multi-factor authentication is enabled for the maintainer's GitHub account;
- [ ] confirm multi-factor authentication will be enabled for the SignPath account;
- [x] verify the provenance and redistribution license of every packaged `.ttf` font, or remove it from the package;
- [x] document the source and redistribution terms of `estados-municipios.json`;
- [x] confirm packaged runtime and installer components are covered by compatible licenses or SignPath's System Libraries exception;
- [ ] obtain SignPath Foundation acceptance;
- [ ] install the SignPath GitHub App with access limited to this repository;
- [ ] configure the real SignPath identifiers and `SIGNPATH_API_TOKEN`;
- [ ] add and validate the trusted-build submission step;
- [ ] perform the first signing request with manual approval and verify the resulting Authenticode signature.

## Incident handling

If a signing token, workflow or release artifact may have been compromised:

1. stop new releases and signing approvals;
2. revoke or rotate the affected token;
3. preserve the relevant GitHub Actions and SignPath audit records;
4. notify SignPath and users through the repository security process;
5. resume signing only after the build and approval path has been revalidated.
