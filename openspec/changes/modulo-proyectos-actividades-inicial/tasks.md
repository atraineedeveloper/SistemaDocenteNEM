## 1. Dominio de proyectos

- [x] 1.1 Añadir `ProyectoId`, `EstadoProyecto` y `ProyectoDidactico` con identidad, grupo inmutable, datos, periodo, estado y versión.
- [x] 1.2 Implementar normalización y límites de nombre, descripción y observaciones, periodo inclusivo válido y creación inicial en Borrador.
- [x] 1.3 Implementar transiciones Borrador→EnCurso, EnCurso→Finalizado y reapertura Finalizado→EnCurso, rechazando las demás de forma atómica.
- [x] 1.4 Implementar actualización de datos y periodo sin permitir cambiar GrupoId ni versión arbitrariamente.
- [x] 1.5 Añadir rehidratación neutral de proyecto que valide identidad, estado, versión y todos los datos antes de devolver el agregado.
- [x] 1.6 Añadir pruebas Core de nombres, límites, normalización, periodos invertidos y duraciones atípicas permitidas.
- [x] 1.7 Añadir pruebas Core de estados, transiciones inválidas, reapertura, grupo inmutable, versión y atomicidad de cambios.
- [x] 1.8 Añadir pruebas Core de rehidratación válida e inválida y ausencia de colecciones o mutabilidad expuestas.

## 2. Dominio de actividades y entregas

- [x] 2.1 Añadir `ActividadId`, `EstadoActividad`, `EstadoEntrega`, registro de entrega y `ActividadProyecto` como agregado independiente.
- [x] 2.2 Implementar título obligatorio normalizado, límites de textos, fecha dentro del periodo y pertenencias inmutables a proyecto y grupo.
- [x] 2.3 Implementar padrón completo con una entrega única por estudiante, estados explícitos y observación opcional limitada.
- [x] 2.4 Implementar cambios de datos y entregas como mutaciones atómicas, sin API de dominio para mover actividad ni guardar registros aislados.
- [x] 2.5 Implementar anulación irreversible y reglas que dejan una actividad anulada sin edición de datos o entregas.
- [x] 2.6 Añadir rehidratación neutral completa de actividad y entregas con validación previa de todas las invariantes.
- [x] 2.7 Añadir pruebas Core de creación, periodo dentro/fuera, pertenencias, títulos, textos y versiones.
- [x] 2.8 Añadir pruebas Core de padrón completo, duplicados, estados inválidos, observaciones, cambios y atomicidad.
- [x] 2.9 Añadir pruebas Core de anulación, edición rechazada tras anular y rehidratación válida e inválida.

## 3. Contratos y casos de uso de proyectos

- [x] 3.1 Definir `IAlmacenamientoProyectos` con operaciones específicas de carga, listado, guardado versionado, consulta de actividades incompatibles y eliminación restringida.
- [x] 3.2 Añadir entradas y snapshots inmutables `ProyectoResumen` y `ProyectoDetalle`, materializando arreglos y sin exponer agregados.
- [x] 3.3 Implementar CrearProyecto y ObtenerProyecto con validación de grupo y traducción de ausencia normal.
- [x] 3.4 Implementar ListarProyectosDelGrupo con orden por estado, fecha inicial descendente, nombre e identidad.
- [x] 3.5 Implementar ActualizarProyecto con carga fresca, versión esperada y bloqueo del periodo cuando existan fechas incompatibles ordenadas.
- [x] 3.6 Implementar CambiarEstadoProyecto y reapertura explícita con concurrencia optimista.
- [x] 3.7 Implementar EliminarProyectoBorradorSinActividades validando estado, versión y ausencia de cualquier actividad.
- [x] 3.8 Añadir excepciones identificables de periodo incompatible y concurrencia sin filtrar detalles técnicos.
- [x] 3.9 Añadir dobles manuales y pruebas Application de creación, obtención, ausencia, actualización, orden y snapshots inmutables.
- [x] 3.10 Añadir pruebas Application de reducción incompatible, transiciones, reapertura, versión obsoleta y eliminación permitida/rechazada.

## 4. Contratos y casos de uso de actividades

- [x] 4.1 Definir `IAlmacenamientoActividadesProyecto` con carga, listado, guardado versionado, anulación y eliminación específica sin repositorio genérico.
- [x] 4.2 Añadir entradas y snapshots inmutables `ActividadProyectoDetalle` y `EntregaActividadDetalle` con conteos y situación activa actual.
- [x] 4.3 Implementar PrepararNuevaActividad cargando proyecto y grupo frescos, activos en Pendiente y cero persistencias.
- [x] 4.4 Implementar CrearActividad con revalidación de proyecto editable, periodo, padrón activo completo y una sola persistencia del agregado.
- [x] 4.5 Implementar ObtenerActividad y ListarActividadesDelProyecto con enriquecimiento desde matrícula actual y orden determinista.
- [x] 4.6 Implementar ActualizarActividad sin mover pertenencias y bloqueando proyecto Finalizado o actividad Anulada.
- [x] 4.7 Implementar GuardarEntregasActividad sobre el padrón histórico completo, conservando inactivos y excluyendo altas retroactivas.
- [x] 4.8 Implementar AnularActividad con confirmación externa y EliminarActividadSinSeguimiento sólo cuando todas las entregas estén Pendiente.
- [x] 4.9 Calcular total, Pendiente, Entregada y NoEntregada sin porcentaje ni calificación y excluir anuladas de agregaciones.
- [x] 4.10 Añadir pruebas Application de preparación, ausencia de guardado, activos iniciales, histórico inactivo y alta no retroactiva.
- [x] 4.11 Añadir pruebas Application de crear, editar, guardar padrón completo, conteos, orden, anular y eliminar permitido/rechazado.
- [x] 4.12 Añadir pruebas Application de proyecto Finalizado, periodo inválido, pertenencias ajenas, versión obsoleta y fallo técnico conservable.
- [x] 4.13 Verificar mediante pruebas de referencias que Application y sus pruebas no cargan Data, SQLite, Presentation ni WPF.

## 5. Esquema SQLite versión 3

- [x] 5.1 Extender el inicializador para crear directamente v3 en una base nueva con las estructuras anteriores y las nuevas.
- [x] 5.2 Implementar validación completa de v2 y migración v2→v3 mediante una única transacción, estableciendo `user_version = 3` sólo al final.
- [x] 5.3 Mantener la ruta v1→v2→v3 con validación por etapa y rechazo no destructivo de versiones posteriores o estructuras incompatibles.
- [x] 5.4 Crear `proyectos_didacticos` con claves, FK de grupo, clave candidata, límites, fechas canónicas, estados, versión e índices.
- [x] 5.5 Crear `actividades_proyecto` con FK compuesta a proyecto/grupo, clave candidata, límites, fecha, estado, versión e índices.
- [x] 5.6 Crear `entregas_actividad` con PK compuesta y FKs compuestas a actividad/grupo y estudiante/grupo, límites y estados.
- [x] 5.7 Confirmar que ninguna relación histórica usa borrado en cascada y que todas las conexiones activan `PRAGMA foreign_keys = ON`.
- [x] 5.8 Añadir pruebas SQLite reales de base nueva v3, reapertura e inicialización idempotente.
- [x] 5.9 Añadir pruebas de migración v2 real con grupo, activos/inactivos y asistencias, verificando conservación exacta.
- [x] 5.10 Añadir prueba de fallo inducido de migración que conserve v2, datos y ausencia de objetos parciales.
- [x] 5.11 Añadir pruebas de rechazo de versión posterior y estructuras v1, v2 y v3 incompatibles sin reparación.
- [x] 5.12 Añadir pruebas directas de CHECK, fechas imposibles/no canónicas, estados, versiones, textos y claves compuestas.

## 6. Adaptadores SQLite de proyectos y actividades

- [x] 6.1 Implementar persistencia de proyectos por identidad y listado por grupo con consultas parametrizadas y rehidratación completa.
- [x] 6.2 Implementar inserción y actualización versionada de proyecto dentro de una transacción y detectar cero filas como conflicto concurrente.
- [x] 6.3 Implementar consulta de fechas incompatibles y eliminación de proyecto Borrador vacío sin cascadas.
- [x] 6.4 Implementar carga de actividad completa y listado por proyecto mediante una conexión por operación.
- [x] 6.5 Implementar guardado atómico del encabezado y todas las entregas, con versión esperada y sin conexión o transacción por estudiante.
- [x] 6.6 Implementar anulación versionada y eliminación explícita transaccional de entregas y actividad autorizada sin cascada.
- [x] 6.7 Traducir errores técnicos una sola vez a excepciones de Application conservando causa interna y sin exponer SQLite.
- [x] 6.8 Añadir pruebas de contrato de proyectos para CRUD, orden, versiones, periodos y reapertura sobre archivo temporal real.
- [x] 6.9 Añadir pruebas de contrato de actividades para padrón, estados, observaciones, orden, anuladas, históricos y reapertura.
- [x] 6.10 Añadir prueba con trigger `RAISE(ABORT)` que demuestre rollback de encabezado y entregas a mitad del guardado.
- [x] 6.11 Añadir pruebas de concurrencia, pertenencias de otro grupo/proyecto, eliminación restringida y ausencia de cascadas.
- [x] 6.12 Añadir pruebas de aislamiento entre archivos, cierre/reapertura y traducción de errores con causa técnica.

## 7. Presentation portable

- [x] 7.1 Añadir contratos de presentación para casos de uso, mensajes y confirmaciones de reapertura, eliminación, anulación y cambios pendientes.
- [x] 7.2 Implementar ViewModel contenedor de Proyectos con selección coordinada de proyecto y actividad.
- [x] 7.3 Implementar listado/editor de proyectos con filtros, orden recibido, crear, editar, guardar y advertencia no bloqueante de duración.
- [x] 7.4 Implementar comandos de estado, reapertura confirmada y eliminación con `CanExecute` basado en snapshot, estado, actividades y ocupación.
- [x] 7.5 Implementar listado/editor de actividades con búsqueda por texto/fecha, crear, editar, guardar, anular y eliminar según reglas.
- [x] 7.6 Implementar filas observables de entrega con número, nombre, situación actual, estado, observación y selección múltiple lógica.
- [x] 7.7 Implementar filtros Todos, Pendientes, Entregadas, No entregadas, Sólo incidencias, Activos y Activos e inactivos históricos.
- [x] 7.8 Implementar comandos E/N/P, marcar selección, marcar todos Entregada, conteos y actualización inmediata de estado visual.
- [x] 7.9 Implementar snapshot confirmado, copia editable, `TieneCambios`, Guardar y Descartar sin presentar éxitos falsos.
- [x] 7.10 Reutilizar Guardar/Descartar/Cancelar al cambiar actividad, proyecto, módulo o cerrar, bloqueando transición ante fallo.
- [x] 7.11 Implementar mensajes seguros para validación, periodo, concurrencia y persistencia conservando edición local.
- [x] 7.12 Notificar propiedades y `CanExecute` tras selección, edición, filtros, guardado, descarte, estados y `EstaOcupado`.
- [x] 7.13 Añadir pruebas Presentation de proyectos: creación, edición, advertencia, filtros, estados, reapertura, eliminación y conflictos.
- [x] 7.14 Añadir pruebas Presentation de actividades: selección, edición, proyecto Finalizado, anulación, eliminación y cambios pendientes.
- [x] 7.15 Añadir pruebas Presentation de entregas: E/N/P, selección, marcar todos, observaciones, filtros, conteos e inactivos históricos.
- [x] 7.16 Añadir pruebas Presentation de fallos, `CanExecute`, confirmaciones, cierre y conservación completa de edición.
- [x] 7.17 Verificar mediante prueba de referencias que Presentation y sus pruebas no cargan WPF, Data ni SQLite.

## 8. Integración WPF

- [x] 8.1 Ampliar la composición manual para crear persistencias, casos de uso, adaptadores y ViewModels del módulo Proyectos.
- [x] 8.2 Integrar la navegación Grupo/Asistencia/Proyectos en `MainWindow` sin framework de navegación.
- [x] 8.3 Crear una vista redimensionable de tres zonas para proyectos, actividades y detalle/entregas con anchos mínimos y scroll adecuado.
- [x] 8.4 Enlazar editor y acciones de proyecto, incluidos estado, reapertura, advertencia y confirmación de eliminación.
- [x] 8.5 Enlazar listado/editor de actividad, búsqueda, fecha, anulación, eliminación y estados de sólo lectura.
- [x] 8.6 Crear grilla de entregas con indicador Inactivo actualmente, estados compactos, observación, filtros y conteos sin mostrar IDs.
- [x] 8.7 Implementar interacción E/N/P, selector temporal, selección múltiple y `Ctrl+S` limitando code-behind a foco y conducta visual.
- [x] 8.8 Reutilizar el cierre cancelable y la navegación segura con cambios pendientes sin abrir ventanas por estudiante.
- [x] 8.9 Añadir pruebas de composición, navegación, bindings principales, ausencia de SQL/IDs y construcción sin interacción.
- [x] 8.10 Añadir smoke tests automatizados de teclado contextual E/N/P y Ctrl+S, confirmaciones al cambiar entre proyectos/actividades, estructura redimensionable y representación sin pérdida de 40 estudiantes. El redimensionamiento visual interactivo permanece incluido en 9.7.
- [x] 8.11 Añadir regresión WPF que analice `MainWindow.xaml`, exija `Mode=OneWay` en los conteos calculados de proyectos y evite que un binding TwoWay a `Total`, `Pendientes`, `Entregadas` o `NoEntregadas` vuelva a impedir `MainWindow.Show`.

## 9. Verificación final

- [x] 9.1 Ejecutar `dotnet restore` y registrar el resultado exacto.
- [x] 9.2 Ejecutar `dotnet format SistemaDocente.sln --verify-no-changes --no-restore` y corregir sólo formato relacionado.
- [x] 9.3 Ejecutar `dotnet build SistemaDocente.sln --no-restore` con cero errores y sin advertencias nuevas.
- [x] 9.4 Ejecutar `dotnet test SistemaDocente.sln --no-build` y confirmar todas las suites.
- [x] 9.5 Ejecutar `openspec validate --all` y `git diff --check`.
- [x] 9.6 Auditar referencias y código para confirmar ausencia de ciclos, SQL fuera de Data, WPF fuera de App.Wpf, async, `Task.Run`, DI, repositorios genéricos, ORM y paquetes UI externos.
- [ ] 9.7 Probar manualmente creación y ciclo de proyecto, periodos incompatibles, actividad con 40 estudiantes, E/N/P, historial inactivo, anulación, conflictos, reapertura y redimensionamiento.

