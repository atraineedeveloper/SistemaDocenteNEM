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
- `Entregada` permite cualquier nivel de logro evaluativo excepto el legado `NoEntrego`.
- `NoEntregada` no se transforma en una calificación numérica; para compatibilidad, el nivel de logro queda `Pendiente` y el reporte muestra el estado de entrega por separado.
- La migración convierte el legado `NivelLogro.NoEntrego` en `EstadoEntregaActividad.NoEntregada + NivelLogro.Pendiente`; otros niveles distintos de `Pendiente` se migran como `Entregada`; `Pendiente` permanece como entrega `Pendiente`.
- El porcentaje de cumplimiento usa únicamente estados explícitos decididos: `Entregadas / (Entregadas + NoEntregadas) * 100`; las entregas `Pendiente` se muestran aparte y no alteran el porcentaje.
- La configuración pertenece al **grupo**. Un cambio de adscripción se representa con otro grupo/contexto, preservando reportes históricos.

## Fuera de alcance inicial

- calificaciones numéricas y reglas de aprobación;
- convertir una no entrega en cero;
- clasificación individual por etapas de Piaget;
- generación PDF final (se deja preparada la frontera de Reporting y la vista imprimible se aborda posteriormente);
- rankings competitivos de estudiantes.

## Impacto esperado

- Core: nuevo estado de entrega y contexto de grupo.
- Application: contratos/casos de uso para contexto y reportes.
- Data: migración SQLite y persistencia de contexto/estado explícito.
- Reporting: modelos y cálculos agregados reutilizables.
- Presentation/WPF: nuevo módulo Reportes y edición de configuración del grupo.
- Tests: dominio, migración, cálculos y composición WPF.