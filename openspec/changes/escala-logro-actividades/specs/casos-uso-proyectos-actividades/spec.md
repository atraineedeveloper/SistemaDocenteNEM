## MODIFIED Requirements

### Requirement: Conteos de nivel de logro en snapshots de actividad
`ActividadProyectoDetalle` SHALL exponer conteos de `Pendiente`, `Domina`, `Suficiente`, `EnProceso`, `RequiereApoyo` y `NoEntrego` calculados sobre el padron historico de la actividad. Las actividades anuladas SHALL excluirse de los conteos agregados del proyecto. No SHALL calcularse porcentaje ni calificacion numerica.

#### Scenario: Conteos correctos por nivel
- **WHEN** una actividad tiene registros con distintos niveles de logro
- **THEN** el snapshot refleja el conteo exacto de cada nivel y el total coincide con el numero de registros del padron

#### Scenario: Actividad anulada excluida
- **WHEN** una actividad esta anulada
- **THEN** sus registros no contribuyen a ningun conteo del proyecto

### Requirement: Guardar entregas con escala de logro
`GuardarEntregasActividad` SHALL aceptar entradas con `NivelLogro` de tipo `NivelLogro` y SHALL rechazar cualquier valor fuera del conjunto `Pendiente`, `Domina`, `Suficiente`, `EnProceso`, `RequiereApoyo` y `NoEntrego`.

#### Scenario: Guardar nivel de desempeno valido
- **WHEN** se envian registros con cualquier nivel valido de logro
- **THEN** Application los delega a Core sin conversion y persiste el padron completo

#### Scenario: Rechazar nivel invalido
- **WHEN** un registro de entrada contiene un valor fuera del conjunto valido
- **THEN** Core rechaza el padron completo sin persistencia parcial

