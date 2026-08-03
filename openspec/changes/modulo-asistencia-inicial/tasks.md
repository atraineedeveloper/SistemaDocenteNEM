## 1. Dominio de asistencia

- [ ] 1.1 Añadir `EstadoAsistencia`, `RegistroAsistencia` y `AsistenciaDiaria` en Core con la identidad natural grupo-fecha y vistas de solo lectura.
- [ ] 1.2 Implementar creación, cambio de estado y validación atómica de exactamente un estado mutuamente excluyente por registro, incluida `Justificada` como ausencia justificada sin datos adicionales.
- [ ] 1.3 Implementar la fábrica pública neutral de rehidratación que conserve grupo, fecha, estudiantes y estados sin depender de Data.
- [ ] 1.4 Añadir pruebas Core de creación, fechas distintas, estados válidos e inválidos, exclusividad, duplicados, estudiante ausente, atomicidad, datos conservados, solo lectura y rehidratación válida e inválida.

## 2. Contratos y casos de uso

- [ ] 2.1 Añadir `IAlmacenamientoAsistencias` con carga, existencia y guardado completo mediante tipos de Core.
- [ ] 2.2 Añadir entradas y snapshots inmutables de asistencia, incluir situación activa actual, materializar arreglos nuevos y ordenar por número, nombre e identidad sin duplicar nombre ni número en el agregado.
- [ ] 2.3 Implementar carga y existencia distinguiendo ausencia real de errores de persistencia.
- [ ] 2.4 Implementar preparación de un día ausente con estudiantes activos en Presente, sin guardado implícito.
- [ ] 2.5 Implementar preparación de un día histórico mostrando todo su padrón, incluidos estudiantes ahora inactivos, sin agregar retroactivamente estudiantes nuevos y conservando reactivados.
- [ ] 2.6 Implementar guardado de día nuevo con grupo fresco, exactamente una entrada por estudiante activo, rechazo de faltantes, duplicados o ajenos y una única llamada final a Guardar.
- [ ] 2.7 Implementar guardado de día existente con grupo y agregado histórico frescos, exactamente una entrada por fila mostrada, actualización sobre la misma instancia y una única llamada final sin borrar registros.
- [ ] 2.8 Añadir dobles manuales y pruebas Application de ausencia, preparación sin guardado, estados predeterminados, orden, snapshots inmutables e identidades estables.
- [ ] 2.9 Añadir pruebas Application de guardado nuevo y existente, conjuntos faltantes, duplicados o ajenos, cero guardados tras error de dominio y exactamente un guardado tras éxito.
- [ ] 2.10 Añadir pruebas Application de estudiante desactivado visible, estudiante reactivado conservado, estudiante nuevo ausente del histórico, persistencia sin borrado y recarga del estado anterior tras fallo al guardar.
- [ ] 2.11 Verificar mediante prueba de referencias que Application y Application.Tests no cargan Data, SQLite, Presentation ni WPF.

## 3. Esquema SQLite versión 2

- [ ] 3.1 Extender el inicializador para crear directamente la versión 2 en una base nueva y conservar los rechazos existentes de versión 0 con objetos y archivos no SQLite.
- [ ] 3.2 Implementar la validación completa de v1 y su migración mediante una sola transacción, estableciendo `user_version = 2` únicamente después de crear correctamente índice, tablas e índices.
- [ ] 3.3 Crear tablas de días y registros, checks de fecha ISO y estado, claves foráneas, clave candidata de pertenencia e índices requeridos.
- [ ] 3.4 Validar de forma completa e idempotente una versión 2 y rechazar versiones posteriores o estructuras incompatibles sin reparación automática.
- [ ] 3.5 Añadir pruebas SQLite reales con una base v1 auténtica y datos para base nueva, migración, reapertura, inicialización idempotente, versión posterior y estructuras incompatibles.
- [ ] 3.6 Añadir una prueba de fallo inducido de migración que conserve `user_version = 1`, no deje objetos parciales y no altere grupos ni estudiantes.
- [ ] 3.7 Añadir pruebas SQLite de cadena de fecha vacía, formato incorrecto, mes 13, día 00, 31 de febrero, 29 de febrero no bisiesto, fecha con hora y fecha bisiesta válida.
- [ ] 3.8 Añadir pruebas SQLite reales de estados fuera de rango, duplicados, claves foráneas y estudiante perteneciente a otro grupo.

## 4. Adaptador SQLite de asistencia

- [ ] 4.1 Implementar carga y existencia por grupo-fecha con `PRAGMA foreign_keys = ON`, ausencia normal, análisis estricto `DateOnly`, recorrido canónico y rehidratación completa.
- [ ] 4.2 Implementar guardado transaccional mediante upsert sin borrar físicamente ningún registro del padrón histórico.
- [ ] 4.3 Traducir errores propios de Data una sola vez a `ErrorPersistenciaAplicacionException` conservando `InnerException`.
- [ ] 4.4 Añadir pruebas de contrato con archivo temporal único para carga, existencia, estados, identidades, actualización y reapertura.
- [ ] 4.5 Añadir una prueba con trigger `RAISE(ABORT)` que demuestre rollback del encabezado y registros después de un fallo real a mitad del guardado.
- [ ] 4.6 Añadir pruebas de conservación y edición de registros históricos inactivos, aislamiento entre archivos temporales y traducción de errores con causa técnica.

## 5. ViewModel de asistencia

- [ ] 5.1 Añadir abstracciones comprobables para casos de uso de asistencia, reloj local y diálogo Guardar/Descartar/Cancelar sin referencias a Data ni WPF.
- [ ] 5.2 Implementar el modelo observable de fila y `GestionAsistenciaViewModel` con fecha, estados, conteos, total y comandos síncronos.
- [ ] 5.3 Implementar snapshot confirmado, copia editable, `EsPersistido`, detección de cambios y Guardar habilitado para todo borrador nuevo aunque todas sus filas estén en Presente.
- [ ] 5.4 Implementar carga de fecha, guardado completo único, Marcar todos presentes y actualización visual sólo después de éxito.
- [ ] 5.5 Implementar el flujo Guardar/Descartar/Cancelar reutilizable al cambiar fecha, navegar a Grupo y cerrar; impedir la transición cuando guardar falla y conservar todo el contexto al cancelar.
- [ ] 5.6 Implementar mensajes seguros en español para validación, conflicto y persistencia, conservando edición y snapshot confirmado ante errores.
- [ ] 5.7 Añadir pruebas Presentation de fecha local inicial, fecha inválida, día nuevo pendiente, día guardado, orden recibido, inactivo histórico visible y ausencia de guardado al abrir.
- [ ] 5.8 Añadir pruebas Presentation de edición de todas las filas históricas, conteos completos, Marcar todos presentes, «Falta justificada», `Ctrl+S`, habilitación de Guardar y una sola operación completa por acción.
- [ ] 5.9 Añadir pruebas Presentation de guardado exitoso, fallo de dominio, fallo de persistencia y ausencia de actualización visual falsa.
- [ ] 5.10 Añadir pruebas Presentation del mismo flujo Guardar/Descartar/Cancelar al cambiar fecha, navegar a Grupo y cerrar, incluido borrador nuevo, cancelación íntegra y fallo al guardar.
- [ ] 5.11 Añadir una prueba o smoke test de captura y conteos con entre 30 y 40 filas.
- [ ] 5.12 Verificar mediante prueba de referencias que Presentation y sus pruebas no cargan Data, Microsoft.Data.Sqlite ni ventanas reales.

## 6. Integración WPF

- [ ] 6.1 Ampliar la raíz de composición manual para crear el adaptador y los casos de uso de asistencia usando la ruta productiva existente.
- [ ] 6.2 Integrar navegación mínima entre gestión de grupo y asistencia en `MainWindow` sin framework de navegación.
- [ ] 6.3 Crear la vista de asistencia con `DatePicker`, `DataGrid`, «Falta justificada», «Inactivo actualmente», conteos de todo el padrón visible, indicador de guardado y acciones accesibles por teclado.
- [ ] 6.4 Enlazar Marcar todos presentes y `Ctrl+S`, deshabilitar acciones incompatibles durante operaciones y no mostrar identidades internas.
- [ ] 6.5 Implementar el servicio WPF de mensajes y confirmación triple, limitando el code-behind al comportamiento visual y al cierre cancelable.
- [ ] 6.6 Añadir pruebas de composición y smoke tests no interactivos para verificar dependencias, navegación y ausencia de acceso SQL desde vistas y controles.

## 7. Verificación

- [ ] 7.1 Ejecutar `dotnet restore` y registrar el resultado exacto.
- [ ] 7.2 Ejecutar `dotnet format --verify-no-changes` y corregir únicamente formato relacionado con el cambio.
- [ ] 7.3 Ejecutar `dotnet build` y resolver errores o advertencias introducidos por el cambio.
- [ ] 7.4 Ejecutar `dotnet test` y confirmar el resultado completo de todas las capas.
- [ ] 7.5 Ejecutar `openspec validate --all` y confirmar que todos los artefactos y especificaciones son válidos.
- [ ] 7.6 Revisar el diff final para confirmar que no se introdujeron EF Core, Dapper, toolkit MVVM, MediatR, repositorios genéricos, contenedor DI ni funcionalidades fuera de alcance.
