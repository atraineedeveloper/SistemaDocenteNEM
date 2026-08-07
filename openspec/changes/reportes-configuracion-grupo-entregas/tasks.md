# Tasks: Reportes, configuración contextual y entrega explícita

## Dominio y persistencia
- [x] 1. Definir semántica de `EstadoEntregaActividad` separada de `NivelLogro`.
- [x] 2. Implementar estado explícito de entrega en Core y contratos Application.
- [x] 3. Adoptar y documentar la estrategia SQLite definitiva: extensión aditiva versionada `reportes-contexto-entregas` v1 sobre `user_version = 6`, con conversión legacy y sin reconstruir la tabla base.
- [x] 4. Agregar configuración contextual 1:1 por grupo y persistencia.
- [x] 5. Adaptar demo a contexto de grupo y estados de entrega.

## Reporting
- [x] 6. Activar `SistemaDocente.Reporting` con modelos/cálculos puros.
- [x] 7. Implementar reporte individual.
- [x] 8. Implementar reporte grupal.
- [x] 9. Cubrir cumplimiento real: entregadas/no entregadas/pendientes.

## Presentación y WPF
- [x] 10. Agregar `GestionReportesViewModel` y navegación global Reportes.
- [x] 11. Crear vista Reportes con modos Individual/Grupal según wireframe aprobado.
- [x] 12. Crear ventana Configuración del grupo con etapa cognoscitiva grupal de Piaget.
- [x] 13. Integrar configuración desde Grupo y Reportes sin alterar las vistas aprobadas más de lo necesario; ambas superficies reutilizan la misma ventana y ViewModel contextual.
- [x] 14. Adaptar Evaluación para estado explícito sin reintroducir selector de actividad; matriz, guardado, filtros, editor y atajos ya conservan entrega/nivel de forma separada.

## Calidad
- [x] 15. Agregar pruebas Core/Application/Data/Reporting/Presentation/WPF para invariantes, compatibilidad legacy, extensión SQLite, cálculos, matriz y composición.
- [x] 16. Actualizar arquitectura y guía de reportes/configuración, incluida documentación de demo y persistencia aditiva.
- [x] 17. Ejecutar format/build/test/OpenSpec/diff-check en Windows mediante GitHub Actions; CI verde con build Release, 308 pruebas, OpenSpec completo y whitespace check.
- [ ] 18. Validar manualmente reportes, configuración, temas y escalado.
