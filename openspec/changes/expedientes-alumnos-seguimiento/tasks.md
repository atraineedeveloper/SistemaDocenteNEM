## 1. Domain & Core Layer (`SistemaDocente.Core`)

- [ ] 1.1 Crear Value Objects y Entidades de Expediente (`ExpedienteEstudiante`, `ObservacionCronologica`, `AcuerdoTutor`, `AlertaPedagogica`).
- [ ] 1.2 Implementar reglas de dominio para agregar observaciones cronológicas y alertas pedagógicas formativas.

## 2. Application Layer (`SistemaDocente.Application`)

- [ ] 2.1 Crear DTOs de expediente (`ExpedienteEstudianteDetalle`, `ResumenAsistenciaEstudiante`, `HistorialEntregasEstudiante`).
- [ ] 2.2 Crear casos de uso `GestionExpedientesCasosUso` para consultar ficha consolidada y registrar anotaciones pedagógicas/acuerdos.

## 3. Data & SQLite Layer (`SistemaDocente.Data`)

- [ ] 3.1 Actualizar `EsquemaSqlite.cs` con migración `user_version = 5` y tablas SQLite para expedientes y observaciones.
- [ ] 3.2 Crear `PersistenciaExpedienteSqlite` con ADO.NET directo síncrono.

## 4. Presentation & WPF Layer (`SistemaDocente.Presentation` & `SistemaDocente.App.Wpf`)

- [ ] 4.1 Crear `GestionExpedienteViewModel` con estado, filtros y comandos de captura.
- [ ] 4.2 Crear vista dedicada `ExpedienteEstudianteWindow.xaml` y `ExpedienteEstudianteWindow.xaml.cs`.
- [ ] 4.3 Integrar botón "Ver Expediente" en la vista de Grupo (`GestionGrupoViewModel`).

## 5. Testing & OpenSpec

- [ ] 5.1 Pruebas unitarias en Core, Application, Data, Presentation y App.Wpf.
- [ ] 5.2 Validar con `openspec validate --all`.
