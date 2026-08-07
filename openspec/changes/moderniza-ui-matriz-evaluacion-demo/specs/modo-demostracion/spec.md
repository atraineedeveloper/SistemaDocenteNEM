## Purpose

Permite validar la aplicación con datos ficticios representativos sin escribir ni borrar información de producción.

## ADDED Requirements

### Requirement: Almacenamiento demo aislado
El modo `--demo` SHALL usar rutas de base SQLite y estado de aplicación distintas de producción. Ninguna operación demo SHALL escribir en los archivos productivos.

#### Scenario: Abrir modo demostración
- **WHEN** la aplicación inicia con `--demo`
- **THEN** crea o abre exclusivamente el almacenamiento identificado como demostración

### Requirement: Reinicio demo seguro
`--demo-reset` SHALL borrar únicamente archivos ubicados en rutas de demostración y SHALL reconstruir el dataset ficticio de forma determinista. El reset SHALL rechazarse para rutas de producción.

#### Scenario: Reiniciar datos ficticios
- **WHEN** se inicia con `--demo-reset`
- **THEN** los archivos demo previos se eliminan y se vuelven a sembrar sin modificar producción

### Requirement: Dataset representativo
El dataset demo SHALL incluir un grupo principal cercano a 30 estudiantes, un estudiante histórico inactivo, una alta posterior, asistencia de varios días, proyectos en distintos estados, múltiples actividades, evaluación variada, observaciones y seguimiento pedagógico.

#### Scenario: Alta posterior
- **WHEN** se abre Evaluación del proyecto de demostración
- **THEN** la estudiante incorporada posteriormente presenta celdas no aplicables en actividades anteriores y editables en posteriores

### Requirement: Identificación visible del modo demo
El shell SHALL indicar visualmente que la aplicación usa datos ficticios para reducir el riesgo de confundir la sesión con producción.

#### Scenario: Ventana en demostración
- **WHEN** el almacenamiento activo es demo
- **THEN** el encabezado o título de la aplicación muestra una indicación `DEMO`