## 1. Implementar valores e identidades de Core

- [x] 1.1 Crear `GrupoId` y `EstudianteId` como identificadores opacos, fuertemente tipados y distintos, basados en `Guid` generado internamente por Core
- [x] 1.2 Definir `DomainValidationException` para valores inválidos y `DomainConflictException` para conflictos de invariantes
- [x] 1.3 Implementar la normalización común de espacios sin alterar mayúsculas, acentos, signos, guiones ni apóstrofos
- [x] 1.4 Implementar y validar el nombre obligatorio del grupo con longitud normalizada máxima de 100 caracteres
- [x] 1.5 Implementar y validar el nombre único visible del estudiante con longitud normalizada máxima de 150 caracteres, sin usarlo como identidad ni exigir unicidad
- [x] 1.6 Implementar el número de lista como entero mayor que cero, sin límite superior ni continuidad obligatoria

## 2. Implementar el agregado de grupo

- [x] 2.1 Implementar la creación de grupos y estudiantes con identidades generadas por Core, sin aceptar identidades externas ni diseñar reconstrucción desde persistencia
- [x] 2.2 Encapsular la colección de estudiantes y exponer únicamente vistas de solo lectura
- [x] 2.3 Implementar el alta de estudiantes en estado activo y la unicidad del número sólo entre activos del mismo grupo
- [x] 2.4 Rechazar conflictos con `DomainConflictException` sin reasignar automáticamente números ni dejar cambios parciales
- [x] 2.5 Implementar el cambio explícito de número para estudiantes activos e inactivos con las validaciones correspondientes
- [x] 2.6 Implementar desactivación y reactivación conservando identidad y datos, con comportamiento idempotente para estados ya alcanzados
- [x] 2.7 Comprobar de nuevo la unicidad al reactivar y rechazar atómicamente la operación si otro activo usa el número
- [x] 2.8 Implementar la consulta de estudiantes activos ordenada por número de lista y después por nombre visible

## 3. Probar nombres, números e identidades

- [x] 3.1 Probar generación interna, tipos incompatibles y estabilidad de `GrupoId` y `EstudianteId`
- [x] 3.2 Probar nombres de grupo obligatorios, normalización de espacios, conservación de caracteres y límites de 100 y 101 caracteres
- [x] 3.3 Probar nombres de estudiante obligatorios, normalización de espacios, conservación de acentos, mayúsculas, guiones y apóstrofos, y límites de 150 y 151 caracteres
- [x] 3.4 Probar que nombres visibles repetidos se aceptan y reciben identidades distintas
- [x] 3.5 Probar rechazo de cero y negativos, ausencia de límite superior adicional y aceptación de huecos en números de lista

## 4. Probar invariantes, estado y consultas

- [x] 4.1 Probar unicidad de números sólo entre estudiantes activos del mismo grupo y permitir el mismo número en grupos diferentes
- [x] 4.2 Probar que un activo puede reutilizar el número conservado por un inactivo y que no hay reasignaciones automáticas
- [x] 4.3 Probar el cambio explícito de número para estudiantes activos e inactivos, incluido el cambio previo a reactivar
- [x] 4.4 Probar el conflicto al reactivar y verificar que identidad, datos, estado y colección permanecen sin cambios
- [x] 4.5 Probar desactivación y reactivación con conservación de datos, además de ambos comportamientos idempotentes
- [x] 4.6 Probar atomicidad completa ante cada `DomainValidationException` y `DomainConflictException`
- [x] 4.7 Probar que las vistas no son mutables y que la consulta excluye inactivos y mantiene el orden determinista por número y nombre

## 5. Verificar alcance e independencia

- [x] 5.1 Ejecutar las pruebas de Core y confirmar que todos los escenarios de la especificación quedan cubiertos
- [x] 5.2 Ejecutar restauración, compilación, pruebas y verificación de formato de la solución
- [x] 5.3 Confirmar que Core no incorpora SQLite, repositorios, migraciones, WPF, ViewModels ni otros componentes de UI
- [x] 5.4 Confirmar que no se incorporan grado, grupo, turno, escuela, ciclo escolar, asistencia, actividades, evaluación, CURP, tutores, domicilio, teléfono, datos médicos, importación ni eliminación definitiva
