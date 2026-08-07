## Purpose

Define la configuración escolar y pedagógica persistente que contextualiza cada grupo sin convertir referencias generales en diagnósticos individuales.

## ADDED Requirements

### Requirement: Contexto persistente uno a uno por grupo
El sistema SHALL permitir asociar a cada `GrupoId` un único contexto con ciclo escolar, escuela, CCT, entidad federativa, municipio, localidad, grado, grupo, turno, docente responsable, periodo de responsabilidad y horario. Los campos contextuales SHALL poder quedar vacíos para no bloquear grupos existentes.

#### Scenario: Grupo existente sin configuración
- **WHEN** se abre la configuración de un grupo que todavía no tiene contexto persistido
- **THEN** la aplicación presenta valores vacíos o no especificados y permite guardarlos progresivamente

#### Scenario: Reapertura del grupo
- **WHEN** se guarda el contexto y posteriormente se vuelve a abrir el mismo grupo
- **THEN** se recuperan los valores persistidos para ese `GrupoId`

### Requirement: Referencia cognoscitiva grupal
La configuración SHALL permitir seleccionar una etapa cognoscitiva de Piaget como referencia pedagógica general del grupo y SHALL comunicar que no constituye diagnóstico ni clasificación individual de estudiantes.

#### Scenario: Operaciones concretas
- **WHEN** el docente selecciona `Operaciones concretas`
- **THEN** el valor se conserva como atributo del contexto grupal y no se escribe en expedientes individuales

### Requirement: Edición compartida desde Grupo y Reportes
Grupo y Reportes SHALL abrir la misma experiencia de configuración y SHALL reutilizar el mismo estado contextual para evitar copias divergentes.

#### Scenario: Abrir desde dos superficies
- **WHEN** el usuario abre Configuración desde Grupo y después desde Reportes
- **THEN** ambas superficies muestran el mismo contexto persistido del grupo activo

### Requirement: Historial contextual por identidad de grupo
Un cambio de adscripción o contexto escolar SHALL representarse mediante otro grupo/contexto cuando corresponda y no SHALL reescribir retrospectivamente el contexto de un grupo histórico.

#### Scenario: Cambio de grupo
- **WHEN** el docente empieza a trabajar con otro grupo y configura su contexto
- **THEN** el contexto del grupo anterior permanece asociado únicamente a su identidad histórica