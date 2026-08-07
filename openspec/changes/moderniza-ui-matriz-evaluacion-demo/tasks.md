# Tasks: Modernización UI, matriz de evaluación y modo demo

## Diseño común

- [x] Documentar propuesta y decisiones del cambio.
- [x] Modernizar encabezado global sin añadir navegación lateral duplicada.
- [x] Mantener temas claro, oscuro y alto contraste mediante recursos semánticos.
- [x] Añadir indicador visual de modo DEMO.

## Grupo

- [x] Añadir métricas Total/Activos.
- [x] Mejorar jerarquía visual, búsqueda, tabla y action bar.
- [x] Mantener virtualización y ventanas dedicadas de estudiante/expediente.

## Asistencia

- [x] Modernizar jerarquía y superficies sin reducir densidad operativa.
- [x] Mantener columnas congeladas, separación semanal y P/F/R/J contextuales.
- [x] Priorizar `Guardar cambios` y reducir ruido de acciones secundarias.

## Proyectos

- [x] Añadir búsqueda de proyectos y métricas Total/En curso/Borradores.
- [x] Modernizar tabla y sustituir copy `Ver / Editar Detalle` por `Abrir proyecto`.
- [x] Mantener ventanas dedicadas y no reintroducir master-detail.

## Evaluación

- [x] Eliminar selector de actividad.
- [x] Crear modelo visual de matriz estudiante × actividad.
- [x] Generar columnas dinámicas con código estable derivado de `ActividadId` y tooltip nombre+fecha.
- [x] Congelar Núm. y Estudiante.
- [x] Representar padrones históricos con celdas no aplicables `—`.
- [x] Seleccionar actividad implícitamente por columna/celda actual.
- [x] Mostrar métricas de la actividad seleccionada.
- [x] Mantener D/S/E/R/N/P únicamente dentro de la grilla.
- [x] Añadir acción masiva por actividad seleccionada.
- [x] Preservar observaciones mediante editor compacto de celda.
- [x] Guardar actividades modificadas secuencialmente sin cambiar atomicidad de dominio.
- [x] Proteger cambios pendientes al cambiar de proyecto/salir.

## Modo demo

- [x] Separar rutas demo de producción.
- [x] Implementar `--demo` y `--demo-reset`.
- [x] Crear dataset ficticio con ~30 estudiantes y datos variados.
- [x] Incluir estudiante inactivo histórico y alta posterior.
- [x] Sembrar asistencia con P/F/R/J.
- [x] Sembrar proyectos Borrador/En curso/Finalizado y múltiples actividades.
- [x] Sembrar niveles de logro/observaciones variados.
- [x] Sembrar notas pedagógicas y acuerdos de tutor.

## Pruebas y cierre

- [x] Actualizar pruebas Presentation de Evaluación matricial.
- [x] Actualizar pruebas WPF de composición/estructura/teclado.
- [x] Añadir pruebas de rutas demo y aislamiento respecto a producción.
- [ ] `dotnet format --verify-no-changes`.
- [ ] `dotnet build` sin errores ni warnings.
- [ ] `dotnet test` completo.
- [ ] `openspec validate --all`.
- [ ] `git diff --check`.
- [ ] Prueba manual a 100 %, 125 % y 150 % con Claro/Oscuro/Alto contraste.
- [ ] Prueba manual de `--demo-reset`, navegación y todas las ventanas dedicadas.
