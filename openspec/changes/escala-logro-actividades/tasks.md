## 1. Core: NivelLogro

- [x] 1.1 Renombrar la enumeracion `EstadoEntrega` a `NivelLogro` y agregar los valores `Domina`, `Suficiente`, `EnProceso`, `RequiereApoyo` y `NoEntrego`, conservando `Pendiente = 0` como estado inicial.
- [x] 1.2 Renombrar el campo `EstadoEntrega` a `NivelLogro` en `RegistroEntregaActividad` y actualizar la validacion de rehidratacion para los seis valores validos.
- [x] 1.3 Actualizar pruebas Core de padron para usar los seis valores de `NivelLogro` y verificar rechazo de valores fuera del conjunto.

## 2. Application: snapshots y conteos

- [x] 2.1 Renombrar el campo `EstadoEntrega` a `NivelLogro` en `EntregaActividadDetalle` y actualizar la proyeccion desde el agregado.
- [x] 2.2 Reemplazar los tres conteos (Pendiente, Entregada, NoEntregada) por seis conteos (Pendiente, Domina, Suficiente, EnProceso, RequiereApoyo, NoEntrego) en `ActividadProyectoDetalle` y actualizar el calculo.
- [x] 2.3 Actualizar `GuardarEntregasActividad` para aceptar entradas con `NivelLogro` y rechazar valores fuera del conjunto valido.
- [x] 2.4 Actualizar pruebas Application de entregas: conteos por nivel, guardar con los seis valores, rechazar nivel invalido y excluir anuladas de agregaciones.

## 3. Data: migracion SQLite v3 a v4

- [x] 3.1 Implementar la migracion v3 a v4 que recrea `entregas_actividad` con CHECK ampliado de 0 a 5 dentro de una unica transaccion mediante la tecnica de tabla temporal, conservando todos los datos existentes.
- [x] 3.2 Actualizar la inicializacion directa de base nueva para crear `entregas_actividad` con el CHECK de 0 a 5 y establecer `user_version = 4` desde el inicio.
- [x] 3.3 Actualizar la validacion completa de v4 y rechazar versiones incompatibles sin reparacion automatica.
- [x] 3.4 Actualizar los adaptadores de lectura y escritura de entregas para mapear entre los valores enteros 0-5 y los valores del enumerado `NivelLogro`.
- [x] 3.5 Anadir pruebas SQLite de base nueva v4, migracion v3 real con datos, fallo de migracion que conserva v3, reapertura e inicializacion idempotente.
- [x] 3.6 Anadir pruebas de CHECK con valores fuera de rango (6, -1) y pruebas de adaptador para los seis valores de nivel de logro.

## 4. Presentation: ViewModels y comandos

- [x] 4.1 Actualizar `FilaEntregaViewModel` para usar `NivelLogro` con etiquetas cortas (D, S, EP, RA, NE, guion para Pendiente) y la propiedad de diferenciacion visual.
- [x] 4.2 Reemplazar los comandos E/N/P por D/S/E/R/N en el ViewModel de actividad y actualizar `CanExecute` correspondiente.
- [x] 4.3 Actualizar filtros: ampliar Solo incidencias a (Pendiente o RequiereApoyo o NoEntrego) y agregar filtros individuales por nivel de logro.
- [x] 4.4 Actualizar conteos expuestos en el ViewModel de actividad para los seis niveles.
- [x] 4.5 Actualizar pruebas Presentation de filas de entrega, comandos de nivel, filtros y conteos con los seis valores de `NivelLogro`.

## 5. App.Wpf: rediseno de la vista de Proyectos

- [x] 5.1 Redisenar el panel de entregas en `MainWindow.xaml` para mostrar la etiqueta compacta del nivel de logro con diferenciacion cromatica mediante estilos WPF nativos (sin paquetes externos).
- [x] 5.2 Actualizar el code-behind de la grilla para manejar los nuevos atajos D/S/E/R/N y delegar al ViewModel sin logica de dominio.
- [x] 5.3 Redisenar los botones de accion masiva de la grilla para reflejar los cinco niveles de logro en lugar de E/N/P.
- [x] 5.4 Actualizar los filtros visibles en la barra de herramientas de la grilla para incluir los nuevos niveles.
- [x] 5.5 Revisar y ajustar el diseno general del panel de tres zonas para mejorar la claridad del flujo proyecto->actividad->evaluacion.

## 6. Verificacion final

- [x] 6.1 Ejecutar `dotnet build SistemaDocente.sln` con cero errores y sin advertencias nuevas.
- [x] 6.2 Ejecutar `dotnet test SistemaDocente.sln` y confirmar que todas las suites pasan.
- [x] 6.3 Ejecutar `openspec validate --all` y verificar que el change es valido.
- [x] 6.4 Probar manualmente el flujo completo: crear proyecto, agregar actividad, asignar niveles de logro con teclado y guardar.

