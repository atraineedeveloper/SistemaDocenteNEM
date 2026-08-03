## Purpose

Define una capa de aplicación independiente de la interfaz y de la infraestructura concreta que coordina Core y persistencia para administrar grupos y estudiantes mediante comandos, consultas y resultados verificables.

## ADDED Requirements

### Requirement: Coordinación mediante un puerto de persistencia
Application SHALL depender únicamente de Core y SHALL definir `IAlmacenamientoGrupos` con las operaciones exactas `Grupo? Cargar(GrupoId)`, `bool Existe(GrupoId)` y `void Guardar(Grupo)`. Data SHALL implementar ese puerto. Application MUST NOT depender de Data, WPF, `Microsoft.Data.Sqlite`, SQL ni tipos visuales.

#### Scenario: Construir casos de uso con un doble
- **WHEN** los casos de uso se construyen con un doble de `IAlmacenamientoGrupos`
- **THEN** pueden ejecutarse sin cargar Data, SQLite ni WPF

#### Scenario: Usar persistencia SQLite mediante el puerto
- **WHEN** la raíz de composición proporciona la implementación Data del puerto
- **THEN** los mismos casos de uso coordinan Core con SQLite sin conocer el proveedor concreto

### Requirement: Resultados públicos exactos
Application SHALL exponer los siguientes resultados: `CrearGrupo`, `CargarGrupo` y `CambiarNombreGrupo` devuelven `GrupoDetalle`; `AgregarEstudiante`, `RenombrarEstudiante`, `CambiarNumeroLista`, `EditarEstudiante`, `DesactivarEstudiante` y `ReactivarEstudiante` devuelven `EstudianteDetalle`; `Existe` devuelve `bool`; y las consultas de estudiantes devuelven `IReadOnlyList<EstudianteDetalle>`.

#### Scenario: Obtener el resultado declarado por cada operación
- **WHEN** una operación termina correctamente
- **THEN** devuelve exactamente el tipo de resultado definido para ella y no un agregado o entidad de Core

### Requirement: Snapshots inmutables y materializados
`GrupoDetalle` y `EstudianteDetalle` SHALL ser records inmutables. `GrupoDetalle` SHALL contener `GrupoId`, nombre visible e `IReadOnlyList<EstudianteDetalle>`. `EstudianteDetalle` SHALL contener `EstudianteId`, nombre visible, número de lista y estado activo. Toda colección de salida SHALL materializarse como una matriz nueva. Application MUST NOT exponer `Grupo`, `Estudiante` ni colecciones internas.

#### Scenario: Proyectar un grupo
- **WHEN** Application crea un `GrupoDetalle`
- **THEN** copia sus estudiantes a una matriz nueva y el consumidor no obtiene acceso mutable al dominio

#### Scenario: Proyectar un estudiante
- **WHEN** Application crea un `EstudianteDetalle`
- **THEN** conserva su identidad, nombre, número y estado sin exponer la entidad original

### Requirement: Orden determinista de estudiantes
La colección de `GrupoDetalle`, la consulta de todos los estudiantes y la consulta de estudiantes activos SHALL ordenarse primero por número de lista, después por nombre visible y finalmente por `EstudianteId`.

#### Scenario: Ordenar estudiantes con números distintos
- **WHEN** se proyectan estudiantes con diferentes números de lista
- **THEN** aparecen en orden ascendente por número

#### Scenario: Desempatar nombres e identidades
- **WHEN** dos estudiantes tienen el mismo número y nombre permitido por el estado del agregado
- **THEN** se ordenan por `EstudianteId` después de comparar número y nombre

### Requirement: Crear y persistir un grupo
`CrearGrupo` SHALL crear el agregado mediante Core sin aceptar un `GrupoId`, SHALL invocar `Guardar` exactamente una vez y sólo después del guardado exitoso SHALL devolver `GrupoDetalle`.

#### Scenario: Crear un grupo válido
- **WHEN** se solicita crear un grupo con un nombre válido
- **THEN** Core genera la identidad, Application guarda una vez y devuelve `GrupoDetalle`

#### Scenario: Rechazar creación inválida
- **WHEN** Core rechaza el nombre del grupo
- **THEN** Application conserva la excepción de dominio y no invoca `Guardar`

### Requirement: Cargar un grupo y consultar existencia
`CargarGrupo` SHALL cargar por `GrupoId`, devolver `GrupoDetalle` cuando exista y lanzar `GrupoNoEncontradoException` cuando exista una ausencia real. `Existe` SHALL devolver `false` únicamente ante ausencia real y SHALL conservar cualquier fallo de persistencia. Ninguna de estas consultas SHALL invocar `Guardar`.

#### Scenario: Cargar un grupo existente
- **WHEN** se carga un `GrupoId` almacenado
- **THEN** Application devuelve `GrupoDetalle` con las identidades persistidas y no guarda

#### Scenario: Cargar un grupo inexistente
- **WHEN** `Cargar` devuelve ausencia para el identificador solicitado
- **THEN** Application lanza `GrupoNoEncontradoException`

#### Scenario: Consultar ausencia real
- **WHEN** el puerto determina que un grupo no existe
- **THEN** `Existe` devuelve `false` y no guarda

#### Scenario: No convertir un error técnico en ausencia
- **WHEN** el adaptador falla al comprobar existencia
- **THEN** `Existe` no devuelve `false` y propaga `ErrorPersistenciaAplicacionException`

### Requirement: Cambiar el nombre del grupo
`CambiarNombreGrupo` SHALL cargar una instancia fresca, delegar el cambio a Core, guardar exactamente una vez tras el éxito y devolver `GrupoDetalle`.

#### Scenario: Cambiar un nombre válido
- **WHEN** Core acepta el nuevo nombre
- **THEN** Application guarda una vez y devuelve el grupo actualizado con el mismo `GrupoId`

#### Scenario: Rechazar un nombre inválido
- **WHEN** Core rechaza el nuevo nombre
- **THEN** Application conserva la excepción de dominio y no invoca `Guardar`

### Requirement: Agregar un estudiante
`AgregarEstudiante` SHALL cargar una instancia fresca del grupo, delegar el alta a Core sin aceptar `EstudianteId`, guardar exactamente una vez tras el éxito y devolver `EstudianteDetalle`.

#### Scenario: Agregar un estudiante válido
- **WHEN** Core acepta el nombre y número de lista
- **THEN** genera el `EstudianteId`, Application guarda una vez y devuelve el estudiante activo

#### Scenario: Rechazar un alta inválida
- **WHEN** Core rechaza el nombre, número o conflicto de lista
- **THEN** Application conserva la excepción de dominio y no invoca `Guardar`

### Requirement: Modificar estudiantes
Application SHALL permitir renombrar, cambiar número de lista, desactivar y reactivar mediante operaciones públicas de Core. Cada comando SHALL cargar una instancia fresca, guardar exactamente una vez tras el éxito y devolver `EstudianteDetalle` con identidades estables.

#### Scenario: Renombrar un estudiante
- **WHEN** Core acepta el nuevo nombre
- **THEN** Application guarda una vez y devuelve el estudiante actualizado con el mismo `EstudianteId`

#### Scenario: Cambiar el número de lista
- **WHEN** Core acepta el nuevo número
- **THEN** Application guarda una vez y devuelve el estudiante actualizado

#### Scenario: Desactivar un estudiante
- **WHEN** Core acepta desactivar al estudiante
- **THEN** Application guarda una vez y devuelve `EstudianteDetalle` inactivo con identidad y datos conservados

#### Scenario: Reactivar un estudiante
- **WHEN** Core acepta reactivar al estudiante
- **THEN** Application guarda una vez y devuelve `EstudianteDetalle` activo con identidad y datos conservados

#### Scenario: Guardar un comando idempotente aceptado
- **WHEN** Core acepta una desactivación ya inactiva o una reactivación ya activa como idempotente
- **THEN** Application invoca `Guardar` exactamente una vez y devuelve `EstudianteDetalle`

#### Scenario: Rechazar una modificación
- **WHEN** Core rechaza un nombre, número, identidad o conflicto
- **THEN** Application conserva la excepción de dominio y no invoca `Guardar`

### Requirement: Editar nombre y número de estudiante atómicamente
`EditarEstudiante` SHALL recibir `GrupoId`, `EstudianteId`, nombre visible y número de lista; cargar el grupo una sola vez; aplicar en memoria el renombrado y el cambio de número mediante Core; e invocar `Guardar` exactamente una vez después de que ambas mutaciones terminen correctamente. SHALL devolver `EstudianteDetalle` únicamente después del guardado y SHALL conservar `GrupoId` y `EstudianteId`. Application MUST NOT duplicar validaciones de Core.

Esta operación SHALL ser una excepción justificada a la regla general de una operación pública de Core por comando: un comando MAY coordinar varias mutaciones de Core cuando representan una sola acción atómica del usuario y existe un único guardado final.

#### Scenario: Editar nombre y número válidos
- **WHEN** Core acepta el nombre y el número nuevos
- **THEN** Application carga una vez, guarda una vez y devuelve el estudiante con ambos cambios e identidad estable

#### Scenario: Rechazar nombre inválido
- **WHEN** Core rechaza el nombre antes de cambiar el número
- **THEN** Application conserva la excepción de dominio y no invoca `Guardar`

#### Scenario: Rechazar número después de nombre válido
- **WHEN** Core acepta el nombre en memoria pero rechaza el número por validación o conflicto
- **THEN** Application conserva la excepción de dominio, no invoca `Guardar` y el estado persistido anterior permanece intacto

#### Scenario: Fallar en el guardado único
- **WHEN** ambas mutaciones terminan correctamente pero `Guardar` falla
- **THEN** Application no devuelve éxito y un comando posterior carga el estado persistido anterior

#### Scenario: Editar en grupo inexistente
- **WHEN** la carga única devuelve ausencia
- **THEN** Application lanza `GrupoNoEncontradoException` y no invoca `Guardar`

### Requirement: Consultar estudiantes
`ObtenerEstudiantesActivos` y `ObtenerTodosLosEstudiantes` SHALL cargar una instancia fresca, devolver una matriz nueva como `IReadOnlyList<EstudianteDetalle>` en el orden determinista definido y no guardar.

#### Scenario: Obtener estudiantes activos
- **WHEN** se consultan los activos de un grupo con estados mixtos
- **THEN** el resultado contiene sólo activos ordenados por número, nombre e identidad y no se guarda

#### Scenario: Obtener todos los estudiantes
- **WHEN** se consultan todos los estudiantes
- **THEN** el resultado contiene activos e inactivos ordenados por número, nombre e identidad y no expone la colección interna

### Requirement: Consistencia entre modificación y guardado
Application MUST NOT mantener agregados entre llamadas. Cada comando sobre un grupo existente SHALL cargar una instancia fresca, aplicar una operación de Core y guardar exactamente una vez sólo después del éxito. Un fallo de dominio MUST impedir `Guardar`; un fallo de persistencia MUST impedir un resultado exitoso.

#### Scenario: Error de dominio antes del guardado
- **WHEN** una operación de Core lanza `DomainValidationException` o `DomainConflictException`
- **THEN** el puerto no recibe ninguna llamada a `Guardar`

#### Scenario: Fallo al guardar una modificación válida
- **WHEN** Core acepta la modificación pero `Guardar` lanza `ErrorPersistenciaAplicacionException`
- **THEN** el comando falla y no devuelve el estado modificado

#### Scenario: No reutilizar una instancia tras fallo de guardado
- **GIVEN** un doble carga una copia del estado persistido anterior
- **WHEN** un comando modifica esa instancia, `Guardar` falla y un comando posterior vuelve a ejecutarse
- **THEN** el comando posterior vuelve a cargar el estado anterior y Application no reutiliza la instancia modificada

### Requirement: Única frontera de traducción de errores
El adaptador Data SHALL convertir sus excepciones propias de acceso, esquema, integridad o proveedor en `ErrorPersistenciaAplicacionException` y SHALL conservar la excepción técnica como `InnerException`. La fachada Application MUST NOT volver a envolver esa excepción. Las excepciones de dominio SHALL conservarse sin traducción.

#### Scenario: Traducir una excepción de Data una sola vez
- **WHEN** Data encuentra un fallo técnico al cargar, comprobar existencia o guardar
- **THEN** el consumidor recibe `ErrorPersistenciaAplicacionException` cuya `InnerException` es la causa técnica original

#### Scenario: No volver a envolver en la fachada
- **WHEN** el puerto lanza `ErrorPersistenciaAplicacionException`
- **THEN** la fachada propaga la misma excepción y no crea otra envoltura

#### Scenario: Conservar una excepción de dominio
- **WHEN** Core rechaza una operación
- **THEN** el consumidor recibe la misma familia de excepción de dominio y no un error de persistencia

### Requirement: Identidades no escritas manualmente
Los comandos de creación MUST NOT aceptar `GrupoId` ni `EstudianteId`. Los comandos sobre entidades existentes SHALL aceptar identificadores tipados obtenidos del sistema y MUST NOT definir campos visuales para que el docente escriba identidades.

#### Scenario: Crear identidades en Core
- **WHEN** se crea un grupo o se agrega un estudiante
- **THEN** Core genera la identidad y Application la devuelve en el snapshot correspondiente

#### Scenario: Conservar identidades estables
- **WHEN** se ejecutan operaciones sucesivas sobre entidades existentes
- **THEN** los snapshots conservan sus `GrupoId` y `EstudianteId`
