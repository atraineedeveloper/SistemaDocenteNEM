## Context

El corte diario ya implementado contiene `AsistenciaDiaria`, casos de uso diarios, persistencia SQLite v2 y una vista WPF funcional. La revisión manual confirmó la corrección funcional, pero reveló que la lista vertical con `ComboBox` no escala bien como experiencia principal. La evolución debe reutilizar esas piezas y conservar las dependencias Core ← Application ← adaptadores/composición.

## Goals / Non-Goals

**Goals:**

- Ofrecer una captura mensual rápida, accesible y segura para 30–40 estudiantes y todas las fechas lectivas reales del mes.
- Coordinar la lectura mensual sin llamadas desordenadas desde Presentation.
- Hacer explícita la atomicidad diaria y el posible progreso parcial al guardar varios días.
- Mantener históricos, porcentajes y borradores distinguibles del estado confirmado.

**Non-Goals:**

- Crear un agregado mensual, una transacción mensual o un control de hoja de cálculo genérico.
- Cambiar el esquema SQLite v2, agregar paquetes UI o eliminar todavía la vista diaria.
- Añadir festivos, calendarios oficiales, reportes, gráficas, exportación o múltiples grupos.

## Decisions

### 1. El mes es una proyección; el día sigue siendo el agregado

Core no cambia. `AsistenciaDiaria` continúa siendo la única raíz y la única unidad atómica de persistencia. Application construirá `AsistenciaMesDetalle` a partir de la matrícula actual, las fechas del mes y cero o más agregados diarios rehidratados. Ningún tipo mensual tendrá mutaciones de dominio ni identidad propia.

### 2. Calendario lectivo mínimo

Application definirá `ICalendarioLectivo` con una operación equivalente a `EsLaborable(DateOnly)`. La implementación MVP considerará laborables lunes a viernes y no laborables sábado y domingo. No conocerá festivos. De este modo el ViewModel no codifica reglas de calendario y una implementación futura podrá ampliarse sin alterar la grilla.

La proyección de columnas incluirá únicamente fechas cuyo `DayOfWeek` sea `Monday`, `Tuesday`, `Wednesday`, `Thursday` o `Friday`. Sábados y domingos no producirán columnas ni celdas. Un mes que comience o termine en fin de semana o a mitad de semana empezará y terminará en su primera y última fecha lectiva real, sin columnas vacías.

### 3. Consulta mensual específica y carga por intervalo

`IAlmacenamientoAsistencias` añadirá una operación equivalente a `IReadOnlyList<AsistenciaDiaria> CargarIntervalo(GrupoId, DateOnly desde, DateOnly hasta)`. Data la implementará con una sola conexión, consultas parametrizadas y rehidratación completa de todos los días encontrados. No se crea un repositorio genérico.

`GestionAsistenciaCasosUso.CargarMes(grupoId, año, mes)` validará año 1–9999 y mes 1–12, calculará exactamente el intervalo, cargará una instancia fresca del grupo y el intervalo una vez, y materializará arreglos nuevos. Febrero bisiesto se derivará de `DateOnly`/`DateTime.DaysInMonth`, no de tablas manuales.

### 4. Forma exacta del snapshot mensual

Se añadirán records inmutables equivalentes a:

- `AsistenciaMesDetalle`: grupo, año, mes, días, estudiantes y días persistidos;
- `AsistenciaDiaColumnaDetalle`: fecha lectiva, número, abreviatura española, persistencia y señal visual de cierre semanal;
- `AsistenciaEstudianteMesDetalle`: identidad interna, nombre y número actuales, actividad actual, estados por fecha, conteos confirmados y porcentaje confirmado;
- un valor de celda que distingue estado confirmado, borrador Presente y no aplicable.

La unión de filas contiene estudiantes activos actuales más toda identidad presente en un padrón guardado del mes. Para un día guardado sólo existen celdas para su padrón histórico; un estudiante incorporado después obtiene una celda no aplicable. Para un día lectivo no guardado, los activos actuales reciben borrador `Presente` y los inactivos históricos reciben una celda no aplicable. Los nombres y números siempre proceden de la matrícula actual; no se reconstruye una fotografía histórica.

El orden contractual será número actual, nombre ordinal e identidad.

### 5. Resúmenes y fórmula aprobada

Los conteos confirmados consideran exclusivamente registros de días laborables persistidos. El denominador de un estudiante son los días laborables guardados en cuyo padrón histórico aparece. La fórmula es:

`(Presentes + Retardos) / días contabilizados × 100`

`Falta` y `Justificada` no cuentan como presencia; `Justificada` se muestra separadamente como «Falta justificada». Si el denominador es cero se muestra `—`. Presentation puede calcular conteos visuales sobre la copia editable, pero los marcará como borrador mientras difieran del snapshot confirmado.

### 6. Fechas modificadas y estados del mes

El ViewModel conservará el snapshot confirmado, una copia editable por celda y un `HashSet<DateOnly>` de fechas modificadas. Abrir el mes no agrega fechas al conjunto ni persiste los borradores Presente. Editar una celda o usar «Marcar todo presente» agrega su fecha; si todas sus celdas vuelven al snapshot, la fecha se retira.

Un día laborable no persistido puede guardarse explícitamente mediante «Guardar día» aunque aún no figure modificado. «Guardar cambios del mes» sólo procesa fechas del conjunto explícito, evitando persistir automáticamente todos los días futuros mostrados como borrador.

Los estados visibles se determinan así:

- `Sin registros en el mes`: ningún día persistido y ninguna fecha modificada;
- `Guardado`: existen registros y no hay fechas modificadas ni borradores explícitos;
- `Cambios sin guardar`: hay fechas modificadas;
- `Mes parcialmente guardado`: existen algunos días persistidos y otros días laborables permanecen como borrador; no representa un fallo parcial.

### 7. Guardado diario y secuencial

«Guardar día» usa el comando diario existente con la entrada completa de la columna activa. «Guardar cambios del mes» ordena las fechas ascendentemente y coordina un guardado diario por fecha. Después de cada éxito sustituye únicamente ese día en el snapshot confirmado y lo elimina del conjunto modificado.

Si una fecha falla, el coordinador se detiene. Application comunicará contexto mediante `GuardadoMesInterrumpidoException`, que contendrá las fechas confirmadas en esta ejecución, la fecha fallida y conservará como `InnerException` el error de persistencia ya traducido por Data. Esta excepción no vuelve a traducir el error técnico: añade progreso de la operación compuesta. Presentation informa los éxitos previos y mantiene editables la fecha fallida y las posteriores.

No existe rollback de fechas ya confirmadas y nunca se presenta el mes como atómico.

### 8. Estrategia WPF: DataGrid con columnas dinámicas

Se elige `DataGrid` con columnas generadas al cambiar de mes, frente a un control compuesto con `ScrollViewer` sincronizados. `DataGrid` ofrece selección de celda, virtualización de filas, desplazamiento, encabezados verticalmente persistentes y navegación básica ya probada. Las columnas 0 y 1 —número y nombre— usarán `FrozenColumnCount = 2`; después se genera una columna compacta por fecha lectiva real y al final cinco columnas de resumen.

Los encabezados de día serán controles visuales de dos líneas: número y abreviatura española. Un viernes tendrá borde derecho neutro y más grueso únicamente cuando exista otra fecha lectiva posterior en el mismo mes. La separación se deriva del `DayOfWeek` real, nunca de contar bloques de cinco columnas, y no crea una columna adicional. Las columnas de resumen se desplazan con el contenido; congelarlas también a la derecha requeriría dos grillas sincronizadas y se descarta por fragilidad. La barra inferior permanece fuera del scroll.

La generación de columnas y traducción de eventos de celda pueden vivir en un comportamiento visual o code-behind limitado; estados, validación, selección lógica, guardado y filtros permanecen en el ViewModel.

### 9. Interacción exacta de celda

- Clic simple: selecciona una celda, sin cambiar su estado.
- `P`, `F`, `R` o `J`: asigna el estado correspondiente y avanza a la siguiente fila visible de la misma fecha; en la última fila conserva la selección.
- Doble clic o `Enter`: abre un selector compacto con los cuatro estados; al elegir o presionar `Enter` confirma y cierra.
- `Escape`: cierra el selector y restaura el valor que tenía al iniciar esa edición.
- Flechas: mueven la celda activa con la navegación nativa de la grilla.
- `Home`/`End`: mueven al primer/último día editable de la fila cuando no está abierto el selector.
- `Ctrl+S`: guarda el día seleccionado, la acción principal.
- `PageUp`/`PageDown`: solicita cambio al mes anterior/siguiente cuando no se está editando una celda.

Nunca se usa un `ComboBox` permanente ni menú contextual como único mecanismo. Cada celda muestra `P/F/R/J` o un símbolo acompañado por texto accesible y estilos contrastados.

### 10. Selector, columna activa y acciones

La cabecera tendrá mes anterior, `ComboBox` de los 12 meses en español, selector de año válido en el rango de `DateOnly`, mes siguiente e «Ir al mes actual». Cambiar mes/año con fechas modificadas reutiliza Guardar/Descartar/Cancelar. Guardar continúa sólo si todas las fechas pendientes se confirman; un fallo conserva el mes actual. Cancelar conserva mes, selección y edición.

La columna activa expone una fecha completa con cultura española, por ejemplo «Lunes 3 de agosto de 2026». «Marcar todo el día como Presente» afecta sólo sus celdas editables; «Descartar cambios del día» restaura sólo esa fecha; «Guardar día» persiste sólo esa columna.

### 11. Filtros y estado visual

La búsqueda usa coincidencia parcial de nombre sin distinguir mayúsculas/minúsculas. Los filtros serán Todos, Con incidencias, Sólo activos y Activos e inactivos históricos. Incidencia significa al menos una Falta, Retardo o Falta justificada en el mes visual. Filtrar no cambia el orden ni la copia editable.

Las tarjetas superiores muestran alumnos visibles, días guardados y conteos visuales de los cuatro estados. Cuando incluyen borradores muestran explícitamente «Incluye cambios sin guardar». La barra inferior fija contiene estado, ayuda, Descartar cambios, Guardar día y Guardar cambios del mes.

### 12. Conservación temporal de la vista diaria

La vista mensual será predeterminada. Un conmutador interno permitirá abrir la vista diaria existente sin duplicar casos de uso ni estado; ambos modos usan los mismos comandos diarios. La vista diaria no se elimina hasta completar las pruebas manuales de la grilla mensual.

## Risks / Trade-offs

- **[Guardar mes puede terminar parcialmente]** → Orden determinista, snapshot actualizado tras cada éxito, excepción con fechas confirmadas y mensaje explícito.
- **[Hasta 23 columnas lectivas más resúmenes]** → Columnas compactas, dos congeladas, virtualización de filas y scroll horizontal.
- **[DataGrid no congela columnas a la derecha]** → Se prioriza estabilidad; los resúmenes permanecen al final y la barra inferior conserva las acciones visibles.
- **[Borradores Presente podrían confundirse con datos guardados]** → Estado por día, leyenda y marcadores de borrador; abrir nunca persiste.
- **[Nombres actuales en históricos]** → Se mantiene la decisión aprobada y se indica que no existe fotografía histórica.

## Migration Plan

1. Añadir snapshots, calendario y pruebas mensuales de Application sin alterar Core.
2. Añadir y probar la consulta SQLite por intervalo sobre el esquema v2 existente.
3. Implementar el ViewModel mensual, guardado diario/secuencial, filtros y pruebas sin WPF.
4. Integrar el `DataGrid` dinámico y mantener accesible la vista diaria existente.
5. Ejecutar pruebas de meses de 28–31 días, cantidades lectivas reales, límites semanales, 40 estudiantes, navegación, fallos intermedios y reapertura.
6. Realizar prueba manual de febrero, abril y agosto antes de considerar retirar la vista diaria en otro cambio.

No hay migración de datos ni de esquema. El rollback de esta evolución consiste en volver a la vista diaria; los agregados diarios guardados siguen siendo compatibles.

## Open Questions

Ninguna.
