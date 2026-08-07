# Design: Reportes, configuración contextual y entrega explícita

## 1. Modelo conceptual

```text
Grupo
├── ContextoGrupo (1:1)
│   ├── ciclo escolar
│   ├── escuela / CCT
│   ├── entidad / municipio / localidad
│   ├── grado / grupo / turno
│   ├── etapa cognoscitiva grupal (Piaget)
│   ├── docente responsable
│   ├── periodo de responsabilidad
│   └── horario
├── Estudiantes
├── Asistencia
├── Proyectos
│   └── Actividades
│       └── EntregaActividad
│           ├── EstadoEntregaActividad
│           ├── NivelLogro
│           └── Observación
└── Expediente

Reporting
├── ReporteIndividual
└── ReporteGrupal
```

`ContextoGrupo` no se coloca en cada estudiante. Una mudanza o cambio de adscripción docente no reescribe el grupo anterior: se crea/usa otro grupo con su propio contexto.

## 2. Etapa cognoscitiva de Piaget

Se registra como contexto pedagógico **grupal**. Valores iniciales:

- No especificada;
- Sensoriomotora;
- Preoperacional;
- Operaciones concretas;
- Operaciones formales.

No representa un diagnóstico clínico ni una clasificación individual. La UI debe explicarlo como referencia general del grupo.

## 3. Entrega y nivel de logro son dimensiones distintas

Nuevo enum:

```text
EstadoEntregaActividad
0 Pendiente
1 Entregada
2 NoEntregada
```

Reglas:

- una actividad recién creada genera padrón histórico con `Pendiente + NivelLogro.Pendiente`;
- marcar `Entregada` no obliga a evaluar inmediatamente: puede existir `Entregada + NivelLogro.Pendiente`;
- marcar `NoEntregada` fuerza `NivelLogro.Pendiente` para evitar estados contradictorios;
- asignar un nivel evaluativo (`Domina`, `Suficiente`, `EnProceso`, `RequiereApoyo`) fuerza `Entregada`;
- `NivelLogro.NoEntrego` se conserva temporalmente en el enum sólo para compatibilidad binaria/migración, pero nuevos flujos no lo producen.

### Migración desde esquema 6

La tabla actual usa `entregas_actividad.estado_entrega` para almacenar `NivelLogro`. En esquema 7 se separa:

```text
entregas_actividad
actividad_id
estudiante_id
grupo_id
estado_entrega    -- 0 Pendiente, 1 Entregada, 2 NoEntregada
nivel_logro       -- NivelLogro
observacion
```

Conversión:

```text
legado NoEntrego       -> NoEntregada + Pendiente
legado Pendiente        -> Pendiente + Pendiente
legado Domina/Suf/etc.  -> Entregada + mismo nivel
```

La migración es transaccional y conserva padrón/observación.

## 4. Cálculos de cumplimiento

Por estudiante:

```text
Entregadas
NoEntregadas
PendientesDeRegistro
Decididas = Entregadas + NoEntregadas
Cumplimiento = Entregadas / Decididas * 100
```

Si `Decididas == 0`, se muestra `—` en vez de 0 %. Los pendientes se muestran explícitamente y no se confunden con no entrega.

Los niveles de logro se agregan sólo sobre entregas aplicables; una no entrega se contabiliza en cumplimiento, no como nivel cognitivo.

## 5. Configuración contextual del grupo

Se persiste en tabla 1:1 `configuracion_grupo`, con FK a `grupos`.

Campos iniciales:

```text
ciclo_escolar
nombre_escuela
cct
entidad_federativa
municipio
localidad
grado
grupo
turno
etapa_cognoscitiva
docente_responsable
responsable_desde
responsable_hasta
hora_entrada
hora_salida
```

Todos salvo `grupo_id` admiten valor vacío/no especificado para no bloquear grupos existentes. Fechas y horas son opcionales. La configuración se puede completar progresivamente.

## 6. Reporting

`SistemaDocente.Reporting` deja de ser sólo reserva y contiene modelos/cálculos puros. No accede a SQLite ni a WPF.

Application coordina lecturas existentes y entrega datos a Reporting. Reporting calcula métricas y devuelve snapshots listos para Presentation.

### Reporte individual

Incluye:

- identidad y contexto del grupo;
- asistencia por mes y promedio;
- entregadas/no entregadas/pendientes y cumplimiento;
- distribución de niveles de logro;
- proyectos/actividades aplicables;
- fortalezas, dificultades, apoyos, observaciones y acuerdos disponibles en expediente.

### Reporte grupal

Incluye:

- total/históricos/activos;
- asistencia agregada;
- cumplimiento de entregas;
- distribución de niveles de logro;
- proyectos y actividades;
- tabla de seguimiento individual sin ranking competitivo.

## 7. UI de Reportes

Se agrega una sola pestaña global `Reportes`, no dos módulos separados.

Dentro:

```text
REPORTES
[ Individual ] [ Grupal ]

contexto del grupo + periodo

contenido
```

Individual permite seleccionar estudiante. Grupal muestra agregados y tabla de seguimiento. La versión inicial es interactiva; impresión/PDF queda preparada para el siguiente corte.

## 8. Configuración UI

La configuración se abre como ventana dedicada desde Grupo y desde Reportes cuando falte contexto. No se incorpora al header global.

Usa labels visibles, secciones `Contexto escolar`, `Datos del grupo`, `Referencia pedagógica`, `Responsabilidad docente` y `Horario`, con footer fijo Guardar/Cancelar.

## 9. Compatibilidad

- no cambia la identidad histórica de grupos, actividades ni estudiantes;
- no reescribe padrones antiguos;
- mantiene `ActividadId` como identidad;
- mantiene `NivelLogro.NoEntrego` sólo como valor legado durante transición, pero la UI nueva usa estado explícito;
- modo demo debe sembrar contexto y mezcla de estados de entrega.

## 10. Validación

Automática:

- invariantes Core;
- migración v6→v7;
- persistencia/reapertura;
- cálculo de cumplimiento con pendientes;
- reportes individual/grupal;
- bindings/navegación WPF.

Manual:

- Reportes con grupo vacío y demo;
- scroll/redimensionamiento;
- Claro/Oscuro/Alto contraste;
- 100/125/150 %;
- coherencia entre Evaluación y Reportes después de cambiar entrega/nivel.