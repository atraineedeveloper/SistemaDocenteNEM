# Tasks

## Release contract
- [x] 1. Define GitHub Releases as the durable distribution surface and Actions artifacts as temporary CI outputs.
- [x] 2. Keep `Directory.Build.props` as the authoritative semantic product version.
- [ ] 3. Add strict `vMAJOR.MINOR.PATCH` tag validation and require exact match with `VersionPrefix`.
- [ ] 4. Automatically classify `0.x` releases as pre-releases.

## Release workflow
- [ ] 5. Add a tag-triggered Windows release workflow.
- [ ] 6. Re-run format, Release build, full tests, OpenSpec and whitespace on tagged source.
- [ ] 7. Reuse verified Inno Setup 7.0.2 acquisition.
- [ ] 8. Reuse the existing self-contained installer build path.
- [ ] 9. Generate `SHA256SUMS.txt` for the exact installer asset.
- [ ] 10. Create the GitHub Release only after all validation/build steps succeed.
- [ ] 11. Upload installer and checksum assets.
- [ ] 12. Use GitHub-generated notes plus an unsigned-development warning.
- [ ] 13. Require the release tag to already exist with `gh release create --verify-tag`.

## Regression/documentation
- [ ] 14. Add automated repository tests for the release workflow contract.
- [ ] 15. Document release versioning, pre-release policy, assets and future signing boundary.
- [ ] 16. Update README/roadmap to distinguish Releases, Actions artifacts and GitHub Packages.
- [ ] 17. Validate the change with normal Windows CI and OpenSpec.

## Acceptance
- [ ] 18. Merge the release workflow into `main`.
- [ ] 19. Create `v0.1.0` from the merged `main` commit and verify that the automated GitHub pre-release contains the expected installer and checksum.
