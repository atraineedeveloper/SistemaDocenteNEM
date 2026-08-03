## 1. Configurar SDK y política común

- [x] 1.1 Actualizar el `global.json` existente para usar el SDK `10.0.110` con `rollForward` igual a `latestPatch`
- [x] 1.2 Crear la configuración común con `AnalysisLevel=latest-recommended`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `Nullable=enable` e `ImplicitUsings=enable`
- [x] 1.3 Verificar que no exista `NoWarn` global y documentar cualquier excepción puntual, justificada y localizada
- [x] 1.4 Confirmar que esta etapa no incorpora analizadores externos adicionales

## 2. Crear solución y proyectos

- [x] 2.1 Crear la solución .NET 10 y los proyectos `SistemaDocente.Core`, `SistemaDocente.Data`, `SistemaDocente.Reporting` y `SistemaDocente.App.Wpf` en sus rutas bajo `src/`
- [x] 2.2 Crear los proyectos xUnit `SistemaDocente.Core.Tests` y `SistemaDocente.Data.Tests` en sus rutas bajo `tests/`
- [x] 2.3 Agregar los seis proyectos a la solución sin introducir entidades, tablas SQLite ni funciones docentes

## 3. Configurar arquitectura y compilación

- [x] 3.1 Configurar las referencias productivas permitidas: Data y Reporting hacia Core, y App.Wpf hacia Core, Data y Reporting, sin ciclos
- [x] 3.2 Configurar Core.Tests hacia Core y Data.Tests hacia Data y Core, manteniendo ambos independientes de WPF
- [x] 3.3 Aplicar `net10.0` a los proyectos portables y comprobar que heredan la configuración común de compilación
- [x] 3.4 Configurar App.Wpf con `net10.0-windows`, WPF habilitado y `EnableWindowsTargeting=true`
- [x] 3.5 Documentar las responsabilidades de proyectos, las referencias permitidas y la prohibición de lógica pedagógica o acceso SQLite en ventanas, controles y code-behind
- [x] 3.6 Inspeccionar y documentar los `ProjectReference` de cada `.csproj`, sin añadir automatización arquitectónica en esta etapa

## 4. Verificar la fundación

- [x] 4.1 Añadir comprobaciones fundacionales mínimas que demuestren que Core.Tests y Data.Tests se descubren y ejecutan sin WPF, sin implementar comportamiento docente
- [x] 4.2 Ejecutar `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` sobre la solución en Fedora y registrar el resultado
- [ ] 4.3 Ejecutar `dotnet restore`, `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes` sobre la solución en Windows y registrar el resultado; esta tarea debe permanecer pendiente mientras solo se disponga de Fedora
- [ ] 4.4 Ejecutar App.Wpf y completar su validación visual básica exclusivamente en Windows; esta tarea debe permanecer pendiente y no puede completarse desde Fedora
- [x] 4.5 Confirmar que no se eligieron proveedor/estrategia SQLite, Entity Framework, Dapper, acceso directo, toolkit/implementación MVVM ni analizadores externos adicionales
- [x] 4.6 Revisar que la implementación no incluya entidades, esquema/migraciones SQLite, reportes funcionales ni funciones docentes
