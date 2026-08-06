## 1. Crear la capa de aplicación

- [x] 1.1 Crear `SistemaDocente.Application` para `net10.0`, agregarlo bajo `src/` y hacer que referencie únicamente Core
- [x] 1.2 Crear `SistemaDocente.Application.Tests` bajo `tests/` con referencias únicamente a Application y Core
- [x] 1.3 Actualizar Data para que referencie Application y Core sin crear ciclos
- [x] 1.4 Permitir que Data.Tests referencie Data, Application y Core y confirmar que no requiere App.Wpf
- [x] 1.5 Inspeccionar el grafo final de referencias productivas y de pruebas

## 2. Definir contratos, snapshots y errores

- [x] 2.1 Definir `IAlmacenamientoGrupos` exactamente con `Grupo? Cargar(GrupoId)`, `bool Existe(GrupoId)` y `void Guardar(Grupo)`
- [x] 2.2 Definir `GrupoNoEncontradoException` y `ErrorPersistenciaAplicacionException` sin referencias a Data ni SQLite
- [x] 2.3 Definir `GrupoDetalle` como record inmutable con `GrupoId`, nombre e `IReadOnlyList<EstudianteDetalle>`
- [x] 2.4 Definir `EstudianteDetalle` como record inmutable con `EstudianteId`, nombre, número y estado
- [x] 2.5 Implementar proyecciones que materialicen matrices nuevas y nunca expongan `Grupo`, `Estudiante` ni colecciones internas
- [x] 2.6 Ordenar las proyecciones por número de lista, nombre visible y `EstudianteId`

## 3. Implementar comandos y consultas de grupo

- [x] 3.1 Implementar `CrearGrupo` sin identidad de entrada, con un guardado y resultado `GrupoDetalle` posterior al guardado exitoso
- [x] 3.2 Implementar `CargarGrupo` con resultado `GrupoDetalle`, sin guardado y con `GrupoNoEncontradoException` ante ausencia
- [x] 3.3 Implementar `Existe` con resultado `bool`, `false` sólo ante ausencia real y sin convertir fallos técnicos en ausencia
- [x] 3.4 Implementar `CambiarNombreGrupo` con carga fresca, operación de Core, un guardado y resultado `GrupoDetalle`
- [x] 3.5 Implementar `ObtenerTodosLosEstudiantes` y `ObtenerEstudiantesActivos` como `IReadOnlyList<EstudianteDetalle>` materializadas y ordenadas

## 4. Implementar comandos de estudiantes

- [x] 4.1 Implementar `AgregarEstudiante` sin identidad de entrada, con carga fresca, un guardado y resultado `EstudianteDetalle`
- [x] 4.2 Implementar `RenombrarEstudiante` con carga fresca, un guardado y resultado `EstudianteDetalle`
- [x] 4.3 Implementar `CambiarNumeroLista` con carga fresca, un guardado y resultado `EstudianteDetalle`
- [x] 4.4 Implementar `DesactivarEstudiante` y `ReactivarEstudiante` con carga fresca, un guardado incluso si Core acepta la operación como idempotente y resultado `EstudianteDetalle`
- [x] 4.5 Garantizar que los errores de dominio no invocan `Guardar` y que ningún fallo de persistencia devuelve un resultado exitoso
- [x] 4.6 Garantizar que la fachada no conserva agregados entre llamadas

## 5. Adaptar la persistencia SQLite al puerto

- [x] 5.1 Hacer que el adaptador SQLite de Data implemente `IAlmacenamientoGrupos` sin mover SQL ni tipos del proveedor a Application
- [x] 5.2 Implementar `Existe` para devolver `false` únicamente ante ausencia real
- [x] 5.3 Establecer en Data la única frontera de traducción de errores de acceso, esquema, integridad y proveedor a `ErrorPersistenciaAplicacionException`
- [x] 5.4 Conservar la excepción técnica original como `InnerException`
- [x] 5.5 Confirmar que la fachada Application propaga `ErrorPersistenciaAplicacionException` sin volver a envolverla
- [x] 5.6 Confirmar que Core y Application no referencian Data, `Microsoft.Data.Sqlite` ni WPF

## 6. Probar la orquestación con dobles

- [x] 6.1 Crear dobles manuales de almacenamiento persistido, grabación de llamadas y fallo controlado para Application.Tests
- [x] 6.2 Probar creación y persistencia de grupo con identidad generada, `GrupoDetalle` y exactamente un guardado
- [x] 6.3 Probar carga existente, ausencia mediante `GrupoNoEncontradoException` y `Existe` verdadero o falso sin guardados
- [x] 6.4 Probar que `Existe` no convierte un fallo técnico en `false`
- [x] 6.5 Probar alta de estudiante con identidad generada, `EstudianteDetalle` y exactamente un guardado
- [x] 6.6 Probar renombrado de grupo y estudiante y cambio de número con tipos de resultado e identidades estables
- [x] 6.7 Probar desactivación y reactivación, incluidos casos idempotentes, con exactamente un guardado por comando aceptado
- [x] 6.8 Probar ambas consultas con activos e inactivos, orden por número, nombre e identidad y matrices nuevas de sólo lectura
- [x] 6.9 Probar que los snapshots son records inmutables y no exponen agregados, entidades ni colecciones internas
- [x] 6.10 Probar cada operación inválida o conflictiva y la ausencia de guardado tras la excepción de dominio
- [x] 6.11 Probar fallos de persistencia en crear, cargar, existe y cada comando modificador sin resultado exitoso ni doble envoltura
- [x] 6.12 Probar explícitamente que, tras modificar una instancia y fallar `Guardar`, un comando posterior carga el estado persistido anterior y no reutiliza la instancia modificada
- [x] 6.13 Verificar que Application.Tests se ejecuta sin referenciar Data, SQLite ni WPF

## 7. Probar el adaptador de Data con SQLite real

- [x] 7.1 Añadir pruebas de contrato de `Cargar`, `Existe` y `Guardar` mediante `IAlmacenamientoGrupos` usando un archivo SQLite temporal por prueba
- [x] 7.2 Probar `Existe` para presencia y ausencia reales y para un fallo técnico
- [x] 7.3 Probar que Data traduce cada familia de error propia una sola vez y conserva la causa técnica como `InnerException`
- [x] 7.4 Confirmar que las pruebas existentes de esquema, restricciones, identidades y atomicidad SQLite continúan pasando

## 8. Verificar arquitectura, solución y alcance

- [x] 8.1 Ejecutar `dotnet restore`, `dotnet format --verify-no-changes`, `dotnet build` y `dotnet test` sobre la solución completa
- [x] 8.2 Confirmar que Application no contiene SQL, tipos SQLite, WPF ni reglas duplicadas del dominio
- [x] 8.3 Confirmar que Data sólo se prevé desde la futura raíz de composición de App.Wpf y que ventanas, controles y ViewModels dependerán de Application
- [x] 8.4 Confirmar que App.Wpf no referencia `Microsoft.Data.Sqlite`
- [x] 8.5 Confirmar que no se añadieron WPF funcional, ViewModels, navegación, contenedor DI, API asíncrona, `CancellationToken`, caché, concurrencia, asistencia, actividades, evaluación, reportes funcionales ni importación

## 9. Corregir edición atómica de estudiante

- [x] 9.1 Implementar `EditarEstudiante` con una carga, dos mutaciones de Core en memoria, un guardado y proyección posterior
- [x] 9.2 Probar éxito con identidades estables, una carga y un guardado
- [x] 9.3 Probar nombre inválido y número inválido o conflictivo con cero guardados y estado persistido intacto
- [x] 9.4 Probar fallo de guardado, carga posterior del estado anterior y grupo inexistente
