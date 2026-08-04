# Guía de Desarrollo

## Introducción

Esta guía está dirigida a desarrolladores que deseen contribuir al proyecto **Sistema Docente Local**. Describe el flujo de trabajo, convenciones, herramientas y procesos necesarios para desarrollar en este código base.

---

## Configuración del Entorno

### Requisitos Previos

1. **.NET 10 SDK** - Requerido para compilar y ejecutar
2. **Git** - Control de versiones
3. **IDE recomendado**: Visual Studio 2022+, JetBrains Rider, o VS Code con extensión C#
4. **SQLite** - Incluido vía NuGet (Microsoft.Data.Sqlite)

### Primeros Pasos

```bash
# Clonar repositorio
git clone <url-del-repositorio>
cd SistemaDocenteLocal

# Restaurar dependencias
dotnet restore SistemaDocente.sln

# Compilar solución
dotnet build SistemaDocente.sln

# Ejecutar pruebas
dotnet test SistemaDocente.sln

# Ejecutar aplicación WPF (Windows)
dotnet run --project src/SistemaDocente.App.Wpf
```

---

## Arquitectura del Proyecto

### Estructura de Directorios

```
/workspace
├── src/                          # Código fuente productivo
│   ├── SistemaDocente.Core/      # Dominio (entidades, agregados, reglas)
│   ├── SistemaDocente.Application/ # Casos de uso y puertos
│   ├── SistemaDocente.Data/      # Persistencia SQLite
│   ├── SistemaDocente.Presentation/ # MVVM portable
│   ├── SistemaDocente.Reporting/ # Reportes (pendiente)
│   └── SistemaDocente.App.Wpf/   # Interfaz WPF y composición
├── tests/                        # Proyectos de prueba
│   ├── SistemaDocente.Core.Tests/
│   ├── SistemaDocente.Application.Tests/
│   ├── SistemaDocente.Data.Tests/
│   ├── SistemaDocente.Presentation.Tests/
│   └── SistemaDocente.App.Wpf.Tests/
├── openspec/                     # Especificaciones OpenSpec
│   ├── specs/                    # Especificaciones aprobadas
│   └── changes/                  # Cambios propuestos/archivados
├── docs/                         # Documentación
└── SistemaDocente.sln            # Solución principal
```

### Capas Arquitectónicas

| Capa | Proyecto | Responsabilidad | Dependencias |
|------|----------|-----------------|--------------|
| **Dominio** | Core | Entidades, agregados, invariantes | Ninguna |
| **Aplicación** | Application | Casos de uso, puertos | Core |
| **Infraestructura** | Data | Adaptadores SQLite | Application + Core |
| **Presentación** | Presentation | ViewModels, MVVM | Application |
| **Interfaz** | App.Wpf | Vistas WPF, composición | Presentation + Application + Data |
| **Reportes** | Reporting | Generación de reportes | Core |

---

## Flujo de Trabajo OpenSpec

El proyecto utiliza **desarrollo guiado por especificaciones** mediante OpenSpec.

### Ciclo de Vida de un Cambio

1. **Explorar necesidad** - Identificar requerimiento
2. **Crear propuesta** (`proposal.md`) - Describir el cambio propuesto
3. **Definir especificación** (`spec.md`) - Requisitos detallados
4. **Diseñar solución** (`design.md`) - Enfoque técnico
5. **Listar tareas** (`tasks.md`) - Checklist implementable
6. **Aprobar** - Revisión y validación
7. **Implementar** - Codificación siguiendo tasks.md
8. **Validar** - Ejecutar `openspec validate --all`
9. **Archivar** - Mover a `archive/` tras completar

### Estructura de un Cambio

```
openspec/changes/<nombre-del-cambio>/
├── proposal.md          # Propuesta inicial
├── design.md            # Diseño técnico
├── tasks.md             # Lista de tareas verificables
└── specs/               # Especificaciones relacionadas
    └── <nombre-spec>/
        └── spec.md      # Especificación detallada
```

### Comandos OpenSpec

```bash
# Validar todas las especificaciones
openspec validate --all

# Validar un cambio específico
openspec validate openspec/changes/<nombre-del-cambio>
```

---

## Convenciones de Código

### Estilo y Formato

El proyecto incluye un archivo `.editorconfig` que define:

- Indentación: 4 espacios
- Longitud máxima de línea: según criterio
- Convenciones de nomenclatura .NET estándar
- Uso implícito de tipos donde sea claro

**Comando de formato:**
```bash
dotnet format SistemaDocente.sln --verify-no-changes
```

### Nomenclatura

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Clases | PascalCase | `Grupo`, `AsistenciaDiaria` |
| Métodos | PascalCase | `AgregarEstudiante`, `Guardar` |
| Propiedades | PascalCase | `NombreVisible`, `Id` |
| Campos privados | _camelCase | `_estudiantes`, `_almacenamiento` |
| Parámetros | camelCase | `grupoId`, `nombreVisible` |
| Interfaces | I-PascalCase | `IAlmacenamientoGrupos` |
| Excepciones | *Exception | `DomainValidationException` |

### Principios de Diseño

1. **Inmutabilidad por defecto** - Colecciones expuestas como `IReadOnlyList<T>`
2. **Identidades fuertes** - Wrappers tipados para IDs (`GrupoId`, `EstudianteId`)
3. **Agregados explícitos** - Límites claros de transacción
4. **Excepciones de dominio** - Validación temprana con mensajes claros
5. **Sin dependencias cíclicas** - Verificado por pruebas de auditoría

---

## Estrategia de Pruebas

### Tipos de Pruebas

| Tipo | Ubicación | Herramienta | Cobertura |
|------|-----------|-------------|-----------|
| **Unitarias - Dominio** | Core.Tests | xUnit | Invariantes, mutaciones |
| **Unitarias - Application** | Application.Tests | xUnit + dobles manuales | Casos de uso |
| **Integración - Data** | Data.Tests | xUnit + SQLite real | Persistencia, migración |
| **Unitarias - Presentation** | Presentation.Tests | xUnit | ViewModels, comandos |
| **Composición** | App.Wpf.Tests | xUnit | Ensamblado, navegación |
| **Auditoría** | Varios | Scripts personalizados | Referencias, arquitectura |

### Ejecutar Pruebas

```bash
# Todas las pruebas
dotnet test SistemaDocente.sln

# Proyecto específico
dotnet test tests/SistemaDocente.Core.Tests

# Con cobertura (requiere coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Filtrar por nombre
dotnet test --filter "FullyQualifiedName~Grupo"
```

### Dobles Manuales

El proyecto NO usa frameworks de mocking. Los dobles se implementan manualmente:

```csharp
// Ejemplo: Doble manual de IAlmacenamientoGrupos
public class AlmacenamientoGruposEnMemoria : IAlmacenamientoGrupos
{
    private readonly Dictionary<GrupoId, Grupo> _grupos = new();
    
    public void Guardar(Grupo grupo) => _grupos[grupo.Id] = grupo;
    
    public Grupo? Cargar(GrupoId grupoId) 
        => _grupos.TryGetValue(grupoId, out var g) ? g : null;
    
    // ... demás métodos
}
```

---

## Desarrollo de Características

### Agregar Nueva Funcionalidad

1. **Crear especificación** en `openspec/specs/` o `openspec/changes/`
2. **Definir tareas** en `tasks.md`
3. **Implementar en orden**:
   - Dominio (Core) primero
   - Casos de uso (Application)
   - Persistencia (Data) si aplica
   - Presentación (Presentation)
   - Integración Wpf (App.Wpf)
4. **Escribir pruebas** para cada capa
5. **Validar** con `openspec validate`
6. **Verificar formato** con `dotnet format`

### Ejemplo: Agregar Nuevo Estado

```csharp
// 1. Definir en Core
public enum EstadoPersonalizado
{
    Opcion1 = 0,
    Opcion2 = 1,
    Opcion3 = 2
}

// 2. Agregar validación en DomainValidationException si es inválido
// 3. Actualizar casos de uso en Application
// 4. Persistir en Data (agregar CHECK constraint en SQLite)
// 5. Mostrar en Presentation (ViewModel)
// 6. Enlazar en App.Wpf (XAML)
```

---

## Persistencia SQLite

### Esquema Actual

**Versión:** 3

**Tablas principales:**
- `grupos` - Grupos escolares
- `estudiantes` - Estudiantes con número de lista
- `asistencias_diarias` - Encabezado por fecha
- `registros_asistencia` - Registros individuales
- `proyectos_didacticos` - Proyectos (v3)
- `actividades_proyecto` - Actividades (v3)
- `entregas_actividad` - Entregas (v3)

### Migraciones

Las migraciones son manuales y se definen en `EsquemaSqlite.cs`:

```csharp
internal const int VersionActual = 3;

// Nueva base → v3 directa
// v1 → v2 → v3: migración secuencial con validación
```

**Reglas:**
- Cada migración es transaccional
- `user_version` solo cambia al completar
- Fallo intermedio → rollback completo
- No hay migración hacia atrás

### Acceder a Datos

```csharp
// Patrón: una conexión por operación
using var conexion = new SqliteConnection("Data Source=archivo.db");
conexion.Open();
conexion.Execute("PRAGMA foreign_keys = ON");

using var transaccion = conexion.BeginTransaction();
try
{
    // Operaciones...
    transaccion.Commit();
}
catch
{
    transaccion.Rollback();
    throw;
}
```

---

## Presentación MVVM

### ViewModel Base

Todos los ViewModels heredan de `ViewModelBase`:

```csharp
public class MiViewModel : ViewModelBase
{
    private string _titulo;
    
    public string Titulo
    {
        get => _titulo;
        set
        {
            _titulo = value;
            OnPropertyChanged();
        }
    }
}
```

### Comandos

Usar `RelayCommand` para acciones:

```csharp
public ICommand GuardarCommand { get; }

public MiViewModel()
{
    GuardarCommand = new RelayCommand(
        execute: Guardar,
        canExecute: () => TieneCambios);
}

private void Guardar()
{
    // Lógica de guardado
}
```

### Servicios de Presentación

Los servicios abstractos permiten testing sin WPF:

```csharp
public interface IServiciosPresentacion
{
    void MostrarMensaje(string mensaje, string titulo);
    bool MostrarConfirmacion(string mensaje, string titulo);
    void MostrarError(string mensaje, string titulo);
}
```

---

## Composición Manual (App.Wpf)

No hay contenedor de inyección de dependencias. La composición es manual en `App.xaml.cs`:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    // 1. Conexión SQLite
    var ruta = RutasAplicacion.ObtenerRutaBaseDeDatos();
    var conexion = new SqliteConnection($"Data Source={ruta}");
    
    // 2. Adaptadores
    var almacenamientoGrupos = new PersistenciaGrupoSqlite(conexion);
    var almacenamientoAsistencias = new PersistenciaAsistenciaSqlite(conexion);
    
    // 3. Casos de uso
    var gestionGrupo = new GestionGrupoCasosUso(almacenamientoGrupos);
    var gestionAsistencia = new GestionAsistenciaCasosUso(...);
    
    // 4. Servicios de presentación
    IServiciosPresentacion serviciosWpf = new ServiciosWpf();
    
    // 5. ViewModels
    var mainWindowVm = new MainWindowViewModel(gestionGrupo, gestionAsistencia, ...);
    
    // 6. MainWindow
    var window = new MainWindow { DataContext = mainWindowVm };
    window.Show();
}
```

---

## Verificación y Calidad

### Antes de Commit

```bash
# 1. Restaurar
dotnet restore

# 2. Formatear
dotnet format SistemaDocente.sln --verify-no-changes

# 3. Compilar
dotnet build SistemaDocente.sln --no-restore

# 4. Pruebas
dotnet test SistemaDocente.sln --no-build

# 5. Validar especificaciones
openspec validate --all

# 6. Verificar cambios Git
git diff --check
```

### Auditorías Automatizadas

El proyecto incluye verificaciones de:
- Ausencia de dependencias cíclicas
- SQL solo en capa Data
- WPF solo en App.Wpf
- No uso de `async`/`await` innecesario
- No uso de `Task.Run`
- No repositorios genéricos
- No ORM externo

---

## Resolución de Problemas

### Error de Compilación

1. Verificar versión de .NET SDK: `dotnet --version`
2. Limpiar solución: `dotnet clean`
3. Restaurar: `dotnet restore`
4. Reconstruir: `dotnet build --no-incremental`

### Fallo en Pruebas

1. Identificar proyecto fallido
2. Ejecutar solo ese proyecto: `dotnet test tests/<Proyecto>.Tests`
3. Verificar datos de prueba temporales
4. Revisar logs de SQLite si aplica

### Conflicto de Migración

1. Verificar `user_version` actual: `PRAGMA user_version;`
2. Comparar con `VersionActual` en código
3. Si es menor: ejecutar migración
4. Si es mayor: error incompatible (no hay downgrade)

---

## Recursos Adicionales

- [Documentación de Arquitectura](architecture.md)
- [Referencia de API](api-reference.md)
- [Especificaciones OpenSpec](../openspec/specs/)
- [README Principal](../README.md)

---

## Contacto y Contribución

Para contribuir:
1. Revisar issues abiertos
2. Crear propuesta en `openspec/changes/`
3. Seguir flujo OpenSpec
4. Enviar PR con validaciones completas
