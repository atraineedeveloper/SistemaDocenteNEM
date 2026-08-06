## MODIFIED Requirements

### Requirement: Padron completo de entregas con escala de logro
Una actividad nueva SHALL incluir exactamente un registro `Pendiente` por cada estudiante activo del grupo. Cada registro SHALL contener `EstudianteId`, `NivelLogro` y observacion opcional de hasta 500 caracteres. Los valores validos de `NivelLogro` SHALL ser exclusivamente `Pendiente`, `Domina`, `Suficiente`, `EnProceso`, `RequiereApoyo` y `NoEntrego`; no SHALL haber duplicados ni estudiantes ajenos al grupo. Los valores `Domina`, `Suficiente`, `EnProceso` y `RequiereApoyo` representan niveles de desempeno de actividades realizadas; `NoEntrego` representa incumplimiento; y `Pendiente` representa que aun no se ha registrado evaluacion.

#### Scenario: Crear padron inicial con nivel pendiente
- **WHEN** se prepara una actividad para un grupo con estudiantes activos
- **THEN** existe exactamente un registro con `NivelLogro = Pendiente` por cada estudiante activo y ninguno por estudiantes inactivos

#### Scenario: Padron invalido
- **WHEN** faltan registros, hay duplicados o aparece un nivel de logro fuera del conjunto valido
- **THEN** la actividad completa se rechaza sin cambios parciales

#### Scenario: Registrar nivel de desempeno
- **WHEN** se asigna `Domina`, `Suficiente`, `EnProceso` o `RequiereApoyo` a un registro
- **THEN** el registro refleja el desempeno evaluado por el docente

#### Scenario: Registrar incumplimiento
- **WHEN** se asigna `NoEntrego` a un registro
- **THEN** el registro indica que el estudiante no entrego la actividad

