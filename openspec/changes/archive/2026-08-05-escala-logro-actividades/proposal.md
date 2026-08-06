## Why

La NEM organiza el aprendizaje por proyectos y evalua el desempeno del estudiante en cada actividad mediante una escala de logro de cuatro niveles, no mediante la simple verificacion de entrega. El modulo de Proyectos implementado registra unicamente `Entregada / NoEntregada / Pendiente`, lo que imposibilita capturar la calidad del trabajo realizado y priva al docente de la informacion formativa que necesita para hacer seguimiento real de cada alumno.

Adicionalmente, la interfaz WPF actual no refleja el flujo pedagogico natural: primero se crea el proyecto, luego se agregan actividades a ese proyecto y finalmente se evalua el desempeno de cada estudiante en cada actividad. La disposicion actual mezcla estos pasos de forma confusa y la experiencia de usuario es pobre.

## What Changes

- Reemplazar `EstadoEntrega` en Core con una escala de logro de cinco valores: `Domina` (D), `Suficiente` (S), `EnProceso` (EP), `RequiereApoyo` (RA) y `NoEntrego` (NE), donde los cuatro primeros representan niveles de desempeno para actividades entregadas y `NoEntrego` es el caso especial de incumplimiento.
- Actualizar `RegistroEntregaActividad` en Core, conteos en Application, adaptador SQLite en Data y esquema de base de datos (migrar `user_version = 3` a `user_version = 4`) para reflejar los cinco estados.
- Redisenar completamente la interfaz WPF del modulo Proyectos para reflejar el flujo pedagogico real: seleccion de proyecto, lista de actividades, grilla de evaluacion de desempeno por estudiante.
- Reemplazar los botones E/N/P por controles compactos que asignen los cinco niveles de logro, con atajos de teclado D/S/EP/RA/NE y un selector visual claro.
- Actualizar conteos en ViewModels y snapshots: `Domina`, `Suficiente`, `EnProceso`, `RequiereApoyo`, `NoEntrego` y `Pendiente`.
- Actualizar pruebas de todas las capas afectadas.

## Capabilities

### Modified Capabilities

- `actividades-proyecto`: reemplazar `EstadoEntrega` por escala de logro de cinco valores con sus invariantes, validacion y rehidratacion.
- `casos-uso-proyectos-actividades`: conteos y snapshots actualizados con los cinco estados; sin cambio en operaciones de coordinacion.
- `persistencia-sqlite-proyectos-actividades`: migracion v3 a v4 que redefine la columna de estado de entrega; CHECK actualizado.
- `interfaz-proyectos-actividades`: rediseno completo de la vista WPF con flujo proyecto, actividad, evaluacion y escala de logro NEM.

## Impact

- **Core:** `EstadoEntrega` cambia de 3 a 5 valores; todo codigo que compare o asigne estados de entrega debera actualizarse.
- **Application:** snapshots `EntregaActividadDetalle` y conteos de `ActividadProyectoDetalle` actualizados; `GuardarEntregasActividad` acepta los cinco estados.
- **Data:** migracion v3 a v4 con CHECK actualizado; adaptadores de lectura/escritura de entregas.
- **Presentation:** ViewModels de fila de entrega actualizados; comandos de teclado redisenados; filtros ampliados.
- **App.Wpf:** vista de tres zonas redisenada con flujo claro, controles de escala de logro y atajo por nivel.
- **Compatibilidad:** la migracion preserva grupos, estudiantes, asistencia, proyectos y actividades; solo actualiza la columna de estado de entregas existentes; las pendientes permanecen Pendiente.
