# Integración continua

El repositorio usa GitHub Actions mediante `.github/workflows/ci.yml`.

## Plataforma

El job principal se ejecuta en `windows-latest` porque la solución contiene `SistemaDocente.App.Wpf` y pruebas `net10.0-windows`. Un checkout limpio evita que binarios de otra rama contaminen `dotnet test --no-build`.

## Herramientas fijadas

- .NET SDK 10 (`10.0.x`);
- Node.js 24;
- OpenSpec `1.6.0`.

OpenSpec se fija a una versión concreta para que una nueva publicación del CLI no cambie inesperadamente la validación de un PR existente.

## Validaciones

El workflow ejecuta, en orden:

```powershell
dotnet restore SistemaDocente.sln
dotnet format SistemaDocente.sln --verify-no-changes --no-restore
dotnet build SistemaDocente.sln --configuration Release --no-restore
dotnet test SistemaDocente.sln --configuration Release --no-build
openspec validate --all
git diff --check
```

Un fallo detiene el job y deja visible el paso responsable en la pestaña **Actions** y en los checks del pull request.

## Cuándo se ejecuta

- pull requests dirigidos a `main`;
- pushes a `main` después de integrar cambios;
- ejecución manual mediante `workflow_dispatch` para validar una rama que todavía no tenga pull request.

No se ejecuta también por cada push a `feature/**` cuando ya existe un PR, para evitar dos jobs idénticos por el mismo commit.

## Validación local recomendada

Antes de subir cambios puede ejecutarse la misma secuencia. Si un build falla, no conviene interpretar resultados posteriores de `dotnet test --no-build`, porque podrían existir DLL de pruebas compiladas en una rama anterior. En ese caso se debe corregir primero el build y volver a compilar antes de ejecutar las pruebas sin build.