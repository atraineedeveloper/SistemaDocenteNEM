## MODIFIED Requirements

### Requirement: Flujo pedagogico claro en tres zonas
La interfaz SHALL reflejar el flujo docente natural mediante tres zonas visualmente diferenciadas: panel izquierdo de lista de proyectos, panel central de actividades del proyecto seleccionado y panel derecho de evaluacion de desempeno de la actividad seleccionada. El docente SHALL poder navegar el flujo de izquierda a derecha sin pasos ambiguos.

#### Scenario: Navegacion natural del flujo
- **WHEN** el docente selecciona un proyecto
- **THEN** el panel central muestra sus actividades y el panel derecho permanece vacio hasta seleccionar una actividad

#### Scenario: Seleccion de actividad muestra evaluacion
- **WHEN** el docente selecciona una actividad del proyecto activo
- **THEN** el panel derecho muestra la grilla de evaluacion con todos los estudiantes del padron historico

### Requirement: Captura de nivel de logro eficiente
La grilla de evaluacion SHALL mostrar numero de lista, nombre, indicador de inactivo actualmente, nivel de logro actual y observacion por estudiante. SHALL permitir asignar nivel mediante atajos de teclado `D` (Domina), `S` (Suficiente), `E` (En proceso), `R` (Requiere apoyo) y `N` (No entrego) sobre filas seleccionadas. SHALL mostrar conteos de Pendiente, Domina, Suficiente, En proceso, Requiere apoyo y No entrego. No SHALL usar ComboBox permanente cuando el selector este cerrado.

#### Scenario: Atajo de teclado por nivel
- **WHEN** el docente selecciona filas y presiona D, S, E, R o N
- **THEN** se asigna el nivel correspondiente a todas las filas seleccionadas y se recalculan conteos

#### Scenario: Marcar todos con un nivel
- **WHEN** el docente usa la accion de marcar todos con un nivel especifico
- **THEN** todos los registros editables del padron reciben ese nivel

### Requirement: Representacion visual de la escala de logro
Cada nivel de logro SHALL representarse con una etiqueta clara en espanol y un indicador visual compacto: `Domina` (D), `Suficiente` (S), `En proceso` (EP), `Requiere apoyo` (RA) y `No entrego` (NE). `Pendiente` SHALL indicar visualmente que aun no se ha evaluado. La interfaz SHALL usar distincion cromatica o icono para diferenciar los cuatro niveles de desempeno, el incumplimiento y el estado pendiente.

#### Scenario: Distincion visual entre niveles
- **WHEN** la grilla muestra registros con distintos niveles
- **THEN** cada nivel es distinguible visualmente del resto sin depender solo del color

#### Scenario: Pendiente diferenciado
- **WHEN** un registro esta en estado Pendiente
- **THEN** se muestra de forma que el docente identifique facilmente que no ha sido evaluado aun

### Requirement: Filtros actualizados para la escala de logro
La grilla SHALL ofrecer filtros: Todos, Pendientes, Domina, Suficiente, En proceso, Requiere apoyo, No entrego, Solo incidencias (Pendiente o Requiere apoyo o No entrego), Activos y Activos e inactivos historicos.

#### Scenario: Filtro de incidencias
- **WHEN** el docente filtra Solo incidencias
- **THEN** permanecen unicamente estudiantes con Pendiente, Requiere apoyo o No entrego

