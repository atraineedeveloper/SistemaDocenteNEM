# Reportes, configuración del grupo y estado de entrega

Esta guía resume las decisiones funcionales y técnicas del módulo de Reportes, la configuración contextual del grupo y la separación entre entrega y nivel de logro.

## 1. Configuración contextual del grupo

La configuración pertenece al grupo, no al estudiante. Se guarda 1:1 por `GrupoId` y puede completarse progresivamente.

Campos actuales:

- ciclo escolar;
- nombre de escuela y CCT;
- entidad federativa, municipio y localidad;
- grado, grupo y turno;
- etapa de desarrollo cognoscitivo grupal de referencia;
- docente responsable;
- periodo de responsabilidad;
- hora de entrada y salida.

### Etapa de Piaget

La etapa cognoscitiva es una **referencia pedagógica general del grupo**. No debe presentarse ni utilizarse como diagnóstico individual.

Opciones:

- No especificada;
- Sensoriomotora;
- Preoperacional;
- Operaciones concretas;
- Operaciones formales.

La misma `ConfiguracionGrupoWindow` se abre desde:

- `GrupoView` → `⚙ Configurar grupo`;
- `ReportesView` → `⚙ Configurar grupo`.

Ambas vistas reciben la misma instancia de `ConfiguracionGrupoViewModel` desde `MainWindow`.

## 2. Entrega y nivel de logro

Son dimensiones distintas.

### Estado de entrega

```text
Pendiente
Entregada
NoEntregada
```

### Nivel de logro

```text
Pendiente
Domina
Suficiente
EnProceso
RequiereApoyo
```

`NivelLogro.NoEntrego` permanece sólo para compatibilidad con datos/código legado. Los flujos nuevos no deben producirlo.

### Combinaciones válidas relevantes

| Estado de entrega | Nivel de logro | Significado |
| --- | --- | --- |
| Pendiente | Pendiente | todavía no se registra si entregó |
| Entregada | Pendiente | trabajo recibido, todavía no evaluado |
| Entregada | Domina/Suficiente/EnProceso/RequiereApoyo | trabajo recibido y evaluado |
| NoEntregada | Pendiente | se registró que no fue entregado |

Reglas automáticas:

- marcar `NoEntregada` fuerza nivel `Pendiente`;
- asignar un nivel evaluativo fuerza estado `Entregada`;
- una no entrega no se convierte en cero ni en nivel cognitivo.

## 3. Matriz de Evaluación

La vista sigue siendo estudiante × actividad. No existe un selector separado de actividad.

Representación compacta:

```text
P  pendiente de entrega
N  no entregada
✓  entregada, pendiente de evaluación
D  domina
S  suficiente
E  en proceso
R  requiere apoyo
—  actividad no aplicable por padrón histórico
```

### Atajos

Cuando el foco está dentro de la matriz:

| Tecla | Acción |
| --- | --- |
| `T` | marcar Entregada y dejar nivel Pendiente |
| `N` | marcar No entregada |
| `P` | volver a Pendiente de entrega |
| `D` | Domina + Entregada |
| `S` | Suficiente + Entregada |
| `E` | En proceso + Entregada |
| `R` | Requiere apoyo + Entregada |
| `Enter` / `F2` | abrir editor de celda |
| `Ctrl+S` | guardar cambios |

El editor de celda muestra por separado:

1. Estado de entrega;
2. Nivel de logro;
3. Observación.

El nivel sólo se habilita cuando el estado es `Entregada`.

## 4. Filtros y métricas

La matriz puede filtrar por:

- todas;
- entregadas;
- no entregadas;
- pendientes de entrega;
- entregadas pendientes de evaluación;
- Domina;
- Suficiente;
- En proceso;
- Requiere apoyo;
- incidencias;
- sólo estudiantes activos;
- activos e inactivos históricos.

Métricas de actividad seleccionada:

- total aplicable;
- pendientes de entrega;
- entregadas;
- no entregadas;
- entregadas pendientes de evaluación;
- requiere apoyo.

## 5. Reportes

Existe una sola pestaña global `Reportes` con dos modos.

### Individual

Incluye:

- identidad y contexto del grupo;
- asistencia mensual/promedio;
- cumplimiento de entrega;
- distribución de niveles de logro;
- proyectos y actividades aplicables;
- fortalezas;
- dificultades;
- apoyos;
- observaciones;
- acuerdos con tutores.

### Grupal

Incluye:

- matrícula histórica y activa;
- asistencia agregada;
- cumplimiento de entrega;
- distribución de logro;
- evolución mensual;
- seguimiento individual sin ranking competitivo.

### Porcentaje de cumplimiento

```text
Entregadas / (Entregadas + NoEntregadas) × 100
```

Las pendientes no entran en el denominador. Si todavía no hay entregas decididas, la UI muestra `—` en lugar de 0 %.

## 6. Persistencia SQLite

El esquema base se mantiene en:

```text
PRAGMA user_version = 6
```

La capacidad nueva utiliza una extensión aditiva:

```text
esquema_extensiones
nombre: reportes-contexto-entregas
version: 1
```

Tablas nuevas:

- `configuracion_grupo`;
- `estados_entrega_actividad`.

La columna histórica `entregas_actividad.estado_entrega` sigue almacenando temporalmente `NivelLogro`. El adaptador hace escritura dual y lectura combinada.

Conversión automática de datos legacy:

```text
NoEntrego       -> NoEntregada + Pendiente
Pendiente       -> Pendiente + Pendiente
Domina/Suf/etc. -> Entregada + mismo nivel
```

No debe elevarse `user_version` ni reconstruirse la tabla base para esta capacidad sin una nueva decisión arquitectónica explícita.

## 7. Compatibilidad legacy

`EntradaEntregaActividad` distingue entradas nuevas de llamadas antiguas:

- constructor con `EstadoEntregaActividad` → estado explícito;
- constructor antiguo sólo con `NivelLogro` → entrada legacy.

Cuando una edición legacy trae `Pendiente` sin expresar el estado, Application conserva el estado histórico existente. Esto evita que editar título, fecha u observaciones de una actividad borre accidentalmente un estado como `Entregada + Pendiente`.

## 8. Modo demo

El modo demo siembra contexto ficticio independiente de producción, incluyendo:

- escuela/CCT;
- grado y grupo;
- turno;
- etapa `Operaciones concretas`;
- docente responsable;
- horario.

Las rutas de producción y demo siguen totalmente separadas.

## 9. Validación requerida antes del merge

En Windows:

```powershell
dotnet restore SistemaDocente.sln
dot format SistemaDocente.sln --verify-no-changes --no-restore
dotnet build SistemaDocente.sln --no-restore
dotnet test SistemaDocente.sln --no-build
openspec validate --all
git diff --check
```

Validación manual mínima:

- abrir Configuración desde Grupo y Reportes;
- guardar/reabrir contexto;
- marcar `T`, `N`, `P`, `D`, `S`, `E`, `R` en Evaluación;
- confirmar que `T` conserva `Entregada + Pendiente` después de guardar/reabrir;
- comprobar que Reportes refleja los cambios;
- probar Claro/Oscuro/Alto contraste;
- probar escalado 100/125/150 %;
- probar demo con `--demo-reset`.
