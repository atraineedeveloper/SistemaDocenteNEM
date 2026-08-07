## Purpose

Consolida información de asistencia, cumplimiento, niveles de logro y seguimiento pedagógico en reportes reutilizables sin introducir rankings competitivos ni acoplar Reporting a WPF o SQLite.

## ADDED Requirements

### Requirement: Reporte individual del estudiante
El sistema SHALL generar un reporte individual con identidad del estudiante, contexto del grupo, asistencia mensual y global, cumplimiento de entregas, distribución de niveles de logro, actividades aplicables y elementos pedagógicos disponibles del expediente.

#### Scenario: Estudiante con información parcial
- **WHEN** un estudiante tiene asistencia, algunas actividades y notas pedagógicas pero no acuerdos con tutor
- **THEN** el reporte incluye la información disponible y no inventa datos para las secciones ausentes

### Requirement: Reporte grupal sin ranking competitivo
El sistema SHALL generar un reporte grupal con totales históricos y activos, asistencia agregada, cumplimiento de entregas, distribución de niveles de logro y seguimiento individual resumido. El reporte SHALL evitar ordenar o etiquetar estudiantes como ranking competitivo.

#### Scenario: Grupo con estudiantes activos e históricos
- **WHEN** se genera el reporte grupal
- **THEN** se distinguen matrícula histórica y alumnos activos y se conserva el seguimiento de estudiantes históricos aplicables

### Requirement: Cumplimiento basado en estados explícitos
El porcentaje de cumplimiento SHALL calcularse como `Entregadas / (Entregadas + NoEntregadas) * 100`. Las entregas pendientes SHALL mostrarse por separado y no SHALL entrar al denominador.

#### Scenario: Una entregada, una no entregada y una pendiente
- **WHEN** el conjunto contiene exactamente esos tres estados
- **THEN** el cumplimiento es 50 %, las pendientes se reportan como 1 y el total aplicable es 3

#### Scenario: Sin entregas decididas
- **WHEN** todas las actividades aplicables permanecen pendientes
- **THEN** el porcentaje de cumplimiento se representa como no disponible en vez de `0 %`

### Requirement: Distribución de logro excluye no entregas
La distribución de niveles de logro SHALL contabilizar resultados evaluativos de entregas aplicables y no SHALL convertir `NoEntregada` en un nivel cognitivo ni en una calificación numérica.

#### Scenario: Actividad no entregada
- **WHEN** una actividad tiene estado `NoEntregada`
- **THEN** aumenta el conteo de no entregadas y no aumenta `Domina`, `Suficiente`, `EnProceso` ni `RequiereApoyo`

### Requirement: Reporting permanece portable
`SistemaDocente.Reporting` SHALL contener modelos y cálculos puros y no SHALL depender de WPF ni acceder directamente a SQLite. Application SHALL coordinar las fuentes y entregar snapshots a Reporting.

#### Scenario: Calcular un reporte en prueba unitaria
- **WHEN** se construyen fuentes de reporte en memoria
- **THEN** el generador produce métricas sin crear ventanas, conexiones SQLite ni adaptadores de infraestructura