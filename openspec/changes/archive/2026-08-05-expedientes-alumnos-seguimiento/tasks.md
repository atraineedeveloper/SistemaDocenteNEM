## 1. Domain & Core Layer (`SistemaDocente.Core`)

- [x] 1.1 Crear Value Objects y Entidades de Expediente (`ExpedienteEstudiante`, `ObservacionCronologica`, `AcuerdoTutor`, `AlertaPedagogica`, `ValidadorContenidoPedagogico`).
- [x] 1.2 Implementar reglas de dominio para agregar observaciones cronológicas y alertas pedagógicas formativas sin emisión ni registro de diagnósticos clínicos.

## 2. Application Layer (`SistemaDocente.Application`)

- [x] 2.1 Crear DTOs de expediente (`ExpedienteEstudianteDetalle`, `ResumenAsistenciaEstudiante`, `HistorialEntregaEstudiante`).
- [x] 2.2 Crear casos de uso `GestionExpedienteCasosUso` para consultar ficha consolidada y registrar anotaciones pedagógicas/acuerdos.

## 3. Data & SQLite Layer (`SistemaDocente.Data`)

- [x] 3.1 Actualizar `EsquemaSqlite.cs` con migración `user_version = 5` y tablas SQLite con integridad foránea activa.
- [x] 3.2 Crear `PersistenciaExpedienteSqlite` con ADO.NET directo síncrono.

## 4. Presentation & WPF Layer (`SistemaDocente.Presentation` & `SistemaDocente.App.Wpf`)

- [x] 4.1 Crear `GestionExpedienteViewModel` con estado, apoyos y comandos de captura.
- [x] 4.2 Crear vista dedicada `ExpedienteEstudianteWindow.xaml` y `ExpedienteEstudianteWindow.xaml.cs`.
- [x] 4.3 Integrar botón "Ver Expediente" en la vista de Grupo (`MainWindow`).

## 5. Testing & OpenSpec

- [x] 5.1 Pruebas unitarias en Core, Application, Data, Presentation y App.Wpf.
- [x] 5.2 Validar con `openspec validate --all`.
