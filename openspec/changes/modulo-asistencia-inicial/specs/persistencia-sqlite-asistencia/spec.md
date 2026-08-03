## Purpose

Conserva agregados diarios en SQLite y permite cargarlos eficientemente por intervalo para construir proyecciones mensuales sin cambiar el esquema existente.

## ADDED Requirements

### Requirement: Esquema versión 2 sin cambios
La evolución mensual SHALL reutilizar las tablas, restricciones, claves, índices y `PRAGMA user_version = 2` existentes. No SHALL añadir tablas mensuales ni migrar el esquema salvo una contradicción documentada.

#### Scenario: Abrir una base v2 compatible
- **WHEN** se usa la consulta mensual sobre una base válida existente
- **THEN** `user_version` y la estructura permanecen sin cambios

### Requirement: Persistencia diaria atómica
Cada `AsistenciaDiaria` SHALL continuar guardándose con una transacción propia mediante upsert, sin borrar registros históricos. SQLite no SHALL ofrecer ni simular una transacción mensual distribuida entre llamadas diarias.

#### Scenario: Guardar dos días
- **WHEN** Application guarda dos fechas secuencialmente
- **THEN** cada fecha tiene su propio commit y el éxito de la primera no se revierte si falla la segunda

### Requirement: Carga específica por intervalo
El adaptador SHALL cargar por `GrupoId`, fecha inicial y fecha final inclusivas usando una sola conexión y consultas parametrizadas, devolver agregados diarios completos ordenados por fecha y distinguir intervalo vacío de error técnico.

#### Scenario: Mes sin registros
- **WHEN** el intervalo no contiene asistencias
- **THEN** se devuelve una colección vacía sin crear datos

#### Scenario: Mes parcialmente guardado
- **WHEN** sólo algunas fechas del intervalo existen
- **THEN** se devuelven exactamente esos agregados completos en orden

#### Scenario: Rango inválido
- **WHEN** la fecha inicial es posterior a la final
- **THEN** se rechaza la operación antes de consultar

#### Scenario: Reapertura
- **WHEN** se cierra y reabre el archivo después de guardar varios días
- **THEN** la consulta de intervalo conserva fechas, identidades, estados y padrones históricos

### Requirement: Integridad y fechas canónicas
La consulta por intervalo SHALL mantener `PRAGMA foreign_keys = ON`, análisis estricto `DateOnly`, recorrido canónico `yyyy-MM-dd` y rehidratación completa. No SHALL normalizar datos manipulados ni exponer `SqliteException`.

#### Scenario: Fecha persistida manipulada
- **WHEN** una fila contiene una fecha no canónica o imposible
- **THEN** la carga falla de forma identificable y no devuelve días parciales
