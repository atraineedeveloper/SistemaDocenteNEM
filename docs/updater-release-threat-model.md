# Updater and release threat model

## Scope

This model covers the path from an accepted commit and version tag to a GitHub Release, update discovery, installer download, local verification, Inno Setup execution and application restart.

It does not treat classroom data as release input. The updater, release client and installer must not read SQLite, backups, exports or pedagogical records.

## Security objectives

1. Only an installer published for `atraineedeveloper/SistemaDocenteNEM`, with the exact selected tag and filename, may enter the update flow.
2. Corrupted, substituted, oversized or incomplete downloads must never be executed.
3. A release tag must identify the repository version and a commit reachable from `main`.
4. Installation remains an explicit teacher action and never blocks normal offline work.
5. The updater and installer must not directly modify classroom storage.
6. Release credentials and future signing keys must not be exposed to build output or committed files.

## Assets and trust boundaries

| Asset | Required property | Boundary |
| --- | --- | --- |
| Installer and installed binaries | Integrity and publisher authenticity | GitHub Actions → GitHub Release → teacher PC |
| Release tag and metadata | Integrity, correct version and source commit | Repository → release workflow |
| `SHA256SUMS.txt` | Integrity and exact filename binding | Release workflow → update client |
| GitHub token | Least privilege and confidentiality | Actions runner → GitHub API |
| Future Authenticode key | Confidentiality and controlled use | Signing service/HSM → signed artifacts |
| Classroom storage | Confidentiality, integrity and availability | Application data root; outside updater ownership |
| Update cache | Bounded disk use and no execution before verification | Network → `%LOCALAPPDATA%\AulaRaiz\Updates` |

The downloaded installer and checksum are untrusted until the exact filename, repository/tag URL and SHA-256 value are validated. GitHub is currently the distribution trust root. SHA-256 detects changed bytes but does not establish an independent publisher identity because the checksum and installer are published through the same account and Release.

## Attacker assumptions

The model considers:

- a network attacker attempting substitution or redirection;
- malformed or hostile public Release metadata;
- accidental publication from an unmerged branch or mismatched version tag;
- corrupted, truncated or unexpectedly large assets;
- compromise of a dependency, GitHub Action, runner or repository publisher;
- local replacement of a cached installer between WPF verification and helper execution.

Malware already executing as the same Windows user can modify per-user program and cache files. The helper is not elevated and is not treated as a privilege boundary; preventing all same-user compromise is outside this updater's capability.

## Threats, controls and residual risk

| Threat | Current control | Residual risk / next control |
| --- | --- | --- |
| Release metadata points to another repository or asset | Exact HTTPS path binding to this repository, selected tag and expected filenames | A compromised authorized publisher can still replace both assets |
| Installer changes in transit or cache | Temporary download, SHA-256 verification, atomic publication and helper re-verification | Checksum shares the GitHub trust root; require Authenticode before broad distribution |
| Oversized response exhausts disk | 64 KiB checksum limit and 512 MiB installer limit, enforced from headers and streamed bytes | Cache retention still needs a lifecycle policy |
| Partial or failed download appears ready | `.download` sibling and deletion on failure; final path only after verification | Abrupt machine loss can leave a temporary file, which is never executable by the flow |
| Tag publishes code outside accepted `main` | Release workflow requires exact semantic version and commit ancestry from `origin/main` | A compromised maintainer can still place malicious code in `main`; branch protection/review remains administrative |
| Build step reuses repository credentials | Checkout does not persist the GitHub token in local Git configuration | The release job still needs `contents: write` to publish; stronger isolation can split build and publish jobs |
| Vulnerable dependency enters release | Release restore runs the repository's direct/transitive NuGet audit gate | Zero-day or compromised-but-not-yet-advised packages remain possible |
| Silent or forced update disrupts work | Two explicit consent boundaries and normal pending-change handling | Social engineering through release notes remains possible; notes are informational, not executable |
| Updater modifies classroom data | Updater has no Data dependency and never opens SQLite | The newly installed application owns normal migrations and must keep its own migration safeguards |
| Same-user cache tampering after WPF verification | Helper recomputes SHA-256 immediately before execution | Same-user malware that can also alter the helper/arguments is outside this non-elevated boundary |
| Unknown Windows publisher | Release warning discloses unsigned status | Production Authenticode signing and signature verification are required before broad distribution |

## Enforced invariants

The update client must reject a candidate or download when:

- the tag is not exactly `vMAJOR.MINOR.PATCH`;
- the target is not newer than the installed version;
- either asset URL is not the exact path under this repository and tag;
- the checksum entry does not bind 64 hexadecimal digits to the expected installer filename;
- the checksum exceeds 64 KiB;
- the installer declares or streams more than 512 MiB;
- the calculated hash differs from the expected value.

The helper must re-verify SHA-256 before launching Inno Setup and must wait for a successful installer exit code before restarting AulaRaíz.

## Release controls

The release workflow:

- runs only for `vMAJOR.MINOR.PATCH` tags;
- requires the tag version to equal `Directory.Build.props`;
- requires the tagged commit to be reachable from `origin/main`;
- restores with the CI NuGet audit policy;
- validates format, Release build, tests, OpenSpec and whitespace;
- acquires the declared Inno Setup release and verifies its GitHub asset;
- builds the installer from tagged source;
- generates and re-checks `SHA256SUMS.txt`;
- publishes with a warning while Authenticode is absent.

Repository settings should complement code controls with protected `main`, required CI, restricted release/tag creation, least-privilege maintainers and strong GitHub account authentication.

## Acceptance and incident response

Before merging updater/release changes:

1. CI and installer lifecycle tests must pass.
2. Tests must cover wrong-repository URLs, checksum mismatch and size rejection.
3. A controlled Demo update should prove postponement, pending-change cancellation, successful restart and classroom-data preservation.
4. A deliberately altered installer must never execute.

If a published asset or release credential is suspected compromised:

1. disable or remove the affected Release from discovery;
2. do not reuse the tag for different bytes;
3. rotate affected credentials and review Actions/audit logs;
4. publish a new version from an accepted `main` commit;
5. communicate the affected versions and hashes;
6. once signing exists, revoke/replace the certificate when its key may be compromised.

## Residual decision

This hardening meaningfully reduces accidental publication, cross-repository substitution, corrupt downloads and resource exhaustion. It does not claim publisher authenticity. Broad institutional distribution remains blocked on the separate production Authenticode signing work.
