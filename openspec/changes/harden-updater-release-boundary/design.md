# Design: updater and release trust-boundary hardening

## Trust decision

GitHub remains the distribution trust root until production Authenticode exists. The client may trust release metadata only enough to locate assets after binding each URL to:

- HTTPS;
- host `github.com`;
- repository `atraineedeveloper/SistemaDocenteNEM`;
- the selected semantic tag;
- the exact installer or checksum filename;
- no query or fragment.

SHA-256 remains an integrity control, not independent publisher authentication.

## Bounded download

`SHA256SUMS.txt` remains limited to 64 KiB. The installer receives a 512 MiB ceiling. The client rejects an excessive `Content-Length` before streaming and also checks cumulative streamed bytes so a missing or dishonest header cannot bypass the limit.

The existing temporary sibling, hash verification and atomic final move remain unchanged. Failure cleanup never publishes the temporary file as ready.

## Release workflow

The tagged workflow fetches `origin/main` and requires `GITHUB_SHA` to be its ancestor. This prevents accidental publication from an unmerged feature commit while retaining semantic tag/version validation.

Checkout credentials are not persisted in local Git configuration. The release job still receives `contents: write` because it publishes the Release; splitting build and publication into separately permissioned jobs remains a future defense-in-depth option.

The tagged restore runs `AuditPipeline=true`, matching the dependency policy enforced in normal CI.

## Failure behavior

Invalid asset paths fail before any HTTP asset request. Oversized installers fail with a stable technical error code and the temporary file is removed or remains unpublished. These failures do not block startup, open SQLite or expose response bodies in diagnostics.

## Residual risks

A compromised authorized GitHub publisher can replace the installer and checksum together. A compromised Actions dependency or runner can affect produced bytes. Same-user malware can alter per-user files. Production Authenticode, protected signing infrastructure and administrative repository controls are required to reduce those risks.
