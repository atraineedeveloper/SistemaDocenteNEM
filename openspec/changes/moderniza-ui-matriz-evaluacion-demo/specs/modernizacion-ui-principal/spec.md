## Purpose

Unifica la jerarquía y las acciones de las vistas principales sin duplicar la navegación ni introducir dependencias UI adicionales.

## ADDED Requirements

### Requirement: Navegación global única
La aplicación SHALL conservar una sola navegación global superior para Grupo, Asistencia, Proyectos y Evaluación y no SHALL introducir una barra lateral permanente duplicada.

#### Scenario: Cambiar de módulo
- **WHEN** el usuario navega entre módulos principales
- **THEN** usa el mismo encabezado global y sólo cambia la superficie principal de contenido

### Requirement: Jerarquía visual consistente
Grupo, Asistencia, Proyectos y Evaluación SHALL usar encabezados, tarjetas, herramientas, tablas y barras de acciones coherentes con los recursos semánticos compartidos de la aplicación.

#### Scenario: Cambiar tema
- **WHEN** cambia el tema visual soportado
- **THEN** las vistas principales resuelven colores mediante recursos semánticos y conservan contraste y legibilidad

### Requirement: Acción primaria reconocible
Cada vista principal SHALL distinguir su acción primaria de acciones secundarias sin convertir todas las operaciones en botones de igual énfasis.

#### Scenario: Vista Grupo
- **WHEN** se muestra la lista de estudiantes
- **THEN** `Agregar estudiante` se presenta como acción primaria y las operaciones de edición o expediente permanecen secundarias

### Requirement: Densidad operativa preservada
La modernización visual SHALL conservar la densidad necesaria en superficies de captura como Asistencia y Evaluación, incluyendo virtualización y desplazamiento controlado por las grillas.

#### Scenario: Captura con grupo numeroso
- **WHEN** la tabla contiene decenas de estudiantes
- **THEN** la interfaz conserva filas legibles, controles accesibles y desplazamiento sin envolver la grilla en un `ScrollViewer` externo no acotado