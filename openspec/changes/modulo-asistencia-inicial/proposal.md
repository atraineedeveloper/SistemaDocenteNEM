# Change: Módulo inicial de asistencia con grilla mensual

## Why

La primera interfaz diaria ya permite persistir asistencia correctamente, pero una lista vertical con un selector permanente por estudiante no ofrece la velocidad ni la visión de conjunto que necesita un docente. El módulo debe evolucionar a una grilla mensual moderna sin reemplazar el dominio ni la persistencia diaria ya implementados.

## What Changes

- Convertir la grilla mensual en la interfaz principal de asistencia, con estudiantes en filas y los días reales del mes en columnas.
- Mantener `AsistenciaDiaria` como único agregado de Core; el mes será una proyección inmutable y una unidad visual, nunca un agregado ni una transacción de dominio.
- Añadir una consulta mensual coordinada en Application y una carga eficiente por intervalo en el puerto específico de asistencia.
- Mostrar la unión de estudiantes activos actuales y estudiantes presentes en cualquier padrón histórico del mes.
- Añadir selector de mes y año, navegación mensual y exclusivamente columnas lectivas de lunes a viernes; sábados y domingos no se representan en la grilla.
- Permitir captura rápida mediante selección, teclas `P/F/R/J`, `Enter`, flechas, `Escape`, `Ctrl+S`, `PageUp` y `PageDown`.
- Añadir guardado del día seleccionado y guardado secuencial de fechas modificadas, conservando la atomicidad SQLite por `AsistenciaDiaria`.
- Añadir resúmenes mensuales, porcentaje de asistencia, tarjetas generales, búsqueda y filtros de incidencias y situación actual.
- Conservar temporalmente la vista diaria existente como modo alternativo hasta que la grilla mensual supere la prueba manual.
- Ampliar las pruebas de Application, Data, Presentation y WPF sin añadir un agregado mensual ni cambiar el esquema SQLite.

## Capabilities

### New Capabilities

- `asistencia-diaria`: mantiene el agregado diario y sus invariantes como única unidad de dominio y persistencia.
- `casos-uso-asistencia`: amplía la coordinación diaria con proyección, consulta y guardado mensual secuencial.
- `persistencia-sqlite-asistencia`: amplía el adaptador diario con una consulta eficiente por intervalo, sin modificar el esquema v2.
- `interfaz-asistencia`: sustituye la captura principal por una grilla mensual y conserva la vista diaria como alternativa temporal.

### Modified Capabilities

- Ninguna.

## Impact

- **Core:** sin agregado mensual ni nuevas reglas; `AsistenciaDiaria` continúa intacta.
- **Application:** calendario lectivo mínimo, snapshots mensuales, consulta de mes, resúmenes y coordinación de guardados diarios.
- **Data:** consulta parametrizada por rango usando una conexión y el esquema SQLite v2 existente.
- **Presentation:** ViewModel mensual, celdas editables, conjunto de fechas modificadas, navegación, filtros, conteos y confirmaciones.
- **App.Wpf:** `DataGrid` con columnas lectivas dinámicas, dos columnas congeladas, encabezados dobles, separadores visuales tras los viernes que cierran una semana visible, leyenda, barra inferior y atajos.
- **Persistencia:** cada fecha conserva su propia transacción. Guardar el mes puede confirmar fechas anteriores antes de que falle una posterior; nunca se afirma atomicidad mensual.
- **Compatibilidad:** se reutilizan identidades, padrones históricos, nombres actuales, casos diarios, traducción de errores y esquema v2. No se agregan paquetes UI ni referencias arquitectónicas.
- **Fuera de alcance:** calendario SEP, festivos configurables, horarios, varias sesiones, asistencia por materia, reportes oficiales, impresión, exportación, gráficas, alertas, comunicación con padres, múltiples grupos, sincronización y dispositivos móviles.
