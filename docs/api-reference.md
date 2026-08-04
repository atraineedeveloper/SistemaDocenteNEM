# Documentación de API y Clases Principales

## Visión General

Este documento describe las clases y componentes principales del sistema por capa arquitectónica.

---

## Capa Core (`SistemaDocente.Core`)

### Agregados de Dominio

#### `Grupo`
Agregado raíz que representa un grupo escolar con sus estudiantes.

**Propiedades:**
- `Id : GrupoId` - Identidad única del grupo
- `NombreVisible : string` - Nombre del grupo (máx. 100 caracteres)
- `Estudiantes : IReadOnlyList<Estudiante>` - Lista de solo lectura de todos los estudiantes
- `EstudiantesActivos : IReadOnlyList<Estudiante>` - Estudiantes activos ordenados por número de lista

**Métodos estáticos:**
- `Crear(string nombreVisible) : Grupo` - Crea un nuevo grupo con identidad automática
- `Rehidratar(GrupoId id, string nombreVisible, IReadOnlyCollection<DatosEstudianteRehidratado> estudiantes) : Grupo` - Reconstruye un grupo desde persistencia

**Métodos de instancia:**
- `AgregarEstudiante(string nombreVisible, int numeroLista) : Estudiante` - Agrega estudiante con número de lista único
- `RenombrarEstudiante(EstudianteId estudianteId, string nombreVisible)` - Cambia nombre de estudiante existente
- `CambiarNumeroLista(EstudianteId estudianteId, int numeroLista)` - Modifica número de lista
- `DesactivarEstudiante(EstudianteId estudianteId)` - Marca estudiante como inactivo
- `ReactivarEstudiante(EstudianteId estudianteId)` - Reactiva estudiante inactivo
- `Renombrar(string nombreVisible)` - Cambia nombre del grupo

**Invariantes:**
- Números de lista únicos para estudiantes activos
- Identidades de estudiantes únicas dentro del grupo
- Nombres normalizados (trim + validación de longitud)

---

#### `AsistenciaDiaria`
Agregado que representa la asistencia de un día específico para un grupo.

**Propiedades:**
- `GrupoId : GrupoId` - Referencia al grupo
- `Fecha : DateOnly` - Fecha de la asistencia (identidad natural junto con GrupoId)
- `Registros : IReadOnlyList<RegistroAsistencia>` - Registros de asistencia por estudiante

**Métodos estáticos:**
- `Crear(GrupoId grupoId, DateOnly fecha, IReadOnlyCollection<EstadoEstudianteAsistencia> estados) : AsistenciaDiaria` - Crea nueva asistencia con estados iniciales
- `Rehidratar(GrupoId grupoId, DateOnly fecha, IReadOnlyCollection<DatosRegistroAsistenciaRehidratado> registros) : AsistenciaDiaria` - Reconstruye desde persistencia

**Métodos de instancia:**
- `CambiarEstado(EstudianteId estudianteId, EstadoAsistencia estado)` - Modifica estado de asistencia de un estudiante

**Invariantes:**
- Máximo un registro por estudiante
- Único estado válido por registro
- Atomicidad: todos los registros se guardan o revierten juntos

---

#### `ProyectoDidactico`
Agregado que representa un proyecto didáctico con periodo y estado.

**Propiedades:**
- `Id : ProyectoId` - Identidad única del proyecto
- `GrupoId : GrupoId` - Grupo propietario (inmutable)
- `Nombre : string` - Nombre del proyecto (máx. 150 caracteres)
- `Descripcion : string` - Descripción (máx. 2000 caracteres)
- `FechaInicio : DateOnly` - Inicio del periodo
- `FechaTermino : DateOnly` - Fin del periodo (debe ser >= FechaInicio)
- `Estado : EstadoProyecto` - Estado actual (Borrador, EnCurso, Finalizado)
- `Observaciones : string` - Observaciones (máx. 2000 caracteres)
- `Version : int` - Versión para concurrencia optimista

**Métodos estáticos:**
- `Crear(GrupoId grupoId, string nombre, DateOnly fechaInicio, DateOnly fechaTermino) : ProyectoDidactico` - Crea proyecto en estado Borrador
- `Rehidratar(...)` - Reconstruye desde persistencia validando invariantes

**Métodos de instancia:**
- `Iniciar()` - Transición Borrador → EnCurso
- `Finalizar()` - Transición EnCurso → Finalizado
- `Reabrir()` - Transición Finalizado → EnCurso
- `ActualizarDatos(string nombre, string descripcion, string observaciones)` - Modifica datos básicos
- `ActualizarPeriodo(DateOnly fechaInicio, DateOnly fechaTermino)` - Modifica periodo (valida compatibilidad)

**Transiciones de estado válidas:**
```
Borrador → EnCurso
EnCurso → Finalizado
Finalizado → EnCurso
```

---

#### `ActividadProyecto`
Agregado que representa una actividad dentro de un proyecto didáctico.

**Propiedades:**
- `Id : ActividadId` - Identidad única de la actividad
- `ProyectoId : ProyectoId` - Proyecto padre (inmutable)
- `GrupoId : GrupoId` - Grupo propietario (inmutable)
- `Titulo : string` - Título de la actividad (máx. 150 caracteres)
- `Descripcion : string` - Descripción (máx. 2000 caracteres)
- `FechaLimite : DateOnly` - Fecha límite de entrega (debe estar dentro del periodo del proyecto)
- `Estado : EstadoActividad` - Estado (Pendiente, EnProceso, Completada, Anulada)
- `Version : int` - Versión para concurrencia optimista
- `Entregas : IReadOnlyList<EntregaActividad>` - Entregas por estudiante

**Métodos estáticos:**
- `Crear(ProyectoId proyectoId, GrupoId grupoId, string titulo, DateOnly fechaLimite) : ActividadProyecto` - Crea actividad con padrón completo
- `Rehidratar(...)` - Reconstruye desde persistencia

**Métodos de instancia:**
- `ActualizarDatos(string titulo, string descripcion, DateOnly fechaLimite)` - Modifica datos
- `RegistrarEntrega(EstudianteId estudianteId, EstadoEntrega estado, string? observacion)` - Registra o actualiza entrega
- `Anular()` - Marca actividad como anulada (irreversible)

**Invariantes:**
- Una entrega por estudiante como máximo
- Fecha límite dentro del periodo del proyecto padre
- No se puede modificar si está anulada
- Estados explícitos: Pendiente, Entregada, NoEntregada

---

### Entidades de Valor

#### `Estudiante`
Entidad dentro del agregado Grupo.

**Propiedades:**
- `Id : EstudianteId` - Identidad única
- `NombreVisible : string` - Nombre (máx. 150 caracteres)
- `NumeroLista : int` - Número de lista (> 0)
- `EstaActivo : bool` - Estado de activación

---

#### `RegistroAsistencia`
Entidad dentro del agregado AsistenciaDiaria.

**Propiedades:**
- `EstudianteId : EstudianteId` - Referencia al estudiante
- `Estado : EstadoAsistencia` - Estado (Presente, Ausente, Justificada, Tardanza)

**Métodos:**
- `CambiarEstado(EstadoAsistencia estado)` - Modifica estado atómicamente

---

#### `EntregaActividad`
Entidad dentro del agregado ActividadProyecto.

**Propiedades:**
- `EstudianteId : EstudianteId` - Referencia al estudiante
- `Estado : EstadoEntrega` - Estado (Pendiente, Entregada, NoEntregada)
- `Observacion : string?` - Observación opcional (máx. 500 caracteres)

---

### Identidades Fuertes

| Tipo | Descripción |
|------|-------------|
| `GrupoId` | Identidad de grupo (wrapper de GUID) |
| `EstudianteId` | Identidad de estudiante (wrapper de GUID) |
| `ProyectoId` | Identidad de proyecto (wrapper de GUID) |
| `ActividadId` | Identidad de actividad (wrapper de GUID) |

---

### Excepciones de Dominio

| Excepción | Uso |
|-----------|-----|
| `DomainValidationException` | Violación de invariantes o reglas de validación |
| `DomainConflictException` | Conflicto de estado o inconsistencia lógica |

---

## Capa Application (`SistemaDocente.Application`)

### Casos de Uso

#### `GestionGrupoCasosUso`
Coordina operaciones de gestión de grupos y estudiantes.

**Dependencias:**
- `IAlmacenamientoGrupos` - Puerto de persistencia de grupos

**Métodos públicos:**
| Método | Descripción | Retorna |
|--------|-------------|---------|
| `CrearGrupo(string nombreVisible)` | Crea nuevo grupo | `GrupoDetalle` |
| `CargarGrupo(GrupoId grupoId)` | Carga grupo existente | `GrupoDetalle` |
| `Existe(GrupoId grupoId)` | Verifica existencia | `bool` |
| `CambiarNombreGrupo(GrupoId, string)` | Renombra grupo | `GrupoDetalle` |
| `AgregarEstudiante(GrupoId, string, int)` | Agrega estudiante | `EstudianteDetalle` |
| `RenombrarEstudiante(GrupoId, EstudianteId, string)` | Cambia nombre estudiante | `EstudianteDetalle` |
| `CambiarNumeroLista(GrupoId, EstudianteId, int)` | Cambia número de lista | `EstudianteDetalle` |
| `EditarEstudiante(GrupoId, EstudianteId, string, int)` | Edita nombre y número | `EstudianteDetalle` |
| `DesactivarEstudiante(GrupoId, EstudianteId)` | Desactiva estudiante | `EstudianteDetalle` |
| `ReactivarEstudiante(GrupoId, EstudianteId)` | Reactiva estudiante | `EstudianteDetalle` |
| `ObtenerTodosLosEstudiantes(GrupoId)` | Lista todos los estudiantes | `IReadOnlyList<EstudianteDetalle>` |

---

#### `GestionAsistenciaCasosUso`
Coordina operaciones de asistencia diaria y mensual.

**Dependencias:**
- `IAlmacenamientoGrupos` - Puerto de persistencia de grupos
- `IAlmacenamientoAsistencias` - Puerto de persistencia de asistencias
- `ICalendarioLectivo` - Servicio de calendario (default: Lunes a Viernes)

**Métodos públicos:**
| Método | Descripción | Retorna |
|--------|-------------|---------|
| `Cargar(GrupoId, DateOnly)` | Carga asistencia existente | `AsistenciaDiaDetalle?` |
| `Preparar(GrupoId, DateOnly)` | Prepara asistencia nueva o existente | `AsistenciaDiaDetalle` |
| `Existe(GrupoId, DateOnly)` | Verifica existencia de asistencia | `bool` |
| `CargarMes(GrupoId, int anio, int mes)` | Carga proyección mensual | `AsistenciaMesDetalle` |
| `GuardarDia(GrupoId, DateOnly, IEnumerable<EntradaEstadoAsistencia>)` | Guarda asistencia de un día | `void` |
| `GuardarVariosDias(GrupoId, IEnumerable<(DateOnly, IEnumerable<EntradaEstadoAsistencia>)>)` | Guarda múltiples días secuencialmente | `(int exitosos, DateOnly? fallido)` |

---

#### `GestionProyectosActividadesCasosUso`
Coordina operaciones de proyectos didácticos y actividades.

**Dependencias:**
- `IAlmacenamientoProyectos` - Puerto de persistencia de proyectos
- `IAlmacenamientoActividadesProyecto` - Puerto de persistencia de actividades
- `IAlmacenamientoGrupos` - Puerto de persistencia de grupos

**Métodos principales:**
- `CrearProyecto(GrupoId, string, DateOnly, DateOnly)` - Crea proyecto en borrador
- `ObtenerProyecto(ProyectoId)` - Obtiene detalle completo
- `ListarProyectosDelGrupo(GrupoId)` - Lista proyectos ordenados
- `ActualizarProyecto(ProyectoId, ...)` - Actualiza datos y periodo
- `CambiarEstadoProyecto(ProyectoId, EstadoProyecto)` - Cambia estado
- `ReabrirProyecto(ProyectoId)` - Reabre proyecto finalizado
- `EliminarProyectoBorradorSinActividades(ProyectoId)` - Elimina si es posible
- `PrepararNuevaActividad(ProyectoId)` - Prepara creación de actividad
- `CrearActividad(ProyectoId, string, DateOnly)` - Crea actividad
- `ObtenerActividad(ActividadId)` - Obtiene detalle completo
- `ListarActividadesDelProyecto(ProyectoId)` - Lista actividades
- `ActualizarActividad(ActividadId, ...)` - Actualiza datos
- `GuardarEntregasActividad(ActividadId, IEnumerable<...>)` - Guarda entregas
- `AnularActividad(ActividadId)` - Anula actividad
- `EliminarActividadSinSeguimiento(ActividadId)` - Elimina si no tiene seguimiento

---

### Puertos de Persistencia (Interfaces)

| Interfaz | Responsabilidad |
|----------|-----------------|
| `IAlmacenamientoGrupos` | CRUD de grupos y estudiantes |
| `IAlmacenamientoAsistencias` | CRUD de asistencias diarias por rango |
| `IAlmacenamientoProyectos` | CRUD versionado de proyectos |
| `IAlmacenamientoActividadesProyecto` | CRUD versionado de actividades y entregas |

---

### DTOs y Snapshots

#### Detalles (Application)
- `GrupoDetalle` - Snapshot inmutable de grupo
- `EstudianteDetalle` - Snapshot inmutable de estudiante
- `AsistenciaDiaDetalle` - Detalle de asistencia diaria
- `AsistenciaMesDetalle` - Proyección mensual completa
- `ProyectoDetalle` - Detalle completo de proyecto
- `ActividadProyectoDetalle` - Detalle completo de actividad con entregas

---

### Excepciones de Aplicación

| Excepción | Descripción |
|-----------|-------------|
| `GrupoNoEncontradoException` | Grupo solicitado no existe |
| `ErrorPersistenciaAplicacionException` | Error técnico de persistencia traducido |
| `ConflictoConcurrenciaException` | Conflicto de versión optimista |
| `GuardadoMesInterrumpidoException` | Guardado múltiple interrumpido parcialmente |
| `PeriodoProyectoIncompatibleException` | Periodo incompatible con actividades existentes |

---

## Capa Data (`SistemaDocente.Data`)

### Adaptadores SQLite

#### `PersistenciaGrupoSqlite`
Implementación de `IAlmacenamientoGrupos`.

**Responsabilidades:**
- Crear/actualizar grupos y estudiantes en SQLite
- Cargar grupo completo con estudiantes
- Consultar existencia por ID
- Manejar transacciones por agregado

---

#### `PersistenciaAsistenciaSqlite`
Implementación de `IAlmacenamientoAsistencias`.

**Responsabilidades:**
- Insertar/actualizar asistencias diarias atómicamente
- Cargar asistencia por fecha específica
- Cargar intervalo de fechas para proyección mensual
- Encabezado y registros en misma transacción

---

#### `PersistenciaProyectosSqlite`
Implementación de `IAlmacenamientoProyectos` e `IAlmacenamientoActividadesProyecto`.

**Responsabilidades:**
- CRUD de proyectos con control de versión
- CRUD de actividades con entregas atómicas
- Consultas de fechas incompatibles
- Validación de eliminación restringida

---

### Esquema y Migración

#### `EsquemaSqlite`
Clase estática que define el esquema de la base de datos.

**Versión actual:** `3`

**Tablas principales:**
- `grupos` - Grupos escolares
- `estudiantes` - Estudiantes por grupo
- `asistencias_diarias` - Encabezado de asistencia por día
- `registros_asistencia` - Registros individuales de asistencia
- `proyectos_didacticos` - Proyectos didácticos (v3)
- `actividades_proyecto` - Actividades por proyecto (v3)
- `entregas_actividad` - Entregas por actividad y estudiante (v3)

**Migraciones soportadas:**
- v1 → v2: Validación y conservación
- v2 → v3: Agrega tablas de proyectos/actividades
- Nueva base: Crea directamente en v3

---

### Excepciones de Data

| Excepción | Descripción |
|-----------|-------------|
| `DataAccessException` | Error de acceso a datos envuelto |
| `DataIntegrityException` | Violación de integridad referencial |
| `SchemaIncompatibleException` | Esquema incompatible con versión esperada |

---

## Capa Presentation (`SistemaDocente.Presentation`)

### ViewModels

#### `MainWindowViewModel`
ViewModel contenedor principal.

**Propiedades:**
- `GestionGrupoViewModel` - VM de gestión de grupo
- `GestionAsistenciaMensualViewModel` - VM de asistencia mensual
- `GestionProyectosViewModel` - VM de proyectos y actividades
- `ModuloActual` - Módulo seleccionado (Grupo, Asistencia, Proyectos)

**Métodos:**
- `SeleccionarModulo(Modulo modulo)` - Cambia módulo visible

---

#### `GestionGrupoViewModel`
Gestión de grupo y estudiantes.

**Propiedades clave:**
- `Grupo` - Detalle del grupo actual
- `Estudiantes` - Lista observable de estudiantes
- `EstudianteSeleccionado` - Estudiante seleccionado
- `TieneCambios` - Indica cambios pendientes
- `MensajeError` - Mensaje de error si existe

**Comandos:**
- `CrearGrupoCommand`
- `GuardarCambiosCommand`
- `DescartarCambiosCommand`
- `AgregarEstudianteCommand`
- `EditarEstudianteCommand`
- `DesactivarEstudianteCommand`

---

#### `GestionAsistenciaMensualViewModel`
Gestión de asistencia mensual.

**Propiedades clave:**
- `MesActual` - Mes y año visualizados
- `Dias` - Columnas de días lectivos
- `Estudiantes` - Filas de estudiantes con asistencias
- `Filtro` - Filtro de visualización
- `TotalDiasLaborables` - Conteo de días laborables
- `EstaEditando` - Indica modo edición

**Comandos:**
- `NavegarMesAnteriorCommand`
- `NavegarMesSiguienteCommand`
- `IrAMesActualCommand`
- `EditarDiaCommand`
- `GuardarDiaCommand`
- `GuardarMesCommand`

---

#### `GestionProyectosViewModel`
Gestión de proyectos y actividades.

**Propiedades clave:**
- `Proyectos` - Lista de proyectos del grupo
- `ProyectoSeleccionado` - Proyecto activo
- `Actividades` - Actividades del proyecto seleccionado
- `ActividadSeleccionada` - Actividad activa
- `Entregas` - Entregas de la actividad seleccionada
- `FiltroEntregas` - Filtro de visualización de entregas
- `ConteoPendientes`, `ConteoEntregadas`, `ConteoNoEntregadas` - Conteos

**Comandos:**
- `CrearProyectoCommand`
- `EditarProyectoCommand`
- `CambiarEstadoProyectoCommand`
- `ReabrirProyectoCommand`
- `EliminarProyectoCommand`
- `CrearActividadCommand`
- `GuardarEntregasCommand`
- `AnularActividadCommand`

---

### Infraestructura MVVM

#### `ViewModelBase`
Clase base para todos los ViewModels.

**Propiedades:**
- Implementa `INotifyPropertyChanged`

**Métodos:**
- `OnPropertyChanged(string propertyName)` - Notifica cambio de propiedad

---

#### `RelayCommand`
Implementación de `ICommand`.

**Constructor:**
- `RelayCommand(Action execute, Func<bool>? canExecute = null)`

**Métodos:**
- `Execute(object? parameter)` - Ejecuta comando
- `CanExecute(object? parameter)` - Verifica si puede ejecutar
- `NotifyCanExecuteChanged()` - Notifica cambio de CanExecute

---

### Servicios Abstractos

#### `ServiciosPresentacion`
Contratos para servicios de presentación.

**Interfaz:**
- `MostrarMensaje(string mensaje, string titulo)` - Muestra mensaje informativo
- `MostrarConfirmacion(string mensaje, string titulo)` - Solicita confirmación booleana
- `MostrarError(string mensaje, string titulo)` - Muestra error

---

## Capa App.Wpf (`SistemaDocente.App.Wpf`)

### Composición Raíz

#### `App.xaml.cs`
Punto de entrada y composición manual.

**Responsabilidades:**
- Inicializar aplicación WPF
- Crear conexión SQLite
- Instanciar adaptadores de persistencia
- Crear casos de uso
- Construir ViewModels
- Mostrar MainWindow

---

#### `MainWindow.xaml.cs`
Code-behind de ventana principal.

**Responsabilidades:**
- Enlazar DataContext con MainWindowViewModel
- Manejar navegación visual entre módulos
- Limitarse a comportamiento WPF puro (sin lógica de negocio)

---

#### `ServiciosWpf`
Implementación concreta de `ServiciosPresentacion` para WPF.

**Métodos:**
- `MostrarMensaje` - MessageBox.Show informativo
- `MostrarConfirmacion` - MessageBox.Show con Yes/No
- `MostrarError` - MessageBox.Show de error

---

#### `AlmacenamientoEstadoJson`
Persistencia mínima del estado de la aplicación.

**Archivo:** `app-state.json`

**Datos almacenados:**
- `UltimoGrupoId` - Último grupo abierto

**Nota:** Solo almacena estado de UI, no datos del dominio.

---

## Reporting (`SistemaDocente.Reporting`)

### Estado Actual

Proyecto reservado para funcionalidad de reportes futura.

**Dependencias:**
- Solo `SistemaDocente.Core`

**Funcionalidad pendiente:**
- Generación de reportes de asistencia
- Exportación de datos
- Respaldos

---

## Resumen de Dependencias

```
Core (independiente)
    ↑
Application → Core
    ↑
Presentation → Application
    ↑
App.Wpf → Presentation + Application + Data

Data → Application + Core
Reporting → Core
```

**Reglas de dependencia:**
- Core no depende de ningún otro proyecto productivo
- Data solo es consumido por App.Wpf (raíz de composición)
- Presentation es portable (no conoce WPF, Data ni SQLite)
- No existen ciclos
