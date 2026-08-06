## MODIFIED Requirements

### Requirement: Esquema SQLite version 4 con escala de logro
La base de datos SHALL migrar de `user_version = 3` a `user_version = 4` mediante una unica transaccion. La columna `estado` de `entregas_actividad` SHALL aceptar los valores `0` (Pendiente), `1` (Domina), `2` (Suficiente), `3` (EnProceso), `4` (RequiereApoyo) y `5` (NoEntrego). El CHECK de la columna SHALL actualizarse para cubrir exactamente esos seis valores. Los registros existentes con estado `0` (Pendiente) permanecen sin cambio; no existe un estado de migracion para registros previos Entregada o NoEntregada porque esta migracion ocurre antes de que existan registros con esos valores en produccion.

#### Scenario: Base nueva en version 4
- **WHEN** se inicializa una base de datos nueva
- **THEN** se crea directamente en v4 con el CHECK de seis valores para nivel de logro

#### Scenario: Migracion v3 a v4 exitosa
- **WHEN** se abre una base en v3 con proyectos, actividades y entregas Pendiente
- **THEN** la migracion actualiza el CHECK, conserva todos los datos y establece `user_version = 4`

#### Scenario: Fallo de migracion revierte
- **WHEN** falla cualquier paso de la migracion v3 a v4
- **THEN** la base permanece en v3 sin cambios parciales

