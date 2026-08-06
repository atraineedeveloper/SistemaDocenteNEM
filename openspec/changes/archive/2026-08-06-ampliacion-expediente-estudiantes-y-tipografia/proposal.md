# Proposal: Ampliación de Información de Alumnos y Tipografía Montserrat

## Why
Para un expediente escolar completo y profesional alineado con los estándares del sistema educativo y de la NEM, es necesario enriquecer los datos de identificación del estudiante con su estructura de apellidos, fecha de nacimiento, género/sexo, fecha de ingreso y observaciones particulares (sin incluir la CURP). Asimismo, se requiere unificar la tipografía visual de toda la aplicación WPF a **Montserrat** para ofrecer una experiencia estética premium y moderna.

## What Changes
1. **Dominio (`SistemaDocente.Core`)**:
   - Extender la entidad `Estudiante` con: `PrimerApellido`, `SegundoApellido`, `Nombres`, `FechaNacimiento`, `Genero` (`Hombre`, `Mujer`, `NoEspecificado`), `FechaIngreso` y `Observaciones`.
   - Mantener la inmutabilidad y validaciones cualitativas (incluyendo `ValidadorContenidoPedagogico` para `Observaciones`).
2. **Persistencia (`SistemaDocente.Data`)**:
   - Incrementar la versión de la base SQLite a `user_version = 6`.
   - Migrar la tabla `estudiantes` para almacenar los nuevos campos sin perder la información preexistente.
3. **Presentación & WPF (`SistemaDocente.Presentation` & `SistemaDocente.App.Wpf`)**:
   - Actualizar los formularios de captura y edición de estudiantes para incorporar los campos estructurados (apellidos, nombres, fecha de nacimiento, género, fecha de ingreso, observaciones).
   - Presentar los datos ampliados en la ficha del expediente individual (`ExpedienteEstudianteWindow`).
   - Aplicar el estilo tipográfico global a **Montserrat** en los recursos globales de `App.xaml`.

## Capabilities
- Permite capturar y consultar nombre desglosado por apellidos, fecha de nacimiento, edad calculada, sexo/género, fecha de ingreso y observaciones.
- Aplica tipografía **Montserrat** uniformemente en toda la interfaz WPF de la aplicación.
