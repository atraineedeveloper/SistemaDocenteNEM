# Continuous integration

The repository uses GitHub Actions through `.github/workflows/ci.yml`.

## Platform

The main job runs on `windows-latest` because the solution contains `SistemaDocente.App.Wpf` and `net10.0-windows` tests. A clean checkout prevents binaries from another branch from contaminating `dotnet test --no-build`.

## Pinned tools

- .NET SDK 10 (`10.0.x`);
- Node.js 24;
- OpenSpec `1.6.0`.

OpenSpec is pinned to a concrete version so a new CLI release cannot unexpectedly change validation behavior for an existing pull request.

## Validation sequence

The workflow executes, in order:

```powershell
dotnet restore SistemaDocente.sln
dotnet format SistemaDocente.sln --verify-no-changes --no-restore
dotnet build SistemaDocente.sln --configuration Release --no-restore
dotnet test SistemaDocente.sln --configuration Release --no-build
openspec validate --all
git diff --check
```

A failure stops the job and leaves the responsible step visible in the **Actions** tab and in the pull-request checks.

## Triggers

CI runs for:

- pull requests targeting `main`;
- pushes to `main` after integration;
- manual `workflow_dispatch` execution when a branch needs validation before it has a pull request.

It does not also run for every push to `feature/**` when a pull request already exists, avoiding duplicate jobs for the same commit.

## Recommended local validation

Before pushing changes, the same sequence can be executed locally. If a build fails, do not rely on later `dotnet test --no-build` results because test DLLs may have been compiled on a previous branch. Fix and rebuild first, then run tests without build.