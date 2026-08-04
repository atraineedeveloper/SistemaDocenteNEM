## MODIFIED Requirements

### Requirement: Base SQLite local e inicialización automática
El sistema SHALL almacenar los agregados en un archivo SQLite local sin servidor. Al recibir una ruta válida donde no existe un archivo, Data SHALL crear la base y el esquema versión 3 automáticamente, incluidas las estructuras vigentes de grupo, estudiantes, asistencia, proyectos y actividades.

#### Scenario: Crear automáticamente una base nueva
- **WHEN** se inicializa la persistencia con una ruta válida cuyo archivo no existe
- **THEN** se crea un archivo SQLite con `user_version` igual a 3 y el esquema completo compatible

#### Scenario: Reabrir una base existente
- **WHEN** se cierra y vuelve a abrir una base v3 compatible creada o migrada previamente
- **THEN** el esquema y todos los datos guardados permanecen disponibles sin reinicialización destructiva

### Requirement: Inicialización y compatibilidad estrictas del esquema
Data SHALL interpretar `user_version` y validar la estructura existente antes de modificar la base. `user_version = 0` en una base vacía SHALL crear atómicamente la versión 3; versión 0 con objetos preexistentes MUST ser rechazada. Una v1 compatible SHALL migrarse transaccionalmente a v2 y después a v3; una v2 compatible SHALL migrarse transaccionalmente a v3; una v3 SHALL aceptarse sólo si toda su estructura es compatible. Una versión mayor que 3 o cualquier estructura incompatible MUST rechazarse sin reparar, borrar, recrear ni modificar automáticamente el archivo.

#### Scenario: Inicialización idempotente de versión 3
- **WHEN** se inicializa de nuevo una base con `user_version = 3` y estructura compatible
- **THEN** la inicialización termina correctamente sin alterar esquema ni datos

#### Scenario: Migrar versión uno
- **WHEN** se abre una base v1 compatible
- **THEN** las migraciones ordenadas producen una v3 compatible conservando grupos y estudiantes

#### Scenario: Migrar versión dos
- **WHEN** se abre una base v2 compatible
- **THEN** se obtiene una v3 compatible conservando grupos, estudiantes y asistencia

#### Scenario: Rechazar versión posterior
- **WHEN** la base declara `user_version` mayor que 3
- **THEN** Data informa un error de esquema y deja el archivo sin cambios

#### Scenario: Rechazar versión cero con objetos preexistentes
- **WHEN** una base con `user_version = 0` contiene cualquier objeto de esquema creado por el usuario
- **THEN** Data informa un error de esquema y no crea, repara ni elimina objetos

#### Scenario: Rechazar estructura incompatible
- **WHEN** una base declara una versión conocida pero carece de una tabla, columna, restricción o índice requerido
- **THEN** Data informa un error de esquema y no modifica la estructura
