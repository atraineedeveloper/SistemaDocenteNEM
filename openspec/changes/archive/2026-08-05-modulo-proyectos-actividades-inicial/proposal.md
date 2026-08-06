## Why

El trabajo docente se organiza principalmente mediante proyectos y actividades, pero el sistema todavía no permite planearlos ni registrar el cumplimiento de cada estudiante. Este primer corte vertical crea la base pedagógica y técnica que después alimentará evaluación, expediente y reportes, sin adelantar calificaciones ni planeación NEM detallada.

## What Changes

- Incorporar `ProyectoDidactico` como agregado identificado y contenedor pedagógico y temporal del grupo, con creación, edición, consulta, transiciones `Borrador`/`EnCurso`/`Finalizado`, reapertura explícita y eliminación restringida.
- Incorporar `ActividadProyecto` como agregado independiente, siempre perteneciente a un proyecto, con fecha real, estado activa/anulada y un padrón completo de entregas guardado atómicamente.
- Registrar por estudiante `Pendiente`, `Entregada` o `NoEntregada` y una observación breve, conservando inactivos históricos y evitando incorporaciones retroactivas.
- Añadir casos de uso específicos, snapshots inmutables, ordenamientos, filtros, conteos, validación de periodos, cambios pendientes y errores seguros.
- Migrar SQLite de `user_version = 2` a `user_version = 3` de forma transaccional y crear tablas relacionales para proyectos, actividades y entregas sin destruir grupo, estudiantes ni asistencia.
- Añadir una interfaz WPF completa de Proyectos con lista de proyectos, actividades y grilla de entregas, integrada a `MainWindow` mediante MVVM portable y composición manual.
- Añadir pruebas por capa y auditorías de dependencias, persistencia real, composición y ausencia de dependencias prohibidas.
- Mantener fuera de alcance calificaciones, rúbricas, archivos adjuntos, evidencias digitales, reportes y campos detallados de planeación NEM.

## Capabilities

### New Capabilities

- `proyectos-didacticos`: agregado, invariantes, estados, reapertura, periodo y eliminación de proyectos del grupo.
- `actividades-proyecto`: agregado de actividad, padrón y entregas, historial, anulación, eliminación restringida y atomicidad por actividad.
- `casos-uso-proyectos-actividades`: contratos, preparación, comandos, consultas, snapshots, ordenamientos, conteos y conflictos coordinados por Application.
- `persistencia-sqlite-proyectos-actividades`: esquema v3, migración desde v2, integridad relacional y persistencia transaccional de proyectos, actividades y entregas.
- `interfaz-proyectos-actividades`: ViewModels y experiencia WPF integrada para gestionar proyectos, actividades y entregas.

### Modified Capabilities

- `persistencia-sqlite-grupo-estudiantes`: actualizar la versión vigente de la base a v3, conservando las estructuras y garantías existentes de grupo, estudiantes y asistencia durante inicialización, migración y reapertura.

## Impact

- **Core:** nuevos agregados, identidades, estados y reglas de proyecto, actividad y entrega; sin dependencias nuevas.
- **Application:** puertos específicos y casos de uso para proyectos y actividades, snapshots inmutables, validación coordinada y conflictos de concurrencia y periodo.
- **Data:** migración SQLite v2→v3, nuevas tablas, restricciones, índices y adaptadores con `Microsoft.Data.Sqlite` directo.
- **Presentation:** ViewModels portables, comandos, filtros, confirmaciones, edición con snapshot y mensajes seguros.
- **App.Wpf:** nueva navegación a Proyectos, vista redimensionable de tres zonas, grilla de entregas y composición manual.
- **Reporting:** sin funcionalidad nueva; consumirá estos datos en cambios posteriores.
- **Compatibilidad:** se preservan base, grupos, estudiantes, asistencia y `app-state.json`; no se introducen ORM, contenedor DI, asincronía, repositorios genéricos, framework de navegación ni paquetes UI externos.
