## Purpose

Separa el cumplimiento de entrega del nivel de logro para representar con precisión trabajos pendientes, recibidos, no entregados y evaluados.

## ADDED Requirements

### Requirement: Estado de entrega independiente del nivel de logro
Cada entrega de actividad SHALL conservar `EstadoEntregaActividad` y `NivelLogro` como dimensiones distintas. Una actividad nueva SHALL iniciar con `Pendiente + NivelLogro.Pendiente`.

#### Scenario: Trabajo recibido aún no evaluado
- **WHEN** el docente marca una actividad como `Entregada` sin asignar todavía un nivel de logro
- **THEN** se conserva `Entregada + NivelLogro.Pendiente`

#### Scenario: Trabajo no entregado
- **WHEN** el docente marca una actividad como `NoEntregada`
- **THEN** el nivel de logro queda `Pendiente` y la no entrega se representa únicamente mediante el estado explícito

### Requirement: Normalización de combinaciones
Asignar `Domina`, `Suficiente`, `EnProceso` o `RequiereApoyo` SHALL forzar `EstadoEntregaActividad.Entregada`. `NivelLogro.NoEntrego` SHALL aceptarse sólo como valor legacy y SHALL normalizarse a `NoEntregada + Pendiente`.

#### Scenario: Evaluar una entrega pendiente
- **WHEN** se asigna `Suficiente` a una entrega que estaba pendiente
- **THEN** el estado resultante es `Entregada` y el nivel resultante es `Suficiente`

#### Scenario: Cargar valor legacy NoEntrego
- **WHEN** una entrada legacy contiene `NivelLogro.NoEntrego`
- **THEN** la aplicación la interpreta como `EstadoEntregaActividad.NoEntregada + NivelLogro.Pendiente`

### Requirement: Persistencia aditiva compatible con esquema base v6
La persistencia SHALL mantener `PRAGMA user_version = 6` y SHALL versionar esta capacidad mediante la extensión `reportes-contexto-entregas`. La inicialización SHALL ser transaccional e idempotente y no SHALL reconstruir destructivamente `entregas_actividad`.

#### Scenario: Primera apertura de una base v6
- **WHEN** una base v6 sin la extensión es inicializada
- **THEN** se crean las estructuras aditivas, se convierten los valores legacy y se registra la versión de extensión sin cambiar `PRAGMA user_version`

#### Scenario: Reapertura posterior
- **WHEN** una base ya extendida vuelve a inicializarse
- **THEN** no se duplican registros ni se vuelve a transformar información ya migrada

### Requirement: Evaluación conserva ambas dimensiones
La matriz, los filtros, las acciones masivas y el editor de Evaluación SHALL cargar, modificar, guardar y recargar estado de entrega, nivel de logro y observación sin perder una dimensión al editar la otra.

#### Scenario: Guardar entregada pendiente de evaluación
- **WHEN** una celda queda `Entregada + Pendiente`, se guarda y se vuelve a cargar
- **THEN** la celda continúa entregada y pendiente de evaluación

#### Scenario: Atajos de entrega y logro
- **WHEN** el foco está dentro de la grilla de Evaluación
- **THEN** `T/N/P` modifican el estado de entrega y `D/S/E/R` modifican el nivel de logro de la celda actual