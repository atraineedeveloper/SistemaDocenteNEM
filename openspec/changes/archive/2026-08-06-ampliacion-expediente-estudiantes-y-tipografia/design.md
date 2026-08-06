# Design: Ampliación de Información de Alumnos y Tipografía Montserrat

## Modelo de Dominio

### Enum `GeneroEstudiante`
```csharp
public enum GeneroEstudiante
{
    NoEspecificado = 0,
    Hombre = 1,
    Mujer = 2
}
```

### Extensión de la Entidad `Estudiante`
- `PrimerApellido`: string (normalizado, max 100)
- `SegundoApellido`: string (normalizado, max 100)
- `Nombres`: string (normalizado, max 100)
- `FechaNacimiento`: DateOnly?
- `Edad`: int? (calculada automáticamente si `FechaNacimiento` existe)
- `Genero`: GeneroEstudiante
- `FechaIngreso`: DateOnly?
- `Observaciones`: string (validado por `ValidadorContenidoPedagogico`, max 1000)

## Esquema SQLite (`user_version = 6`)

```sql
-- DDL v6 de la tabla estudiantes
CREATE TABLE estudiantes (
    id TEXT NOT NULL,
    grupo_id TEXT NOT NULL,
    nombre TEXT NOT NULL, -- Nombre completo derivado para compatibilidad
    primer_apellido TEXT NOT NULL DEFAULT '',
    segundo_apellido TEXT NOT NULL DEFAULT '',
    nombres TEXT NOT NULL DEFAULT '',
    fecha_nacimiento TEXT,
    genero INTEGER NOT NULL DEFAULT 0,
    fecha_ingreso TEXT,
    observaciones TEXT NOT NULL DEFAULT '',
    numero_lista INTEGER NOT NULL,
    activo INTEGER NOT NULL,
    PRIMARY KEY (id, grupo_id),
    FOREIGN KEY (grupo_id) REFERENCES grupos(id) ON DELETE RESTRICT
);
```

## Sistema Tipográfico
Configuración en `App.xaml`:
```xml
<FontFamily x:Key="AppFontFamily">Montserrat, Segoe UI, sans-serif</FontFamily>
```
Con selectores globales `TargetType="Control"` y `TargetType="TextBlock"` estableciendo `FontFamily="{StaticResource AppFontFamily}"`.
