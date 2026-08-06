# Checklist de módulos del Sistema Docente NEM

**Estado del proyecto:** aplicación local de escritorio para un docente de primaria pública, construida con C# .NET, WPF y SQLite.

## Módulos ya desarrollados

- [x] **Fundación técnica**
  - Solución por capas.
  - Composición manual.
  - Persistencia local con SQLite.
  - Pruebas por proyecto.
  - Validación mediante OpenSpec.

- [x] **Grupo y estudiantes**
  - Registro del grupo.
  - Alta y edición de estudiantes con datos desglosados (Primer Apellido, Segundo Apellido, Nombres, Edad calculada dinámicamente, Género, Fecha de Ingreso y Observaciones cualitativas).
  - Exclusión explícita de CURP.
  - Activación e inactivación.
  - Conservación del historial.
  - Persistencia local con migración SQLite v6.
  - Tipografía tipográfica global Montserrat en la interfaz WPF.

- [x] **Asistencia**
  - Asistencia diaria como unidad atómica.
  - Vista mensual por días lectivos.
  - Estados: Presente, Falta, Retardo y Falta justificada.
  - Guardado por día y guardado secuencial de cambios mensuales.
  - Conteos y porcentajes.
  - Historial de estudiantes inactivos.

- [~] **Proyectos, actividades y entregas**
  - Agregados de ProyectoDidactico y ActividadProyecto.
  - Actividades dentro de un proyecto.
  - Entregas por estudiante.
  - Estados Pendiente, Entregada y No entregada.
  - Migración SQLite a versión 3.
  - Interfaz WPF de tres zonas.
  - Pendiente: terminar correcciones menores de interfaz, prueba manual completa y cierre formal del cambio.

---

# Módulos pendientes

## 1. Planeación didáctica NEM

- [ ] Crear planeaciones asociadas a un proyecto.
- [ ] Registrar periodo de trabajo.
- [ ] Definir propósito del proyecto.
- [ ] Registrar campo o campos formativos.
- [ ] Registrar contenidos.
- [ ] Registrar procesos de desarrollo de aprendizaje.
- [ ] Registrar ejes articuladores.
- [ ] Registrar metodología del proyecto.
- [ ] Registrar producto final esperado.
- [ ] Registrar recursos y materiales.
- [ ] Registrar adecuaciones o apoyos.
- [ ] Relacionar actividades ya existentes con la planeación.
- [ ] Consultar y editar la planeación durante el desarrollo del proyecto.

**Descripción:**  
Este módulo convertiría cada proyecto en una planeación didáctica completa y alineada con el Plan de Estudio 2022. El proyecto seguiría siendo el contenedor principal y las actividades serían las acciones concretas realizadas dentro de él.

## 2. Evaluación formativa

- [ ] Definir criterios de evaluación por proyecto o actividad.
- [ ] Registrar valoraciones cualitativas.
- [ ] Registrar niveles de desempeño.
- [ ] Crear rúbricas sencillas.
- [ ] Evaluar actividades entregadas.
- [ ] Registrar retroalimentación.
- [ ] Distinguir actividad no entregada de actividad aún no evaluada.
- [ ] Consultar avances por estudiante.
- [ ] Conservar historial de evaluaciones.

**Descripción:**  
Permitirá evaluar el proceso y no solamente asignar una calificación final. Se apoyará en las actividades y entregas que ya existen.

## 3. Calificaciones y periodos de evaluación

- [ ] Configurar periodos de evaluación.
- [ ] Registrar calificaciones por campo formativo.
- [ ] Definir cómo se integran actividades, proyectos y criterios.
- [ ] Calcular resultados sin ocultar el origen de los datos.
- [ ] Permitir ajustes manuales justificados.
- [ ] Registrar observaciones del periodo.
- [ ] Consultar historial de calificaciones.
- [ ] Evitar promedios engañosos cuando falten evidencias.

**Descripción:**  
Este módulo transformará la información formativa en resultados de periodo. Debe construirse después de la evaluación para que las calificaciones tengan una base clara.

## 4. Expediente y seguimiento individual del alumno

- [x] Crear una ficha individual por estudiante.
- [x] Integrar asistencia.
- [x] Integrar entregas de actividades.
- [x] Integrar evaluaciones.
- [ ] Integrar calificaciones.
- [x] Registrar fortalezas.
- [x] Registrar dificultades.
- [x] Registrar apoyos aplicados.
- [x] Registrar acuerdos con familiares o tutores.
- [x] Registrar observaciones cronológicas.
- [x] Mostrar alertas pedagógicas sin emitir diagnósticos.

**Descripción:**  
Reunirá en un solo lugar la información relevante de cada alumno para facilitar el acompañamiento, las reuniones con familias y la toma de decisiones pedagógicas.

## 5. Diario y bitácora docente

- [ ] Registrar lo ocurrido cada día.
- [ ] Relacionar entradas con proyectos y actividades.
- [ ] Registrar avances, dificultades e incidentes.
- [ ] Registrar ajustes hechos a la planeación.
- [ ] Buscar por fecha, proyecto o estudiante.
- [ ] Mantener notas privadas del docente.
- [ ] Convertir observaciones relevantes en seguimiento individual.

**Descripción:**  
Servirá como memoria profesional del docente y permitirá documentar cambios entre lo planeado y lo realizado.

## 6. Comunicación y acuerdos con familias

- [ ] Registrar reuniones con madres, padres o tutores.
- [ ] Registrar motivo de la reunión.
- [ ] Registrar acuerdos.
- [ ] Registrar compromisos y fechas de seguimiento.
- [ ] Relacionar acuerdos con el estudiante.
- [ ] Consultar acuerdos pendientes.
- [ ] Generar un resumen imprimible.
- [ ] Proteger información sensible.

**Descripción:**  
Ayudará a conservar evidencia clara de reuniones y acuerdos, evitando depender únicamente de notas sueltas o mensajes informales.

## 7. Incidencias y convivencia escolar

- [ ] Registrar incidencias de manera objetiva.
- [ ] Diferenciar hechos, interpretaciones y acciones tomadas.
- [ ] Registrar personas involucradas.
- [ ] Registrar medidas de atención.
- [ ] Registrar seguimiento.
- [ ] Relacionar protocolos escolares cuando corresponda.
- [ ] Aplicar controles de privacidad.
- [ ] Evitar etiquetas o diagnósticos sobre estudiantes.

**Descripción:**  
Permitirá documentar situaciones de convivencia con lenguaje profesional y trazabilidad, sin convertir el sistema en una herramienta de sanción automática.

## 8. Reportes

- [ ] Reporte mensual de asistencia.
- [ ] Reporte de faltas, retardos y justificadas.
- [ ] Reporte de entregas.
- [ ] Reporte de actividades por proyecto.
- [ ] Reporte de evaluación por estudiante.
- [ ] Reporte de seguimiento individual.
- [ ] Resumen para reunión con familias.
- [ ] Reporte de cierre de proyecto.
- [ ] Vista previa antes de imprimir.
- [ ] Exportación a PDF.

**Descripción:**  
Concentrará los datos de los demás módulos en documentos claros. Conviene desarrollarlo cuando evaluación y seguimiento ya estén disponibles.

## 9. Evidencias digitales

- [ ] Adjuntar fotografías.
- [ ] Adjuntar documentos.
- [ ] Adjuntar productos de los estudiantes.
- [ ] Relacionar archivos con proyecto, actividad y alumno.
- [ ] Registrar descripción y fecha.
- [ ] Evitar duplicados innecesarios.
- [ ] Definir límites de tamaño.
- [ ] Organizar archivos fuera de la base SQLite.
- [ ] Detectar archivos faltantes.
- [ ] Proteger datos personales.

**Descripción:**  
Ampliará el concepto actual de evidencia, que por ahora representa la entrega registrada. Este módulo incorporará archivos reales sin guardarlos directamente dentro de SQLite.

## 10. Calendario escolar y agenda

- [ ] Registrar días lectivos.
- [ ] Registrar suspensiones.
- [ ] Registrar Consejo Técnico Escolar.
- [ ] Registrar eventos.
- [ ] Registrar fechas de proyectos y actividades.
- [ ] Mostrar próximos pendientes.
- [ ] Relacionar agenda con planeaciones.
- [ ] Evitar modificar automáticamente registros históricos.

**Descripción:**  
Permitirá adaptar asistencia, proyectos y planeaciones al calendario real de la escuela. Debe diseñarse con cuidado para no alterar datos ya guardados.

## 11. Importación de estudiantes

- [ ] Importar desde Excel o CSV.
- [ ] Validar encabezados.
- [ ] Detectar registros duplicados.
- [ ] Mostrar vista previa.
- [ ] Permitir corregir errores antes de importar.
- [ ] Mantener la importación en una transacción.
- [ ] Generar resumen de resultados.
- [ ] No sobrescribir alumnos existentes sin confirmación.

**Descripción:**  
Reducirá el trabajo inicial de captura cuando el docente reciba una lista institucional.

## 12. Exportación de datos

- [ ] Exportar estudiantes.
- [ ] Exportar asistencia.
- [ ] Exportar proyectos y actividades.
- [ ] Exportar entregas.
- [ ] Exportar evaluaciones.
- [ ] Exportar a CSV o Excel.
- [ ] Excluir información sensible cuando corresponda.
- [ ] Permitir seleccionar periodo y contenido.

**Descripción:**  
Facilitará utilizar los datos en herramientas externas y entregar información a la escuela sin depender completamente de la aplicación.

## 13. Respaldos y restauración

- [ ] Crear respaldo manual.
- [ ] Crear respaldo automático.
- [ ] Incluir base de datos, configuración y evidencias.
- [ ] Validar el respaldo antes de confirmarlo.
- [ ] Restaurar con confirmación.
- [ ] Conservar copia de seguridad antes de restaurar.
- [ ] Mostrar fecha y tamaño de cada respaldo.
- [ ] Detectar respaldos incompatibles.
- [ ] Evitar pérdida de datos durante actualizaciones.

**Descripción:**  
Es indispensable antes de usar el sistema con información real durante todo el ciclo escolar.

## 14. Configuración del sistema

- [ ] Datos del docente.
- [ ] Datos de la escuela.
- [ ] Ciclo escolar.
- [ ] Preferencias de fecha.
- [ ] Carpeta de evidencias.
- [ ] Carpeta de respaldos.
- [ ] Apariencia y tamaño de interfaz.
- [ ] Reglas configurables que no afecten el historial.
- [ ] Información de versión y diagnóstico.

**Descripción:**  
Centralizará valores que no deben quedar codificados directamente y permitirá adaptar el sistema al contexto del docente.

## 15. Privacidad y seguridad local

- [ ] Inventario de datos personales tratados.
- [ ] Avisos sobre información sensible.
- [ ] Bloqueo local opcional.
- [ ] Protección de respaldos.
- [ ] Registro seguro de errores.
- [ ] Ocultar rutas, SQL y trazas en mensajes al usuario.
- [ ] Eliminación o anonimización controlada.
- [ ] Política de conservación de datos.
- [ ] Revisión de evidencias y observaciones sensibles.

**Descripción:**  
Este trabajo debe acompañar a todos los módulos, especialmente expediente, familias, incidencias y evidencias digitales.

## 16. Instalación y actualización

- [ ] Crear instalador para Windows.
- [ ] Comprobar requisitos de .NET.
- [ ] Crear accesos directos.
- [ ] Mantener datos al actualizar.
- [ ] Ejecutar migraciones de base de datos.
- [ ] Mostrar versión instalada.
- [ ] Permitir desinstalar sin borrar datos accidentalmente.
- [ ] Probar instalación en una computadora limpia.

**Descripción:**  
Convertirá la solución de desarrollo en una aplicación que pueda instalarse y usarse de forma cotidiana.

## 17. Accesibilidad y calidad de interfaz

- [ ] Navegación completa por teclado.
- [ ] Orden de tabulación.
- [ ] Etiquetas accesibles.
- [ ] Contraste suficiente.
- [ ] No depender únicamente del color.
- [ ] Escalado de Windows.
- [ ] Resoluciones pequeñas.
- [ ] Mensajes de error claros.
- [ ] Estados de carga y guardado visibles.
- [ ] Pruebas con listas de 40 estudiantes.

**Descripción:**  
No es un módulo aislado, sino una línea de trabajo transversal para asegurar que todas las pantallas sean claras, rápidas y utilizables.

---

# Orden de desarrollo recomendado

1. [ ] Cerrar formalmente Proyectos, Actividades y Entregas.
2. [ ] Planeación didáctica NEM.
3. [ ] Evaluación formativa.
4. [ ] Calificaciones y periodos.
5. [ ] Expediente y seguimiento individual.
6. [ ] Diario docente.
7. [ ] Comunicación con familias.
8. [ ] Reportes.
9. [ ] Respaldos y restauración.
10. [ ] Importación y exportación.
11. [ ] Evidencias digitales.
12. [ ] Calendario y agenda.
13. [ ] Configuración, privacidad e instalación.

---

# Criterio para considerar terminado cada módulo

Un módulo se considera terminado cuando:

- [ ] La especificación OpenSpec está completa.
- [ ] El diseño no deja decisiones abiertas.
- [ ] Core contiene únicamente reglas de dominio.
- [ ] Application contiene casos de uso y contratos específicos.
- [ ] Data persiste sin filtrar SQLite a otras capas.
- [ ] Presentation no depende de WPF ni Data.
- [ ] La interfaz WPF es usable.
- [ ] Existen pruebas automatizadas suficientes.
- [ ] `dotnet format` pasa.
- [ ] `dotnet build` pasa sin errores ni advertencias.
- [ ] `dotnet test` pasa.
- [ ] `openspec validate --all` pasa.
- [ ] La prueba manual fue completada.
- [ ] La arquitectura fue actualizada cuando corresponde.
- [ ] El cambio fue auditado.
- [ ] OpenSpec fue archivado.
- [ ] La rama fue fusionada con `main`.
