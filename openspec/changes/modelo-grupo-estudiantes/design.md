## Context

Core todavía no cuenta con el modelo del que dependerán asistencia, actividades y evaluación. La solución ya establece a Core como proyecto portable e independiente de Data y App.Wpf; este cambio debe respetar esa dirección de dependencias. Véanse `proposal.md` y `specs/gestion-grupo-estudiantes/spec.md` para la motivación y el contrato de comportamiento.

## Goals / Non-Goals

**Goals:**

- Modelar al grupo como límite de consistencia de sus estudiantes.
- Mantener dentro de Core las invariantes de identidad, nombres, número de lista y estado.
- Exponer operaciones atómicas que no permitan dejar el modelo en un estado inválido.
- Proporcionar consultas de solo lectura con orden determinista.
- Hacer comprobables todas las reglas mediante pruebas unitarias aisladas.

**Non-Goals:**

- Diseñar SQLite, repositorios, migraciones o reconstrucción desde persistencia.
- Diseñar WPF, vistas, formularios, ViewModels ni otros flujos de UI.
- Incorporar grado, grupo, turno, escuela o ciclo escolar como atributos separados.
- Incorporar asistencia, actividades, evaluación, CURP, tutores, domicilio, teléfono, datos médicos o importación.
- Incorporar eliminación definitiva de estudiantes.

## Decisions

### Grupo como agregado propietario de estudiantes

`Grupo` controlará el alta, la activación, la desactivación y el cambio explícito de número de sus estudiantes. Así protege las reglas que sólo tienen sentido dentro del grupo, especialmente la unicidad entre estudiantes activos. Los consumidores no podrán modificar directamente su colección.

Alternativa descartada: tratar `Grupo` y `Estudiante` como registros independientes y validar desde un servicio. Introduciría coordinación externa y permitiría estados inválidos.

### Nombres visibles normalizados sin alterar su escritura

El grupo tendrá un nombre visible obligatorio de hasta 100 caracteres y cada estudiante un único nombre visible obligatorio de hasta 150. Antes de validar y almacenar, se quitarán los espacios iniciales y finales y cada secuencia interna de espacios se reducirá a uno. La longitud se comprobará sobre el valor normalizado. Fuera de esa normalización, se conservarán mayúsculas, acentos, signos, guiones y apóstrofos tal como fueron escritos. Los nombres de estudiantes no serán únicos y nunca funcionarán como identidad.

Alternativa descartada: comparar, cambiar mayúsculas o eliminar diacríticos. Alteraría el nombre visible aportado por el usuario sin una necesidad del dominio.

### Identidades opacas y fuertemente tipadas

`GrupoId` y `EstudianteId` serán tipos distintos y opacos basados en `Guid`. Core generará el valor al crear cada entidad y la API de creación no aceptará identidades suministradas por consumidores. Este cambio no diseñará constructores, fábricas ni rutas para reconstrucción desde persistencia.

Alternativas descartadas: usar `Guid` sin tipo, porque permitiría mezclar accidentalmente identidades; usar nombre o número de lista, porque son datos de negocio modificables o repetibles.

### Número de lista positivo y no contiguo

El número de lista será un entero mayor que cero. Core no impondrá límite superior ni exigirá una secuencia contigua, por lo que se permiten huecos y el mismo número en grupos diferentes.

Alternativa descartada: limitar el número por tamaño de grupo. Acoplaría el modelo a una regla administrativa no solicitada.

### Unicidad sólo entre estudiantes activos

Un número estará disponible si ningún otro estudiante activo del mismo grupo lo usa. Los inactivos conservarán su número, pero no lo reservarán. Un conflicto al agregar, cambiar número o reactivar producirá `DomainConflictException`, sin reasignar estudiantes ni modificar parcialmente el grupo. Antes de reactivar será posible cambiar explícitamente el número del estudiante inactivo.

Alternativas descartadas: reservar números de inactivos, porque impediría reutilizarlos; reasignar automáticamente, porque produciría cambios implícitos en otros estudiantes.

### Estado reversible e idempotente

Desactivar y reactivar conservarán identidad, nombre y número. Desactivar a quien ya está inactivo y reactivar a quien ya está activo serán operaciones idempotentes. No existirá eliminación definitiva.

Alternativa descartada: eliminar al desactivar. Rompería la continuidad del estudiante y el futuro historial académico.

### Excepciones y atomicidad

Valores inválidos —por ejemplo, nombre vacío, nombre demasiado largo o número no positivo— producirán `DomainValidationException`. Los conflictos con invariantes del agregado producirán `DomainConflictException`. Toda validación necesaria ocurrirá antes de modificar estado, de modo que una operación fallida deje al grupo y a sus estudiantes sin cambios parciales.

Alternativa descartada: un resultado booleano, porque perdería la distinción explícita entre entrada inválida y conflicto de dominio solicitada para Core.

### Colecciones de solo lectura y orden determinista

Las colecciones expuestas serán vistas de solo lectura y nunca revelarán la colección mutable interna. La consulta de estudiantes activos devolverá primero por número de lista ascendente y, para empates que puedan existir fuera del conjunto activo, por nombre visible. La comparación secundaria deberá ser determinista y documentada en la implementación sin normalizar ni modificar el texto almacenado.

Alternativa descartada: conservar el orden de inserción, porque no expresa el orden escolar requerido y puede variar con la historia de operaciones.

## Risks / Trade-offs

- [Un estudiante inactivo puede conservar un número ocupado después por otro activo] → Comprobar la unicidad al reactivar y permitir cambiar explícitamente su número mientras está inactivo.
- [Los nombres repetidos impiden identificar estudiantes por texto] → Usar exclusivamente `EstudianteId` para identidad y el nombre sólo para presentación.
- [Las vistas de solo lectura pueden ser copias para garantizar encapsulación] → Priorizar la protección de invariantes; optimizar sólo si una medición futura lo justifica.
- [No diseñar reconstrucción limita por ahora la integración con persistencia] → Resolverla en un cambio futuro dedicado cuando existan requisitos de almacenamiento.
- [La comparación secundaria de nombres puede variar según cultura si no se fija] → Elegir una comparación determinista en Core y cubrirla con pruebas sin alterar el nombre almacenado.

## Migration Plan

No hay datos ni modelo previo que migrar. La incorporación será aditiva dentro de Core; si la implementación se revierte, se retirarán únicamente los tipos y pruebas introducidos por este cambio.
