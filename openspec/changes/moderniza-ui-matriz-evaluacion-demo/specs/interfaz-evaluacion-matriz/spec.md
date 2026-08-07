## Purpose

Define la matriz de Evaluación como superficie estudiante × actividad, manteniendo padrones históricos, accesibilidad y atomicidad por actividad.

## ADDED Requirements

### Requirement: Matriz estudiante por actividad
Evaluación SHALL mostrar estudiantes en filas y actividades del proyecto en columnas, sin un selector independiente de actividad. `Núm.` y `Estudiante` SHALL permanecer congelados durante el desplazamiento horizontal.

#### Scenario: Proyecto con varias actividades
- **WHEN** el usuario selecciona un proyecto con varias actividades
- **THEN** la vista muestra una columna por actividad y permite comparar el avance de cada estudiante entre columnas

### Requirement: Código visual estable de actividad
Cada columna SHALL mostrar un código visual derivado de `ActividadId` que no dependa del orden, título ni fecha de la actividad. El nombre y la fecha SHALL permanecer disponibles mediante tooltip y nombre accesible.

#### Scenario: Reordenar actividades
- **WHEN** cambia el orden de las actividades del proyecto
- **THEN** cada actividad conserva el mismo código visual

### Requirement: Padrón histórico respetado
Una celda SHALL ser editable únicamente cuando el estudiante pertenecía al padrón histórico de esa actividad. Una alta posterior SHALL aparecer como `—` en actividades previas y no SHALL agregarse retroactivamente.

#### Scenario: Alta posterior al inicio del proyecto
- **WHEN** un estudiante se incorpora después de las primeras actividades
- **THEN** sus celdas previas son no aplicables y las posteriores pueden editarse

### Requirement: Teclado contextual y editor compacto
Los atajos de evaluación SHALL procesarse únicamente cuando el foco pertenece a la grilla. `Enter` o `F2` y doble clic SHALL abrir un editor compacto de la celda sin ensanchar permanentemente la matriz.

#### Scenario: Escribir dentro de un TextBox
- **WHEN** el foco está en un control de texto de la vista
- **THEN** las letras de atajo no modifican ninguna celda

### Requirement: Guardado secuencial por actividad
Presentation SHALL detectar las columnas modificadas y guardar cada actividad de forma secuencial usando la operación atómica existente de padrón completo. Un fallo posterior no SHALL revertir actividades ya confirmadas.

#### Scenario: Falla la segunda actividad
- **WHEN** la primera actividad se guarda correctamente y la segunda falla
- **THEN** la primera permanece confirmada y la segunda conserva su edición local

### Requirement: Celdas y columnas virtualizadas
La matriz SHALL mantener virtualización de filas y columnas y SHALL controlar su propio desplazamiento para seguir siendo utilizable con grupos y proyectos de tamaño operativo.

#### Scenario: Grupo numeroso
- **WHEN** se abre la matriz con decenas de estudiantes y múltiples actividades
- **THEN** la grilla conserva desplazamiento y selección sin renderizar toda la matriz fuera de su viewport