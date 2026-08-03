## Why

El modelo de grupos y estudiantes existe únicamente en memoria, por lo que sus identidades, datos y estados no sobreviven al cierre de la aplicación. Se necesita una persistencia SQLite local, transaccional e independiente de WPF que conserve el agregado sin debilitar sus reglas de dominio.

## What Changes

- Incorporar en `SistemaDocente.Data` persistencia SQLite local mediante acceso directo con `Microsoft.Data.Sqlite`.
- Exigir que Data reciba siempre una ruta explícita; la futura composición de App.Wpf usará por defecto `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db`, sin que Data consulte carpetas del sistema ni dependa de WPF.
- Crear e inicializar automáticamente un esquema versión 1 para grupos y estudiantes, con comprobación estricta de versión y estructura, claves, relación, restricciones e índices.
- Permitir guardar grupos nuevos, actualizar agregados existentes, cargar por `GrupoId` y representar explícitamente la ausencia de un grupo.
- Guardar el agregado completo dentro de una única transacción y garantizar rollback completo ante errores, incluidos fallos reales a mitad de la operación.
- Conservar exactamente identidades, nombres normalizados, números de lista y estados activos o inactivos.
- Añadir a Core una fábrica pública neutral equivalente a `Grupo.Rehidratar`, junto con un tipo neutral para los datos de cada estudiante, sin cambiar `Grupo.Crear` ni `AgregarEstudiante`.
- Impedir que un `EstudianteId` almacenado se traslade a otro `GrupoId` mediante upsert.
- Definir errores de infraestructura y rechazo no destructivo de archivos dañados, versiones posteriores o estructuras incompatibles.
- Añadir pruebas de integración con archivos SQLite temporales aislados, sin instalación externa.
- Mantener WPF y las demás funciones docentes fuera del acceso directo a SQLite y fuera de este cambio.

## Capabilities

### New Capabilities

- `persistencia-sqlite-grupo-estudiantes`: Define creación, inicialización estricta, guardado transaccional, actualización y carga del agregado de grupo en una base SQLite local.

### Modified Capabilities

- `gestion-grupo-estudiantes`: Incorpora una fábrica pública y neutral para reconstruir atómicamente grupos y estudiantes con identidades y estados existentes, sin cambiar las rutas de creación normal.

## Impact

- Proyectos futuros afectados: `SistemaDocente.Data`, `SistemaDocente.Data.Tests` y el contrato neutral de rehidratación necesario en `SistemaDocente.Core` y `SistemaDocente.Core.Tests`.
- Nueva dependencia prevista exclusivamente en Data: `Microsoft.Data.Sqlite`; Data.Tests la consumirá transitivamente.
- La futura composición de App.Wpf será responsable de calcular la ruta productiva predeterminada y entregarla a Data; App.Wpf no ejecutará SQL ni dependerá de tipos del proveedor.
- Core permanecerá sin referencias a Data, SQLite o `Microsoft.Data.Sqlite`; no se utilizará `InternalsVisibleTo`.
- No se incorporarán Entity Framework Core, Dapper, servidores de base de datos ni modalidad portable.
- Permanecen fuera de alcance UI, ventanas, ViewModels, navegación, servicios visuales, asistencia, actividades, evaluación, reportes, respaldos, cifrado, concurrencia multiusuario, importación desde Excel y datos personales adicionales.
