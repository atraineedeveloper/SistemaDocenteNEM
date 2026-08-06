## 1. Core Layer (`SistemaDocente.Core`)
- [ ] 1.1 Crear enum `GeneroEstudiante`.
- [ ] 1.2 Extender la entidad `Estudiante` con apellidos, nombres, fecha de nacimiento, género, CURP, fecha de ingreso y observaciones.

## 2. Data & SQLite Layer (`SistemaDocente.Data`)
- [ ] 2.1 Actualizar `EsquemaSqlite.cs` con migración `user_version = 6`.
- [ ] 2.2 Actualizar `PersistenciaGrupoSqlite.cs` para guardar y cargar los campos extendidos del estudiante.

## 3. Presentation & WPF Layer (`SistemaDocente.Presentation` & `SistemaDocente.App.Wpf`)
- [ ] 3.1 Actualizar ViewModel y diálogos de creación/edición de estudiantes (`AgregarEstudianteWindow` / `EditarEstudianteWindow` o diálogo).
- [ ] 3.2 Mostrar la información extendida y edad en la ventana del expediente (`ExpedienteEstudianteWindow`).
- [ ] 3.3 Configurar la fuente **Montserrat** globalmente en `App.xaml`.

## 4. Testing & Verification
- [ ] 4.1 Añadir pruebas unitarias para campos extendidos y migración v5->v6.
- [ ] 4.2 Ejecutar `dotnet test` y `openspec validate --all`.
