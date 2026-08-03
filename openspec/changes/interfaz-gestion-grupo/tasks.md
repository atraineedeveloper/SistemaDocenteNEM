## 1. Crear Presentation y el grafo aprobado

- [x] 1.1 Crear `src/SistemaDocente.Presentation` para `net10.0` con referencia únicamente a Application
- [x] 1.2 Crear `tests/SistemaDocente.Presentation.Tests` con referencias únicamente a Presentation y Application
- [x] 1.3 Actualizar App.Wpf para referenciar Presentation, Application y Data sin añadir `Microsoft.Data.Sqlite`
- [x] 1.4 Agregar ambos proyectos a la solución y verificar que no existen ciclos

## 2. Implementar MVVM mínimo

- [x] 2.1 Implementar `ViewModelBase` con notificación de propiedades
- [x] 2.2 Implementar `RelayCommand` con ejecución, `CanExecute` y notificación de disponibilidad
- [x] 2.3 Definir servicios abstractos de mensajes, confirmación y app-state sin WPF, Data ni SQLite
- [x] 2.4 Definir ViewModels y modelos visuales mínimos sin infraestructura genérica adicional

## 3. Implementar app-state del único grupo

- [x] 3.1 Resolver y modelar el archivo `%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json` con únicamente `GrupoId`
- [x] 3.2 Implementar lectura de referencia ausente, válida, vacía, inválida y dañada
- [x] 3.3 Implementar escritura atómica mediante temporal y reemplazo en el mismo directorio
- [x] 3.4 Guardar la referencia sólo después de crear el grupo exitosamente
- [x] 3.5 Implementar olvido explícito de una referencia huérfana sin tocar SQLite
- [x] 3.6 Probar ausencia, corrupción, identidad inválida, grupo inexistente, escritura exitosa y ausencia de escritura tras fallo

## 4. Configurar la raíz de composición

- [x] 4.1 Resolver LocalApplicationData y las rutas completas de SQLite y app-state
- [x] 4.2 Construir `PersistenciaGrupoSqlite`, `GestionGrupoCasosUso`, app-state, servicios WPF y ViewModels manualmente
- [x] 4.3 Entregar el ViewModel a MainWindow y limitar Data a esta raíz
- [x] 4.4 Manejar fallos de composición con mensaje general sin rutas, trazas ni excepciones internas

## 5. Implementar bienvenida y carga

- [x] 5.1 Implementar en MainWindow el panel de bienvenida con nombre y creación sin IDs
- [x] 5.2 Mostrar bienvenida cuando no exista referencia o sea inválida
- [x] 5.3 Cargar automáticamente el grupo con referencia válida
- [x] 5.4 Mostrar inconsistencia y permitir olvidar referencia cuando el grupo no exista
- [x] 5.5 Probar primera apertura, creación válida e inválida, carga y referencia huérfana

## 6. Implementar gestión en MainWindow

- [x] 6.1 Implementar panel de gestión y panel integrado para cambiar el nombre del grupo
- [x] 6.2 Implementar DataGrid de sólo lectura con número, nombre y estado, selección de fila y columnas explícitas sin IDs
- [x] 6.3 Deshabilitar orden de encabezados que altere la secuencia recibida
- [x] 6.4 Diferenciar inactivos mediante texto y estilo, no sólo color
- [x] 6.5 Ubicar acciones principales fuera de cada fila

## 7. Implementar edición de estudiantes

- [x] 7.1 Implementar panel integrado para agregar estudiante
- [x] 7.2 Implementar panel integrado para renombrar y cambiar número del estudiante seleccionado
- [x] 7.3 Conservar entradas y mostrar validaciones o conflictos junto al panel
- [x] 7.4 Implementar cancelación que descarte edición sin invocar Application
- [x] 7.5 Implementar confirmación pequeña antes de desactivar y reactivación directa del inactivo seleccionado
- [x] 7.6 Actualizar lista y encabezado sólo con resultados confirmados por Application

## 8. Implementar teclado, ocupado y errores

- [x] 8.1 Definir orden de Tab y foco inicial de cada panel mediante XAML o code-behind puramente visual
- [x] 8.2 Implementar Enter para confirmar y Escape para cancelar y volver al estado anterior
- [x] 8.3 Implementar `EstaOcupado`, bloqueo mediante `CanExecute` y restauración en `finally`
- [x] 8.4 Confirmar que no se usan `async`, `Task.Run` ni `CancellationToken` y documentar la limitación de renderizado del indicador
- [x] 8.5 Traducir validación, conflicto, ausencia y persistencia a mensajes seguros sin SQL, rutas, `InnerException` ni trazas
- [x] 8.6 Conservar el último snapshot confirmado ante cualquier error

## 9. Probar y verificar

- [x] 9.1 Probar creación, carga, alta, edición, renombrado de grupo, desactivación y reactivación
- [x] 9.2 Probar entradas conservadas, cancelación de edición y confirmación o cancelación de desactivación
- [x] 9.3 Probar fallo de persistencia sin actualización visual falsa, bloqueo con `EstaOcupado`, orden recibido y ausencia de IDs visibles
- [x] 9.4 Verificar que Presentation.Tests no referencia ni carga WPF, Data o SQLite y que Data sólo se usa en la raíz de composición
- [x] 9.5 Ejecutar `dotnet restore`, `dotnet format --verify-no-changes`, `dotnet build`, `dotnet test` y una comprobación manual de arranque, teclado y cierre de App.Wpf

## 10. Confirmar alcance

- [x] 10.1 Confirmar que Core, esquema SQLite y reglas de Application no fueron modificados
- [x] 10.2 Confirmar que no se añadieron toolkits MVVM, DI, navegación, ventanas modales de edición, async ni infraestructura genérica excesiva
- [x] 10.3 Confirmar que no se añadieron asistencia, actividades, evaluación, reportes, importación, múltiples grupos, múltiples usuarios, sincronización, instalador, actualización automática ni datos personales adicionales

## 11. Corregir regresión de edición atómica

- [x] 11.1 Exponer `EditarEstudiante` en el puerto de Presentation y delegarlo al caso de uso de Application
- [x] 11.2 Sustituir las llamadas sucesivas de renombrado y cambio de número por una única edición atómica
- [x] 11.3 Probar edición exitosa con una sola operación de Application
- [x] 11.4 Probar conflicto de número conservando snapshot, entradas y panel sin invocar comandos independientes
