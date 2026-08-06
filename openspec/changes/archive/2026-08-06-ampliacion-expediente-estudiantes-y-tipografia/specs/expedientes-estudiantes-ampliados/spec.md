# expedientes-estudiantes-ampliados Specification

## ADDED Requirements

### Requirement: Registro Estructurado del Estudiante
La entidad estudiante MUST almacenar primer apellido, segundo apellido, nombres, fecha de nacimiento, sexo/género, fecha de ingreso y observaciones pedagógicas particulares.

#### Scenario: Creación de estudiante con datos estructurados
- **WHEN** un docente registra a un nuevo alumno ingresando sus apellidos, nombres, fecha de nacimiento y género
- **THEN** el sistema guarda la información desglosada y valida que las observaciones pedagógicas no contengan diagnósticos clínicos.

#### Scenario: Presentación de edad y datos en el expediente
- **WHEN** el docente consulta la ficha de expediente de un estudiante
- **THEN** el sistema calcula la edad actual en años basándose en la fecha de nacimiento y muestra el perfil completo estructurado.

### Requirement: Tipografía Global Montserrat
Toda la interfaz visual WPF del sistema MUST emplear la familia tipográfica **Montserrat** (con caída a Segoe UI / sans-serif).

#### Scenario: Aplicación uniforme de tipografía Montserrat
- **WHEN** el usuario navega por las ventanas y controles de la aplicación WPF
- **THEN** los controles tipográficos se renderizan utilizando la fuente Montserrat.
