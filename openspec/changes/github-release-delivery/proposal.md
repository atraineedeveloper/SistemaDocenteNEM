# GitHub release delivery

## Why

AulaRaíz now has a validated Windows installer and semantic product version, but CI artifacts are temporary test outputs rather than a durable public distribution channel. GitHub Releases should become the repository's official versioned download surface while preserving the existing installer, data-compatibility and signing boundaries.

## What changes

- Add a tag-driven GitHub Actions release workflow for tags shaped as `vMAJOR.MINOR.PATCH`.
- Require the tag version to match `Directory.Build.props` exactly before packaging.
- Re-run the normal quality gates on the tagged source before release publication.
- Reuse the verified Inno Setup acquisition and existing self-contained installer build script.
- Attach the Windows installer and a SHA-256 checksum file to each GitHub Release.
- Mark `0.x` releases as pre-releases automatically; releases beginning at `1.0.0` are eligible to be stable.
- Generate release notes through GitHub and prepend a clear unsigned-development warning until Authenticode signing is implemented.
- Keep GitHub Packages out of scope because AulaRaíz is currently delivered as one desktop application rather than reusable NuGet/container packages.

## Non-goals

- Automatic in-app release discovery or downloading.
- Forced/background updates.
- Authenticode signing or certificate management.
- Publishing internal projects as GitHub Packages/NuGet packages.
- Creating releases from arbitrary branch builds or unversioned commits.
