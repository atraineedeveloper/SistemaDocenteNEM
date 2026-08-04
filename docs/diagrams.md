# Diagramas del Sistema

## Diagrama de Arquitectura en Capas

```
┌─────────────────────────────────────────────────────────────────┐
│                    SistemaDocente.App.Wpf                        │
│                   (Interfaz + Composición)                       │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ MainWindow  │  │ ServiciosWpf │  │ AlmacenamientoEstado │   │
│  │   XAML      │  │  (MessageBox)│  │     (JSON)           │   │
│  └─────────────┘  └──────────────┘  └──────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                SistemaDocente.Presentation                       │
│                      (MVVM Portable)                             │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │ MainWindowViewModel│  │GestionGrupoVM  │  │GestionAsist. │  │
│  └──────────────────┘  └──────────────────┘  └──────────────┘  │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │GestionProyectosVM│  │  ViewModelBase   │  │ RelayCommand │  │
│  └──────────────────┘  └──────────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                 SistemaDocente.Application                       │
│                  (Casos de Uso + Puertos)                        │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │GestionGrupoCasos │  │GestionAsist.Casos│  │GestionProy.  │  │
│  └──────────────────┘  └──────────────────┘  └──────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │            Puertos (Interfaces)                          │   │
│  │ IAlmacenamientoGrupos | IAlmacenamientoAsistencias       │   │
│  │ IAlmacenamientoProyectos | IAlmacenamientoActividades    │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                   SistemaDocente.Core                            │
│               (Dominio - Entidades + Reglas)                     │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────┐            │
│  │   Grupo    │  │AsistenciaDiaria│  │ProyectoDidactico│        │
│  └────────────┘  └──────────────┘  └──────────────┘            │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────┐            │
│  │ Estudiante │  │RegistroAsist.│  │ActividadProy.│            │
│  └────────────┘  └──────────────┘  └──────────────┘            │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Identidades: GrupoId, EstudianteId, ProyectoId, ActividadId│  │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                   SistemaDocente.Data                            │
│                (Adaptadores SQLite)                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐  │
│  │PersistenciaGrupo │  │PersistenciaAsist.│  │PersistenciaPr│  │
│  └──────────────────┘  └──────────────────┘  └──────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              EsquemaSqlite (v3) + Migraciones            │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                        SQLite (.db)                              │
│  grupos | estudiantes | asistencias_diarias | registros_asist. │
│  proyectos_didacticos | actividades_proyecto | entregas_activ. │
└─────────────────────────────────────────────────────────────────┘
```

---

## Grafo de Dependencias

```
                    Core
                      ↑
         ┌────────────┼────────────┐
         │            │            │
         │        Application      │
         │            ↑            │
         │            │            │
         │    ┌───────┴───────┐    │
         │    │               │    │
    Reporting  │          Data     │
         ↑     │               ↑   │
         │     │               │   │
         └─────┴───────────────┘   │
                      ↑            │
                      │            │
                Presentation       │
                      ↑            │
                      │            │
                   App.Wpf ────────┘

Leyenda:
  → "depende de" o "referencia a"
  
Ejemplo: Application → Core significa "Application depende de Core"
```

**Reglas:**
- `Core` no tiene dependencias de otros proyectos productivos
- `Data` solo es referenciado por `App.Wpf` (composición raíz)
- `Presentation` es portable (no conoce WPF, Data ni SQLite)
- No existen ciclos en el grafo

---

## Diagrama de Agregados de Dominio

### Agregado Grupo

```
┌─────────────────────────────────────────────────────┐
│                    GRUPO (Agregado Raíz)            │
│─────────────────────────────────────────────────────│
│ Id: GrupoId                                         │
│ NombreVisible: string                               │
│─────────────────────────────────────────────────────│
│ + Crear(nombre): Grupo                              │
│ + Rehidratar(id, nombre, estudiantes): Grupo        │
│ + AgregarEstudiante(nombre, numero): Estudiante     │
│ + RenombrarEstudiante(id, nombre)                   │
│ + CambiarNumeroLista(id, numero)                    │
│ + DesactivarEstudiante(id)                          │
│ + ReactivarEstudiante(id)                           │
│ + Renombrar(nuevoNombre)                            │
└─────────────────────────────────────────────────────┘
                         │
                         │ Contiene
                         ↓
         ┌───────────────────────────────────┐
         │   ESTUDIANTE (Entidad)            │
         │───────────────────────────────────│
         │ Id: EstudianteId                  │
         │ NombreVisible: string             │
         │ NumeroLista: int                  │
         │ EstaActivo: bool                  │
         └───────────────────────────────────┘
```

**Invariantes:**
- Números de lista únicos para estudiantes activos
- Identidades únicas dentro del grupo
- Nombre normalizado (trim, longitud máxima 100/150)

---

### Agregado AsistenciaDiaria

```
┌─────────────────────────────────────────────────────┐
│              ASISTENCIA DIARIA (Agregado Raíz)      │
│─────────────────────────────────────────────────────│
│ GrupoId: GrupoId                                    │
│ Fecha: DateOnly                                     │
│─────────────────────────────────────────────────────│
│ + Crear(grupoId, fecha, estados): AsistenciaDiaria  │
│ + Rehidratar(grupoId, fecha, registros): Asistencia │
│ + CambiarEstado(estudianteId, estado)               │
└─────────────────────────────────────────────────────┘
                         │
                         │ Contiene
                         ↓
         ┌───────────────────────────────────┐
         │ REGISTRO ASISTENCIA (Entidad)     │
         │───────────────────────────────────│
         │ EstudianteId: EstudianteId        │
         │ Estado: EstadoAsistencia          │
         │───────────────────────────────────│
         │ + CambiarEstado(estado)           │
         └───────────────────────────────────┘
```

**Invariantes:**
- Máximo un registro por estudiante
- Único estado válido por registro
- Atomicidad: todos los registros se guardan o revierten juntos
- Identidad natural: `GrupoId + Fecha`

---

### Agregado ProyectoDidactico

```
┌─────────────────────────────────────────────────────┐
│            PROYECTO DIDACTICO (Agregado Raíz)       │
│─────────────────────────────────────────────────────│
│ Id: ProyectoId                                      │
│ GrupoId: GrupoId (inmutable)                        │
│ Nombre: string                                      │
│ Descripcion: string                                 │
│ FechaInicio: DateOnly                               │
│ FechaTermino: DateOnly                              │
│ Estado: EstadoProyecto                              │
│ Observaciones: string                               │
│ Version: int                                        │
│─────────────────────────────────────────────────────│
│ + Crear(grupoId, nombre, inicio, termino): Proyecto │
│ + Rehidratar(...): Proyecto                         │
│ + Iniciar()           // Borrador → EnCurso         │
│ + Finalizar()         // EnCurso → Finalizado       │
│ + Reabrir()           // Finalizado → EnCurso       │
│ + ActualizarDatos(...)                              │
│ + ActualizarPeriodo(inicio, termino)                │
└─────────────────────────────────────────────────────┘

Transiciones válidas:
  Borrador ─────→ EnCurso ─────→ Finalizado
     ↑               ↑              │
     │               └──────────────┘
     └──────────── Reabrir
```

---

### Agregado ActividadProyecto

```
┌─────────────────────────────────────────────────────┐
│            ACTIVIDAD PROYECTO (Agregado Raíz)       │
│─────────────────────────────────────────────────────│
│ Id: ActividadId                                     │
│ ProyectoId: ProyectoId (inmutable)                  │
│ GrupoId: GrupoId (inmutable)                        │
│ Titulo: string                                      │
│ Descripcion: string                                 │
│ FechaLimite: DateOnly                               │
│ Estado: EstadoActividad                             │
│ Version: int                                        │
│─────────────────────────────────────────────────────│
│ + Crear(proyectoId, grupoId, titulo, fecha): Activ. │
│ + Rehidratar(...): Actividad                        │
│ + ActualizarDatos(titulo, desc, fecha)              │
│ + RegistrarEntrega(estId, estado, obs)              │
│ + Anular()                                          │
└─────────────────────────────────────────────────────┘
                         │
                         │ Contiene
                         ↓
         ┌───────────────────────────────────┐
         │ ENTREGA ACTIVIDAD (Entidad)       │
         │───────────────────────────────────│
         │ EstudianteId: EstudianteId        │
         │ Estado: EstadoEntrega             │
         │ Observacion: string?              │
         └───────────────────────────────────┘
```

**Invariantes:**
- Una entrega por estudiante como máximo
- Fecha límite dentro del periodo del proyecto padre
- No modificable si está anulada

---

## Diagrama de Flujo: Gestión de Grupo

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Usuario    │     │  ViewModel   │     │  Caso de Uso │
│   (WPF)      │     │ (Presentation)│    │ (Application)│
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                    │                    │
       │ Click "Agregar"    │                    │
       ├────────────────────►                    │
       │                    │                    │
       │                    │ Ejecutar comando   │
       │                    ├────────────────────►
       │                    │                    │
       │                    │                    │ Validar datos
       │                    │                    │ Crear entidad
       │                    │                    │ Persistir
       │                    │                    ├───────┐
       │                    │                    │       │
       │                    │                    │◄──────┤
       │                    │                    │ SQLite│
       │                    │                    ├───────┘
       │                    │                    │
       │                    │ Retornar detalle   │
       │                    │◄───────────────────┤
       │                    │                    │
       │ Actualizar vista   │                    │
       │◄───────────────────┤                    │
       │                    │                    │
```

---

## Diagrama de Flujo: Guardado de Asistencia

```
Usuario → ViewModel → CasoDeUso → Persistencia → SQLite
   │          │           │            │           │
   │ Editar   │           │            │           │
   │ día      │           │            │           │
   │─────────►│           │            │           │
   │          │           │            │           │
   │          │ Guardar   │            │           │
   │          │──────────►│            │           │
   │          │           │            │           │
   │          │           │ Validar    │           │
   │          │           │ entrada    │           │
   │          │           │ completa   │           │
   │          │           │            │           │
   │          │           │ Iniciar    │           │
   │          │           │ transacción│           │
   │          │           │───────────►│           │
   │          │           │            │           │
   │          │           │            │ INSERT/   │
   │          │           │            │ UPDATE    │
   │          │           │            │──────────►│
   │          │           │            │           │
   │          │           │            │◄──────────┤
   │          │           │            │ Commit    │
   │          │           │◄───────────┤           │
   │          │◄──────────┤            │           │
   │          │ Confirmar │            │           │
   │◄─────────┤            │            │           │
   │Actualizar│            │            │           │
   │          │            │            │           │
```

**Nota:** El guardado mensual ejecuta transacciones diarias sucesivas, NO hay atomicidad mensual.

---

## Diagrama de Estados: Proyecto Didáctico

```
                    ┌─────────────┐
                    │   BORRADOR  │
                    └──────┬──────┘
                           │ Iniciar()
                           ↓
                    ┌─────────────┐
        ┌───────────│  EN CURSO   │───────────┐
        │           └──────┬──────┘           │
        │                  │ Finalizar()      │
        │                  ↓                  │
        │           ┌─────────────┐           │
        │           │ FINALIZADO  │           │
        │           └──────┬──────┘           │
        │                  │                  │
        └──────────────────┴ Reabrir() ───────┘


Estados inválidos (rechazados):
  Borrador → Finalizado (debe pasar por En Curso)
  Borrador → Borrador (ya está)
  En Curso → Borrador (no hay reversa)
  Finalizado → Borrador (no hay reversa)
  Finalizado → Finalizado (ya está)
```

---

## Diagrama de Secuencia: Carga de Asistencia Mensual

```
Usuario  Presentation  Application  Data  SQLite
   │          │            │         │      │
   │ Selecciona mes       │         │      │
   │─────────►│            │         │      │
   │          │ CargarMes  │         │      │
   │          │───────────►│         │      │
   │          │            │         │      │
   │          │            │ Calcular│      │
   │          │            │ rango   │      │
   │          │            │         │      │
   │          │            │CargarIntervalo│
   │          │            │────────►│      │
   │          │            │         │      │
   │          │            │         │SELECT│
   │          │            │         │──────►
   │          │            │         │      │
   │          │            │         │◄──────┤
   │          │            │         │ Datos│
   │          │            │◄────────┤      │
   │          │            │         │      │
   │          │            │ Proyectar      │
   │          │            │ fechas lectivas│
   │          │            │                │
   │          │◄───────────┤                │
   │          │ DetalleMes │                │
   │◄─────────┤            │                │
   │ Mostrar  │            │                │
   │          │            │                │
```

---

## Diagrama ER: Base de Datos SQLite (v3)

```
┌─────────────────────┐       ┌─────────────────────┐
│      grupos         │       │    estudiantes      │
│─────────────────────│       │─────────────────────│
│ id (PK)             │◄──────│ grupo_id (FK)       │
│ nombre              │   1:N │ id (PK)             │
└─────────────────────┘       │ nombre              │
                              │ numero_lista        │
                              │ activo              │
                              └─────────────────────┘
                                       │
                                       │ 1:N
                                       ↓
┌─────────────────────┐       ┌─────────────────────┐
│ asistencias_diarias │       │  registros_asistencia│
│─────────────────────│       │─────────────────────│
│ grupo_id (PK, FK)   │◄──────│ grupo_id (PK, FK)   │
│ fecha (PK)          │  1:1  │ fecha (PK, FK)      │
└─────────────────────┘       │ estudiante_id (PK)  │
                              │ estado              │
                              └─────────────────────┘

┌─────────────────────┐       ┌─────────────────────┐
│ proyectos_didacticos│       │  actividades_proyecto│
│─────────────────────│       │─────────────────────│
│ proyecto_id (PK)    │◄──────│ proyecto_id (PK, FK)│
│ grupo_id (FK)       │  1:N  │ actividad_id (PK)   │
│ nombre              │       │ grupo_id (FK)       │
│ descripcion         │       │ titulo              │
│ fecha_inicio        │       │ descripcion         │
│ fecha_termino       │       │ fecha_limite        │
│ estado              │       │ estado              │
│ observaciones       │       │ version             │
│ version             │       └─────────────────────┘
└─────────────────────┘                │
                                       │ 1:N
                                       ↓
                              ┌─────────────────────┐
                              │   entregas_actividad│
                              │─────────────────────│
                              │ actividad_id (PK,FK)│
                              │ grupo_id (PK, FK)   │
                              │ estudiante_id (PK)  │
                              │ estado              │
                              │ observacion         │
                              └─────────────────────┘
```

**Claves foráneas:**
- Todas con `ON DELETE RESTRICT` (sin cascada)
- `PRAGMA foreign_keys = ON` requerido

---

## Diagrama de Estados: Actividad

```
                    ┌─────────────┐
                    │  PENDIENTE  │
                    └──────┬──────┘
                           │ Actualizar
                           ↓
                    ┌─────────────┐
                    │ EN PROCESO  │
                    └──────┬──────┘
                           │ Completar
                           ↓
                    ┌─────────────┐
                    │ COMPLETADA  │
                    └──────┬──────┘
                           │
                    ┌──────┴──────┐
                    │   ANULADA   │◄── Anular() (irreversible)
                    └─────────────┘

Una vez anulada:
  - No se pueden editar datos
  - No se pueden modificar entregas
  - Se excluye de agregaciones
```

---

## Flujo de Desarrollo OpenSpec

```
┌─────────────┐
│   Explorar  │
│  necesidad  │
└──────┬──────┘
       │
       ↓
┌─────────────┐
│   Crear     │
│  propuesta  │ (proposal.md)
└──────┬──────┘
       │
       ↓
┌─────────────┐
│  Definir    │
│ especific.  │ (spec.md)
└──────┬──────┘
       │
       ↓
┌─────────────┐
│   Diseñar   │
│  solución   │ (design.md)
└──────┬──────┘
       │
       ↓
┌─────────────┐
│   Listar    │
│   tareas    │ (tasks.md)
└──────┬──────┘
       │
       ↓
┌─────────────┐
│   Aprobar   │ ← Revisión
└──────┬──────┘
       │
       ↓
┌─────────────┐
│ Implementar │ ← Siguiendo tasks.md
└──────┬──────┘
       │
       ↓
┌─────────────┐
│  Validar    │ (openspec validate)
└──────┬──────┘
       │
       ↓
┌─────────────┐
│  Archivar   │ → moves to archive/
└─────────────┘
```
