## Purpose

Define la experiencia para que un docente cree y administre un único grupo local y sus estudiantes desde WPF, con estado confirmado, teclado y errores seguros.

## ADDED Requirements

### Requirement: Rutas y composición local
La aplicación SHALL usar `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db` para SQLite y `%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json` para el estado de aplicación. Data SHALL utilizarse únicamente en la raíz de composición y App.Wpf MUST NOT añadir una referencia de paquete a `Microsoft.Data.Sqlite`.

#### Scenario: Iniciar la composición
- **WHEN** se inicia App.Wpf con LocalApplicationData disponible
- **THEN** construye persistencia, casos de uso, almacenamiento de app-state, servicios visuales y ViewModels y entrega el ViewModel a MainWindow

#### Scenario: Fallar durante la composición
- **WHEN** no puede prepararse una ruta o servicio
- **THEN** se muestra un mensaje general y ninguna excepción esperada cierra la aplicación

### Requirement: Estado del único grupo
`app-state.json` SHALL contener únicamente `GrupoId` y SHALL escribirse atómicamente mediante un temporal y reemplazo, sólo después de crear el grupo exitosamente. MUST NOT guardar nombres o datos personales ni borrar, reparar o recrear SQLite.

#### Scenario: Primera apertura
- **WHEN** no existe `app-state.json`
- **THEN** se muestra la bienvenida para crear el grupo

#### Scenario: Guardar referencia tras creación
- **WHEN** la creación y persistencia del grupo terminan correctamente
- **THEN** se escribe atómicamente su `GrupoId` y se muestra gestión

#### Scenario: No guardar referencia tras fallo
- **WHEN** la creación falla por dominio o persistencia
- **THEN** no se crea ni reemplaza `app-state.json` y permanece la bienvenida

#### Scenario: Estado vacío o dañado
- **WHEN** `app-state.json` está vacío, tiene JSON inválido, estructura inesperada o identidad inválida
- **THEN** se muestra un mensaje y se vuelve a bienvenida sin modificar SQLite

#### Scenario: Grupo referenciado inexistente
- **WHEN** el archivo contiene un `GrupoId` válido que Application no encuentra
- **THEN** se informa la inconsistencia y se permite olvidar únicamente la referencia

### Requirement: MainWindow única y paneles integrados
La interfaz SHALL usar una sola MainWindow. Bienvenida, gestión, alta, edición de estudiante y cambio de nombre SHALL ser estados o paneles integrados. MUST NOT crear ventanas modales de edición adicionales.

#### Scenario: Cambiar de bienvenida a gestión
- **WHEN** se crea o carga el grupo
- **THEN** MainWindow cambia de panel sin abrir otra ventana

#### Scenario: Abrir y cancelar una edición
- **WHEN** se abre una edición integrada y el docente la cancela
- **THEN** se descarta el estado editable y se vuelve al panel anterior sin ejecutar un comando

### Requirement: Presentación del grupo y estudiantes
La gestión SHALL mostrar el nombre del grupo y un DataGrid de sólo lectura con número, nombre y estado. MUST NOT mostrar IDs. SHALL conservar exactamente el orden recibido de Application y MUST NOT permitir orden desde encabezados que lo altere.

#### Scenario: Mostrar estados mixtos
- **WHEN** existen estudiantes activos e inactivos
- **THEN** el DataGrid muestra ambos y distingue inactivos con texto y estilo, no sólo color

#### Scenario: Seleccionar y actuar
- **WHEN** el docente selecciona una fila
- **THEN** las acciones aplicables aparecen fuera de la fila y usan internamente la selección sin mostrar su identidad

#### Scenario: Conservar el orden contractual
- **WHEN** Application devuelve estudiantes por número, nombre e identidad
- **THEN** el DataGrid presenta esa secuencia sin reordenarla

### Requirement: Gestión del grupo y estudiantes
La interfaz SHALL permitir cambiar el nombre del grupo, agregar estudiante, renombrarlo y cambiar su número mediante Application. El encabezado y la lista SHALL actualizarse únicamente después del éxito.

#### Scenario: Crear grupo válido
- **WHEN** se confirma un nombre válido y Application lo guarda
- **THEN** se muestra el grupo creado sin exponer `GrupoId`

#### Scenario: Cargar grupo configurado
- **WHEN** app-state contiene una referencia válida existente
- **THEN** se carga y muestra el grupo y su lista

#### Scenario: Alta válida
- **WHEN** se confirma nombre y número válidos
- **THEN** el estudiante aparece en la lista confirmada

#### Scenario: Alta o edición inválida
- **WHEN** Application rechaza nombre o número
- **THEN** la lista no cambia, el mensaje aparece junto a la edición y la entrada se conserva

#### Scenario: Conflicto de número
- **WHEN** Application informa un número ocupado
- **THEN** se conserva la entrada y se muestra el conflicto junto al panel

#### Scenario: Renombrar o renumerar
- **WHEN** una edición termina exitosamente
- **THEN** la lista refleja exclusivamente el resultado confirmado y conserva el orden recibido

### Requirement: Desactivación y reactivación
La interfaz SHALL pedir confirmación antes de desactivar. Cancelar MUST NOT invocar Application. La reactivación SHALL ejecutarse desde el estudiante inactivo seleccionado.

#### Scenario: Confirmar desactivación
- **WHEN** el docente confirma y Application guarda
- **THEN** la lista muestra el estudiante inactivo

#### Scenario: Cancelar desactivación
- **WHEN** el docente cancela la confirmación
- **THEN** no se invoca el comando y el estado visual no cambia

#### Scenario: Reactivar
- **WHEN** se reactiva exitosamente un estudiante seleccionado
- **THEN** la lista confirmada lo muestra activo

### Requirement: Teclado y foco
Tab SHALL recorrer controles en orden coherente; Enter SHALL confirmar la acción principal de una edición; Escape SHALL cancelar y volver al estado anterior; cada panel SHALL definir foco inicial. Ningún flujo MUST exigir ratón.

#### Scenario: Editar sólo con teclado
- **WHEN** el docente abre un panel y usa Tab, Enter o Escape
- **THEN** puede recorrer, confirmar o cancelar con foco inicial predecible

### Requirement: Estado ocupado y comandos duplicados
Los ViewModels SHALL exponer `EstaOcupado` y controlar `CanExecute`. Cada operación SHALL restaurar el estado en `finally`. MUST NOT usar `async`, `Task.Run` ni `CancellationToken` en este cambio.

#### Scenario: Bloquear duplicados
- **WHEN** una operación está en curso
- **THEN** las acciones incompatibles no pueden ejecutarse nuevamente

#### Scenario: Restaurar después de éxito o error
- **WHEN** termina una operación
- **THEN** `EstaOcupado` vuelve a falso y `CanExecute` se actualiza aunque haya ocurrido una excepción

### Requirement: Mensajes seguros y estado confirmado
Validaciones y conflictos SHALL mostrarse junto a la edición conservando entradas. Los errores técnicos SHALL usar un mensaje general y MUST NOT mostrar SQL, rutas, `InnerException` ni trazas. La pantalla SHALL conservar el último snapshot confirmado ante cualquier fallo.

#### Scenario: Persistencia fallida
- **WHEN** un comando aceptado por dominio falla al guardar
- **THEN** encabezado y lista permanecen en el estado anterior y se muestra un mensaje general

#### Scenario: Operación exitosa
- **WHEN** un comando termina correctamente
- **THEN** se refresca mediante Application o se usan exclusivamente resultados confirmados

### Requirement: Presentación comprobable sin infraestructura
Los ViewModels y servicios abstractos SHALL probarse sin WPF, Data, SQLite ni ventanas reales. Las pruebas SHALL cubrir arranque, app-state, creación, carga, edición, errores, cancelaciones, estado ocupado, orden y ausencia de IDs visibles.

#### Scenario: Probar ViewModels con dobles
- **WHEN** Presentation.Tests construye un ViewModel
- **THEN** usa dobles de Application y servicios abstractos sin cargar infraestructura concreta
