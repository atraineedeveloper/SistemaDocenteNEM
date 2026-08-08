# Design

## Distribution boundary

GitHub Actions artifacts remain CI/test outputs. GitHub Releases become the durable versioned distribution surface for validated AulaRaíz installers.

The release flow is:

```text
main is prepared with VersionPrefix X.Y.Z
        ↓
create/push tag vX.Y.Z
        ↓
release workflow validates tag == VersionPrefix
        ↓
format + Release build + tests + OpenSpec + whitespace
        ↓
verified Inno Setup acquisition
        ↓
self-contained win-x64 installer build
        ↓
SHA256SUMS.txt
        ↓
GitHub Release vX.Y.Z
```

## Version contract

`Directory.Build.props` remains the single repository source of the semantic application version. A release tag MUST be exactly `v` plus that version. The workflow must fail before publishing if the tag is malformed or mismatched.

The current release line is `0.1.0`. Any `0.x` release is treated as a pre-release. A version with major version 1 or greater may be published as stable unless a later release policy adds another channel.

## Release assets

Each release contains at minimum:

- `AulaRaiz-Setup-X.Y.Z-win-x64.exe`
- `SHA256SUMS.txt`

The checksum file contains the SHA-256 digest of the exact uploaded installer.

## Release notes

GitHub's generated release notes are used for change history. Until Authenticode signing exists, a fixed warning is prepended explaining that the installer is an unsigned development/pre-release build and Windows may identify the publisher as unknown.

## Security and permissions

The workflow uses the repository-scoped `GITHUB_TOKEN` with `contents: write` only for the release job. No personal access token, package registry credential, signing key or certificate is stored in source.

The Inno Setup compiler acquisition keeps the existing supply-chain control: download the pinned official GitHub release asset and verify its release attestation before execution.

## Failure behavior

No GitHub Release is created if version validation, restore, formatting, build, tests, OpenSpec, whitespace, compiler verification, installer build or checksum generation fails.

The workflow uses `gh release create --verify-tag`; release publication therefore cannot silently create a missing tag at the wrong commit.

## Future evolution

A later signed-distribution change may add Authenticode signing before checksum/release publication. A later updater may query GitHub Releases for newer versions, but it must preserve offline-first behavior and must not send classroom/student data.
