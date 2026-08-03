## Context

Core implementa el agregado `Grupo` y Data implementa su persistencia SQLite transaccional, pero hoy los consumidores tendrían que coordinarlos manualmente. La solución necesita una capa de aplicación no visual que defina los casos de uso y el puerto requerido, sin trasladar reglas del dominio a WPF ni hacer que Application conozca SQLite.

## Goals / Non-Goals

### Goals

- Proporcionar una API de aplicación síncrona para todos los flujos de gestión solicitados.
- Mantener reglas e invariantes exclusivamente en Core.
- Invertir la dependencia de persistencia mediante un puerto específico definido en Application.
- Mantener una única frontera de traducción de errores en el adaptador Data.
- Evitar que una instancia modificada sobreviva entre comandos si el guardado falla.
- Entregar snapshots inmutables, materializados y deterministas.
- Probar la orquestación sin SQLite y la adaptación de Data con SQLite real.

### Non-Goals

- Implementar WPF funcional, ViewModels, navegación o raíz de composición.
- Introducir un contenedor DI, API asíncrona, `CancellationToken`, caché o concurrencia.
- Cambiar reglas de Core, el esquema SQLite o la transacción de guardado de Data.
- Añadir asistencia, actividades, evaluación, reportes funcionales, importación o datos personales.
- Introducir repositorios genéricos, bus de comandos, mediador o framework de aplicación.

## Decisions

### Nuevo proyecto SistemaDocente.Application

Los casos de uso residirán en `SistemaDocente.Application`, con pruebas en `SistemaDocente.Application.Tests`. El grafo será:

- Application referencia únicamente Core.
- Data referencia Application y Core.
- Application.Tests referencia Application y Core.
- Data.Tests puede referenciar Data, Application y Core.

Core conservará únicamente el dominio. Se descarta colocar temporalmente los casos de uso en Core porque mezclaría invariantes con coordinación de persistencia, tratamiento de ausencia y modelos de salida.

### Puerto de persistencia específico

Application definirá exactamente este contrato síncrono:

```csharp
public interface IAlmacenamientoGrupos
{
    Grupo? Cargar(GrupoId grupoId);
    bool Existe(GrupoId grupoId);
    void Guardar(Grupo grupo);
}
```

Data hará que su adaptador SQLite implemente el puerto. Application no conocerá rutas, conexiones, SQL ni tipos de Data. Se descartan un contrato en Core, Application → Data y un repositorio genérico porque violan la dirección elegida o amplían el alcance.

### Fachada sin estado compartido

Una fachada equivalente a `GestionGrupoCasosUso` recibirá `IAlmacenamientoGrupos` por constructor y no conservará agregados entre llamadas.

Cada comando sobre un grupo existente seguirá esta secuencia:

1. Cargar una instancia fresca.
2. Lanzar `GrupoNoEncontradoException` si la carga devuelve ausencia.
3. Invocar exactamente una operación pública de Core.
4. Invocar `Guardar` exactamente una vez si Core termina correctamente, incluso en una operación idempotente aceptada.
5. Crear y devolver el snapshot únicamente después del guardado exitoso.

La regla de una sola operación pública de Core admite una excepción justificada: un comando de aplicación puede coordinar varias mutaciones de Core cuando juntas representan una única acción atómica del usuario. `EditarEstudiante` renombra y cambia el número sobre la misma instancia cargada y realiza un único guardado final. Si cualquiera de las mutaciones falla, no se guarda; si el guardado falla, no se devuelve éxito.

`CrearGrupo` construirá mediante `Grupo.Crear`, guardará exactamente una vez y sólo entonces devolverá el resultado. Las consultas cargarán y proyectarán sin guardar. Un error de dominio ocurre antes de `Guardar`; un fallo de persistencia impide devolver un resultado exitoso.

### Resultados y snapshots

Los resultados públicos serán exactos:

- `CrearGrupo`, `CargarGrupo` y `CambiarNombreGrupo` devuelven `GrupoDetalle`.
- `AgregarEstudiante`, `RenombrarEstudiante`, `CambiarNumeroLista`, `DesactivarEstudiante` y `ReactivarEstudiante` devuelven `EstudianteDetalle`.
- `EditarEstudiante` recibe grupo, estudiante, nombre y número, y devuelve `EstudianteDetalle` sólo después del guardado único.
- `Existe` devuelve `bool`.
- `ObtenerEstudiantesActivos` y `ObtenerTodosLosEstudiantes` devuelven `IReadOnlyList<EstudianteDetalle>`.

`GrupoDetalle` será un record inmutable con `GrupoId`, nombre visible e `IReadOnlyList<EstudianteDetalle>`. `EstudianteDetalle` será un record inmutable con `EstudianteId`, nombre visible, número de lista y estado activo. Cada proyección materializará una matriz nueva; no expondrá `Grupo`, `Estudiante` ni una colección interna.

Las dos consultas de estudiantes, y la colección contenida en `GrupoDetalle`, se ordenarán primero por número de lista, después por nombre visible y finalmente por `EstudianteId`. El último criterio elimina cualquier empate residual y hace el resultado determinista.

Crear grupo y agregar estudiante no aceptarán identidades. Los identificadores tipados de entidades existentes viajarán desde resultados previos, no desde campos para escritura manual del docente.

### Única frontera de traducción de errores

Application definirá `GrupoNoEncontradoException` y `ErrorPersistenciaAplicacionException`. La traducción ocurrirá una sola vez:

- El adaptador Data capturará errores propios de acceso, esquema, integridad o proveedor y lanzará `ErrorPersistenciaAplicacionException` con la excepción técnica como `InnerException`.
- La fachada Application no capturará ni volverá a envolver `ErrorPersistenciaAplicacionException`.
- `DomainValidationException` y `DomainConflictException` se conservarán sin traducción.
- Una carga ausente producirá `GrupoNoEncontradoException` en la fachada.
- `Existe` devolverá `false` únicamente ante ausencia real; cualquier error técnico seguirá siendo `ErrorPersistenciaAplicacionException`.

Data no expondrá `SqliteException` a los consumidores del puerto. Se descarta una excepción única porque impediría distinguir ausencia, dominio e infraestructura.

### Consistencia tras fallos

Application no intentará deshacer la mutación en memoria ni conservará esa instancia. Si `Guardar` falla, el comando falla y la instancia se descarta. Un comando posterior vuelve a invocar `Cargar` y recibe el último estado que el almacenamiento considere persistido.

Una prueba explícita configurará un doble con un estado persistido, hará que un comando modifique la instancia cargada y que `Guardar` falle, y luego ejecutará otro comando. El doble devolverá nuevamente el estado anterior y la prueba demostrará que Application no reutilizó la instancia modificada.

### Estrategia de pruebas

`SistemaDocente.Application.Tests` usará dobles manuales del puerto para verificar secuencia, conteo de guardados, comandos idempotentes, ausencia de guardado tras errores de dominio, resultados exactos, materialización, orden, identidades y descarte de instancias tras fallos. No referenciará Data, SQLite ni WPF.

`SistemaDocente.Data.Tests` usará SQLite real y archivos temporales para comprobar que el adaptador implementa el contrato y realiza la única traducción de errores, preservando `InnerException`. Las restricciones y el esquema SQLite seguirán cubiertos por las pruebas existentes.

### Composición futura de App.Wpf y Reporting

La lógica visual futura de App.Wpf dependerá de Application. Data sólo podrá utilizarse en la raíz de composición para construir el adaptador e inyectarlo; ventanas, controles y ViewModels no usarán clases concretas de Data. App.Wpf no referenciará `Microsoft.Data.Sqlite`.

Reporting podrá componerse posteriormente de acuerdo con la capacidad concreta que se diseñe. Este cambio no añade reportes funcionales ni decide una dependencia adicional para Reporting.

## Risks / Trade-offs

- Data pasa a depender de Application para implementar el puerto. Se mantiene Application libre de Data y se verifica que el grafo sea acíclico.
- La fachada síncrona podría bloquear una UI futura. La API asíncrona y `CancellationToken` quedan para un cambio posterior.
- Guardar tras cada comando incrementa escrituras. Se priorizan consistencia y sencillez para agregados pequeños.
- Un fallo de guardado sucede después de mutar una instancia local. La fachada la descarta y el siguiente comando carga una instancia fresca.
- Los snapshots duplican datos del dominio. Permanecen mínimos, inmutables y materializados para proteger el agregado.

## Migration Plan

1. Crear Application y Application.Tests y agregarlos a la solución.
2. Definir el puerto, los errores, los snapshots y la fachada en Application.
3. Implementar los comandos y consultas con carga fresca y guardado automático.
4. Adaptar Data al puerto y establecer allí la única traducción de errores.
5. Añadir pruebas unitarias de Application con dobles.
6. Añadir pruebas de contrato SQLite en Data.Tests.
7. Verificar referencias, restauración, formato, compilación y pruebas de toda la solución.

Rollback: retirar los dos proyectos nuevos y la implementación del puerto en Data. Core, el esquema y los datos SQLite permanecerán sin cambios.
