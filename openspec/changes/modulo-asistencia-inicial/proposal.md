# Change: Módulo inicial de asistencia

## Why

El sistema ya permite administrar un grupo y sus estudiantes, pero todavía no permite registrar la asistencia diaria. Esta capacidad es el siguiente corte vertical necesario para que un docente pueda preparar, capturar, corregir y conservar la asistencia de una fecha sin trasladar reglas de dominio, SQL ni decisiones de persistencia a la interfaz WPF.

## What Changes

- Añadir el modelo de dominio mínimo de asistencia diaria, con una fecha, exactamente un estado cerrado por estudiante y rehidratación validada.
- Añadir casos de uso y contratos específicos de Application para consultar existencia, cargar, preparar y guardar una asistencia completa sin conservar estado entre llamadas.
- Evolucionar de forma segura y totalmente transaccional el esquema SQLite de la versión 1 a la versión 2 y persistir cada asistencia diaria de manera atómica.
- Conservar el padrón histórico completo de cada día guardado, incluidos estudiantes desactivados posteriormente, sin incorporar retroactivamente estudiantes nuevos.
- Añadir un ViewModel de asistencia y una interfaz WPF en español integrada mediante navegación mínima con la gestión de grupo existente.
- Incorporar detección de cambios sin guardar, confirmación Guardar/Descartar/Cancelar al cambiar de fecha o cerrar, conteos por estado y atajos de teclado.
- Añadir pruebas de dominio, aplicación, persistencia, presentación y composición para todos los escenarios aprobados.

## Capabilities

### New Capabilities

- `asistencia-diaria`: representa y protege las invariantes de una asistencia diaria y sus registros en Core.
- `casos-uso-asistencia`: coordina carga, preparación, existencia y guardado mediante snapshots y puertos neutrales en Application.
- `persistencia-sqlite-asistencia`: migra y valida el esquema SQLite y conserva asistencias completas mediante transacciones.
- `interfaz-asistencia`: presenta y edita la asistencia en WPF mediante MVVM, navegación mínima y confirmaciones de cambios pendientes.

### Modified Capabilities

- Ninguna.

## Impact

- **Core:** nuevos tipos de asistencia y pruebas de invariantes y rehidratación.
- **Application:** puerto, casos de uso, entradas, snapshots y pruebas de coordinación y errores.
- **Data:** esquema SQLite versión 2, adaptador de asistencia y pruebas con archivos temporales reales.
- **Presentation:** ViewModel, filas visuales, señal textual de estudiantes actualmente inactivos, comandos y pruebas sin abrir ventanas.
- **App.Wpf:** composición manual, navegación mínima, vista de asistencia y confirmaciones visuales.
- **Dependencias:** se conserva la dirección existente; Core no depende de capas externas, Application depende sólo de Core, Data implementa los puertos y WPF usa Data únicamente en la raíz de composición.
- **Compatibilidad:** una base válida en versión 1 se valida completamente y se migra a versión 2 en una sola transacción sin perder grupos ni estudiantes; cualquier fallo conserva versión 1 sin objetos parciales.
- **Datos históricos:** asistencia conserva identidad y estado, pero consulta nombre, número de lista y situación activa desde la matrícula actual; esta versión no guarda una fotografía histórica del nombre ni del número.
- **Fuera de alcance:** reportes, porcentajes, observaciones, justificantes, horarios, múltiples sesiones, exportación, sincronización y múltiples grupos.
