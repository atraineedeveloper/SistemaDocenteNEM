## 1. Incorporar rehidratación neutral en Core

- [x] 1.1 Añadir conversiones públicas neutrales entre `Guid` y `GrupoId` o `EstudianteId` sin cambiar su generación interna
- [x] 1.2 Definir el tipo público e inmutable `DatosEstudianteRehidratado` con identidad, nombre, número y estado, sin referencias de infraestructura
- [x] 1.3 Implementar un método estático equivalente a `Grupo.Rehidratar` que reciba `GrupoId`, nombre y la colección completa de datos de estudiantes
- [x] 1.4 Validar el snapshot completo antes de construir el agregado y rechazar nombres no normalizados, identificadores inválidos o repetidos, números no positivos y duplicados entre activos
- [x] 1.5 Garantizar que una rehidratación fallida no devuelva un agregado parcial y que una exitosa conserve todas las identidades y estados
- [x] 1.6 Probar rehidratación válida e inválida y confirmar que `Grupo.Crear` y `AgregarEstudiante` conservan su comportamiento
- [x] 1.7 Confirmar que Core no usa `InternalsVisibleTo` ni referencia Data, SQLite o tipos del proveedor

## 2. Configurar SQLite y contratos de Data

- [x] 2.1 Añadir `Microsoft.Data.Sqlite` exclusivamente a `SistemaDocente.Data`, sin Entity Framework Core ni Dapper
- [x] 2.2 Implementar configuración que exija una ruta explícita y no consulte `%LOCALAPPDATA%`, carpetas del sistema ni WPF
- [x] 2.3 Documentar para la futura composición la ruta predeterminada `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db`, sin implementar modalidad portable
- [x] 2.4 Definir errores de acceso, esquema incompatible e integridad que conserven la causa técnica sin exponer `SqliteException` como contrato público
- [x] 2.5 Configurar cada conexión para aplicar claves foráneas

## 3. Crear y validar el esquema versión 1

- [x] 3.1 Implementar creación atómica de la versión 1 cuando `user_version = 0` y la base esté vacía
- [x] 3.2 Crear tablas de grupos y estudiantes con claves, relación y restricciones de nombres, número y estado
- [x] 3.3 Crear el índice por `grupo_id` y el índice único parcial para números de estudiantes activos
- [x] 3.4 Hacer idempotente la inicialización de una base versión 1 con estructura compatible
- [x] 3.5 Rechazar sin cambios `user_version = 0` con objetos preexistentes y cualquier versión mayor que 1
- [x] 3.6 Verificar la estructura completa de una base versión 1 y rechazar incompatibilidades sin reparar, borrar ni recrear
- [x] 3.7 Rechazar de forma no destructiva archivos que no sean SQLite o bases dañadas

## 4. Implementar guardado transaccional

- [x] 4.1 Implementar guardado completo del grupo y todos sus estudiantes dentro de una única transacción
- [x] 4.2 Implementar inserción o actualización del grupo existente sin cambiar su identidad
- [x] 4.3 Implementar upsert de estudiantes activos e inactivos sin borrado físico ni sincronización destructiva
- [x] 4.4 Impedir que un `EstudianteId` existente cambie de `GrupoId`, informar fallo de integridad y revertir toda la transacción
- [x] 4.5 Garantizar rollback completo ante cualquier error después de una o más escrituras

## 5. Implementar carga y rehidratación

- [x] 5.1 Implementar carga por `GrupoId` con ausencia normal para grupos inexistentes
- [x] 5.2 Leer el grupo y todos sus estudiantes como un snapshot completo sin normalizar ni corregir valores en Data
- [x] 5.3 Convertir los `Guid` almacenados a identificadores tipados y crear los tipos neutrales de estudiante
- [x] 5.4 Invocar `Grupo.Rehidratar` una sola vez con el snapshot completo y traducir datos persistidos inválidos a un fallo de integridad
- [x] 5.5 Verificar que guardar, cerrar, reabrir y cargar conserva nombres, números, identidades y estados

## 6. Probar inicialización y aislamiento con SQLite real

- [x] 6.1 Crear infraestructura de prueba con directorio y archivo SQLite temporal único por caso, cierre de recursos y limpieza posterior
- [x] 6.2 Probar creación automática de una base nueva e inicialización idempotente de una versión 1 compatible
- [x] 6.3 Probar rechazo no destructivo de una versión posterior a 1
- [x] 6.4 Probar rechazo no destructivo de `user_version = 0` con objetos preexistentes
- [x] 6.5 Probar rechazo no destructivo de versión 1 con tabla, columna, restricción o índice incompatible
- [x] 6.6 Probar rechazo y conservación exacta de un archivo que no sea SQLite
- [x] 6.7 Probar cierre y reapertura del archivo con conservación de datos
- [x] 6.8 Probar que dos rutas temporales distintas no comparten estado

## 7. Probar restricciones e integridad

- [x] 7.1 Probar que las claves foráneas rechazan estudiantes huérfanos en conexiones de Data
- [x] 7.2 Probar nombres vacíos después de `trim` y límites máximos de 100 y 150 caracteres
- [x] 7.3 Probar rechazo de números cero y negativos
- [x] 7.4 Probar rechazo de estados distintos de 0 y 1
- [x] 7.5 Probar rechazo de números duplicados entre activos del mismo grupo
- [x] 7.6 Probar coincidencias permitidas entre estudiantes inactivos y entre grupos diferentes
- [x] 7.7 Probar que un nombre manipulado no se corrige en Data y que Core rechaza la rehidratación
- [x] 7.8 Probar que el intento de mover un `EstudianteId` a otro grupo falla y conserva su asociación original

## 8. Probar operaciones del agregado persistido

- [x] 8.1 Probar guardado y carga de un grupo nuevo con estabilidad de `GrupoId` y `EstudianteId`
- [x] 8.2 Probar actualización del nombre del grupo y de nombres y números de estudiantes sin duplicar registros
- [x] 8.3 Probar persistencia y carga de estudiantes activos e inactivos
- [x] 8.4 Probar persistencia de desactivación y reactivación conservando identidad y datos
- [x] 8.5 Probar ausencia normal para un `GrupoId` no almacenado
- [x] 8.6 Instalar un trigger de prueba con `RAISE(ABORT)` que falle después de una escritura intermedia y verificar rollback total desde una conexión nueva

## 9. Verificar arquitectura y solución

- [x] 9.1 Ejecutar `dotnet restore`, `dotnet format --verify-no-changes`, `dotnet build` y `dotnet test` sobre la solución
- [x] 9.2 Confirmar que sólo Data contiene acceso SQLite y que App.Wpf no ejecuta comandos ni referencia tipos del proveedor
- [x] 9.3 Confirmar que no se incorporaron UI, ViewModels, asistencia, actividades, evaluación, reportes, respaldos, cifrado, concurrencia multiusuario, importación ni datos personales adicionales
