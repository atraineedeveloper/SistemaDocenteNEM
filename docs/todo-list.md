# Lista de Tareas Pendientes (TODO)

## Resumen Ejecutivo

Este documento recopila todas las tareas pendientes identificadas en el proyecto, incluyendo pruebas manuales pendientes y funcionalidades no implementadas.

**Última actualización:** Generado automáticamente desde especificaciones OpenSpec

---

## Tareas Pendientes de Pruebas Manuales

### Módulo Proyectos y Actividades

**Ubicación:** `openspec/changes/modulo-proyectos-actividades-inicial/tasks.md`

| ID | Tarea | Estado | Prioridad |
|----|-------|--------|-----------|
| 9.7 | Probar manualmente creación y ciclo de proyecto, periodos incompatibles, actividad con 40 estudiantes, E/N/P, historial inactivo, anulación, conflictos, reapertura y redimensionamiento | ⏳ Pendiente | Alta |

**Detalle de la prueba 9.7:**
- [ ] Creación de proyecto didáctico
- [ ] Ciclo completo de estados (Borrador → En Curso → Finalizado → Reabrir)
- [ ] Validación de periodos incompatibles con actividades existentes
- [ ] Actividad con 40 estudiantes (rendimiento y UI)
- [ ] Estados de entrega: Entregada (E), No entregada (N), Pendiente (P)
- [ ] Historial de estudiantes inactivos
- [ ] Anulación de actividad
- [ ] Conflictos de concurrencia (versión)
- [ ] Reapertura de proyecto finalizado
- [ ] Redimensionamiento de zonas en la interfaz

---

### Módulo Asistencia

**Ubicación:** `openspec/changes/modulo-asistencia-inicial/tasks.md`

| ID | Tarea | Estado | Prioridad |
|----|-------|--------|-----------|
| 12.6 | Probar manualmente febrero, abril y agosto, navegación mensual, 30–40 estudiantes, scroll, columnas congeladas, teclado, guardado diario y mensual, fallo simulado, reapertura, inactivo histórico, filtros, porcentaje y redimensionamiento | ⏳ Pendiente | Alta |

**Detalle de la prueba 12.6:**
- [ ] Mes de febrero (28/29 días)
- [ ] Mes de abril (30 días)
- [ ] Mes de agosto (31 días)
- [ ] Navegación entre meses (anterior/siguiente)
- [ ] Carga con 30-40 estudiantes
- [ ] Scroll vertical y horizontal
- [ ] Columnas congeladas (números/nombres)
- [ ] Navegación con teclado
- [ ] Guardado diario individual
- [ ] Guardado mensual múltiple
- [ ] Simulación de fallo en guardado
- [ ] Reapertura de día ya guardado
- [ ] Estudiantes inactivos en histórico
- [ ] Filtros de visualización
- [ ] Porcentaje de asistencia
- [ ] Redimensionamiento de ventana

---

## Funcionalidades Pendientes de Implementación

### Módulo Reportes

**Estado:** Reservado para desarrollo futuro

**Funcionalidades pendientes:**
- [ ] Generación de reportes de asistencia mensual
- [ ] Exportación a PDF
- [ ] Exportación a Excel/CSV
- [ ] Reportes de proyectos y actividades
- [ ] Estadísticas de cumplimiento
- [ ] Historial académico por estudiante
- [ ] Respaldos automáticos
- [ ] Importación de datos desde CSV/Excel

**Proyecto relacionado:** `SistemaDocente.Reporting` (actualmente vacío)

---

### Módulo Evaluación

**Estado:** No implementado

**Funcionalidades generales pendientes:**
- [ ] Registro de evaluación formativa
- [ ] Rúbricas de evaluación
- [ ] Escalas personalizadas
- [ ] Promedios por actividad
- [ ] Promedios por proyecto
- [ ] Historial de evaluaciones

**Nota:** Esta funcionalidad fue mencionada como objetivo inicial pero aún no tiene especificación OpenSpec aprobada.

---

### Módulo Importación

**Estado:** No implementado

**Funcionalidades generales pendientes:**
- [ ] Importar lista de estudiantes desde CSV
- [ ] Importar desde Excel
- [ ] Mapeo de columnas
- [ ] Validación previa de datos
- [ ] Manejo de duplicados
- [ ] Rollback en caso de error

---

### Módulo Respaldos

**Estado:** No implementado

**Funcionalidades generales pendientes:**
- [ ] Crear respaldo completo de base de datos
- [ ] Programar respaldos automáticos
- [ ] Restaurar desde respaldo
- [ ] Verificar integridad de respaldo
- [ ] Compresión de archivos de respaldo
- [ ] Almacenamiento en ubicación externa

---

## Deuda Técnica Conocida

### Código

| Área | Descripción | Impacto |
|------|-------------|---------|
| Presentación | MVVM básico propio (no usa toolkit) | Bajo - Funciona pero requiere mantenimiento |
| Composición | Inyección de dependencias manual | Bajo - Explícito pero verboso |
| Testing | Dobles manuales (sin framework de mocking) | Medio - Más código de test pero sin dependencias externas |

### Documentación

| Área | Descripción | Estado |
|------|-------------|--------|
| Especificaciones | Algunas especificaciones pueden requerir actualización | Revisión pendiente |
| Diagramas | Diagramas UML formales ausentes | Parcialmente cubierto por diagrams.md |
| Comentarios XML | XML docs en código fuente limitados | Mejorable |

---

## Mejoras Potenciales (No Críticas)

### UX/UI

- [ ] Animaciones suaves en transiciones
- [ ] Temas claros/oscuros
- [ ] Personalización de colores
- [ ] Atajos de teclado configurables
- [ ] Tooltips informativos extendidos

### Rendimiento

- [ ] Virtualización de listas para grupos muy grandes (>50 estudiantes)
- [ ] Caché de consultas frecuentes
- [ ] Lazy loading de actividades históricas

### Accesibilidad

- [ ] Soporte completo para lectores de pantalla
- [ ] Navegación solo con teclado mejorada
- [ ] Contraste de colores verificable WCAG
- [ ] Textos alternativos en elementos visuales

---

## Issues de Mantenimiento

### Dependencias

| Paquete | Versión | Acción |
|---------|---------|--------|
| .NET SDK | 10 | Mantener actualizado |
| Microsoft.Data.Sqlite | Latest | Monitorear actualizaciones |
| xUnit | Latest | Monitorear actualizaciones |

### Herramientas Externas

| Herramienta | Uso | Estado |
|-------------|-----|--------|
| OpenSpec CLI | Validación de especificaciones | Requerida |
| Git | Control de versiones | Configurado |
| dotnet format | Formato de código | Configurado en .editorconfig |

---

## Próximos Pasos Recomendados

### Corto Plazo (1-2 semanas)

1. **Completar pruebas manuales pendientes**
   - Ejecutar prueba 9.7 (Proyectos/Actividades)
   - Ejecutar prueba 12.6 (Asistencia)
   - Documentar resultados

2. **Estabilizar módulo actual**
   - Corregir bugs reportados
   - Mejorar mensajes de error
   - Optimizar rendimiento si es necesario

### Mediano Plazo (1-3 meses)

3. **Implementar módulo de Reportes**
   - Crear especificación OpenSpec
   - Diseñar arquitectura
   - Implementar reportes básicos de asistencia

4. **Mejorar documentación**
   - Agregar ejemplos de uso
   - Crear tutoriales paso a paso
   - Documentar troubleshooting común

### Largo Plazo (3-6 meses)

5. **Funcionalidades avanzadas**
   - Módulo de evaluación formativa
   - Sistema de importación/exportación
   - Respaldos automáticos

6. **Refinamiento**
   - Mejoras de UX basadas en feedback
   - Optimización de rendimiento
   - Accesibilidad mejorada

---

## Cómo Contribuir

Si deseas ayudar con alguna tarea pendiente:

1. **Selecciona una tarea** de esta lista
2. **Revisa especificaciones** relacionadas en `openspec/`
3. **Crea una propuesta** si es funcionalidad nueva
4. **Sigue el flujo OpenSpec** para implementación
5. **Envía PR** con validaciones completas

---

## Referencias

- [README Principal](../README.md)
- [Guía de Desarrollo](development-guide.md)
- [Especificaciones OpenSpec](../openspec/specs/)
- [Cambios Propuestos](../openspec/changes/)

---

## Notas

- Esta lista se genera manualmente y puede quedar desactualizada
- Para ver el estado más reciente, revisar directamente los archivos `tasks.md` en `openspec/changes/`
- Las prioridades están sujetas a cambio según necesidades del proyecto
