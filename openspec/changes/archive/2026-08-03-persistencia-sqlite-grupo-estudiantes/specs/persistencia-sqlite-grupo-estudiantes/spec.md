## Purpose

Define la persistencia local y transaccional de agregados de grupo y estudiantes en SQLite, conservando sus identidades, datos y estados sin depender de WPF ni de un servidor externo.

## ADDED Requirements

### Requirement: Ruta SQLite explícita y ubicación productiva
Data SHALL recibir siempre una ruta explícita para el archivo SQLite y MUST NOT consultar carpetas del sistema ni depender de WPF. La futura composición de App.Wpf SHALL usar por defecto `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db` y entregar esa ruta a Data. Las pruebas SHALL usar rutas temporales. La modalidad portable queda fuera de este cambio.

#### Scenario: Inicializar Data con ruta explícita
- **WHEN** un consumidor configura Data con una ruta válida
- **THEN** Data usa exactamente esa ruta sin calcular otra ubicación ni consultar WPF

#### Scenario: Usar ruta temporal en pruebas
- **WHEN** una prueba crea su persistencia
- **THEN** entrega a Data una ruta temporal única y no usa la ubicación productiva

### Requirement: Base SQLite local e inicialización automática
El sistema SHALL almacenar los agregados en un archivo SQLite local sin servidor. Al recibir una ruta válida donde no existe un archivo, Data SHALL crear la base y el esquema versión 1 automáticamente.

#### Scenario: Crear automáticamente una base nueva
- **WHEN** se inicializa la persistencia con una ruta válida cuyo archivo no existe
- **THEN** se crea un archivo SQLite con `user_version` igual a 1 y el esquema compatible

#### Scenario: Reabrir una base existente
- **WHEN** se cierra y vuelve a abrir una base compatible creada previamente
- **THEN** el esquema y los datos guardados permanecen disponibles sin reinicialización destructiva

### Requirement: Inicialización y compatibilidad estrictas del esquema
Data SHALL interpretar `user_version` y la estructura existente antes de modificar la base. `user_version = 0` en una base vacía SHALL crear atómicamente la versión 1. `user_version = 0` con objetos preexistentes MUST ser rechazado. `user_version = 1` SHALL aceptarse sólo si toda la estructura requerida es compatible. Una versión mayor que 1 o cualquier estructura incompatible MUST rechazarse sin reparar, borrar, recrear ni modificar automáticamente el archivo.

#### Scenario: Inicialización idempotente de versión 1
- **WHEN** se inicializa de nuevo una base con `user_version = 1` y estructura compatible
- **THEN** la inicialización termina correctamente sin alterar esquema ni datos

#### Scenario: Rechazar versión posterior
- **WHEN** la base declara `user_version` mayor que 1
- **THEN** Data informa un error de esquema y deja el archivo sin cambios

#### Scenario: Rechazar versión cero con objetos preexistentes
- **WHEN** una base con `user_version = 0` contiene cualquier objeto de esquema creado por el usuario
- **THEN** Data informa un error de esquema y no crea, repara ni elimina objetos

#### Scenario: Rechazar versión uno con estructura incompatible
- **WHEN** una base declara `user_version = 1` pero carece de una tabla, columna, restricción o índice requerido
- **THEN** Data informa un error de esquema y no modifica la estructura

### Requirement: Esquema relacional de grupos y estudiantes
El esquema SHALL almacenar grupos y estudiantes en estructuras separadas, SHALL relacionar cada estudiante con exactamente un grupo y SHALL conservar `GrupoId`, `EstudianteId`, nombre visible normalizado, número de lista y estado activo/inactivo. Las identidades SHALL ser claves estables y la relación SHALL tener integridad referencial habilitada en cada conexión.

#### Scenario: Inspeccionar el esquema inicial
- **WHEN** se inspecciona una base recién inicializada
- **THEN** existen estructuras para grupos y estudiantes con claves primarias, clave foránea, campos obligatorios e índices requeridos

#### Scenario: Rechazar estudiante huérfano
- **WHEN** se intenta insertar directamente un estudiante cuyo grupo no existe
- **THEN** SQLite rechaza el registro mediante la clave foránea y no crea un estudiante huérfano

### Requirement: Restricciones de nombres en SQLite
SQLite SHALL exigir que los nombres de grupos y estudiantes no sean vacíos después de aplicar `trim` y SHALL limitar sus longitudes a 100 y 150 caracteres respectivamente. Core seguirá siendo responsable de normalizar los nombres. Data MUST NOT corregir silenciosamente nombres manipulados, y la rehidratación MUST rechazar cualquier nombre almacenado que incumpla las invariantes completas de Core.

#### Scenario: Rechazar nombre vacío o compuesto por espacios
- **WHEN** se intenta escribir directamente un nombre vacío o compuesto sólo por espacios en grupos o estudiantes
- **THEN** SQLite rechaza la escritura mediante una restricción

#### Scenario: Aplicar límites de nombres
- **WHEN** se intenta escribir directamente un nombre de grupo de más de 100 caracteres o de estudiante de más de 150
- **THEN** SQLite rechaza la escritura mediante una restricción

#### Scenario: Rechazar al cargar un nombre manipulado
- **WHEN** Data lee un nombre que SQLite admite pero que no está normalizado según Core
- **THEN** Data no lo corrige y la rehidratación rechaza el snapshot completo

### Requirement: Restricciones de número, estado y unicidad activa
SQLite SHALL exigir números de lista mayores que cero y estados representados exclusivamente por 0 o 1. SHALL existir un índice único parcial que impida repetir un número entre estudiantes activos del mismo grupo, pero SHALL permitir el mismo número en grupos distintos y entre estudiantes inactivos. El esquema SHALL incluir un índice para cargar estudiantes por grupo.

#### Scenario: Rechazar número cero y negativo
- **WHEN** se intenta escribir directamente un estudiante con número cero o negativo
- **THEN** SQLite rechaza cada escritura mediante una restricción

#### Scenario: Rechazar estado fuera de cero y uno
- **WHEN** se intenta escribir directamente un estudiante con un estado distinto de 0 o 1
- **THEN** SQLite rechaza la escritura mediante una restricción

#### Scenario: Rechazar duplicado activo en un grupo
- **WHEN** se intenta almacenar dos estudiantes activos con el mismo grupo y número de lista
- **THEN** SQLite rechaza el conflicto mediante el índice único parcial

#### Scenario: Permitir coincidencias entre inactivos y grupos distintos
- **WHEN** el mismo número pertenece a estudiantes inactivos del mismo grupo o a estudiantes de grupos diferentes
- **THEN** SQLite permite almacenar los registros

### Requirement: Guardado transaccional del agregado completo
Cada operación de guardado SHALL incluir el grupo y todos sus estudiantes en una única transacción SQLite. SHALL insertar un grupo nuevo o actualizar uno existente con la misma identidad. Cualquier fallo MUST revertir la transacción completa y dejar la base en el estado anterior.

#### Scenario: Guardar un grupo nuevo
- **WHEN** se guarda por primera vez un grupo válido con estudiantes
- **THEN** el grupo y todos sus estudiantes quedan almacenados con sus identidades y datos dentro de una única transacción confirmada

#### Scenario: Actualizar un grupo existente
- **WHEN** se vuelve a guardar un grupo cuya identidad ya existe
- **THEN** se actualizan el nombre del grupo y los datos y estados de sus estudiantes sin duplicar identidades

#### Scenario: Revertir un fallo real a mitad del guardado
- **WHEN** un trigger temporal con `RAISE(ABORT)` provoca un fallo después de que la transacción haya escrito parte del agregado
- **THEN** se revierte toda la transacción y ninguna modificación del grupo ni de sus estudiantes queda guardada

### Requirement: Identidad de estudiante ligada a su grupo
Un `EstudianteId` almacenado SHALL permanecer asociado a su `GrupoId` original. El upsert MUST NOT trasladarlo a otro grupo. Un intento de guardar el mismo `EstudianteId` bajo otro `GrupoId` SHALL tratarse como fallo de integridad y SHALL provocar rollback completo.

#### Scenario: Rechazar traslado de estudiante entre grupos
- **WHEN** un guardado intenta usar un `EstudianteId` existente con un `GrupoId` diferente
- **THEN** Data informa un fallo de integridad, revierte toda la operación y conserva la asociación original

### Requirement: Sincronización sin borrado físico
El guardado SHALL insertar o actualizar los registros presentes en el agregado y SHALL conservar estudiantes inactivos. Mientras Core no ofrezca eliminación definitiva, Data MUST NOT borrar físicamente estudiantes como efecto de guardar el agregado.

#### Scenario: Conservar estudiante inactivo
- **WHEN** se guarda un grupo después de desactivar a un estudiante
- **THEN** el registro permanece almacenado con la misma identidad, nombre, número y estado inactivo

#### Scenario: Guardar nuevamente sin borrar registros
- **WHEN** se guarda de nuevo un agregado existente
- **THEN** Data actualiza sus registros sin ejecutar sincronización destructiva

### Requirement: Carga por identidad y ausencia explícita
Data SHALL permitir cargar un grupo por `GrupoId`. Cuando exista, SHALL leer un snapshot completo y reconstruirlo mediante `Grupo.Rehidratar`, conservando identidades, nombres, números y estados sin generar identidades nuevas. Cuando no exista, SHALL devolver ausencia normal y MUST NOT crear un grupo.

#### Scenario: Guardar y cargar un agregado
- **WHEN** se guarda un grupo y después se carga por su identidad
- **THEN** el agregado cargado contiene exactamente el mismo `GrupoId`, nombre y conjunto de estudiantes

#### Scenario: Conservar identidades y estados al cargar
- **WHEN** un grupo guardado contiene estudiantes activos e inactivos
- **THEN** la carga conserva cada `EstudianteId`, nombre, número y estado

#### Scenario: Consultar un grupo inexistente
- **WHEN** se carga un `GrupoId` que no está almacenado
- **THEN** Data devuelve ausencia sin informar un error de infraestructura ni modificar la base

### Requirement: Persistencia de cambios del dominio
Al volver a guardar y cargar un agregado, Data SHALL conservar cambios válidos realizados mediante Core en el nombre del grupo, nombres y números de estudiantes, y activación o desactivación.

#### Scenario: Persistir nombres y números actualizados
- **WHEN** se cambian nombres y números mediante el agregado, se guarda y se vuelve a cargar
- **THEN** la carga devuelve los valores actualizados con las mismas identidades

#### Scenario: Persistir desactivación y reactivación
- **WHEN** se desactiva o reactiva un estudiante, se guarda y se vuelve a cargar
- **THEN** la carga conserva el estado resultante y los demás datos del estudiante

### Requirement: Errores de infraestructura no destructivos
Data SHALL distinguir fallos generales de acceso, fallos de integridad y errores de esquema incompatible, conservando la causa técnica disponible. Un archivo que no sea SQLite, una base dañada o una estructura incompatible MUST NOT eliminarse, reemplazarse, repararse ni reinicializarse automáticamente.

#### Scenario: Rechazar archivo que no sea SQLite
- **WHEN** la ruta contiene un archivo que no es una base SQLite válida
- **THEN** Data informa un error de infraestructura y conserva exactamente el archivo

#### Scenario: Informar fallo de integridad
- **WHEN** SQLite rechaza una escritura por clave, relación, restricción o trigger
- **THEN** Data expone un fallo de integridad identificable, conserva la causa y revierte la transacción

### Requirement: Pruebas con SQLite real y aislado
Las pruebas de persistencia SHALL usar SQLite real mediante archivos temporales independientes, SHALL permitir cerrar y reabrir conexiones y MUST NOT depender de una instalación externa ni compartir estado entre pruebas.

#### Scenario: Aislamiento entre pruebas
- **WHEN** dos pruebas crean rutas temporales distintas
- **THEN** los datos escritos por una prueba no son visibles para la otra

#### Scenario: Reapertura durante una prueba
- **WHEN** una prueba guarda datos, cierra todos los recursos y vuelve a abrir el mismo archivo temporal
- **THEN** puede cargar los datos persistidos desde SQLite

### Requirement: Separación de Data, Core y WPF
La implementación SQLite SHALL residir en `SistemaDocente.Data`. Core MUST NOT referenciar Data ni SQLite, y WPF MUST NOT ejecutar comandos SQLite ni depender de tipos concretos del proveedor para acceder a datos.

#### Scenario: Inspeccionar dependencias
- **WHEN** se revisan referencias y código de Core, Data y App.Wpf
- **THEN** sólo Data contiene la dependencia y los comandos SQLite, Core permanece independiente y App.Wpf sólo será responsable de proporcionar la ruta configurada en una composición futura
