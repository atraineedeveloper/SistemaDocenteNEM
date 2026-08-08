# GitHub Releases

AulaRaíz uses GitHub Releases as the durable, versioned download surface for installable Windows builds. GitHub Actions artifacts remain temporary CI/manual-test outputs; GitHub Packages are not part of the current desktop distribution model.

## Release version source

`Directory.Build.props` is authoritative for the semantic product version:

```xml
<VersionPrefix>0.1.0</VersionPrefix>
```

A release tag must be exactly `v` followed by that version. For example:

```text
VersionPrefix 0.1.0  -> tag v0.1.0
VersionPrefix 0.2.0  -> tag v0.2.0
VersionPrefix 1.0.0  -> tag v1.0.0
```

The release workflow rejects malformed tags and version mismatches before it can publish a release.

## Release channel policy

Versions with major version `0` are development pre-releases. The workflow marks them as GitHub pre-releases and does not mark them as the latest stable release.

Starting at major version `1`, the workflow is capable of creating a normal stable release. Production readiness at that point still depends on the product's privacy/security, signing and operational acceptance criteria.

## Automated release flow

Pushing a matching `vMAJOR.MINOR.PATCH` tag starts `.github/workflows/release.yml` on Windows. The workflow:

1. verifies the tag syntax and exact match with `VersionPrefix`;
2. restores the solution;
3. verifies formatting;
4. builds Release with warnings treated as errors;
5. runs the complete test suite;
6. validates all OpenSpec content;
7. checks whitespace;
8. downloads pinned Inno Setup 7.0.2 from its official GitHub release and verifies the release asset attestation;
9. builds the existing self-contained `win-x64` installer through `scripts/build-installer.ps1`;
10. calculates the installer's SHA-256 digest;
11. writes `SHA256SUMS.txt`;
12. verifies the checksum file against the installer;
13. creates the GitHub Release using the already-existing tag;
14. uploads the installer and checksum file;
15. asks GitHub to generate release notes from repository history.

No release is published if an earlier step fails.

## Release assets

For version `0.1.0`, the expected assets are:

```text
AulaRaiz-Setup-0.1.0-win-x64.exe
SHA256SUMS.txt
```

A user can verify the downloaded installer on Windows with:

```powershell
Get-FileHash .\AulaRaiz-Setup-0.1.0-win-x64.exe -Algorithm SHA256
```

The result should match the digest stored in `SHA256SUMS.txt` from the same release.

## Unsigned development warning

The current release workflow does not perform Authenticode signing. Until a trusted signing workflow exists, every automated release prepends a warning that Windows may identify the publisher as unknown and that the SHA-256 checksum should be verified before redistributing the installer.

A future signing change should sign the application/installer before checksum generation and release publication. Certificates/private keys must remain outside repository source and ordinary artifacts.

## Creating the next release

Prepare releases from `main`, not from an arbitrary feature branch:

1. choose the next semantic version;
2. update `VersionPrefix`, `AssemblyVersion`, `FileVersion` and `InformationalVersion` together in `Directory.Build.props`;
3. complete the normal OpenSpec/PR/CI/manual-validation process for that version;
4. merge using the agreed squash strategy;
5. create and push the matching tag on the accepted `main` commit;
6. allow the Release workflow to validate, build and publish.

Example after `main` has been accepted as `0.2.0`:

```powershell
git checkout main
git pull
git tag v0.2.0
git push origin v0.2.0
```

Do not create the tag before the intended release commit is merged and accepted. `gh release create --verify-tag` is used deliberately so the workflow never invents a missing tag from another commit.

## Releases vs Actions artifacts vs Packages

- **GitHub Release:** durable public/versioned application delivery; this is where AulaRaíz installers belong.
- **Actions artifact:** temporary output for CI and manual validation of a particular workflow run.
- **GitHub Packages:** dependency/package registries such as NuGet, npm or containers. AulaRaíz does not need this merely to distribute its Windows EXE installer.

If the repository later extracts genuinely reusable .NET libraries consumed by separate applications, those libraries can independently evaluate GitHub Packages/NuGet publishing without changing the desktop Release model.
