## 1. Dominio de asistencia

- [x] 1.1 Añadir `EstadoAsistencia`, `RegistroAsistencia` y `AsistenciaDiaria` en Core con la identidad natural grupo-fecha y vistas de solo lectura.
- [x] 1.2 Implementar creación, cambio de estado y validación atómica de exactamente un estado mutuamente excluyente por registro, incluida `Justificada` como ausencia justificada sin datos adicionales.
- [x] 1.3 Implementar la fábrica pública neutral de rehidratación que conserve grupo, fecha, estudiantes y estados sin depender de Data.
- [x] 1.4 Añadir pruebas Core de creación, fechas distintas, estados válidos e inválidos, exclusividad, duplicados, estudiante ausente, atomicidad, datos conservados, solo lectura y rehidratación válida e inválida.

## 2. Contratos y casos de uso

- [x] 2.1 Añadir `IAlmacenamientoAsistencias` con carga, existencia y guardado completo mediante tipos de Core.
- [x] 2.2 Añadir entradas y snapshots inmutables de asistencia, incluir situación activa actual, materializar arreglos nuevos y ordenar por número, nombre e identidad sin duplicar nombre ni número en el agregado.
- [x] 2.3 Implementar carga y existencia distinguiendo ausencia real de errores de persistencia.
- [x] 2.4 Implementar preparación de un día ausente con estudiantes activos en Presente, sin guardado implícito.
- [x] 2.5 Implementar preparación de un día histórico mostrando todo su padrón, incluidos estudiantes ahora inactivos, sin agregar retroactivamente estudiantes nuevos y conservando reactivados.
- [x] 2.6 Implementar guardado de día nuevo con grupo fresco, exactamente una entrada por estudiante activo, rechazo de faltantes, duplicados o ajenos y una única llamada final a Guardar.
- [x] 2.7 Implementar guardado de día existente con grupo y agregado histórico frescos, exactamente una entrada por fila mostrada, actualización sobre la misma instancia y una única llamada final sin borrar registros.
- [x] 2.8 Añadir dobles manuales y pruebas Application de ausencia, preparación sin guardado, estados predeterminados, orden, snapshots inmutables e identidades estables.
- [x] 2.9 Añadir pruebas Application de guardado nuevo y existente, conjuntos faltantes, duplicados o ajenos, cero guardados tras error de dominio y exactamente un guardado tras éxito.
- [x] 2.10 Añadir pruebas Application de estudiante desactivado visible, estudiante reactivado conservado, estudiante nuevo ausente del histórico, persistencia sin borrado y recarga del estado anterior tras fallo al guardar.
- [x] 2.11 Verificar mediante prueba de referencias que Application y Application.Tests no cargan Data, SQLite, Presentation ni WPF.

## 3. Esquema SQLite versión 2

- [x] 3.1 Extender el inicializador para crear directamente la versión 2 en una base nueva y conservar los rechazos existentes de versión 0 con objetos y archivos no SQLite.
- [x] 3.2 Implementar la validación completa de v1 y su migración mediante una sola transacción, estableciendo `user_version = 2` únicamente después de crear correctamente índice, tablas e índices.
- [x] 3.3 Crear tablas de días y registros, checks de fecha ISO y estado, claves foráneas, clave candidata de pertenencia e índices requeridos.
- [x] 3.4 Validar de forma completa e idempotente una versión 2 y rechazar versiones posteriores o estructuras incompatibles sin reparación automática.
- [x] 3.5 Añadir pruebas SQLite reales con una base v1 auténtica y datos para base nueva, migración, reapertura, inicialización idempotente, versión posterior y estructuras incompatibles.
- [x] 3.6 Añadir una prueba de fallo inducido de migración que conserve `user_version = 1`, no deje objetos parciales y no altere grupos ni estudiantes.
- [x] 3.7 Añadir pruebas SQLite de cadena de fecha vacía, formato incorrecto, mes 13, día 00, 31 de febrero, 29 de febrero no bisiesto, fecha con hora y fecha bisiesta válida.
- [x] 3.8 Añadir pruebas SQLite reales de estados fuera de rango, duplicados, claves foráneas y estudiante perteneciente a otro grupo.

## 4. Adaptador SQLite de asistencia

- [x] 4.1 Implementar carga y existencia por grupo-fecha con `PRAGMA foreign_keys = ON`, ausencia normal, análisis estricto `DateOnly`, recorrido canónico y rehidratación completa.
- [x] 4.2 Implementar guardado transaccional mediante upsert sin borrar físicamente ningún registro del padrón histórico.
- [x] 4.3 Traducir errores propios de Data una sola vez a `ErrorPersistenciaAplicacionException` conservando `InnerException`.
- [x] 4.4 Añadir pruebas de contrato con archivo temporal único para carga, existencia, estados, identidades, actualización y reapertura.
- [x] 4.5 Añadir una prueba con trigger `RAISE(ABORT)` que demuestre rollback del encabezado y registros después de un fallo real a mitad del guardado.
- [x] 4.6 Añadir pruebas de conservación y edición de registros históricos inactivos, aislamiento entre archivos temporales y traducción de errores con causa técnica.

## 5. ViewModel de asistencia

- [x] 5.1 Añadir abstracciones comprobables para casos de uso de asistencia, reloj local y diálogo Guardar/Descartar/Cancelar sin referencias a Data ni WPF.
- [x] 5.2 Implementar el modelo observable de fila y `GestionAsistenciaViewModel` con fecha, estados, conteos, total y comandos síncronos.
- [x] 5.3 Implementar snapshot confirmado, copia editable, `EsPersistido`, detección de cambios y Guardar habilitado para todo borrador nuevo aunque todas sus filas estén en Presente.
- [x] 5.4 Implementar carga de fecha, guardado completo único, Marcar todos presentes y actualización visual sólo después de éxito.
- [x] 5.5 Implementar el flujo Guardar/Descartar/Cancelar reutilizable al cambiar fecha, navegar a Grupo y cerrar; impedir la transición cuando guardar falla y conservar todo el contexto al cancelar.
- [x] 5.6 Implementar mensajes seguros en español para validación, conflicto y persistencia, conservando edición y snapshot confirmado ante errores.
- [x] 5.7 Añadir pruebas Presentation de fecha local inicial, fecha inválida, día nuevo pendiente, día guardado, orden recibido, inactivo histórico visible y ausencia de guardado al abrir.
- [x] 5.8 Añadir pruebas Presentation de edición de todas las filas históricas, conteos completos, Marcar todos presentes, «Falta justificada», `Ctrl+S`, habilitación de Guardar y una sola operación completa por acción.
- [x] 5.9 Añadir pruebas Presentation de guardado exitoso, fallo de dominio, fallo de persistencia y ausencia de actualización visual falsa.
- [x] 5.10 Añadir pruebas Presentation del mismo flujo Guardar/Descartar/Cancelar al cambiar fecha, navegar a Grupo y cerrar, incluido borrador nuevo, cancelación íntegra y fallo al guardar.
- [x] 5.11 Añadir una prueba o smoke test de captura y conteos con entre 30 y 40 filas.
- [x] 5.12 Verificar mediante prueba de referencias que Presentation y sus pruebas no cargan Data, Microsoft.Data.Sqlite ni ventanas reales.

## 6. Integración WPF

- [x] 6.1 Ampliar la raíz de composición manual para crear el adaptador y los casos de uso de asistencia usando la ruta productiva existente.
- [x] 6.2 Integrar navegación mínima entre gestión de grupo y asistencia en `MainWindow` sin framework de navegación.
- [x] 6.3 Crear la vista de asistencia con `DatePicker`, `DataGrid`, «Falta justificada», «Inactivo actualmente», conteos de todo el padrón visible, indicador de guardado y acciones accesibles por teclado.
- [x] 6.4 Enlazar Marcar todos presentes y `Ctrl+S`, deshabilitar acciones incompatibles durante operaciones y no mostrar identidades internas.
- [x] 6.5 Implementar el servicio WPF de mensajes y confirmación triple, limitando el code-behind al comportamiento visual y al cierre cancelable.
- [x] 6.6 Añadir pruebas de composición y smoke tests no interactivos para verificar dependencias, navegación y ausencia de acceso SQL desde vistas y controles.

## 7. Verificación

- [x] 7.1 Ejecutar `dotnet restore` y registrar el resultado exacto.
- [x] 7.2 Ejecutar `dotnet format --verify-no-changes` y corregir únicamente formato relacionado con el cambio.
- [x] 7.3 Ejecutar `dotnet build` y resolver errores o advertencias introducidos por el cambio.
- [x] 7.4 Ejecutar `dotnet test` y confirmar el resultado completo de todas las capas.
- [x] 7.5 Ejecutar `openspec validate --all` y confirmar que todos los artefactos y especificaciones son válidos.
- [x] 7.6 Revisar el diff final para confirmar que no se introdujeron EF Core, Dapper, toolkit MVVM, MediatR, repositorios genéricos, contenedor DI ni funcionalidades fuera de alcance.

## 8. Proyección mensual de Application

- [x] 8.1 Añadir `ICalendarioLectivo` y su implementación lunes-viernes, con pruebas de laborables y fines de semana.
- [x] 8.2 Añadir snapshots inmutables de mes, columnas, estudiantes y celdas, materializando arreglos nuevos sin exponer agregados.
- [x] 8.3 Ampliar `IAlmacenamientoAsistencias` con carga específica por intervalo inclusivo y sin crear un repositorio genérico.
- [x] 8.4 Implementar `CargarMes` con validación de año/mes y generación correcta del intervalo de meses de 28, 29, 30 y 31 días.
- [x] 8.5 Construir la unión de matrícula activa actual y padrones históricos, conservando inactivos con historial y excluyendo altas retroactivas de días guardados.
- [x] 8.6 Proyectar borrador Presente para activos en fechas lectivas no guardadas y no aplicable para estudiantes fuera del padrón.
- [x] 8.7 Implementar orden determinista, conteos confirmados y porcentaje `(Presentes + Retardos) / días contabilizados × 100`, incluida ausencia con denominador cero.
- [x] 8.8 Implementar guardado del día seleccionado reutilizando el comando diario y una sola persistencia del agregado.
- [x] 8.9 Implementar guardado secuencial de fechas ordenadas y `GuardadoMesInterrumpidoException` con éxitos previos, fecha fallida y causa traducida.
- [x] 8.10 Añadir pruebas Application de longitudes, fechas, abreviaturas, laborabilidad, unión histórica, orden, conteos, porcentajes y consultas sin estado compartido.
- [x] 8.11 Añadir pruebas Application de error técnico al cargar intervalo, guardado de un día, varios días y fallo intermedio sin intentar fechas posteriores.
- [x] 8.12 Verificar que Core no incorpora agregado mensual y que Application mantiene independencia de Data, SQLite, Presentation y WPF.

## 9. Consulta SQLite por intervalo

- [x] 9.1 Implementar carga inclusiva por `GrupoId` y rango `DateOnly` usando una sola conexión y parámetros.
- [x] 9.2 Rehidratar todos los agregados encontrados en orden de fecha con análisis canónico estricto y `PRAGMA foreign_keys = ON`.
- [x] 9.3 Mantener `user_version = 2`, tablas y restricciones sin migración ni objetos mensuales.
- [x] 9.4 Traducir una sola vez errores de consulta por intervalo y no exponer `SqliteException`.
- [x] 9.5 Añadir pruebas Data de rango inválido, mes vacío, mes parcial, mes completo, reapertura y conservación histórica.
- [x] 9.6 Añadir pruebas Data de error técnico y confirmar mediante esquema real que la consulta mensual no modifica datos ni versión.

## 10. ViewModel mensual

- [x] 10.1 Añadir modelos visuales de mes, día y celda, con snapshot confirmado, copia editable y `HashSet<DateOnly>` de fechas modificadas.
- [x] 10.2 Implementar mes/año iniciales, navegación anterior/siguiente, cambio de año e Ir al mes actual con validación completa.
- [x] 10.3 Implementar selección de día/celda y texto de fecha completa en español sin exponer identidades.
- [x] 10.4 Implementar asignación `P/F/R/J`, avance a la siguiente fila visible y restauración al cancelar edición.
- [x] 10.5 Implementar Marcar todo presente y Descartar cambios limitados exclusivamente a la columna activa.
- [x] 10.6 Implementar Guardar día habilitado para una fecha nueva o modificada, deshabilitado para una fecha persistida sin cambios, y con reemplazo inmediato del snapshot mensual confirmado tras el éxito sin recargar el mes.
- [x] 10.7 Implementar Guardar cambios del mes con orden determinista, progreso por fecha, detención ante fallo y conservación de días pendientes.
- [x] 10.8 Implementar estados Guardado, Cambios sin guardar, Mes parcialmente guardado y Sin registros en el mes.
- [x] 10.9 Implementar conteos visuales, porcentajes confirmados y señal explícita cuando las tarjetas incluyen borradores.
- [x] 10.10 Implementar búsqueda por nombre y filtros Todos, Con incidencias, Sólo activos y Activos e inactivos históricos sin alterar el orden base.
- [x] 10.11 Reutilizar Guardar/Descartar/Cancelar al cambiar mes o año, navegar a Grupo y cerrar, impidiendo la transición ante fallo.
- [x] 10.12 Añadir pruebas Presentation de selector mensual, longitudes del intervalo, encabezados, selección, atajos, avance y fechas modificadas.
- [x] 10.13 Añadir pruebas Presentation de habilitación de Guardar día para fechas nuevas o modificadas, deshabilitación para persistidas sin cambios, actualización mensual inmediata, guardado diario/mensual, fallos, confirmaciones, conteos, porcentajes, filtros y búsqueda.
- [x] 10.14 Añadir prueba de rendimiento funcional con 40 estudiantes durante un mes de 31 días y verificar independencia de Data, SQLite y WPF.

## 11. Grilla mensual WPF

- [x] 11.1 Integrar la vista mensual como predeterminada y conservar la vista diaria como modo alternativo dentro de `MainWindow`.
- [x] 11.2 Construir `DataGrid` con Número y Nombre congelados, columnas lectivas dinámicas y cinco columnas de resumen.
- [x] 11.3 Crear encabezados dobles con número y abreviatura; mantener encabezados visibles durante scroll vertical.
- [x] 11.4 Crear celdas compactas `P/F/R/J/—`, estilos contrastados y texto accesible sin `ComboBox` permanente.
- [x] 11.5 Implementar clic simple, doble clic/Enter con selector compacto, Escape, flechas, Home/End y avance vertical como comportamiento visual.
- [x] 11.6 Enlazar `Ctrl+S`, `PageUp`, `PageDown`, acciones de columna, selector español de mes/año e Ir al mes actual.
- [x] 11.7 Añadir tarjetas superiores, leyenda, filtros, búsqueda y barra inferior fija con habilitación basada en estado real.
- [x] 11.8 Limitar code-behind a columnas dinámicas, foco, selección y traducción de eventos visuales; mantener reglas y persistencia fuera.
- [x] 11.9 Añadir pruebas de composición y smoke tests de columnas congeladas, 40 filas en un mes completo, atajos, ausencia de IDs y ausencia de SQL en vistas.

## 12. Verificación de la evolución mensual

- [x] 12.1 Ejecutar `dotnet restore` y registrar el resultado exacto.
- [x] 12.2 Ejecutar `dotnet format --verify-no-changes` y corregir sólo formato relacionado.
- [x] 12.3 Ejecutar `dotnet build` con cero errores y sin advertencias nuevas.
- [x] 12.4 Ejecutar `dotnet test` y confirmar todas las suites.
- [x] 12.5 Ejecutar `openspec validate --all` y revisar el diff contra prohibiciones y alcance.
- [ ] 12.6 Probar manualmente febrero, abril y agosto, navegación mensual, 30–40 estudiantes, scroll, columnas congeladas, teclado, guardado diario y mensual, fallo simulado, reapertura, inactivo histórico, filtros, porcentaje y redimensionamiento.

## 13. Columnas exclusivamente lectivas

- [x] 13.1 Proyectar únicamente fechas de lunes a viernes y actualizar snapshots, celdas, conteos y porcentajes para operar sólo sobre columnas lectivas guardadas.
- [x] 13.2 Marcar como cierre semanal cada viernes con una fecha lectiva posterior, sin contar bloques fijos de cinco columnas ni marcar el último viernes cuando sea la última columna.
- [x] 13.3 Adaptar ViewModel y navegación para que la fecha siguiente a viernes sea lunes y no exista representación de fines de semana.
- [x] 13.4 Adaptar las columnas dinámicas WPF, encabezados y leyenda; aplicar un borde derecho neutro y grueso al cierre semanal sin crear columnas adicionales.
- [x] 13.5 Añadir pruebas de inicios y finales en fin de semana o a mitad de semana, febrero de 28/29 días, cantidad lectiva, cierres semanales y navegación viernes-lunes.
- [x] 13.6 Ejecutar formato, compilación, pruebas y validación OpenSpec de esta corrección.
