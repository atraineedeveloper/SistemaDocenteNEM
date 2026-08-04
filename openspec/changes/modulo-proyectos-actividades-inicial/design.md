## Context

La solución ya separa Core, Application, Data, Presentation y App.Wpf; usa agregados con rehidratación neutral, puertos específicos, snapshots inmutables, SQLite directo, MVVM propio y composición manual. SQLite está en `user_version = 2` y contiene grupo, estudiantes y asistencia diaria. Véase `proposal.md` para la motivación y las especificaciones delta para el comportamiento contractual.

El módulo cruza todas las capas y añade dos unidades con ciclos de vida distintos: el proyecto coordina periodo y estado, mientras cada actividad conserva un padrón histórico y se persiste atómicamente. El diseño debe impedir que esa jerarquía visual se convierta en un agregado gigante o una transacción mensual/proyecto completa.

## Goals / Non-Goals

**Goals:**

- Introducir dos agregados independientes con invariantes completas, rehidratación neutral y concurrencia optimista.
- Preservar integridad de grupo, proyecto, actividad y estudiante tanto en Core/Application como en SQLite.
- Migrar v2 a v3 sin pérdida y mantener una sola transacción por agregado guardado.
- Ofrecer una captura de hasta 40 estudiantes comprobable sin WPF real en Presentation.
- Conservar historial pedagógico sin cascadas destructivas.

**Non-Goals:**

- Convertir el proyecto en agregado o transacción de todas sus actividades.
- Añadir calificaciones, rúbricas, porcentajes académicos, adjuntos, evidencias, reportes o planeación NEM detallada.
- Introducir async, `Task.Run`, ORM, repositorios genéricos, contenedor DI, framework de navegación o paquetes UI.
- Conservar fotografía histórica de nombre o número de lista.

## Decisions

### 1. Dos agregados y referencias por identidad

`ProyectoDidactico` será un agregado con `ProyectoId`, `GrupoId`, datos descriptivos, periodo, `EstadoProyecto` y `Version`. `ActividadProyecto` será otro agregado con `ActividadId`, `ProyectoId`, `GrupoId`, datos, `EstadoActividad`, `Version` y registros de entrega.

La actividad conserva ambas identidades de pertenencia para validar rápidamente y reforzar integridad en SQLite. No contiene una referencia mutable al proyecto y el proyecto no contiene objetos actividad: Application coordina consultas cuando una regla cruza ambos. Se descarta un único agregado Proyecto con todas las actividades porque ampliaría la transacción, obligaría a cargar historial completo y aumentaría conflictos.

Las identidades seguirán el patrón de value objects respaldados por `Guid`. Las versiones serán enteros positivos: 1 al insertar y +1 por actualización confirmada.

### 2. Estados y transiciones cerrados

`EstadoProyecto` tendrá valores estables `Borrador = 0`, `EnCurso = 1` y `Finalizado = 2`. Core permitirá Borrador→EnCurso, EnCurso→Finalizado y Finalizado→EnCurso. La reapertura será una operación explícita; Presentation solicita confirmación antes de llamar al caso de uso. No habrá transición EnCurso→Borrador ni Finalizado→Borrador.

`EstadoActividad` tendrá `Activa = 0` y `Anulada = 1`. Se aprueba Anulada para conservar actividades con seguimiento que ya no deben participar en conteos. La anulación es irreversible en este MVP; reactivar implicaría decisiones pedagógicas adicionales. Una anulada es de sólo lectura.

`EstadoEntrega` tendrá `Pendiente = 0`, `Entregada = 1` y `NoEntregada = 2`. Core validará valores incluso durante rehidratación.

### 3. Límites de texto y periodos

Se fijan límites coherentes y comprobables: proyecto nombre 150, descripción 2000 y observaciones 2000; actividad título 200, descripción 2000 y observaciones generales 2000; observación por estudiante 500 caracteres. Los textos opcionales se normalizan a cadena vacía y los obligatorios usan la normalización de espacios ya adoptada por el dominio.

Core garantiza inicio ≤ término y la actividad recibe el periodo vigente como dato de validación al crear o cambiar fecha. La duración recomendada de 14–31 días no es una invariante: Presentation calcula una advertencia no bloqueante.

Para actualizar el periodo, Application consulta fechas de actividades del proyecto antes de mutar el agregado. Si alguna queda fuera, lanza un conflicto específico con las fechas ordenadas. No ajusta ni elimina actividades.

### 4. Padrón histórico completo dentro de ActividadProyecto

Cada `RegistroEntregaActividad` contiene sólo `EstudianteId`, estado y observación. Core exige identidades únicas y conjunto completo respecto de la entrada validada por Application. Al preparar, Application carga proyecto fresco, verifica que sea editable, carga grupo fresco y construye Pendiente para activos.

Al crear, Application vuelve a cargar proyecto y grupo para evitar decisiones sobre datos obsoletos y exige exactamente los activos vigentes. Al editar una actividad existente usa su padrón histórico completo: no agrega altas posteriores ni elimina inactivos. Nombres, números y actividad actual se enriquecen sólo al proyectar el snapshot.

### 5. Operaciones y concurrencia

Se usarán `IAlmacenamientoProyectos` e `IAlmacenamientoActividadesProyecto`, no un repositorio genérico. Los puertos incluirán carga por identidad, listados específicos, existencia/conteo necesarios para reglas, guardado con versión esperada y eliminaciones restringidas.

Application carga copias frescas para cada escritura. Data implementa concurrencia con `UPDATE ... WHERE id = @id AND version = @versionEsperada`, exige exactamente una fila y eleva un conflicto de Application cuando el contador es cero. Las inserciones empiezan en versión 1. Las respuestas devuelven el snapshot con la nueva versión sólo después del commit.

Crear y actualizar actividad y guardar entregas convergen en un guardado completo del agregado; no habrá API Data para guardar una entrega aislada. Cambiar estado del proyecto y anular actividad también respetan versión.

### 6. Eliminación física mínima y sin cascadas

Application autoriza eliminar un proyecto únicamente tras recargarlo, comprobar Borrador y consultar que no existan actividades. Data vuelve a protegerlo mediante claves foráneas restrictivas.

Una actividad puede eliminarse sólo si todos sus registros siguen Pendiente y existe confirmación visual. La operación elimina entregas y encabezado explícitamente dentro de una transacción; no usa `ON DELETE CASCADE`. Si existe seguimiento, Presentation ofrece Anular. Proyectos EnCurso/Finalizado, actividades anuladas, estudiantes inactivos y entregas históricas nunca se borran como efecto de sincronización.

### 7. Esquema v3 e integridad compuesta

La base nueva se crea directamente en v3. La migración v2→v3 valida primero el esquema v2 completo y, en una transacción, crea:

- `proyectos_didacticos`: PK `proyecto_id`, `grupo_id`, textos, fechas, estado y versión; FK restrictiva a grupos; `UNIQUE(proyecto_id, grupo_id)` e índices por grupo/estado/fecha.
- `actividades_proyecto`: PK `actividad_id`, `proyecto_id`, `grupo_id`, textos, fecha, estado y versión; FK compuesta `(proyecto_id, grupo_id)` al proyecto; `UNIQUE(actividad_id, grupo_id)` e índices por proyecto/fecha.
- `entregas_actividad`: PK `(actividad_id, estudiante_id)`, `grupo_id`, estado y observación; FK compuesta `(actividad_id, grupo_id)` a actividad y `(estudiante_id, grupo_id)` a la clave candidata ya disponible en estudiantes.

Las fechas usan texto canónico con validación de forma y análisis estricto al leer. Los `CHECK` cubren estados, longitudes y versiones. Ninguna FK de historial usa cascada. Las consultas y escrituras usan parámetros y una conexión por operación compuesta.

La migración establece `user_version = 3` al final. Un fallo revierte todo y deja v2 intacta. El inicializador mantiene la ruta v1→v2 existente y después aplica v2→v3, de modo que cada paso valida su versión de origen.

### 8. Snapshots y orden

Application devolverá records inmutables y arreglos nuevos para proyecto, actividad y entrega. Los agregados nunca cruzan hacia Presentation. `ProyectoDetalle` incluirá versión y número de actividades; los conteos opcionales se limitan a información obtenible en la misma consulta específica. `ActividadProyectoDetalle` incluirá versión, estado, padrón y los tres conteos.

Los comparadores implementan exactamente el orden contractual. Las actividades anuladas siguen en listados con su estado, pero se excluyen de agregaciones. No se calcula porcentaje para evitar que un indicador operativo se interprete como calificación.

### 9. Estado de Presentation y confirmaciones

Se crearán ViewModels portables para el módulo contenedor, editor/listado de proyectos, editor/listado de actividades y filas de entrega. Cada editor conserva snapshot confirmado y copia editable. `TieneCambios` se deriva por comparación, incluyendo estados y observaciones.

Cambiar actividad, proyecto, módulo o cerrar usa un único servicio Guardar/Descartar/Cancelar. Guardar sólo permite la transición después del éxito; Descartar restaura el snapshot; Cancelar conserva selección y edición. Los conflictos de versión preservan la copia editable y ofrecen recarga explícita.

Los comandos notifican `CanExecuteChanged` al cambiar selección, contenido, estado, versión, permisos o `EstaOcupado`. La ejecución sigue siendo síncrona y deshabilita acciones incompatibles mientras dura.

### 10. Vista WPF de tres zonas

`MainWindowViewModel` añadirá Proyectos como módulo. Una sola vista WPF redimensionable contendrá panel izquierdo de proyectos, panel central de actividades y panel principal de detalle/entregas. Se usará el `DataGrid` nativo con columnas generadas/estáticas según convenga, selector compacto y atajos E/N/P/Ctrl+S; no habrá ComboBox permanente ni ventanas por estudiante.

El code-behind se limita a foco, selección, teclado y comportamiento visual. IDs no se enlazan. Data se crea únicamente en `App.xaml.cs` y se inyecta manualmente a casos de uso y adaptadores.

### 11. Estrategia de pruebas y auditoría

Core probará invariantes y rehidratación de ambos agregados. Application usará dobles manuales para conjuntos frescos, periodos, concurrencia, historial, orden y snapshots. Data usará archivos SQLite temporales reales y triggers para rollback. Presentation probará comandos, edición, filtros y confirmaciones sin WPF. App.Wpf.Tests comprobará composición, navegación, bindings/estructura, atajos, 40 filas y ausencia de SQL/IDs.

Pruebas de referencias impedirán Core→capas, Application→Data/WPF, Presentation→Data/WPF/SQLite y SQL fuera de Data.

## Risks / Trade-offs

- **[Dos agregados permiten que el periodo cambie mientras se edita una actividad]** → cada escritura recarga proyecto y valida periodo y estado antes de guardar.
- **[La concurrencia optimista añade columnas y conflictos visibles]** → versiones simples, mensajes específicos y conservación de edición local.
- **[Eliminar explícitamente entregas aumenta código Data]** → mantiene visible la intención y evita cascadas destructivas accidentales.
- **[Nombres actuales cambian la presentación de históricos]** → decisión consistente con asistencia; se documenta que no existe fotografía histórica.
- **[Una vista de tres zonas puede estrecharse]** → `Grid` redimensionable, anchos mínimos y scroll; smoke test con 40 estudiantes.
- **[Migración v3 amplía la validación del esquema]** → validadores por versión, transacción única y pruebas de fallo inducido.

## Migration Plan

1. Extender el inicializador para crear v3 directamente y validar v3 completa.
2. Implementar y probar la migración transaccional v2→v3 sobre bases reales con grupo, estudiantes y asistencia.
3. Añadir Core y Application sin conectar aún la interfaz productiva.
4. Implementar adaptadores Data y pruebas de contrato/rollback.
5. Añadir Presentation y WPF, manteniendo los módulos existentes sin cambios funcionales.
6. Ejecutar formato, build, todas las pruebas, validación OpenSpec y auditoría de referencias.

El rollback de código puede volver a una versión anterior sólo antes de que una base se migre. Una vez en v3, la aplicación anterior rechazará la versión posterior sin alterar el archivo; no se realizará downgrade destructivo. Se recomienda respaldar el archivo antes de desplegar la primera versión con migración, dejando la automatización de respaldos de usuario fuera de este cambio.
