## Purpose

Coordina consultas y guardados diarios y mensuales de asistencia mediante snapshots inmutables, sin retener agregados ni exponer infraestructura.

## ADDED Requirements

### Requirement: Consulta mensual coordinada
Application SHALL aceptar `GrupoId`, año y mes válidos, calcular el intervalo real completo, cargar una matrícula fresca y las asistencias mediante una operación coordinada, y devolver una proyección inmutable con columnas únicamente para fechas lectivas de lunes a viernes. No SHALL realizarse la carga mensual mediante llamadas descoordinadas desde Presentation.

#### Scenario: Meses de distinta longitud
- **WHEN** se consultan meses de 28, 29, 30 y 31 días, incluidos febreros bisiesto y no bisiesto
- **THEN** el snapshot contiene exactamente sus fechas de lunes a viernes en orden ascendente, sin sábados, domingos ni columnas vacías

#### Scenario: Año o mes inválido
- **WHEN** año está fuera del rango de `DateOnly` o mes fuera de 1–12
- **THEN** Application rechaza la consulta sin invocar persistencia

#### Scenario: Error técnico en el intervalo
- **WHEN** la persistencia falla al cargar cualquier parte del intervalo
- **THEN** no se devuelve un snapshot mensual parcial y se conserva el error identificable

### Requirement: Calendario lectivo mínimo
Application SHALL usar una abstracción de calendario que para este cambio marque lunes a viernes como laborables y sábado y domingo como no laborables.

#### Scenario: Semana completa
- **WHEN** se proyectan siete fechas consecutivas de lunes a domingo
- **THEN** las primeras cinco son laborables y sábado y domingo no lo son

### Requirement: Snapshot mensual inmutable
Application SHALL materializar arreglos nuevos para `AsistenciaMesDetalle`, sus fechas lectivas, estudiantes y estados por fecha. Cada columna SHALL incluir fecha, número, abreviatura española, persistencia y si requiere separación visual semanal. Esta señal SHALL ser verdadera sólo para un viernes que tenga otra fecha lectiva posterior en el mes. Cada estudiante SHALL incluir identidad interna, nombre y número actuales, actividad actual, celdas, conteos confirmados y porcentaje confirmado. No SHALL exponer agregados ni colecciones internas.

#### Scenario: Dos consultas consecutivas
- **WHEN** se consulta dos veces el mismo mes
- **THEN** se obtienen arreglos distintos y modificar una colección externa no afecta consultas posteriores

### Requirement: Unión de matrícula e históricos
Las filas mensuales SHALL ser la unión de estudiantes activos actuales y estudiantes presentes en cualquier padrón histórico guardado del mes. SHALL ordenarse por número actual, nombre visible y `EstudianteId`.

#### Scenario: Inactivo con historial
- **WHEN** un estudiante ahora inactivo aparece en un día guardado del mes
- **THEN** permanece visible con su estado histórico y situación inactiva

#### Scenario: Estudiante incorporado después
- **WHEN** un estudiante activo actual no pertenece al padrón de un día ya guardado
- **THEN** su celda para ese día es no aplicable y no se incorpora retroactivamente

#### Scenario: Día nuevo laborable
- **WHEN** un día laborable no está guardado
- **THEN** los estudiantes actualmente activos reciben borrador `Presente`, los inactivos históricos reciben no aplicable y no se guarda nada

### Requirement: Sólo columnas lectivas
Sábados y domingos no SHALL producir columnas ni celdas. La lista SHALL comenzar y terminar en las fechas lectivas reales aunque el mes empiece o termine en fin de semana o a mitad de semana.

#### Scenario: Mes comienza en fin de semana
- **WHEN** el día 1 del mes es sábado o domingo
- **THEN** la primera columna corresponde al lunes siguiente

#### Scenario: Mes termina en fin de semana
- **WHEN** los últimos días del mes son sábado y domingo
- **THEN** la última columna corresponde al viernes anterior y no existen columnas posteriores

#### Scenario: Separación semanal
- **WHEN** una columna corresponde a viernes y existe otra fecha lectiva posterior
- **THEN** el snapshot la marca como cierre semanal; el último viernes no se marca si no existe otra columna

### Requirement: Resumen mensual confirmado
Por estudiante, Application SHALL contar estados exclusivamente en días laborables persistidos cuyo padrón histórico contenga al estudiante. El porcentaje SHALL ser `(Presentes + Retardos) / días contabilizados × 100`; Falta y Justificada no cuentan como presencia. Si el denominador es cero SHALL devolverse ausencia de porcentaje, no cero.

#### Scenario: Porcentaje con retardo
- **WHEN** un estudiante tiene un Presente, un Retardo, una Falta y una Justificada en cuatro días contabilizados
- **THEN** su porcentaje confirmado es 50 % y cada estado se cuenta por separado

#### Scenario: Denominador cero
- **WHEN** el estudiante no pertenece a ningún día laborable guardado
- **THEN** el porcentaje confirmado está ausente

### Requirement: Guardar día seleccionado
Application SHALL guardar la entrada completa de una fecha laborable mediante el caso diario existente, exactamente una vez, y devolver el día confirmado sólo después del éxito.

#### Scenario: Día nuevo seleccionado
- **WHEN** se guarda explícitamente una columna laborable no persistida
- **THEN** se valida su padrón actual y se confirma una sola `AsistenciaDiaria`

### Requirement: Guardado mensual secuencial
Application SHALL guardar las fechas solicitadas en orden ascendente mediante transacciones diarias independientes. Después de cada éxito SHALL registrar esa fecha como confirmada. Si una fecha falla SHALL detenerse, informar las fechas confirmadas y la fecha fallida, y no intentar fechas posteriores.

#### Scenario: Todos los días tienen éxito
- **WHEN** se guardan varias fechas válidas
- **THEN** cada fecha se guarda una vez en orden y el resultado informa todas como confirmadas

#### Scenario: Fallo intermedio
- **WHEN** el segundo de tres días falla
- **THEN** el primero permanece confirmado, el segundo y tercero no se presentan como guardados y el tercero no se intenta

### Requirement: Frontera de errores compuesta
Data SHALL continuar traduciendo errores técnicos una sola vez. Para un guardado mensual interrumpido, Application SHALL añadir únicamente contexto de progreso mediante una excepción que conserve el error ya traducido como `InnerException`, las fechas confirmadas y la fecha fallida; no SHALL exponer SQL, rutas ni trazas.

#### Scenario: Persistencia falla en guardado mensual
- **WHEN** Data entrega un error de persistencia durante una fecha
- **THEN** el consumidor puede identificar la fecha fallida y los éxitos previos sin perder la causa traducida
