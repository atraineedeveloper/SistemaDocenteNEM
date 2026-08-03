## Why

Las funciones futuras de asistencia, actividades y evaluación necesitan una representación común y comprobable de un grupo de primaria y de sus estudiantes activos. Definir primero este modelo mínimo en Core evita que reglas esenciales de identidad, nombres, números de lista y estado queden acopladas a persistencia o interfaz gráfica.

## What Changes

- Incorporar en Core el concepto de `Grupo` como agregado con identidad interna, nombre visible normalizado y una colección encapsulada de estudiantes.
- Incorporar el concepto de `Estudiante`, con identidad interna, un único nombre visible normalizado, número de lista y estado activo/inactivo.
- Usar identificadores opacos y fuertemente tipados, basados en `Guid` generado por Core, sin aceptar identidades proporcionadas por consumidores al crear instancias.
- Validar nombres obligatorios, normalizar sus espacios y aplicar longitudes máximas de 100 caracteres para grupos y 150 para estudiantes, conservando el resto de los caracteres escritos.
- Exigir números de lista enteros mayores que cero, sin límite superior en Core y permitiendo huecos.
- Exigir números de lista únicos sólo entre estudiantes activos del mismo grupo; rechazar conflictos sin reasignaciones automáticas.
- Permitir cambios explícitos de número, desactivación y reactivación sin eliminar estudiantes ni sustituir sus identidades o datos.
- Exponer vistas de solo lectura y consultar estudiantes activos en orden determinista por número de lista y después por nombre visible.
- Usar `DomainValidationException` para valores inválidos y `DomainConflictException` para conflictos de invariantes, con operaciones atómicas.
- Añadir pruebas unitarias para todas las reglas del modelo.

## Capabilities

### New Capabilities

- `gestion-grupo-estudiantes`: Define la identidad y las reglas mínimas de un grupo escolar y de sus estudiantes, incluida la consulta ordenada de estudiantes activos.

### Modified Capabilities

Ninguna.

## Impact

- Código futuro afectado: `SistemaDocente.Core` y `SistemaDocente.Core.Tests`.
- No se añaden dependencias de SQLite, repositorios, migraciones, WPF, ViewModels ni bibliotecas de UI.
- No se cambia ningún esquema de datos ni se diseña reconstrucción desde persistencia.
- Permanecen fuera de alcance asistencia, actividades, evaluación, CURP, tutores, domicilio, teléfono, datos médicos e importación.
