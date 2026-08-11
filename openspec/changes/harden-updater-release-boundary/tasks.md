# Tasks

## Update client
- [x] 1. Bind installer/checksum URLs to the exact repository, tag and filenames.
- [x] 2. Enforce the installer size ceiling from headers and streamed bytes.
- [x] 3. Preserve temporary-file cleanup and SHA-256 verification.

## Release workflow
- [x] 4. Require tagged commits to be reachable from `origin/main`.
- [x] 5. Disable persisted checkout credentials.
- [x] 6. Run the CI NuGet audit policy during release restore.

## Evidence and documentation
- [x] 7. Add wrong-repository and oversized-installer regression tests.
- [x] 8. Document the threat model, residual risks and incident response.
- [x] 9. Update installation guidance and the maintained roadmap.
- [x] 10. Run format, Release build, full tests, OpenSpec and whitespace validation in CI.
- [x] 11. Run installer lifecycle CI.
- [ ] 12. Complete controlled manual Demo acceptance before a distribution milestone.
