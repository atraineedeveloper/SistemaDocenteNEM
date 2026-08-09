# Proposal: in-app update coordinator

## Why

AulaRaíz already publishes versioned Windows installers through GitHub Releases and the installer safely upgrades program files without owning classroom data. The remaining user experience requires the teacher to leave AulaRaíz, locate the Release and run the installer manually.

Version `0.2.5` should add an explicit, user-controlled update flow that can discover a newer Release, download and verify it while AulaRaíz remains open, then close, install and reopen the application.

## What changes

- Add an update-discovery contract that understands AulaRaíz semantic versions and Preview releases.
- Query the public GitHub Releases API without sending classroom data or credentials.
- Download the expected installer and `SHA256SUMS.txt` into an application update cache.
- Verify the installer SHA-256 before offering installation.
- Add a WPF update experience with explicit teacher consent.
- Add a small `AulaRaiz.Updater.exe` helper that waits for WPF to exit, re-verifies SHA-256, runs the existing Inno Setup installer silently, and reopens AulaRaíz.
- Preserve Production/Demo launch mode across the restart.
- Bump the installable product version to `0.2.5`.

## Non-goals

- Silent/unattended update installation without teacher consent.
- Updating SQLite directly from the updater.
- Background services or scheduled Windows tasks.
- Sending student, group, school, evaluation or other classroom data to GitHub.
- Authenticode enforcement in this change; SHA-256 verification remains mandatory and signing remains a future distribution hardening step.
