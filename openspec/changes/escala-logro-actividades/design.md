## Context

La solucion ya tiene implementado el modulo de Proyectos completo: Core con dos agregados (ProyectoDidactico y ActividadProyecto), Application con casos de uso y snapshots, Data con SQLite en user_version = 3, Presentation con ViewModels portables y App.Wpf. El problema es que EstadoEntrega tiene tres valores (Pendiente/Entregada/NoEntregada) que no reflejan la escala de logro de la NEM, y la interfaz WPF no refleja el flujo pedagogico natural.

## Goals / Non-Goals

**Goals:**

- Reemplazar EstadoEntrega por NivelLogro de 6 valores que modela la escala real de la NEM.
- Migrar SQLite de v3 a v4 de forma transaccional sin perdida de datos.
- Redisenar la interfaz WPF para que el flujo proyecto->actividad->evaluacion sea intuitivo.
- Ofrecer captura de nivel de logro por teclado con atajos D/S/E/R/N.

**Non-Goals:**

- Agregar calificaciones numericas, porcentajes o rubricas.
- Cambiar la logica de proyectos, actividades, estados o concurrencia optimista.
- Introducir async, ORM, DI, framework de navegacion o paquetes UI externos.

## Decisions

### 1. NivelLogro como enumeracion de seis valores

Se renombrara EstadoEntrega a NivelLogro con valores: Pendiente = 0, Domina = 1, Suficiente = 2, EnProceso = 3, RequiereApoyo = 4, NoEntrego = 5. Los cuatro niveles de desempeno representan actividades evaluadas con distinto grado de logro. NoEntrego representa incumplimiento. Pendiente indica que aun no se ha registrado evaluacion.

El campo en RegistroEntregaActividad cambiara de EstadoEntrega a NivelLogro. Los snapshots de Application y ViewModels de Presentation se actualizaran correspondientemente.

### 2. Conteos en snapshots

ActividadProyectoDetalle pasara de tres conteos a seis: Pendiente, Domina, Suficiente, EnProceso, RequiereApoyo, NoEntrego. El total permanece igual. No se introduce porcentaje.

### 3. Migracion SQLite v3 a v4

La migracion recreara la tabla entregas_actividad con el CHECK ampliado de 0 a 5 usando la tecnica de renombrar tabla original, crear tabla nueva y copiar datos dentro de una transaccion. Se establece user_version = 4 al final. Un fallo revierte a v3 intacta.

### 4. Atajos de teclado en la grilla

Los atajos E/N/P se reemplazan por D (Domina), S (Suficiente), E (En proceso), R (Requiere apoyo) y N (No entrego). Ctrl+S permanece para guardar. El code-behind procesara KeyDown y delegara al ViewModel.

### 5. Rediseno visual de la grilla

Cada nivel tendra etiqueta corta: D, S, EP, RA, NE y guion para Pendiente. Se usara diferenciacion cromatica mediante estilos WPF nativos sin paquetes externos. La distincion no dependera solo del color.

### 6. Filtros ampliados

Solo incidencias cambia a (Pendiente o RequiereApoyo o NoEntrego). Se agregan filtros individuales por nivel de logro.

## Risks / Trade-offs

- Renombrar EstadoEntrega a NivelLogro afecta muchos archivos: la refactorizacion es sistematica y comprobable mediante build y pruebas existentes.
- La migracion v3 a v4 requiere recrear la tabla: SQLite no soporta ALTER COLUMN, se usara tabla temporal dentro de una transaccion.
- Los atajos E y N se reutilizan con distinto significado semantico: E pasa de Entregada a En proceso y N de NoEntregada a No entrego; ambos siguen representando el estado del alumno respecto de la actividad.
