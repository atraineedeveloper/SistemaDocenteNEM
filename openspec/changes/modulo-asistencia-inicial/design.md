## Context

El repositorio ya separa Core, Application, Data, Presentation y App.Wpf. La gestión de grupo usa casos de uso síncronos, puertos específicos, snapshots inmutables, SQLite directo y MVVM básico propio. La base productiva existente usa esquema versión 1 y Data recibe una ruta explícita; App.Wpf resuelve `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db`. Véase [proposal.md](proposal.md) para la motivación y las especificaciones de este cambio para el comportamiento requerido.

## Goals / Non-Goals

**Goals:**

- Entregar un corte vertical de asistencia sin romper la dirección de dependencias actual.
- Hacer que dominio, persistencia y estado visual tengan límites atómicos explícitos.
- Migrar bases versión 1 sin pérdida y validar estrictamente la versión 2.
- Mantener la captura de 30 a 40 estudiantes sencilla y comprobable sin abrir WPF en pruebas de Presentation.

**Non-Goals:**

- Calcular porcentajes, reportes, acumulados, alertas o efectos académicos de los estados.
- Incorporar async, concurrencia, caché, contenedor DI o un framework de navegación.
- Cambiar reglas de Grupo/Estudiante o permitir que WPF acceda a SQL.
- Añadir motivo o evidencia a una falta justificada.
- Conservar fotografías históricas del nombre o número de lista, o añadir reportes, porcentajes, observaciones, justificantes, horarios, múltiples sesiones, exportación, sincronización o múltiples grupos.

## Decisions

### 1. Modelo de dominio mínimo y exacto

Core incorporará `EstadoAsistencia`, `RegistroAsistencia` y el agregado `AsistenciaDiaria`. `AsistenciaDiaria` queda identificada naturalmente por `GrupoId` y `DateOnly`; no se añade `AsistenciaId`. Cada registro usa el `EstudianteId` ya existente y un estado cerrado. La API normal crea el día con un conjunto completo de registros y la fábrica neutral de rehidratación conserva todos los valores después de validar el snapshot entero.

`EstadoAsistencia` tendrá los valores mutuamente excluyentes `Presente`, `Falta`, `Retardo` y `Justificada`; este último significa ausencia justificada y se presentará como «Falta justificada». No contiene motivo, documento ni evidencia. Cada estudiante del padrón tiene exactamente un estado.

Se eligió un agregado por grupo-fecha porque el guardado solicitado es completo y atómico. Un registro independiente como raíz permitiría persistencias parciales y obligaría a reconstruir la invariante de unicidad fuera de Core. `DateOnly` evita hora y zona horaria en el dominio; la fecha local sólo se decide en Presentation mediante un reloj inyectable.

### 2. Semántica de histórico frente a la matrícula actual

Un día no guardado se prepara con todos los estudiantes activos actuales en estado `Presente`. Un día guardado conserva exactamente su padrón histórico y lo muestra completo. Un estudiante agregado después no se incorpora retroactivamente; uno desactivado posteriormente sigue visible y editable con el indicador textual «Inactivo actualmente»; si vuelve a activarse, conserva el mismo registro histórico.

El registro de asistencia guarda sólo `EstudianteId` y estado. Nombre, número de lista y situación activa se consultan desde la matrícula actual al proyectar el día. Por ello esta versión no conserva una fotografía histórica del nombre ni del número. La alternativa de duplicarlos en asistencia introduciría sincronización y semántica temporal no solicitadas.

### 3. Contrato y casos de uso de Application

Se añadirá `IAlmacenamientoAsistencias` con operaciones síncronas equivalentes a:

- `AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha)`;
- `bool Existe(GrupoId grupoId, DateOnly fecha)`;
- `void Guardar(AsistenciaDiaria asistencia)`.

`GestionAsistenciaCasosUso` ofrecerá operaciones equivalentes a `Cargar`, `Preparar`, `Existe` y `Guardar`. `Cargar` representa ausencia de forma normal; `Preparar` carga el grupo y devuelve el día histórico completo o un borrador con los estudiantes activos en Presente sin guardarlo. Los resultados serán `AsistenciaDiaDetalle` y `AsistenciaEstudianteDetalle`, materializados en arreglos nuevos y ordenados por número, nombre e identidad; cada fila incluirá la situación activa actual para Presentation.

Guardar diferencia dos flujos. Para un día nuevo, carga un grupo fresco, obtiene sus estudiantes activos, exige exactamente una entrada por cada identidad —sin faltantes, duplicados ni ajenos—, crea el agregado y guarda una vez. Para un día existente, carga grupo y agregado histórico completos, exige exactamente una entrada por cada fila histórica mostrada, actualiza todos los estados sobre esa misma instancia y guarda una vez conservando grupo, fecha e identidades. Cualquier error de dominio ocurre antes de `Guardar`; un fallo de persistencia no devuelve resultado, y otra llamada vuelve a cargar el último estado confirmado.

Application empleará también `IAlmacenamientoGrupos` para obtener la matrícula actual. No retendrá agregados entre llamadas. Data realizará la única traducción de errores técnicos a `ErrorPersistenciaAplicacionException`; la fachada no volverá a envolverla y las excepciones de dominio conservarán su tipo.

Se prefirieron casos de uso específicos frente a ampliar la fachada de grupo indiscriminadamente: asistencia tiene otro agregado, otro puerto y otra transacción. No se introduce un repositorio genérico ni un bus de comandos.

### 4. Esquema SQLite versión 2

La versión 2 añadirá:

- `asistencias_diarias(grupo_id TEXT, fecha TEXT, PRIMARY KEY (grupo_id, fecha))`;
- `registros_asistencia(grupo_id TEXT, fecha TEXT, estudiante_id TEXT, estado INTEGER, PRIMARY KEY (grupo_id, fecha, estudiante_id))`;
- un índice único auxiliar sobre `estudiantes(id, grupo_id)` para poder exigir con clave foránea compuesta que el estudiante pertenezca al grupo;
- claves foráneas del día al grupo, del registro al día y del registro al estudiante de ese grupo;
- checks de fecha ISO canónica y estado entero entre los cuatro valores asignados;
- índices para búsquedas por grupo-fecha y por estudiante.

Los valores estables serán `Presente = 0`, `Falta = 1`, `Retardo = 2` y `Justificada = 3`. Data serializará `DateOnly` exactamente como `yyyy-MM-dd` con cultura invariante. El check SQLite combinará forma canónica y validez de calendario; al leer, Data analizará estrictamente con `DateOnly` y comprobará que volver a formatear produce la misma cadena. No normalizará ni reparará valores manipulados. Las pruebas cubrirán cadena vacía, formato incorrecto, mes 13, día 00, 31 de febrero, 29 de febrero no bisiesto, fecha con hora y 29 de febrero bisiesto válido.

El inicializador creará directamente v2 en una base vacía, migrará sólo una v1 completamente validada y validará estrictamente una v2. Rechazará v0 con objetos, versiones posteriores, archivos no SQLite y estructuras incompatibles sin reparar ni recrear. Para v1→v2, el índice auxiliar, tablas, índices y `user_version` se ejecutarán en una sola transacción; `user_version = 2` será la última instrucción. Cualquier fallo hará rollback a v1 sin objetos parciales y sin alterar grupos ni estudiantes. La prueba construirá una base v1 auténtica con datos, no una aproximación incompleta.

### 5. Persistencia completa sin borrado histórico

El adaptador SQLite cargará y rehidratará todo el agregado. Guardar ejecutará un upsert del encabezado y de cada registro dentro de una transacción. No realizará borrado físico ni sincronización destructiva: los estudiantes desactivados continúan visibles y editables en el padrón histórico. Application garantiza que un día nuevo contiene el conjunto activo completo y que una actualización contiene exactamente todas las filas históricas mostradas.

Una prueba instalará un trigger temporal con `RAISE(ABORT)` para fallar después de una escritura parcial y comprobar el rollback real. `PRAGMA foreign_keys = ON` se activará en cada conexión.

### 6. MVVM propio y navegación mínima

Presentation incorporará `GestionAsistenciaViewModel`, un modelo de fila observable y comandos basados en la infraestructura MVVM propia existente. Dependerá de una abstracción específica de los casos de uso, un servicio de diálogos y un reloj local; no dependerá de Data, SQLite ni tipos de ventana. Mantendrá dos representaciones: el snapshot confirmado y una copia editable. La comparación de grupo, fecha, identidades y estados determina `TieneCambios`; un borrador con `EsPersistido = false` siempre requiere confirmación aunque todas las filas permanezcan en Presente.

App.Wpf compondrá el adaptador y los casos de uso en `App`, igual que la gestión de grupo. `MainWindow` alternará dos contenidos mediante un estado simple del ViewModel raíz o controles contenidos; no se añade un framework. La vista de asistencia usará `DatePicker`, `DataGrid`, selector de estado, conteos de todas las filas visibles, botón Marcar todos presentes y comando `Ctrl+S`. Conservará el orden recibido de Application, no mostrará IDs y señalará textualmente la inactividad actual. El code-behind sólo interceptará el cierre para delegar la confirmación al ViewModel y cancelar visualmente si corresponde.

### 7. Confirmación Guardar/Descartar/Cancelar

Antes de cambiar de fecha, navegar a Grupo o cerrar, el ViewModel reutilizará el mismo flujo si existen cambios o si el día nunca fue persistido. Guardar ejecuta el único comando completo y sólo continúa tras éxito; Descartar abandona la copia editable sin guardar y continúa; Cancelar conserva íntegramente edición, fecha, módulo y ventana. Un fallo de guardado impide la transición.

Para un día nuevo preparado pero nunca persistido, el estado inicial cuenta como pendiente de confirmación: se muestra «Sin guardar» y Guardar está disponible aunque todas las filas sigan en Presente. Para un día ya guardado, Guardar se deshabilita si no hay diferencias.

## Risks / Trade-offs

- **[Un estudiante nuevo no aparece en días históricos ya guardados]** → Se documenta como conservación del padrón histórico; sólo los días aún no guardados usan la matrícula activa actual.
- **[Nombre y número históricos reflejan la matrícula actual]** → La interfaz señala únicamente la situación activa actual y el alcance documenta expresamente que no existe fotografía histórica de esos campos.
- **[La migración añade una clave candidata sobre estudiantes]** → El inicializador valida primero v1 y crea el índice dentro de la misma transacción que las tablas nuevas.
- **[La captura es síncrona y puede bloquear brevemente la UI]** → Las operaciones son locales y pequeñas; se muestra estado de operación y se deshabilitan acciones. Async queda expresamente fuera de alcance.
- **[Cerrar WPF requiere code-behind]** → El manejador sólo traduce el evento visual cancelable a una decisión del ViewModel; no contiene reglas ni persistencia.
- **[Los valores enteros del enum quedan persistidos]** → Se fijan explícitamente en diseño, checks y pruebas de compatibilidad.

## Migration Plan

1. Añadir y probar el dominio y los casos de uso sin cambiar todavía el esquema productivo.
2. Construir en pruebas una base v1 auténtica con datos y verificar su estructura completa antes de cualquier mutación.
3. Extender el inicializador para crear v2 en bases vacías y ejecutar índice, tablas, índices y `user_version = 2` —al final— dentro de una única transacción para v1.
4. Probar el rollback de migración, confirmando versión 1, ausencia de objetos parciales e integridad de grupos y estudiantes después de un fallo inducido.
5. Añadir el adaptador SQLite y ejecutar pruebas de restricciones de fecha, claves, reapertura y rollback de guardado con archivos temporales reales.
6. Integrar Presentation y App.Wpf mediante composición manual, padrón histórico completo, indicador textual y navegación mínima.
7. Ejecutar restore, formato, build, pruebas completas y validación OpenSpec.

Si la migración falla antes del commit, la transacción deja la base en v1. Después de una migración confirmada no habrá downgrade automático: una versión de la aplicación que sólo entienda v1 deberá usar un respaldo externo, cuya gestión queda fuera de este cambio.

## Open Questions

Ninguna.
