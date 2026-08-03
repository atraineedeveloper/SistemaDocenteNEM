## Purpose

Permite capturar y revisar asistencia mediante una grilla mensual rápida, accesible y segura, conservando temporalmente la vista diaria existente.

## ADDED Requirements

### Requirement: Selector mensual completo
La interfaz SHALL mostrar Mes anterior, selector de los doce meses en español, selector de año válido, Mes siguiente e Ir al mes actual. La selección SHALL cargar todos los días reales del mes y no SHALL usar un `DatePicker` estrecho como selector principal.

#### Scenario: Navegar entre diciembre y enero
- **WHEN** se avanza desde diciembre o retrocede desde enero
- **THEN** mes y año cambian correctamente

#### Scenario: Volver al mes actual
- **WHEN** se activa Ir al mes actual
- **THEN** se seleccionan el mes y año proporcionados por el reloj local

### Requirement: Grilla mensual principal
La vista predeterminada SHALL mostrar estudiantes en filas; Número y Nombre como columnas iniciales; una columna por fecha lectiva real de lunes a viernes; y Presentes, Faltas, Retardos, Faltas justificadas y Porcentaje al final. SHALL soportar 40 estudiantes, el máximo de fechas lectivas del mes, redimensionamiento, virtualización y desplazamiento horizontal.

#### Scenario: Agosto con 40 estudiantes
- **WHEN** se abre agosto con 40 filas visibles
- **THEN** existen únicamente las columnas de lunes a viernes, las filas conservan altura legible y la ventana sigue siendo utilizable

#### Scenario: Encabezado diario
- **WHEN** se muestra una fecha
- **THEN** su encabezado contiene número y abreviatura `L/M/M/J/V`

### Requirement: Encabezados y columnas persistentes
Los encabezados SHALL permanecer visibles durante el desplazamiento vertical. Número y Nombre SHALL permanecer congelados durante el desplazamiento horizontal mediante la capacidad estable de WPF. Los resúmenes SHALL permanecer al final del contenido desplazable.

#### Scenario: Desplazamiento hacia fin de mes
- **WHEN** el usuario desplaza horizontalmente hasta el último día
- **THEN** Número y Nombre continúan visibles y los resúmenes pueden alcanzarse al final

### Requirement: Fines de semana ausentes y separación semanal
Sábados y domingos no SHALL mostrarse como columnas. Un viernes SHALL mostrar un borde derecho neutro y más grueso sólo cuando exista otra fecha lectiva posterior. La separación SHALL depender del `DayOfWeek` real y no del número ordinal de columna, y no SHALL parecer una columna de datos.

#### Scenario: Navegar entre semanas
- **WHEN** la celda activa está en viernes y se navega a la fecha siguiente
- **THEN** la selección pasa directamente al lunes sin atravesar columnas de fin de semana

#### Scenario: Último viernes del mes
- **WHEN** un viernes es la última fecha lectiva representada
- **THEN** no muestra separación semanal a su derecha

### Requirement: Edición compacta y accesible
Cada celda editable SHALL mostrar `P`, `F`, `R` o `J` con texto accesible y estilo contrastado, sin un `ComboBox` permanente. Clic simple SHALL seleccionar; doble clic o `Enter` SHALL abrir un selector compacto; `Escape` SHALL cancelar esa edición y restaurar el valor inicial.

#### Scenario: Selector compacto
- **WHEN** se hace doble clic en una celda laborable
- **THEN** aparecen los cuatro estados y elegir uno actualiza sólo esa celda

### Requirement: Navegación por teclado
Flechas SHALL mover la celda activa; `P/F/R/J` SHALL asignar el estado y avanzar a la siguiente fila visible del mismo día; `Home/End` SHALL mover al primer/último día editable de la fila cuando sea posible; `Ctrl+S` SHALL guardar el día activo; `PageUp/PageDown` SHALL solicitar el mes anterior/siguiente fuera de edición.

#### Scenario: Pase vertical con letras
- **WHEN** se presiona `F` en una celda editable que no está en la última fila
- **THEN** se asigna Falta y la selección avanza a la fila siguiente de la misma fecha

#### Scenario: Última fila
- **WHEN** se asigna un estado mediante letra en la última fila visible
- **THEN** el estado cambia y la selección permanece en esa celda

### Requirement: Estado editable por fecha
El ViewModel SHALL conservar snapshot confirmado, copia editable y un conjunto explícito de fechas modificadas. Abrir el mes no SHALL marcar fechas ni persistir borradores. Una fecha SHALL salir del conjunto si vuelve completamente a su snapshot.

#### Scenario: Editar dos días
- **WHEN** se cambian celdas de dos columnas
- **THEN** el conjunto contiene exactamente esas dos fechas

### Requirement: Guardado del día y del mes
Guardar día SHALL ser la acción principal y persistir sólo la columna activa. SHALL estar habilitada para una fecha lectiva visible no persistida o para una fecha persistida modificada, y deshabilitada para una fecha persistida sin cambios. Tras cada éxito SHALL reemplazar inmediatamente el snapshot mensual confirmado de esa fecha, marcar su columna como persistida y retirar la fecha del conjunto modificado sin recargar todo el mes ni perder borradores de otras fechas. Guardar cambios del mes SHALL ser secundaria, procesar fechas modificadas en orden y actualizar el snapshot tras cada éxito. Ante fallo SHALL conservar edición de la fecha fallida y posteriores e informar éxitos previos; no SHALL afirmar atomicidad mensual.

#### Scenario: Día nuevo o modificado
- **WHEN** la columna activa es lectiva y no persistida, o está persistida pero tiene cambios locales
- **THEN** Guardar día está habilitado

#### Scenario: Día persistido sin cambios
- **WHEN** la columna activa ya está persistida y coincide con su snapshot confirmado
- **THEN** Guardar día está deshabilitado

#### Scenario: Confirmación inmediata del día
- **WHEN** Guardar día termina correctamente
- **THEN** la columna queda persistida y sin cambios pendientes inmediatamente, conservando los borradores de otras fechas sin recargar el mes

#### Scenario: Fallo en segundo día
- **WHEN** el primer día se confirma y falla el segundo
- **THEN** el primero aparece guardado, el segundo continúa modificado y ningún día posterior aparece confirmado

### Requirement: Acciones de columna
La interfaz SHALL mostrar la fecha activa completa en español y ofrecer Marcar todo el día como Presente, Guardar día y Descartar cambios del día. Estas acciones SHALL afectar únicamente la fecha seleccionada.

#### Scenario: Marcar columna
- **WHEN** se activa Marcar todo presente para un martes
- **THEN** cambian únicamente las celdas editables de ese martes

### Requirement: Confirmación al cambiar contexto
Cambiar mes/año, navegar a Grupo o cerrar con fechas modificadas SHALL reutilizar Guardar/Descartar/Cancelar. Guardar continuará sólo si todos los días pendientes tienen éxito; Descartar abandonará la edición sin guardar; Cancelar conservará mes, selección y edición.

#### Scenario: Fallo al guardar antes de cambiar mes
- **WHEN** se elige Guardar y una fecha falla
- **THEN** el mes no cambia y se conserva la edición no confirmada

### Requirement: Resúmenes y borradores distinguibles
Cada fila SHALL mostrar conteos y porcentaje según la fórmula aprobada. Las tarjetas superiores SHALL mostrar alumnos visibles, días guardados y conteos generales. Cuando cualquier conteo visual incluya cambios locales SHALL indicarlo explícitamente y no presentarlo como confirmado.

#### Scenario: Porcentaje sin denominador
- **WHEN** un estudiante no tiene días contabilizados
- **THEN** la grilla muestra `—` en vez de `0 %`

### Requirement: Búsqueda y filtros
La interfaz SHALL permitir búsqueda parcial por nombre y filtros Todos, Con incidencias, Sólo activos y Activos e inactivos históricos. Incidencia SHALL significar al menos una Falta, Retardo o Falta justificada visible. Filtrar no SHALL modificar datos ni el orden contractual.

#### Scenario: Filtrar incidencias
- **WHEN** se activa Con incidencias
- **THEN** sólo permanecen filas con al menos un estado F, R o J en el mes visual

### Requirement: Barra inferior fija
La barra inferior SHALL permanecer visible con estado de guardado, ayuda «P/F/R/J cambia el estado · Ctrl+S guarda», Descartar cambios, Guardar día y Guardar cambios del mes. Cada botón SHALL habilitarse según el día activo y las fechas modificadas.

#### Scenario: Sin día activo
- **WHEN** no hay una columna lectiva seleccionada
- **THEN** Guardar día y Marcar todo presente están deshabilitados

### Requirement: Vista diaria conservada temporalmente
La vista mensual SHALL ser predeterminada y la vista diaria existente SHALL permanecer disponible como modo alternativo dentro de la misma ventana hasta superar las pruebas manuales mensuales, sin duplicar reglas ni persistencia.

#### Scenario: Cambiar a vista diaria
- **WHEN** se activa el modo diario sin cambios pendientes
- **THEN** se muestra la captura diaria existente usando los mismos casos de uso

### Requirement: Errores seguros
Dominio, conflicto y persistencia SHALL producir mensajes corregibles en español sin SQL, rutas, causas internas ni trazas. Un error SHALL conservar la celda editada, no cerrar la aplicación y no marcar como guardado el día fallido.

#### Scenario: Error técnico al guardar día
- **WHEN** la persistencia falla
- **THEN** la celda conserva su edición local y el día permanece sin confirmar
