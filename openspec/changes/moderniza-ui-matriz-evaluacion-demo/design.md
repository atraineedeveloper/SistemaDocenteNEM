# Design: Modernización UI, matriz de evaluación y modo demo

## Principios

1. Mantener una sola navegación global en la parte superior.
2. Usar superficies, jerarquía tipográfica y espacio antes que decoración excesiva.
3. Mantener densidad de escritorio: Asistencia y Evaluación son herramientas operativas, no dashboards web.
4. Los módulos comparten lenguaje visual, no necesariamente la misma estructura interna.
5. No sacrificar teclado, foco, alto contraste ni virtualización.

## Shell

El encabezado pasa de una franja guinda completa a una superficie clara con:

- logo/identidad en guinda;
- selector de grupo;
- acción primaria `Nuevo`;
- pestañas de módulos con texto oscuro y acento/indicador guinda;
- selector de tema;
- distintivo `DEMO` cuando corresponda.

No se añade barra lateral porque duplicaría la navegación y reduciría el espacio horizontal útil de Asistencia/Evaluación.

## Grupo

```text
Lista de estudiantes                         [Cambiar nombre]
Grupo ...
N estudiantes · M activos

[Buscar...]      [Total N] [Activos M]

[ tabla de estudiantes ]

[Ver expediente] [Agregar estudiante] [Editar] [Acciones ▾]
```

`Agregar estudiante` es la acción primaria. El DataGrid conserva virtualización y scroll propio.

## Asistencia

Se conserva la lógica y densidad actuales. La modernización se concentra en encabezado/periodo, métricas compactas, búsqueda/filtros, superficie de la grilla, leyenda y una barra donde `Guardar cambios` es la acción primaria.

La grilla mensual mantiene dos columnas congeladas, separación semanal real y atajos P/F/R/J contextuales.

## Proyectos

```text
Planeación didáctica                         [Nuevo proyecto]
Proyectos didácticos
Organiza proyectos y actividades del grupo

[Total] [En curso] [Borradores]

[Buscar proyecto...] [Estado ▾]

[ tabla ]

[Abrir proyecto]
```

La lista sigue abriendo `DetalleProyectoWindow`, que a su vez mantiene `DetalleActividadWindow`. No se introduce master-detail.

## Evaluación como matriz

La UI deja de tener selector de actividad.

```text
Evaluación formativa
Registro pedagógico de actividades

Proyecto [................................ ▾]

Actividad seleccionada: A4F2C91 · Investigación ... · 10 ago 2026
[Total] [Pend.] [Domina] [Suficiente] [En proceso] [Req. apoyo] [No entregó]

[Buscar estudiante...] [Nivel ▾]

Núm. | Estudiante | A4F2C91 | A10C8D2 | A8E131B | ...
-----|------------|---------|---------|---------|----
  1  | Ana ...    |    S    |    E    |    P    |
  2  | ...        |    D    |    S    |    S    |
```

### Columnas de actividad

- las actividades conservan el orden funcional entregado por Application;
- cada encabezado recibe un código compacto estable con formato `A` + seis caracteres hexadecimales derivados de la identidad inmutable `ActividadId`;
- ejemplo: `A4F2C91`;
- el código no depende de la posición, fecha ni título, por lo que no cambia al reordenar, editar o anular otras actividades;
- no se añade una columna SQLite sólo para una necesidad de presentación; `ActividadId` sigue siendo la identidad real;
- tooltip/nombre accesible: `A4F2C91 · Investigación de noticias · 10/08/2026`;
- actividades anuladas permanecen visibles cuando existan, pero sus celdas son sólo lectura.

### Filas y padrón histórico

La matriz se construye con la unión de estudiantes presentes en los padrones de las actividades del proyecto.

- Si el estudiante pertenece al padrón de la actividad, existe celda evaluable/consultable.
- Si fue incorporado después y no pertenece al padrón histórico, la celda muestra `—` y no es editable.
- La identidad/nombre/número mostrados siguen las proyecciones actuales de Application.
- Se conservan estudiantes históricos inactivos si aparecen en alguna actividad.

### Celdas

- `D` Domina;
- `S` Suficiente;
- `E` En proceso;
- `R` Requiere apoyo;
- `N` No entregó;
- `P` Pendiente;
- `—` No aplicable.

El color siempre acompaña a una letra; nunca es el único indicador.

### Selección

La columna de la celda actual se considera `Actividad seleccionada`. Esto habilita contexto del encabezado, métricas de esa actividad, acción `Marcar a todo el grupo` y edición masiva sólo sobre el padrón real de esa actividad.

### Observación

La observación no ocupa una columna por actividad. Enter, F2 o doble clic sobre una celda abre un editor compacto de la evaluación seleccionada. Cancelar restaura nivel/observación previos a la apertura del editor; aceptar conserva la edición local hasta `Guardar cambios`.

### Guardado

La unidad transaccional sigue siendo una actividad.

`Guardar cambios`:

1. identifica actividades con cambios;
2. guarda cada actividad completa mediante `GuardarEntregas`;
3. confirma localmente cada actividad que se guardó;
4. si una actividad falla, las anteriores ya confirmadas permanecen guardadas y las posteriores conservan su edición local;
5. no se afirma atomicidad del proyecto completo.

Esto replica el principio usado por la asistencia mensual: operación compuesta secuencial sobre unidades atómicas existentes.

## Modo demo

### Separación

Producción:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\data\...
```

Demo:

```text
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\...
```

`--demo` nunca abre ni escribe la base real. `--demo-reset` elimina únicamente los archivos de la carpeta demo y vuelve a crear el conjunto ficticio.

### Dataset

El seeder usa APIs productivas de Core/Application y adaptadores existentes, no SQL directo desde WPF.

Incluye:

- grupo de demostración con alrededor de 30 estudiantes;
- estudiante inactivo con historial;
- estudiante incorporado después de actividades tempranas;
- asistencia de varias semanas con P/F/R/J;
- proyecto finalizado histórico;
- proyecto en curso con 8–10 actividades;
- proyecto borrador;
- niveles de logro variados;
- observaciones en algunas entregas;
- notas pedagógicas y acuerdos de tutor ficticios.

Todos los nombres y datos son inventados. Se identifica claramente el modo con un badge `DEMO` y en el título de ventana.

## Accesibilidad

- tooltips de actividad no dependen sólo del mouse;
- `AutomationProperties.Name` describe encabezados relevantes;
- foco visible;
- atajos simples sólo con foco dentro de la grilla operativa;
- temas Claro/Oscuro/Alto contraste usan recursos semánticos;
- `—` identifica explícitamente celdas no aplicables.