# Automated test coverage and dependency audit

AulaRaíz collects an initial automated-test coverage baseline in the normal Windows CI workflow. The baseline is observational: this change does not impose a repository-wide percentage threshold.

## Coverage collection

CI runs the Release build once and then executes the full solution test graph with the existing Coverlet VSTest collector:

```powershell
dotnet test SistemaDocente.sln `
  --configuration Release `
  --no-build `
  --collect:"XPlat Code Coverage" `
  --settings .\eng\coverage.runsettings `
  --results-directory .\artifacts\coverage
```

The workflow uploads every generated `coverage.cobertura.xml` file as the `aularaiz-coverage-cobertura` artifact for 14 days. Test assemblies and generated compiler/resource sources are excluded from the measurement.

Coverlet's VSTest collector produces one result under a generated test-results directory and does not merge reports or enforce thresholds by itself. The first accepted artifact must therefore be reviewed before selecting any gate.

## First observed baseline

Windows CI run [#328](https://github.com/atraineedeveloper/SistemaDocenteNEM/actions/runs/31491123182) produced five Cobertura reports, one for each test project. Because the VSTest collector does not merge those reports, the table records the highest package-level rate observed for each production assembly across the five files. These values are evidence that the baseline is measurable, not merged repository totals or proposed gates.

| Production assembly | Line coverage | Branch coverage |
| --- | ---: | ---: |
| `SistemaDocente.Core` | 80.65% | 59.39% |
| `SistemaDocente.Application` | 69.91% | 50.28% |
| `SistemaDocente.Data` | 86.04% | 77.77% |
| `SistemaDocente.Presentation` | 67.00% | 43.60% |
| `SistemaDocente.Reporting` | 57.14% | 77.27% |
| `SistemaDocente.Interchange` | 85.36% | 64.48% |
| `SistemaDocente.App.Wpf` | 32.16% | 17.44% |
| `aularaiz` | 72.34% | 39.17% |
| `AulaRaiz.Updater` | 86.36% | 65.62% |

The raw `aularaiz-coverage-cobertura` artifact contained five files and was accepted by the workflow's `if-no-files-found: error` gate. A later threshold proposal should first introduce or select a reproducible merge strategy when a combined repository or cross-suite rate is needed.

## How to interpret the baseline

Coverage is a risk signal, not a product-quality score.

- Core and Application rules should eventually have the strongest line and branch expectations.
- Data migration, import, restore and update-integrity paths should be judged by explicit failure-path coverage.
- WPF coverage should not be compared directly with domain-layer coverage; keyboard, theme, scaling and window behavior still require focused tests and manual validation.
- Generated code, test assemblies and resource designers must not inflate or depress the baseline.
- A future threshold change must document the measured baseline, exclusions and why the selected gates are appropriate.

## NuGet dependency audit

Repository-level MSBuild properties explicitly enable auditing of direct and transitive packages with a minimum reported severity of `moderate`.

Normal local restore keeps NU1900–NU1905 findings visible without turning a newly published advisory into an unexpected local build failure. CI runs:

```powershell
dotnet restore SistemaDocente.sln -p:AuditPipeline=true
```

In that audit pipeline the same findings are promoted to errors. A suppression must identify the exact advisory, document why it does not apply or why the risk is temporarily accepted, and remain a last resort.
