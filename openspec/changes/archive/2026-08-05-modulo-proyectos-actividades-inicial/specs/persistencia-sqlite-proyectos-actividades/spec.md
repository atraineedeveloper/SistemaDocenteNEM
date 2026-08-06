## Purpose

Persiste proyectos, actividades y entregas con integridad relacional y transacciones por agregado sobre la base SQLite local vigente.

## ADDED Requirements

### Requirement: Esquema SQLite versión 3
La versión 3 SHALL añadir estructuras para proyectos didácticos, actividades y entregas, con claves estables, versiones positivas, campos obligatorios, límites, índices y claves foráneas. Los estados SHALL almacenarse como enteros explícitos protegidos por `CHECK`; las fechas SHALL usar `yyyy-MM-dd` canónico y `PRAGMA foreign_keys = ON` SHALL habilitarse en cada conexión.

#### Scenario: Inspeccionar base v3
- **WHEN** se crea o migra correctamente una base
- **THEN** existen tablas, restricciones e índices de proyectos, actividades y entregas con `user_version = 3`

### Requirement: Migración segura de v2 a v3
Data SHALL validar completamente v2 y migrarla a v3 dentro de una única transacción, sin destruir ni alterar datos de grupo, estudiantes o asistencia. SHALL establecer `user_version = 3` sólo después de crear correctamente todos los objetos. Ante error SHALL conservar versión, estructura y datos de v2 sin objetos parciales; una base nueva SHALL crearse directamente en v3.

#### Scenario: Migrar base con datos
- **WHEN** se abre una base v2 compatible con grupo, estudiantes y asistencias
- **THEN** pasa a v3 conservando exactamente los datos anteriores y admitiendo los nuevos módulos

#### Scenario: Fallo de migración
- **WHEN** un error ocurre antes de completar la migración
- **THEN** la base sigue en v2, sin tablas parciales ni cambios en datos existentes

### Requirement: Integridad de pertenencias
El esquema SHALL garantizar que una actividad pertenezca al mismo grupo que su proyecto y que cada estudiante de una entrega pertenezca al grupo de la actividad, mediante claves candidatas y foráneas compuestas equivalentes. MUST NOT permitirse mover identidades entre grupos o proyectos mediante upsert.

#### Scenario: Actividad de grupo distinto
- **WHEN** se intenta insertar una actividad cuyo GrupoId no coincide con el proyecto
- **THEN** SQLite rechaza la escritura y se revierte la operación

#### Scenario: Entrega de estudiante ajeno
- **WHEN** se intenta registrar un estudiante perteneciente a otro grupo
- **THEN** SQLite rechaza la actividad completa

### Requirement: Persistencia transaccional específica
Guardar un proyecto SHALL usar una transacción propia. Guardar una actividad SHALL escribir su encabezado y todas sus entregas mediante una conexión y una transacción, con upsert no destructivo y control de versión. No SHALL abrirse una conexión por estudiante ni abarcar varias actividades en la misma transacción de dominio.

#### Scenario: Rollback a mitad de actividad
- **WHEN** un fallo real ocurre después de escribir encabezado o algunas entregas
- **THEN** se revierte la actividad completa y ninguna entrega parcial queda almacenada

### Requirement: Conservación y eliminación controlada
Las claves foráneas MUST NOT usar cascadas que puedan borrar historial pedagógico accidentalmente. Data SHALL eliminar físicamente sólo después de que Application autorice un proyecto Borrador vacío o una actividad sin seguimiento; las actividades anuladas y sus entregas SHALL conservarse.

#### Scenario: Anular actividad
- **WHEN** se persiste una anulación autorizada
- **THEN** encabezado y entregas permanecen consultables sin borrado físico

### Requirement: Consultas parametrizadas y ordenadas
Data SHALL cargar proyectos por grupo, actividades por proyecto y actividades por intervalo cuando se requiera validar periodos, usando consultas parametrizadas y orden determinista. Una ausencia SHALL devolverse como resultado normal y un fallo técnico SHALL traducirse una sola vez a excepciones aprobadas de Application conservando la causa interna.

#### Scenario: Listar actividades
- **WHEN** un proyecto contiene actividades activas y anuladas
- **THEN** Data devuelve todos los agregados completos en orden contractual sin exponer tipos SQLite

