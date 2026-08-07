# Change: Reportes, configuración contextual del grupo y estado explícito de entrega

## Why

El sistema ya registra grupo, asistencia, proyectos, actividades, niveles de logro y expediente, pero no consolida esa información en reportes pedagógicos individual y grupal. Tampoco existe un contexto escolar persistente por grupo para identificar ciclo, escuela, grado, turno y etapa cognoscitiva de referencia. Además, el estado de entrega estaba implícito en `NivelLogro`, impidiendo distinguir una actividad recibida aún no evaluada de una actividad no entregada.

## What Changes

- Agregar `ContextoGrupo` 1:1 por grupo con ciclo escolar, escuela/CCT, ubicación, grado/grupo/turno, referencia cognoscitiva grupal, docente responsable, periodo y horario.
- Tratar la etapa de Piaget como referencia pedagógica del grupo, nunca como diagnóstico individual.
- Introducir `EstadoEntregaActividad` con `Pendiente`, `Entregada` y `NoEntregada`, separado de `NivelLogro`.
- Permitir explícitamente `Entregada + NivelLogro.Pendiente` cuando el trabajo fue recibido pero aún no evaluado.
- Normalizar `NoEntregada` a `NivelLogro.Pendiente` y mantener `NivelLogro.NoEntrego` sólo como compatibilidad legacy.
- Persistir el estado explícito mediante la extensión SQLite aditiva versionada `reportes-contexto-entregas` v1 sobre `PRAGMA user_version = 6`, sin reconstruir destructivamente `entregas_actividad`.
- Convertir datos legacy de forma transaccional e idempotente.
- Activar `SistemaDocente.Reporting` con reportes individual y grupal y cálculos puros.
- Calcular cumplimiento como `Entregadas / (Entregadas + NoEntregadas) * 100`; los pendientes se muestran aparte y no entran al denominador.
- Agregar navegación global a Reportes y una ventana de Configuración del grupo reutilizada desde Grupo y Reportes.
- Adaptar la matriz de Evaluación, filtros, métricas, editor y atajos para conservar entrega y logro por separado.
- Mantener fuera de alcance calificaciones numéricas, rankings competitivos y generación PDF final.

## Capabilities

### New Capabilities

- `contexto-grupo`: configuración escolar y pedagógica persistente 1:1 por grupo, editable de forma progresiva.
- `estado-entrega-actividad`: seguimiento explícito de entrega independiente del nivel de logro, con compatibilidad legacy y persistencia aditiva.
- `reportes-pedagogicos`: reportes individual y grupal con asistencia, cumplimiento, niveles de logro y seguimiento pedagógico.

### Modified Capabilities

- Ninguna. La adaptación visual de Evaluación se especifica como comportamiento consumidor de `estado-entrega-actividad` sin crear un nuevo agregado.

## Impact

- **Core:** nuevo estado de entrega, contexto de grupo e invariantes de combinación entrega/logro.
- **Application:** contratos y casos de uso para contexto, reportes y compatibilidad de entradas legacy.
- **Data:** extensión SQLite `reportes-contexto-entregas` v1, configuración 1:1, estados explícitos y conversión legacy.
- **Reporting:** modelos y cálculos puros reutilizables, sin dependencia de SQLite ni WPF.
- **Presentation:** ViewModels para configuración/reportes y matriz de evaluación con las dos dimensiones separadas.
- **App.Wpf:** navegación Reportes, `ReportesView`, `ConfiguracionGrupoWindow` y editor de evaluación actualizado.
- **Demo:** contexto ficticio y mezcla de estados `Pendiente`, `Entregada`, `NoEntregada` y `Entregada + Pendiente de evaluación`.
- **Pruebas:** cobertura Core/Application/Data/Reporting/Presentation/WPF y validación manual posterior en Windows.