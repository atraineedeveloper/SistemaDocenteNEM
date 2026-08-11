# Security Policy

AulaRaíz stores educational and personal information locally for classroom workflows. Security reports must therefore avoid exposing real student, teacher, family or school data.

## Supported versions

AulaRaíz is currently a prerelease product. Security fixes are applied to the current `0.2.x` development line.

| Version | Supported |
| --- | --- |
| Current `0.2.x` release | Yes |
| Older prereleases | No |

Users should update to the newest published release after reviewing its release notes and integrity information.

## Reporting a vulnerability

Use GitHub's private vulnerability reporting flow from the repository **Security** tab when it is available.

Do not open a public issue containing:

- exploit details or proof-of-concept code;
- credentials, tokens, signing material or private URLs;
- SQLite databases, diagnostics, exports, PDFs or `.sdocbackup` files;
- real names, identifiers, attendance, evaluation, family or school information.

If private reporting is unavailable, open only a minimal public issue asking the maintainer to enable a private reporting channel. Do not include technical vulnerability details in that issue.

A useful private report includes:

- the affected AulaRaíz version and Production/Demo mode;
- the affected component or workflow;
- reproducible steps using fictitious data;
- expected and observed security impact;
- whether local access, network access or user interaction is required;
- a proposed mitigation, when known.

## Scope priorities

Reports are especially valuable when they concern:

- update discovery, release assets, checksum verification, updater execution or downgrade paths;
- backup/restore integrity, archive handling or Production/Demo isolation;
- unintended disclosure through diagnostics, CLI, exports, PDFs or temporary files;
- SQLite corruption, unsafe migration or restore rollback;
- command or path injection;
- dependency or GitHub Actions supply-chain compromise;
- bypass of explicit consent for sensitive operations.

## Disclosure and response

Please allow maintainers time to reproduce, assess and prepare a coordinated fix before public disclosure. The repository does not promise a fixed response deadline while AulaRaíz remains a prerelease project, but validated high-impact reports should be prioritized.

Security fixes must use fictitious fixtures, preserve classroom data by default and document any migration or compatibility impact.
