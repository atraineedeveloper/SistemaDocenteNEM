# Proposal: Reportes, configuración contextual del grupo y estado explícito de entrega

## Problema

El sistema ya registra grupo, asistencia, proyectos, actividades, niveles de logro y expediente, pero todavía no consolida esa información en reportes pedagógicos individual y grupal. Tampoco existe un contexto escolar persistente por grupo para identificar ciclo, escuela, grado, turno y etapa de desarrollo cognoscitivo de referencia. Finalmente, el estado de entrega está implícito dentro de `NivelLogro`, lo que impide distinguir con rigor entre una actividad pendiente de evaluación y una actividad no entregada.

## Objetivo

Agregar tres capacidades relacionadas:

1. **Configuración contextual por grupo**, vinculada a un ciclo escolar y conservada históricamente aunque el docente cambie de escuela/grupo durante el ciclo.
2. **Estado explícito de entrega**, separado del nivel de logro, con `Pendiente`, `Entregada` y `NoEntregada` para calcular cumplimiento real sin interpretar `NivelLogro.Pendiente` como falta de entrega.
3. **Módulo Reportes**, con vista individual y grupal, basado en proyecciones de datos existentes y preparado para una futura salida imprimible/PDF desde `SistemaDocente.Reporting`.

## Decisiones funcionales

- La **etapa de desarrollo cognoscitivo de Piaget** es contexto del grupo, no diagnóstico individual. Se registra de forma descriptiva, por ejemplo `Operaciones concretas`.
- No se guardan “estilos de aprendizaje” visual/auditivo/kinestésico como diagnóstico individual.
- `EstadoEntregaActividad` es independiente de `NivelLogro`.
- Una actividad nueva inicia con entrega `Pendiente` y nivel de logro `Pendiente`.
- `Entregada` permite cualquier nivel evaluativo y también `NivelLogro.Pendiente` cuando el trabajo ya fue recibido pero aún no evaluado.
- `NoEntregada` no se transforma en una calificación numérica; el nivel de logro queda `Pendiente` y el reporte muestra el estado de entrega por separado.
- La compatibilidad SQLite se implementa mediante una **extensión aditiva versionada** sobre el esquema base v6, sin reconstruir la tabla histórica `entregas_actividad` ni cambiar `PRAGMA user_version`.
- La conversión inicial transforma el legado `NivelLogro.NoEntrego` en `EstadoEntregaActividad.NoEntregada + NivelLogro.Pendiente`; otros niveles distintos de `Pendiente` se interpretan como `Entregada`; `Pendiente` permanece como entrega `Pendiente`.
- El porcentaje de cumplimiento usa únicamente estados explícitos decididos: `Entregadas / (Entregadas + NoEntregadas) * 100`; las entregas `Pendiente` se muestran aparte y no alteran el porcentaje.
- La configuración pertenece al **grupo**. Un cambio de adscripción se representa con otro grupo/contexto, preservando reportes históricos.
- Grupo y Reportes abren la misma ventana de configuración y reutilizan el mismo ViewModel contextual.

## Fuera de alcance inicial

- calificaciones numéricas y reglas de aprobación;
- convertir una no entrega en cero;
- clasificación individual por etapas de Piaget;
- generación PDF final (se deja preparada la frontera de Reporting y la vista imprimible se aborda posteriormente);
- rankings competitivos de estudiantes;
- reconstruir el esquema base v6 únicamente para renombrar/consolidar columnas históricas que ya pueden convivir de forma segura mediante la extensión aditiva.

## Impacto esperado

- Core: nuevo estado de entrega y contexto de grupo.
- Application: contratos/casos de uso para contexto y reportes, con compatibilidad para llamadas legacy de entrega.
- Data: extensión SQLite `reportes-contexto-entregas` v1, persistencia de contexto/estado explícito y conversión de datos legacy.
- Reporting: modelos y cálculos agregados reutilizables.
- Presentation/WPF: nuevo módulo Reportes, edición de configuración del grupo y evaluación con entrega/nivel separados.
- Tests: dominio, compatibilidad SQLite, cálculos, matriz y composición WPF.
