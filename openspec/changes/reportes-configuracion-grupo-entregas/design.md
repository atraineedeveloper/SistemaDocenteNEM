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
- `NivelLogro.NoEntrego` se conserva temporalmente en el enum sólo para compatibilidad binaria/migración, pero los flujos nuevos no lo producen.

### Estrategia SQLite definitiva: extensión aditiva sobre esquema base v6

La tabla histórica `entregas_actividad` tiene una columna llamada `estado_entrega` que, pese a su nombre, almacena valores de `NivelLogro`. Reconstruir ahora esa tabla para convertirla a un esquema v7 obligaría a una migración más invasiva sobre una base v6 ya validada.

La estrategia adoptada para este corte es **mantener `PRAGMA user_version = 6`** y versionar esta capacidad mediante una extensión aditiva independiente:

```text
esquema_extensiones
├── nombre = reportes-contexto-entregas
└── version = 1

entregas_actividad                 -- tabla base v6 conservada
├── actividad_id
├── estudiante_id
├── grupo_id
├── estado_entrega                 -- conserva NivelLogro por compatibilidad
└── observacion

estados_entrega_actividad          -- extensión v1
├── actividad_id
├── estudiante_id
└── estado_entrega                 -- EstadoEntregaActividad real
```

La extensión se inicializa de forma transaccional e idempotente. También crea `configuracion_grupo` y registra su propia versión en `esquema_extensiones`.

Conversión inicial de datos legacy:

```text
NivelLogro.NoEntrego  -> estado explícito NoEntregada + nivel legado normalizado a Pendiente
NivelLogro.Pendiente  -> estado explícito Pendiente + nivel Pendiente
Domina/Suf/etc.       -> estado explícito Entregada + mismo nivel
```

`PersistenciaProyectosSqlite` realiza lectura combinada de ambas tablas y escritura dual: el nivel continúa en la columna base histórica y el estado explícito se guarda en `estados_entrega_actividad`. Esto permite abrir bases v6 existentes sin reconstruir tablas ni cambiar identidades, padrones u observaciones.

Las llamadas legacy de Application que sólo expresan `NivelLogro` se distinguen de las entradas nuevas con estado explícito. Si una edición legacy no expresa un cambio de estado, Application conserva el estado ya persistido para evitar que editar metadatos de una actividad borre, por ejemplo, `Entregada + Pendiente`.

Una futura migración estructural que renombre la columna histórica y consolide ambos valores en una sola tabla queda fuera de este corte. Deberá justificarse por una necesidad adicional y contar con su propia migración y pruebas.

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

La configuración se abre como ventana dedicada desde Grupo y desde Reportes. No se incorpora al header global.

Ambas superficies reutilizan la misma instancia de `ConfiguracionGrupoViewModel` creada en la raíz de composición y la misma `ConfiguracionGrupoWindow`.

Usa labels visibles, secciones `Contexto escolar`, `Datos del grupo`, `Referencia pedagógica`, `Responsabilidad docente` y `Horario`, con footer fijo Guardar/Cancelar.

## 9. Evaluación y estado explícito

La matriz conserva el patrón estudiante × actividad; no se reintroduce un selector separado de actividad.

Cada celda visual mantiene `EstadoEntrega` y `NivelLogro` por separado. La representación compacta es:

```text
P  pendiente de entrega
N  no entregada
✓  entregada, pendiente de evaluación
D  domina
S  suficiente
E  en proceso
R  requiere apoyo
—  no aplicable por padrón histórico
```

Atajos en la grilla:

```text
T = Entregada, todavía pendiente de evaluación
N = No entregada
P = Pendiente de entrega
D/S/E/R = asignar nivel de logro y marcar Entregada
Enter/F2 = editor compacto
```

El editor compacto expone por separado `Estado de entrega`, `Nivel de logro` y `Observación`. El nivel de logro sólo se habilita cuando la actividad está entregada. Guardar la matriz transmite ambas dimensiones a Application y confirma ambas tras la persistencia.

## 10. Compatibilidad

- no cambia la identidad histórica de grupos, actividades ni estudiantes;
- no reescribe padrones antiguos;
- mantiene `ActividadId` como identidad;
- mantiene `NivelLogro.NoEntrego` sólo como valor legado durante transición, pero la UI nueva usa estado explícito;
- mantiene el esquema base en `user_version = 6` y añade capacidad mediante una extensión versionada;
- el modo demo siembra contexto y una mezcla de estados/niveles suficiente para probar reportes y evaluación.

## 11. Validación

Automática:

- invariantes Core de estado/nivel;
- inicialización idempotente de la extensión SQLite;
- conversión legacy `NoEntrego -> NoEntregada + Pendiente`;
- comprobación de que `PRAGMA user_version` permanece en 6;
- persistencia/reapertura de `Entregada + Pendiente`;
- cálculo de cumplimiento con pendientes;
- reportes individual/grupal;
- matriz, filtros, atajos y guardado explícito en Presentation/WPF;
- composición compartida de configuración desde Grupo y Reportes.

Manual:

- Reportes con grupo vacío y demo;
- scroll/redimensionamiento;
- Claro/Oscuro/Alto contraste;
- 100/125/150 %;
- coherencia entre Evaluación y Reportes después de cambiar entrega/nivel;
- configuración abierta desde Grupo y desde Reportes.
