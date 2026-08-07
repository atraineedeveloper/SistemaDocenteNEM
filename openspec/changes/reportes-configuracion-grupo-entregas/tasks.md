# Tasks: Reportes, configuración contextual y entrega explícita

## Dominio y persistencia
- [x] 1. Definir semántica de `EstadoEntregaActividad` separada de `NivelLogro`.
- [ ] 2. Implementar estado explícito de entrega en Core y contratos Application.
- [ ] 3. Migrar SQLite v6→v7 separando `estado_entrega` y `nivel_logro`.
- [ ] 4. Agregar configuración contextual 1:1 por grupo y persistencia.
- [ ] 5. Adaptar demo a contexto de grupo y estados de entrega.

## Reporting
- [ ] 6. Activar `SistemaDocente.Reporting` con modelos/cálculos puros.
- [ ] 7. Implementar reporte individual.
- [ ] 8. Implementar reporte grupal.
- [ ] 9. Cubrir cumplimiento real: entregadas/no entregadas/pendientes.

## Presentación y WPF
- [ ] 10. Agregar `GestionReportesViewModel` y navegación global Reportes.
- [ ] 11. Crear vista Reportes con modos Individual/Grupal según wireframe aprobado.
- [ ] 12. Crear ventana Configuración del grupo con etapa cognoscitiva grupal de Piaget.
- [ ] 13. Integrar configuración desde Grupo/Reportes sin alterar las vistas aprobadas más de lo necesario.
- [ ] 14. Adaptar Evaluación para estado explícito sin reintroducir selector de actividad.

## Calidad
- [ ] 15. Agregar pruebas Core/Application/Data/Reporting/Presentation/WPF.
- [ ] 16. Actualizar arquitectura y guía de reportes/configuración.
- [ ] 17. Ejecutar format/build/test/OpenSpec/diff-check en Windows.
- [ ] 18. Validar manualmente reportes, configuración, temas y escalado.