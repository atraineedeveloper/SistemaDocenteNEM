## Context

`SistemaDocente.Core` contiene el agregado `Grupo`, pero su API actual sólo genera identidades nuevas. `SistemaDocente.Data` referencia Core y está vacío; Core no referencia Data. La especificación de estructura asigna SQLite a Data y prohíbe acceso directo desde WPF. Véanse `proposal.md` y las especificaciones delta para la motivación y el contrato de comportamiento.

## Goals / Non-Goals

**Goals:**

- Persistir un agregado completo en un archivo SQLite local mediante una transacción.
- Conservar exactamente identidades y estado del dominio al cerrar y reabrir la base.
- Duplicar en SQLite restricciones críticas para proteger la integridad ante escrituras externas o defectuosas.
- Mantener proveedor, SQL, inicialización y errores técnicos dentro de Data.
- Rehidratar Core mediante una API pública neutral y atómica.
- Probar contra el motor SQLite real con archivos temporales aislados.

**Non-Goals:**

- Implementar UI, ViewModels, navegación, servicios visuales o acceso SQLite desde WPF.
- Incorporar asistencia, actividades, evaluación, reportes, importación o datos personales adicionales.
- Añadir modalidad portable, eliminación definitiva, respaldos o cifrado.
- Diseñar sincronización remota o concurrencia multiusuario.
- Diseñar una cadena de migraciones más allá del esquema inicial.

## Decisions

### Acceso directo con Microsoft.Data.Sqlite

Data usará `Microsoft.Data.Sqlite` directamente. El modelo requiere dos tablas, consultas acotadas y control explícito de transacciones, restricciones, triggers de prueba y `PRAGMA`; una capa ORM no aporta suficiente valor en esta etapa.

Alternativas descartadas:

- **Entity Framework Core SQLite:** añade modelo de persistencia, change tracking y convenciones desproporcionadas para el esquema inicial.
- **Dapper con SQLite:** mantiene la necesidad de escribir SQL y agrega otra dependencia para un volumen pequeño de filas y columnas.

### Ruta explícita y ubicación productiva conocida fuera de Data

Data recibirá siempre una ruta explícita y validada. No consultará variables de entorno, carpetas del sistema ni componentes WPF. La futura composición de App.Wpf calculará por defecto `%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db`, creará o validará el directorio cuando corresponda y entregará la ruta a Data. Esa composición no forma parte de esta implementación.

Las pruebas entregarán rutas únicas bajo directorios temporales. No habrá modo portable ni un valor implícito basado en el directorio de trabajo.

### Fábrica pública neutral de rehidratación en Core

Core incorporará un método estático equivalente a:

```csharp
public static Grupo Rehidratar(
    GrupoId id,
    string nombreVisible,
    IReadOnlyCollection<DatosEstudianteRehidratado> estudiantes)
```

`DatosEstudianteRehidratado` será un tipo público, inmutable y neutral de Core con `EstudianteId`, nombre visible, número de lista y estado activo. `GrupoId` y `EstudianteId` ofrecerán conversiones públicas neutrales desde y hacia `Guid` para que Data represente valores existentes sin generar otros nuevos.

La fábrica validará primero el nombre y toda la colección en estructuras temporales: identificadores válidos y no repetidos, nombres ya normalizados y dentro del límite, números positivos y unicidad sólo entre activos. Sólo después construirá y devolverá el agregado completo. Un fallo no expondrá ninguna instancia parcial.

`Grupo.Crear` y `AgregarEstudiante` conservarán su generación interna actual. No se usará `InternalsVisibleTo`, y Core no referenciará Data, SQLite ni tipos del proveedor.

Alternativas descartadas:

- **`InternalsVisibleTo`:** acoplaría Core al nombre del ensamblado Data.
- **DTO definido en Data:** invertiría la dependencia o forzaría a Core a conocer infraestructura.
- **Recrear mediante APIs normales:** generaría identidades nuevas y no puede representar de forma directa todos los estados persistidos.

### Esquema relacional y representación exacta

Esquema versión 1 previsto:

- `grupos`: `id TEXT PRIMARY KEY`, `nombre TEXT NOT NULL`, con `CHECK (length(trim(nombre)) BETWEEN 1 AND 100)`.
- `estudiantes`: `id TEXT PRIMARY KEY`, `grupo_id TEXT NOT NULL`, `nombre TEXT NOT NULL` con `CHECK (length(trim(nombre)) BETWEEN 1 AND 150)`, `numero_lista INTEGER NOT NULL CHECK (numero_lista > 0)`, `activo INTEGER NOT NULL CHECK (activo IN (0, 1))` y clave foránea a `grupos(id)` con borrado restringido.
- Índice normal sobre `estudiantes(grupo_id)`.
- Índice único parcial sobre `estudiantes(grupo_id, numero_lista) WHERE activo = 1`.

Los identificadores se escribirán como texto canónico de `Guid`. Cada conexión ejecutará `PRAGMA foreign_keys = ON`, porque SQLite no aplica claves foráneas globalmente por defecto.

Core seguirá normalizando nombres. Las restricciones SQLite sólo proporcionan una defensa mínima frente a vacío después de `trim` y longitud. Data leerá el texto exacto y no lo corregirá; `Grupo.Rehidratar` rechazará nombres manipulados que no coincidan con su forma normalizada.

### Inicialización estricta con PRAGMA user_version

El inicializador abrirá la conexión y clasificará la base antes de escribir:

- Archivo inexistente o base vacía con `user_version = 0`: crear esquema y establecer versión 1 dentro de una transacción.
- Versión 0 con cualquier objeto de usuario en `sqlite_master`: rechazar sin cambios.
- Versión 1: comparar tablas, columnas, claves, restricciones e índices requeridos; aceptar sólo si son compatibles.
- Versión mayor que 1: rechazar sin cambios.
- Archivo no SQLite, base dañada o estructura incompatible: fallar sin reparar, borrar, recrear ni sobrescribir.

Una segunda inicialización compatible será idempotente. No se incorporará todavía una tabla de migraciones ni lógica de reparación.

### Guardado completo mediante upsert sin borrado físico

Guardar recibirá un `Grupo`, iniciará una transacción, hará upsert del grupo y luego de cada estudiante. Se confirmará sólo después de todas las escrituras. Los estudiantes inactivos se persistirán y las filas ausentes no se borrarán, porque Core no ofrece eliminación definitiva.

Para cada estudiante, el SQL distinguirá inserción de actualización. Si el `EstudianteId` ya existe, comprobará que su `grupo_id` coincide con el agregado actual y actualizará sólo los demás campos. Una asociación diferente será un fallo de integridad; no se actualizará la clave foránea y toda la transacción se revertirá.

Alternativa descartada: borrar todos los estudiantes y reinsertarlos. Introduce trabajo destructivo, dificulta diagnosticar fallos y debilita la estabilidad de relaciones.

### Carga consistente y ausencia normal

Cargar consultará primero el grupo y después todos sus estudiantes usando la misma conexión. Un grupo inexistente devolverá ausencia normal, por ejemplo `Grupo?`, y no una excepción. Los resultados se convertirán a identificadores tipados y a `DatosEstudianteRehidratado`; sólo después de reunir el snapshot completo se invocará `Grupo.Rehidratar`.

Data no normalizará ni corregirá datos. Una base manipulada que incumpla invariantes será rechazada por Core y se informará como fallo de integridad de datos persistidos, no como ausencia.

### Errores de infraestructura y fallo seguro

Data expondrá excepciones propias para distinguir error general de acceso, esquema incompatible e integridad de persistencia. Todas conservarán la excepción original cuando exista. `SqliteException` no será parte del contrato público hacia capas superiores.

Una base dañada, un archivo no SQLite o un esquema incompatible fallará de forma no destructiva. La recuperación, los respaldos, el cifrado y los mensajes visuales quedan fuera de alcance.

### Prueba real de atomicidad

La prueba de rollback guardará primero un agregado conocido. Después instalará en esa misma base un trigger temporal o específico de prueba sobre estudiantes que ejecute `RAISE(ABORT)` para uno de los registros intermedios. Se modificará el grupo y más de un estudiante y se intentará guardar. Tras recibir el fallo, una conexión nueva comprobará que ni el cambio del grupo ni ninguna escritura anterior al trigger quedó confirmada.

Esta prueba usa un fallo real del motor dentro de la transacción y no mocks ni una excepción anterior a la primera escritura.

### Pruebas con archivo temporal por caso

Cada prueba creará un directorio temporal único y una base en archivo, inicializará su propio componente Data y eliminará sus recursos al terminar. Se cerrarán y reabrirán conexiones para verificar persistencia real. `Microsoft.Data.Sqlite` aportará el motor nativo; no se requerirá instalación externa.

Las pruebas de restricciones abrirán conexiones con claves foráneas habilitadas y ejecutarán SQL directo sólo como mecanismo de verificación. Se cubrirán base nueva, idempotencia, versiones y estructuras incompatibles, archivo no SQLite, claves foráneas, límites de nombres, número, estado, unicidad, traslado de identidad, rollback, reapertura y aislamiento.

## Risks / Trade-offs

- [La fábrica pública permite representar identidades existentes] → Mantenerla separada de creación, exigir snapshot completo y validar todas las invariantes antes de devolver el agregado.
- [Las restricciones SQLite no reproducen toda la normalización Unicode de Core] → No corregir en Data y validar nuevamente mediante `Grupo.Rehidratar`.
- [Los upserts no eliminan filas ausentes] → Core no ofrece eliminación; posponer sincronización destructiva hasta que exista esa capacidad.
- [Dos escritores pueden producir bloqueo SQLite] → Mantener transacciones cortas y propagar error de infraestructura; concurrencia multiusuario está fuera de alcance.
- [Una base dañada no se recupera automáticamente] → Fallar sin destruir datos; recuperación y respaldos se diseñarán por separado.

## Migration Plan

1. Añadir identificadores convertibles de forma neutral, `DatosEstudianteRehidratado` y `Grupo.Rehidratar` en Core, con pruebas de atomicidad e invariantes.
2. Añadir `Microsoft.Data.Sqlite` únicamente a Data.
3. Incorporar configuración por ruta explícita e inicializador estricto de esquema versión 1.
4. Incorporar guardado y carga transaccionales y errores de infraestructura.
5. Añadir pruebas de integración con archivos temporales, incluidas restricciones y trigger de rollback.
6. Ejecutar restore, format, build y test de la solución completa.

Rollback: retirar la implementación y dependencia de Data y el contrato de rehidratación añadido. Las bases creadas se conservarán; no se borrarán automáticamente.
