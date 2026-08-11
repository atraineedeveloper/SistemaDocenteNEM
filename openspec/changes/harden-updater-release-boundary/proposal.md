# Proposal: harden updater and release trust boundary

## Why

AulaRaíz already verifies downloaded installers with SHA-256, but the installer and checksum share the same GitHub Release trust root. The client also accepts asset URLs from any GitHub repository and does not bound installer size. The release workflow validates the tag version but does not prove that the tagged commit belongs to accepted `main` history.

The current boundary needs an explicit threat model and additional controls before Authenticode signing is introduced.

## What changes

- Bind update asset URLs to the exact AulaRaíz repository, selected tag and expected filenames.
- Reject an installer that declares or streams more than 512 MiB.
- Require release tags to point to commits reachable from `origin/main`.
- Avoid persisting checkout credentials during release builds.
- Apply the repository NuGet audit gate during tagged releases.
- Document assets, attackers, trust boundaries, controls, residual risks and incident response.
- Add automated regression coverage for wrong-repository assets and oversized downloads.

## Non-goals

- Claiming that SHA-256 proves publisher identity.
- Adding or purchasing an Authenticode certificate in this change.
- Protecting against malware that already executes as the same Windows user.
- Changing installer identity, application data paths, SQLite migrations or backup format.
- Adding a forced or unattended update service.

## Privacy and compatibility

The change reads and transmits no classroom data. It does not change SQLite, Production/Demo storage, backups, exports, CLI behavior or installed-version semantics. Rejected updates leave normal offline work available.
