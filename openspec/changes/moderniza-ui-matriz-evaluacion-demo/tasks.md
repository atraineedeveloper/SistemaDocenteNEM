# Tasks: Modernización UI, matriz de evaluación y modo demo

## Diseño común

- [x] Documentar propuesta y decisiones del cambio.
- [ ] Modernizar encabezado global sin añadir navegación lateral duplicada.
- [ ] Mantener temas claro, oscuro y alto contraste mediante recursos semánticos.
- [ ] Añadir indicador visual de modo DEMO.

## Grupo

- [ ] Añadir métricas Total/Activos.
- [ ] Mejorar jerarquía visual, búsqueda, tabla y action bar.
- [ ] Mantener virtualización y ventanas dedicadas de estudiante/expediente.

## Asistencia

- [ ] Modernizar jerarquía y superficies sin reducir densidad operativa.
- [ ] Mantener columnas congeladas, separación semanal y P/F/R/J contextuales.
- [ ] Priorizar `Guardar cambios` y reducir ruido de acciones secundarias.

## Proyectos

- [ ] Añadir búsqueda de proyectos y métricas Total/En curso/Borradores.
- [ ] Modernizar tabla y sustituir copy `Ver / Editar Detalle` por `Abrir proyecto`.
- [ ] Mantener ventanas dedicadas y no reintroducir master-detail.

## Evaluación

- [ ] Eliminar selector de actividad.
- [ ] Crear modelo visual de matriz estudiante × actividad.
- [ ] Generar columnas dinámicas A01/A02/... con tooltip nombre+fecha.
- [ ] Congelar Núm. y Estudiante.
- [ ] Representar padrones históricos con celdas no aplicables `—`.
- [ ] Seleccionar actividad implícitamente por columna/celda actual.
- [ ] Mostrar métricas de la actividad seleccionada.
- [ ] Mantener D/S/E/R/N/P únicamente dentro de la grilla.
- [ ] Añadir acción masiva por actividad seleccionada.
- [ ] Preservar observaciones mediante editor compacto de celda.
- [ ] Guardar actividades modificadas secuencialmente sin cambiar atomicidad de dominio.
- [ ] Proteger cambios pendientes al cambiar de proyecto/salir.

## Modo demo

- [ ] Separar rutas demo de producción.
- [ ] Implementar `--demo` y `--demo-reset`.
- [ ] Crear dataset ficticio con ~30 estudiantes y datos variados.
- [ ] Incluir estudiante inactivo histórico y alta posterior.
- [ ] Sembrar asistencia con P/F/R/J.
- [ ] Sembrar proyectos Borrador/En curso/Finalizado y múltiples actividades.
- [ ] Sembrar niveles de logro/observaciones variados.
- [ ] Sembrar notas pedagógicas y acuerdos de tutor.

## Pruebas y cierre

- [ ] Actualizar pruebas Presentation de Evaluación matricial.
- [ ] Actualizar pruebas WPF de composición/estructura/teclado.
- [ ] Añadir pruebas de rutas demo y aislamiento respecto a producción.
- [ ] `dotnet format --verify-no-changes`.
- [ ] `dotnet build` sin errores ni warnings.
- [ ] `dotnet test` completo.
- [ ] `openspec validate --all`.
- [ ] `git diff --check`.
- [ ] Prueba manual a 100 %, 125 % y 150 % con Claro/Oscuro/Alto contraste.
- [ ] Prueba manual de `--demo-reset`, navegación y todas las ventanas dedicadas.
