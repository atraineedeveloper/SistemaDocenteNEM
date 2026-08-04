# Documentación de Base de Datos

## Visión General

El sistema utiliza **SQLite** como base de datos embebida para almacenamiento local. La persistencia es manejada directamente mediante `Microsoft.Data.Sqlite` sin ORM ni micro-ORM.

---

## Especificaciones Técnicas

| Característica | Valor |
|----------------|-------|
| Motor | SQLite 3 |
| Versión de esquema actual | 3 |
| Acceso | Directo con `Microsoft.Data.Sqlite` |
| ORM | Ninguno (acceso manual) |
| Migraciones | Manuales y transaccionales |
| Foreign Keys | Habilitadas (`PRAGMA foreign_keys = ON`) |

---

## Esquema de Base de Datos (Versión 3)

### Tabla: `grupos`

Almacena los grupos escolares.

```sql
CREATE TABLE grupos (
    id TEXT NOT NULL PRIMARY KEY,
    nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 100)
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `id` | TEXT | PRIMARY KEY | Identificador único (GUID string) |
| `nombre` | TEXT | CHECK, NOT NULL | Nombre visible del grupo (1-100 chars) |

---

### Tabla: `estudiantes`

Almacena estudiantes pertenecientes a un grupo.

```sql
CREATE TABLE estudiantes (
    id TEXT NOT NULL PRIMARY KEY,
    grupo_id TEXT NOT NULL,
    nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
    numero_lista INTEGER NOT NULL CHECK (numero_lista > 0),
    activo INTEGER NOT NULL CHECK (activo IN (0, 1)),
    FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `id` | TEXT | PRIMARY KEY | Identificador único (GUID string) |
| `grupo_id` | TEXT | FK, NOT NULL | Referencia a `grupos.id` |
| `nombre` | TEXT | CHECK, NOT NULL | Nombre del estudiante (1-150 chars) |
| `numero_lista` | INTEGER | CHECK (> 0) | Número de lista en el grupo |
| `activo` | INTEGER | CHECK (0 o 1) | Estado de activación (0=inactivo, 1=activo) |

**Índices:**
```sql
CREATE INDEX ix_estudiantes_grupo_id
ON estudiantes(grupo_id)

CREATE UNIQUE INDEX ux_estudiantes_grupo_numero_activo
ON estudiantes(grupo_id, numero_lista)
WHERE activo = 1

CREATE UNIQUE INDEX ux_estudiantes_id_grupo_id
ON estudiantes(id, grupo_id)
```

**Reglas:**
- Números de lista únicos solo para estudiantes activos dentro del mismo grupo
- Un estudiante pertenece a un único grupo
- No se puede eliminar un grupo con estudiantes (RESTRICT)

---

### Tabla: `asistencias_diarias`

Encabezado de asistencia para un día específico.

```sql
CREATE TABLE asistencias_diarias (
    grupo_id TEXT NOT NULL,
    fecha TEXT NOT NULL CHECK (
        length(fecha) = 10
        AND fecha GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'
        AND CAST(substr(fecha, 6, 2) AS INTEGER) BETWEEN 1 AND 12
        AND CAST(substr(fecha, 9, 2) AS INTEGER) BETWEEN 1 AND 31
        -- Validación adicional de días por mes incluida en CHECK completo
    ),
    PRIMARY KEY (grupo_id, fecha),
    FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `grupo_id` | TEXT | PK, FK, NOT NULL | Referencia al grupo |
| `fecha` | TEXT | PK, CHECK, NOT NULL | Fecha en formato ISO (YYYY-MM-DD) |

**Índices:**
```sql
CREATE INDEX ix_asistencias_diarias_grupo_fecha
ON asistencias_diarias(grupo_id, fecha)
```

**Reglas:**
- Clave primaria compuesta: `(grupo_id, fecha)`
- Máximo un registro por grupo por día
- Formato de fecha validado por CHECK constraint
- No se puede eliminar un grupo con asistencias registradas

---

### Tabla: `registros_asistencia`

Registros individuales de asistencia por estudiante.

```sql
CREATE TABLE registros_asistencia (
    grupo_id TEXT NOT NULL,
    fecha TEXT NOT NULL,
    estudiante_id TEXT NOT NULL,
    estado INTEGER NOT NULL CHECK (estado IN (0, 1, 2, 3)),
    PRIMARY KEY (grupo_id, fecha, estudiante_id),
    FOREIGN KEY (grupo_id, fecha)
        REFERENCES asistencias_diarias(grupo_id, fecha) ON DELETE RESTRICT,
    FOREIGN KEY (estudiante_id, grupo_id)
        REFERENCES estudiantes(id, grupo_id) ON DELETE RESTRICT
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `grupo_id` | TEXT | PK, FK, NOT NULL | Referencia al grupo |
| `fecha` | TEXT | PK, FK, NOT NULL | Fecha de la asistencia |
| `estudiante_id` | TEXT | PK, FK, NOT NULL | Referencia al estudiante |
| `estado` | INTEGER | CHECK (0-3), NOT NULL | Estado de asistencia |

**Estados válidos:**
| Valor | Estado |
|-------|--------|
| 0 | Presente |
| 1 | Ausente |
| 2 | Justificada |
| 3 | Tardanza |

**Índices:**
```sql
CREATE INDEX ix_registros_asistencia_estudiante_id
ON registros_asistencia(estudiante_id)
```

**Reglas:**
- Clave primaria compuesta: `(grupo_id, fecha, estudiante_id)`
- Máximo un registro por estudiante por día
- El estudiante debe existir en el grupo
- La asistencia diaria debe existir
- Atomicidad: encabezado y registros se guardan juntos

---

### Tabla: `proyectos_didacticos` (v3)

Almacena proyectos didácticos.

```sql
CREATE TABLE proyectos_didacticos (
    proyecto_id TEXT NOT NULL PRIMARY KEY,
    grupo_id TEXT NOT NULL,
    nombre TEXT NOT NULL CHECK (length(trim(nombre)) BETWEEN 1 AND 150),
    descripcion TEXT NOT NULL CHECK (length(descripcion) <= 2000),
    fecha_inicio TEXT NOT NULL CHECK (
        length(fecha_inicio) = 10 
        AND fecha_inicio GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' 
        AND date(fecha_inicio) = fecha_inicio
    ),
    fecha_termino TEXT NOT NULL CHECK (
        length(fecha_termino) = 10 
        AND fecha_termino GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' 
        AND date(fecha_termino) = fecha_termino 
        AND fecha_inicio <= fecha_termino
    ),
    estado INTEGER NOT NULL CHECK (estado IN (0, 1, 2)),
    observaciones TEXT NOT NULL CHECK (length(observaciones) <= 2000),
    version INTEGER NOT NULL CHECK (version > 0),
    UNIQUE (proyecto_id, grupo_id),
    FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `proyecto_id` | TEXT | PK, NOT NULL | Identificador único del proyecto |
| `grupo_id` | TEXT | FK, NOT NULL | Grupo propietario |
| `nombre` | TEXT | CHECK, NOT NULL | Nombre del proyecto (1-150 chars) |
| `descripcion` | TEXT | CHECK (≤2000) | Descripción detallada |
| `fecha_inicio` | TEXT | CHECK, NOT NULL | Inicio del periodo (ISO) |
| `fecha_termino` | TEXT | CHECK, NOT NULL | Fin del periodo (ISO, >= inicio) |
| `estado` | INTEGER | CHECK (0-2), NOT NULL | Estado del proyecto |
| `observaciones` | TEXT | CHECK (≤2000) | Observaciones adicionales |
| `version` | INTEGER | CHECK (>0), NOT NULL | Versión para concurrencia optimista |

**Estados válidos:**
| Valor | Estado |
|-------|--------|
| 0 | Borrador |
| 1 | En Curso |
| 2 | Finalizado |

**Índices:**
```sql
CREATE INDEX ix_proyectos_grupo_id
ON proyectos_didacticos(grupo_id)

CREATE INDEX ix_proyectos_estado_fecha
ON proyectos_didacticos(estado, fecha_inicio DESC)
```

---

### Tabla: `actividades_proyecto` (v3)

Almacena actividades dentro de proyectos.

```sql
CREATE TABLE actividades_proyecto (
    actividad_id TEXT NOT NULL PRIMARY KEY,
    proyecto_id TEXT NOT NULL,
    grupo_id TEXT NOT NULL,
    titulo TEXT NOT NULL CHECK (length(trim(titulo)) BETWEEN 1 AND 150),
    descripcion TEXT NOT NULL CHECK (length(descripcion) <= 2000),
    fecha_limite TEXT NOT NULL CHECK (
        length(fecha_limite) = 10 
        AND fecha_limite GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' 
        AND date(fecha_limite) = fecha_limite
    ),
    estado INTEGER NOT NULL CHECK (estado IN (0, 1, 2, 3)),
    version INTEGER NOT NULL CHECK (version > 0),
    UNIQUE (actividad_id, proyecto_id, grupo_id),
    FOREIGN KEY (proyecto_id, grupo_id)
        REFERENCES proyectos_didacticos(proyecto_id, grupo_id) ON DELETE RESTRICT,
    FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `actividad_id` | TEXT | PK, NOT NULL | Identificador único de la actividad |
| `proyecto_id` | TEXT | FK, NOT NULL | Proyecto padre |
| `grupo_id` | TEXT | FK, NOT NULL | Grupo propietario |
| `titulo` | TEXT | CHECK, NOT NULL | Título de la actividad (1-150 chars) |
| `descripcion` | TEXT | CHECK (≤2000) | Descripción de la actividad |
| `fecha_limite` | TEXT | CHECK, NOT NULL | Fecha límite de entrega (ISO) |
| `estado` | INTEGER | CHECK (0-3), NOT NULL | Estado de la actividad |
| `version` | INTEGER | CHECK (>0), NOT NULL | Versión para concurrencia optimista |

**Estados válidos:**
| Valor | Estado |
|-------|--------|
| 0 | Pendiente |
| 1 | En Proceso |
| 2 | Completada |
| 3 | Anulada |

**Índices:**
```sql
CREATE INDEX ix_actividades_proyecto_id
ON actividades_proyecto(proyecto_id)

CREATE INDEX ix_actividades_grupo_id
ON actividades_proyecto(grupo_id)
```

---

### Tabla: `entregas_actividad` (v3)

Registra entregas de actividades por estudiante.

```sql
CREATE TABLE entregas_actividad (
    actividad_id TEXT NOT NULL,
    grupo_id TEXT NOT NULL,
    estudiante_id TEXT NOT NULL,
    estado INTEGER NOT NULL CHECK (estado IN (0, 1, 2)),
    observacion TEXT CHECK (length(observacion) <= 500),
    PRIMARY KEY (actividad_id, grupo_id, estudiante_id),
    FOREIGN KEY (actividad_id, grupo_id)
        REFERENCES actividades_proyecto(actividad_id, grupo_id) ON DELETE RESTRICT,
    FOREIGN KEY (estudiante_id, grupo_id)
        REFERENCES estudiantes(id, grupo_id) ON DELETE RESTRICT
)
```

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `actividad_id` | TEXT | PK, FK, NOT NULL | Referencia a la actividad |
| `grupo_id` | TEXT | PK, FK, NOT NULL | Referencia al grupo |
| `estudiante_id` | TEXT | PK, FK, NOT NULL | Referencia al estudiante |
| `estado` | INTEGER | CHECK (0-2), NOT NULL | Estado de la entrega |
| `observacion` | TEXT | CHECK (≤500), NULLABLE | Observación opcional |

**Estados válidos:**
| Valor | Estado |
|-------|--------|
| 0 | Pendiente |
| 1 | Entregada |
| 2 | No Entregada |

**Índices:**
```sql
CREATE INDEX ix_entregas_actividad_id
ON entregas_actividad(actividad_id)

CREATE INDEX ix_entregas_estudiante_id
ON entregas_actividad(estudiante_id)
```

---

## Migraciones

### Versión 1 → Versión 2

**Cambios:**
- Validación de estructura existente
- Conservación de datos
- Actualización de `user_version` a 2

**Proceso:**
1. Validar estructura v1
2. Ejecutar dentro de transacción
3. Establecer `PRAGMA user_version = 2` solo al completar

---

### Versión 2 → Versión 3

**Cambios:**
- Agrega tablas: `proyectos_didacticos`, `actividades_proyecto`, `entregas_actividad`
- Conserva todas las tablas existentes (grupos, estudiantes, asistencias)

**Proceso:**
1. Validar estructura v2 completa
2. Crear nuevas tablas dentro de transacción
3. Establecer `PRAGMA user_version = 3` solo al completar

---

### Nueva Base de Datos

Una base de datos nueva se crea directamente en **versión 3** con todas las tablas.

---

## Reglas de Integridad

### Foreign Keys

Todas las claves foráneas usan `ON DELETE RESTRICT`:
- No se pueden eliminar grupos con estudiantes, asistencias o proyectos
- No se pueden eliminar asistencias con registros
- No se pueden eliminar proyectos con actividades
- No se pueden eliminar actividades con entregas

**Importante:** `PRAGMA foreign_keys = ON` debe ejecutarse en cada conexión.

### CHECK Constraints

Cada tabla incluye validaciones a nivel de base de datos:
- Longitudes máximas y mínimas de texto
- Formatos de fecha ISO válidos
- Estados dentro de rangos permitidos
- Números positivos donde aplica
- Fechas coherentes (inicio ≤ término)

### Transacciones

**Atomicidad garantizada para:**
- Guardado de grupo + estudiantes
- Guardado de asistencia diaria completa (encabezado + registros)
- Guardado de proyecto con versión
- Guardado de actividad + todas sus entregas

**No hay atomicidad mensual:**
- Guardar múltiples días ejecuta transacciones diarias independientes
- Un fallo intermedio deja confirmados los éxitos previos

---

## Consultas Comunes

### Cargar Grupo Completo

```sql
SELECT g.id, g.nombre,
       e.id, e.nombre, e.numero_lista, e.activo
FROM grupos g
LEFT JOIN estudiantes e ON e.grupo_id = g.id
WHERE g.id = @grupoId
ORDER BY e.numero_lista, e.nombre;
```

### Cargar Asistencia Diaria

```sql
SELECT ra.estudiante_id, ra.estado
FROM asistencias_diarias ad
JOIN registros_asistencia ra 
    ON ra.grupo_id = ad.grupo_id AND ra.fecha = ad.fecha
WHERE ad.grupo_id = @grupoId AND ad.fecha = @fecha;
```

### Cargar Intervalo de Asistencias

```sql
SELECT ad.fecha, ra.estudiante_id, ra.estado
FROM asistencias_diarias ad
JOIN registros_asistencia ra 
    ON ra.grupo_id = ad.grupo_id AND ra.fecha = ad.fecha
WHERE ad.grupo_id = @grupoId
  AND ad.fecha BETWEEN @desde AND @hasta
ORDER BY ad.fecha;
```

### Listar Proyectos por Grupo

```sql
SELECT proyecto_id, nombre, fecha_inicio, fecha_termino, estado, version
FROM proyectos_didacticos
WHERE grupo_id = @grupoId
ORDER BY 
    CASE estado WHEN 1 THEN 0 WHEN 0 THEN 1 ELSE 2 END,
    fecha_inicio DESC,
    nombre;
```

### Cargar Actividad Completa

```sql
SELECT ap.actividad_id, ap.titulo, ap.descripcion, ap.fecha_limite, 
       ap.estado, ap.version,
       ea.estudiante_id, ea.estado, ea.observacion
FROM actividades_proyecto ap
LEFT JOIN entregas_actividad ea 
    ON ea.actividad_id = ap.actividad_id 
    AND ea.grupo_id = ap.grupo_id
WHERE ap.actividad_id = @actividadId;
```

---

## Archivos de Base de Datos

### Ubicación

La base de datos se almacena en:
```
%LOCALAPPDATA%/SistemaDocenteLocal/sistema-docente.db
```

En Linux/macOS:
```
~/.local/share/SistemaDocenteLocal/sistema-docente.db
```

### Archivo de Estado

El archivo `app-state.json` almacena únicamente:
```json
{
  "ultimoGrupoId": "guid-string-aqui"
}
```

**Nota:** Este archivo NO contiene datos del dominio, solo estado de UI.

---

## Consideraciones de Rendimiento

### Índices Estratégicos

Los índices están diseñados para:
- Búsqueda rápida por grupo
- Consultas de rango por fecha
- Ordenamiento por estado y fecha
- Unicidad de números de lista activos

### Patrones de Acceso

**Optimizados:**
- Carga de grupo completo (índice en `grupo_id`)
- Consulta de asistencia por fecha (PK compuesta)
- Listado de proyectos por estado (índice compuesto)
- Búsqueda de entregas por actividad (índice en `actividad_id`)

### Conexiones

- Una conexión por operación
- Conexiones efímeras (abrir, usar, cerrar)
- Sin pooling explícito
- `foreign_keys = ON` en cada apertura

---

## Seguridad

### Validación en Múltiples Niveles

1. **Dominio** - Invariantes en código C#
2. **Aplicación** - Validación de casos de uso
3. **Base de datos** - CHECK constraints y FKs

### Protección contra Corrupción

- Transacciones para operaciones atómicas
- Rollback automático en caso de error
- Validación de esquema antes de migrar
- Rechazo de versiones incompatibles

---

## Monitoreo y Depuración

### Verificar Versión de Esquema

```sql
PRAGMA user_version;
-- Debe retornar: 3
```

### Verificar Integridad

```sql
PRAGMA integrity_check;
-- Debe retornar: ok
```

### Listar Tablas

```sql
SELECT name FROM sqlite_master 
WHERE type='table' 
ORDER BY name;
```

### Verificar Foreign Keys

```sql
PRAGMA foreign_key_check;
-- Debe retornar: (vacío si todo está bien)
```

---

## Backup y Restauración

### Crear Backup

```bash
# Copiar archivo .db mientras la aplicación está cerrada
cp sistema-docente.db sistema-docente-backup-$(date +%Y%m%d).db
```

### Restaurar

```bash
# Reemplazar archivo .db con backup
cp sistema-docente-backup-YYYYMMDD.db sistema-docente.db
```

**Advertencia:** Asegurar que la versión del esquema sea compatible con la versión del código.

---

## Referencias Relacionadas

- [Arquitectura del Sistema](architecture.md)
- [Referencia de API](api-reference.md)
- [Guía de Desarrollo](development-guide.md)
