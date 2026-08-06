# Proposal: Expediente y Seguimiento Individual del Alumno

## Why

El docente requiere una ficha individual integral por estudiante que reúna en un solo lugar la información formativa, asistencia, entregas de proyectos/actividades, evaluaciones y registros cualitativos (fortalezas, dificultades, apoyos, acuerdos con tutores y observaciones cronológicas). Esto facilita el acompañamiento personalizado, la atención a la diversidad bajo la NEM y las reuniones informadas con madres, padres o tutores.

## What Changes

1. **Core Domain (`SistemaDocente.Core`):**
   - Definir la entidad e inmutable `ExpedienteEstudiante` y objetos de valor: `Fortaleza`, `Dificultad`, `ApoyoAplicado`, `AcuerdoTutor`, `ObservacionCronologica`, y `AlertaPedagogica` (informativa, sin diagnósticos clínicos/médicos).
   - Reglas de dominio para registrar eventos y notas cronológicas.

2. **Application Use Cases (`SistemaDocente.Application`):**
   - Casos de uso `ConsultarExpedienteEstudiante`, `RegistrarFortaleza`, `RegistrarDificultad`, `RegistrarApoyoAplicado`, `RegistrarAcuerdoTutor` y `AgregarObservacionCronologica`.
   - Consolidación del resumen histórico: porcentaje de asistencia, historial de entregas de actividades por nivel de logro NEM y alertas pedagógicas formativas.

3. **Data Layer & SQLite Schema (`SistemaDocente.Data`):**
   - Nueva migración `user_version = 5` en `EsquemaSqlite.cs` para crear las tablas `expedientes_estudiantes`, `observaciones_cronologicas_estudiantes` y `acuerdos_tutores_estudiantes`.
   - Repositorio `PersistenciaExpedienteSqlite` con ADO.NET directo, sin ORM ni async/await.

4. **Presentation Layer (`SistemaDocente.Presentation`):**
   - `GestionExpedienteViewModel` para visualizar la ficha individual del alumno, pestañas de resumen/asistencia/entregas/observaciones/acuerdos y comandos para agregar notas u observaciones.
   - Actualización de `MainWindowViewModel` para navegar al expediente desde la lista de alumnos del grupo.

5. **WPF UI Layer (`SistemaDocente.App.Wpf`):**
   - Ventana modal dedicada `ExpedienteEstudianteWindow.xaml` con pestañas organizadas (Resumen Pedagógico, Historial Asistencia/Proyectos, Observaciones Cronológicas, Acuerdos con Tutores).

6. **Testing & Specification:**
   - Pruebas unitarias completas en Core, Application, Data, Presentation y WPF.
