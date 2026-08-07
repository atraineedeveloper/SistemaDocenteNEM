# Change: Modernizar UI, matriz de evaluación y modo demo

## Why

Las vistas principales ya están separadas correctamente, pero la experiencia visual conserva patrones inconsistentes y Evaluación obliga a seleccionar una actividad antes de trabajar. Eso dificulta comparar el avance de un estudiante entre actividades. Además, falta un conjunto seguro de datos ficticios suficientemente rico para validar la aplicación sin contaminar información real.

## What Changes

- Modernizar el shell y las vistas Grupo, Asistencia, Proyectos y Evaluación manteniendo una única navegación superior y sin introducir sidebar.
- Convertir Evaluación en una matriz `estudiante × actividad` sin selector independiente de actividad.
- Congelar `Núm.` y `Estudiante` y generar columnas dinámicas por actividad.
- Derivar un código visual estable de cada `ActividadId`, independiente de título, fecha u orden.
- Representar como no aplicables las celdas de estudiantes que no pertenecían al padrón histórico de una actividad.
- Mantener atajos contextuales de evaluación dentro de la grilla y edición compacta de observación mediante ventana dedicada.
- Guardar cambios de Evaluación secuencialmente por actividad, conservando la actividad como unidad atómica.
- Agregar modo `--demo` con almacenamiento SQLite y estado de aplicación separados de producción.
- Agregar `--demo-reset` para reconstruir de forma determinista el dataset ficticio.
- Sembrar aproximadamente 30 estudiantes, historial/inactivos, alta posterior, asistencia, proyectos, evaluación y expediente pedagógico.
- Mantener fuera de alcance cambios de reglas de negocio, ORM, contenedor DI, framework de navegación, paquetes UI nuevos y reportes/exportación.

## Capabilities

### New Capabilities

- `interfaz-evaluacion-matriz`: matriz estudiante × actividad con identidad visual estable, padrón histórico, teclado contextual y guardado por actividad.
- `modo-demostracion`: almacenamiento ficticio aislado y reiniciable con dataset determinista y representativo.
- `modernizacion-ui-principal`: jerarquía y acciones consistentes en shell y vistas principales sin duplicar navegación.

### Modified Capabilities

- Ninguna. Las capacidades se incorporan como contratos nuevos de interfaz/demostración sin cambiar los agregados de dominio existentes.

## Impact

- **Core/Application/Data:** se reutilizan agregados, identidades y atomicidad existentes; no se introduce una nueva identidad de actividad ni un agregado mensual/evaluación.
- **Presentation:** matriz visual, selección por celda, filtros, cambios pendientes y guardado secuencial por actividad.
- **App.Wpf:** columnas dinámicas, encabezados accesibles, ventanas compactas, shell modernizado y argumentos `--demo`/`--demo-reset`.
- **Persistencia:** producción y demostración usan rutas separadas; el reset sólo puede afectar almacenamiento demo.
- **Pruebas:** regresiones de matriz, teclado contextual, virtualización, rutas demo y composición visual.