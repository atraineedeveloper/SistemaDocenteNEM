# Proposal: Modernizar UI, matriz de evaluación y modo demo

## Problema

Las cuatro vistas principales ya están separadas correctamente, pero visualmente aún conservan una apariencia administrativa/plana y patrones inconsistentes entre módulos. Evaluación requiere seleccionar una actividad antes de evaluar, lo que obliga a cambiar de contexto continuamente y dificulta comparar el desempeño de un estudiante a través de las actividades del proyecto.

También falta un conjunto seguro de datos ficticios suficientemente rico para validar visualmente listas largas, historial, asistencia, proyectos, evaluación y expediente sin contaminar los datos reales del docente.

## Objetivo

Modernizar la experiencia visual de Grupo, Asistencia, Proyectos y Evaluación manteniendo una sola navegación superior y respetando `docs/UI-GUIDELINES.md`.

Evaluación cambiará de `actividad seleccionada → estudiantes` a una matriz `estudiante × actividad`, comparable al modelo mental de la asistencia mensual `estudiante × día`.

Se añadirá un modo de demostración explícito que use almacenamiento separado y cargue datos ficticios representativos.

## Alcance

- Shell/encabezado más ligero y contemporáneo, sin barra lateral duplicada.
- Grupo con jerarquía visual, métricas, búsqueda y tabla mejorada.
- Asistencia con jerarquía, controles y barra de acciones consistentes, preservando su densidad operativa.
- Proyectos con búsqueda, métricas y punto de entrada claro a las ventanas dedicadas.
- Evaluación sin selector de actividad: columnas dinámicas por actividad y filas por estudiante.
- Identificador visual de actividad generado por el sistema (`A01`, `A02`, ...), con nombre/fecha accesibles mediante tooltip y nombre de automatización.
- Columnas `Núm.` y `Estudiante` congeladas en Evaluación.
- Atajos D/S/E/R/N/P sólo dentro de la grilla de Evaluación.
- Guardado de cambios de Evaluación por actividad, secuencialmente, manteniendo la actividad como unidad atómica.
- Celdas no aplicables para estudiantes que no pertenecían al padrón histórico de una actividad.
- Edición de observación de una celda sin ensuciar la matriz principal.
- Modo `--demo` con base SQLite y estado separados de producción.
- Opción `--demo-reset` para reconstruir el conjunto ficticio de forma determinista.
- Datos demo con aproximadamente 30 estudiantes, actividad histórica/inactivos, altas posteriores, asistencia, proyectos, niveles de logro, observaciones pedagógicas y acuerdos con tutor.

## Fuera de alcance

- Cambiar reglas de negocio de asistencia, proyectos o expediente.
- Introducir sidebar permanente.
- Reintroducir master-detail de tres zonas.
- Añadir ORM, framework de navegación, DI container o paquetes UI.
- Convertir el identificador visual `Axx` en una identidad de dominio o clave externa.
- Reportes/exportación.

## Compatibilidad

El almacenamiento real del usuario no se modifica para el modo demo. `--demo` utiliza una carpeta de datos independiente. La matriz de Evaluación reutiliza los agregados y transacciones existentes de `ActividadProyecto`; no cambia la atomicidad vigente.