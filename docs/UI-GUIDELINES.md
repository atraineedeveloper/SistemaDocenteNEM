# UI-GUIDELINES.md
## Sistema Docente NEM — Guía de diseño de interfaz y experiencia de usuario

**Estado:** norma interna de UI/UX  
**Ámbito:** `SistemaDocente.App.Wpf` y toda pantalla futura de la aplicación  
**Plataforma:** Windows Desktop / WPF / .NET  
**Objetivo:** mantener una interfaz moderna, consistente, accesible, rápida de operar y predecible durante el trabajo docente cotidiano.

---

## 1. Principio rector

La interfaz debe sentirse como una aplicación moderna de Windows, no como un formulario administrativo antiguo ni como una hoja de cálculo disfrazada.

Toda decisión visual debe priorizar, en este orden:

1. Comprensión inmediata.
2. Prevención de errores.
3. Rapidez de uso.
4. Consistencia.
5. Accesibilidad.
6. Estética.
7. Densidad de información.

Cuando exista conflicto entre «se ve bonito» y «es claro, rápido, consistente y seguro», debe ganar la segunda opción.

---

## 2. Referencias de diseño

La aplicación adopta como referencias:

- Microsoft Fluent 2;
- patrones actuales de aplicaciones Windows;
- accesibilidad de Windows y WPF;
- WCAG 2.2 como referencia adicional;
- principios de usabilidad: visibilidad del estado, consistencia, prevención de errores, reconocimiento antes que memoria y control del usuario.

WPF no tiene que reproducir WinUI exactamente. Debe adoptar el lenguaje visual y de interacción que resulte viable sin añadir dependencias UI innecesarias.

---

## 3. Personalidad visual

La aplicación debe sentirse profesional, tranquila, clara, moderna, confiable y académica sin ser burocrática.

Evitar:

- exceso de color;
- degradados decorativos;
- sombras grandes;
- bordes oscuros pesados;
- tarjetas dentro de tarjetas sin necesidad;
- controles gigantes;
- iconos decorativos que no aporten significado;
- patrones visuales distintos para acciones equivalentes.

---

## 4. Sistema de diseño y tokens

Los valores visuales deben centralizarse en recursos WPF y reutilizarse. La implementación vigente usa `DesignTokens.xaml` y temas claro, oscuro y alto contraste; las vistas nuevas deben consumir esos recursos en lugar de introducir colores aislados.

### Espaciado

Usar una escala base de 4 px, priorizando múltiplos de 8:

| Token conceptual | Valor | Uso |
| --- | ---: | --- |
| XXS | 4 | ajustes mínimos |
| XS | 8 | controles relacionados |
| S | 12 | label ↔ control |
| M | 16 | padding de panel |
| L | 24 | separación de secciones |
| XL | 32 | bloques principales |
| XXL | 48 | casos especiales |

Reglas:

- controles relacionados: 8–12 px;
- secciones distintas: 16–24 px;
- padding habitual de panel/tarjeta: 16 px;
- padding exterior de vistas: 20–24 px;
- evitar valores arbitrarios repetidos como 13, 17, 19 o 27 px.

### Tipografía

Fuente principal: `Segoe UI Variable` cuando esté disponible; fallback `Segoe UI`.

| Rol | Tamaño orientativo | Peso |
| --- | ---: | --- |
| Título de pantalla | 24–28 | SemiBold |
| Título de sección | 18–20 | SemiBold |
| Subtítulo | 15–16 | SemiBold |
| Texto normal | 13–14 | Regular |
| Texto secundario | 12–13 | Regular |
| Etiqueta compacta | 12 | Medium |

Usar sentence case. Evitar mayúsculas completas, exceso de negritas y texto centrado cuando deba leerse rápidamente.

### Color

Usar recursos semánticos, no colores físicos repartidos por XAML. Ejemplos:

- `PrimaryBrush`;
- `CardBackgroundBrush`;
- `SectionBackgroundBrush`;
- `TextPrimaryBrush`;
- `TextSecondaryBrush`;
- `BorderDefaultBrush`;
- `ErrorBrush`;
- recursos de éxito, advertencia, selección y foco.

La paleta institucional puede evolucionar; las vistas no deben acoplarse a hexadecimales concretos.

No transmitir un estado únicamente mediante color.

---

## 5. Geometría y tamaños

Radios recomendados:

- controles pequeños: 4–6 px;
- tarjetas/paneles: 8 px;
- diálogos/ventanas de detalle: 8–12 px cuando aplique.

Preferir separación por espacio y superficie antes que sombras.

Alturas orientativas:

- botón estándar: 36–40 px;
- campo de texto/selector: 36–40 px;
- fila de `DataGrid`: 38–44 px;
- botón iconográfico: objetivo cómodo de 32–36 px como mínimo visual.

Los controles de una misma barra deben compartir altura.

---

## 6. Layout WPF

Usar `Grid` como sistema principal de layout.

Evitar:

- `Canvas` para formularios normales;
- tamaños rígidos innecesarios;
- márgenes usados para simular columnas;
- `StackPanel` gigantes que impidan redimensionamiento correcto;
- varias zonas con scroll independiente cuando una sola superficie de trabajo sería más clara.

Usar:

- ancho fijo sólo para paneles que realmente necesiten estabilidad;
- `Auto` para contenido pequeño;
- `*` para contenido que debe crecer.

La interfaz debe soportar redimensionamiento sin solapamientos, recortes, pérdida de acciones ni scroll horizontal general innecesario.

---

## 7. Navegación principal

La navegación de `MainWindow` debe ser estable, predecible, persistente y de baja profundidad.

Módulos principales actuales incluyen:

- Grupo;
- Asistencia;
- Proyectos;
- Evaluación.

Los módulos futuros deben integrarse sin mover arbitrariamente los existentes.

Reglas:

- mostrar claramente el módulo activo;
- conservar el contexto al volver a un módulo cuando sea razonable;
- no duplicar la navegación global dentro de ventanas de detalle;
- no crear ventanas innecesarias;
- sí usar ventanas dedicadas cuando aíslen una tarea compleja y reduzcan carga cognitiva.

---

## 8. Selección del patrón de interacción

### 8.1 Lista principal

Usar una vista amplia dentro de `MainWindow` cuando la tarea sea principalmente buscar, filtrar, comparar, seleccionar o consultar un panorama general.

Ejemplos vigentes:

- Grupo → lista de estudiantes;
- Proyectos → lista de proyectos;
- Asistencia → grilla mensual;
- Evaluación → superficie completa de evaluación.

La lista principal no debe contener además un formulario extenso y otras jerarquías editables si eso reduce el espacio útil.

### 8.2 Ventana dedicada de detalle

Usar una ventana dedicada cuando la entidad requiera varios campos editables, validación, acciones de ciclo de vida, contenido secundario o concentración del usuario.

Ejemplos vigentes:

- `EditorEstudianteWindow`;
- `DetalleProyectoWindow`;
- `DetalleActividadWindow`;
- `ExpedienteEstudianteWindow`.

Las ventanas de detalle deben:

- tener `Owner`;
- abrir centradas respecto de su propietario cuando corresponda;
- normalmente usar `ShowInTaskbar=False`;
- proteger cambios pendientes;
- conservar el contexto de la pantalla que las abrió;
- tener un propósito principal claro;
- evitar duplicar la navegación global;
- ser redimensionables si el contenido lo justifica.

### 8.3 Pantalla principal especializada

Usar una superficie principal amplia cuando la tarea sea frecuente, intensiva o necesite mucho espacio.

Ejemplos: asistencia mensual y evaluación de estudiantes.

No comprimir esas tareas dentro de una ventana pequeña o una columna lateral estrecha.

### 8.4 Master-detail

Master-detail es opcional, no una regla del sistema.

Puede usarse sólo cuando lista y detalle necesiten verse simultáneamente, el detalle sea breve, exista poca edición y ambos paneles mantengan espacio suficiente.

No usar master-detail cuando:

- comprima formularios;
- produzca tres zonas estrechas;
- obligue a varios scrolls simultáneos;
- reduzca una grilla operativa importante;
- mezcle planeación, edición y evaluación en el mismo espacio;
- aumente la carga cognitiva frente a ventanas enfocadas.

**Regla:** la jerarquía del dominio no obliga a copiarse literalmente en el layout visual.

---

## 9. Encabezados, acciones y command bars

Cada vista principal debe tener título, contexto relevante y una acción primaria clara cuando exista.

Por zona debe existir normalmente una sola acción primaria visual.

Preferir etiquetas que describan el resultado:

- `Guardar proyecto`;
- `Nueva actividad`;
- `Agregar estudiante`;
- `Marcar entregada`.

Evitar etiquetas vagas como `Aceptar`, `Procesar` o `Ejecutar`.

Acciones destructivas deben diferenciarse visualmente y requerir confirmación sólo cuando el daño no sea fácil de revertir.

---

## 10. Formularios

- label visible encima del control en formularios verticales;
- no usar placeholder como sustituto de label;
- agrupar campos relacionados;
- mostrar el formato esperado cuando pueda haber duda;
- validar sin destruir la captura;
- conservar los valores ante error;
- separar formularios largos en secciones coherentes;
- evitar formularios con demasiados campos en una sola fila.

El control `FormField` y los patrones de validación existentes deben reutilizarse cuando correspondan.

---

## 11. DataGrid y tablas

Las tablas son componentes de primera clase del sistema.

Reglas:

- encabezados claros;
- identidad del estudiante siempre fácil de localizar;
- selección evidente;
- virtualización cuando la lista pueda crecer;
- scroll horizontal sólo cuando la tabla realmente lo exija;
- columna de alumno flexible;
- columnas de estado compactas;
- observaciones con espacio suficiente;
- evitar exceso de líneas verticales.

No usar `ComboBox` permanente en todas las celdas cuando un selector temporal o teclado sea más rápido.

Estado = color + texto/letra/icono; nunca sólo color.

---

## 12. Grillas temporales

Para asistencia u otros datos calendario:

- mostrar sólo fechas relevantes;
- conservar fecha real;
- usar encabezados compactos;
- indicar agrupación temporal con espacio o separador;
- no crear columnas vacías decorativas.

La grilla mensual de asistencia debe conservar su separación semanal real y sus días lectivos visibles.

---

## 13. Teclado y foco

Patrones base:

- `Tab`: siguiente control;
- `Shift+Tab`: anterior;
- flechas: navegación dentro de listas/grillas;
- `Enter`: activar o confirmar;
- `Escape`: cancelar edición contextual;
- `Ctrl+S`: guardar cuando corresponda.

### Regla crítica

Atajos de una sola letra como `P/F/R/J/E/N` sólo pueden funcionar cuando el foco está en el componente operativo correspondiente.

Nunca deben interceptar escritura en `TextBox`, `RichTextBox`, editores de `DataGrid` u otros controles de texto.

Todo control interactivo debe tener foco visible. No eliminar `FocusVisualStyle` sin un reemplazo accesible.

---

## 14. Accesibilidad

Objetivo interno: experiencia equivalente a un nivel AA razonable.

Requisitos:

- navegación por teclado;
- foco visible;
- contraste suficiente;
- estados no expresados sólo por color;
- `AutomationProperties.Name` donde aporte accesibilidad;
- controles estándar cuando sea posible;
- texto escalable;
- mensajes comprensibles;
- prueba con tema de alto contraste.

No crear controles custom cuando uno estándar resuelva adecuadamente la necesidad.

---

## 15. Feedback, errores y cambios pendientes

Toda operación importante debe comunicar estado:

- Sin cambios;
- Cambios sin guardar;
- Guardando…;
- Guardado;
- Error al guardar.

No usar ventanas emergentes para cada guardado correcto; preferir feedback discreto o notificación.

Nunca mostrar al usuario SQL, stack traces, rutas internas, `InnerException` ni códigos técnicos sin significado.

Patrón obligatorio cuando se va a perder una edición:

```text
Tienes cambios sin guardar.

[Guardar] [Descartar] [Cancelar]
```

`Cancelar` debe dejar al usuario exactamente donde estaba.

---

## 16. Estados vacíos

Una pantalla vacía debe explicar:

1. qué falta;
2. para qué sirve;
3. cuál es el siguiente paso.

Usar el patrón `EmptyState` existente cuando corresponda. No mostrar únicamente una tabla vacía.

---

## 17. Temas e internacionalización

La interfaz vigente soporta temas claro, oscuro y alto contraste. Las vistas nuevas deben usar `DynamicResource` para recursos susceptibles de cambio de tema.

Las cadenas de UI reutilizables deben preferir recursos localizados cuando el patrón existente así lo haga. Las ventanas deben conservar `xml:lang="es-MX"`.

No hardcodear colores que sólo funcionen en tema claro.

---

## 18. Rendimiento percibido y escalado

Aunque la aplicación sea local:

- evitar congelar la interfaz sin feedback;
- virtualizar listas y `DataGrid` grandes;
- evitar reconstrucciones visuales innecesarias;
- no cargar recursos pesados antes de necesitarlos.

Probar visualmente a 100 %, 125 % y 150 % de escalado; 200 % cuando sea razonable.

Comprobar texto no cortado, botones visibles, tablas utilizables, diálogos completos y ausencia de solapamientos.

---

## 19. Organización de XAML

Los estilos comunes deben centralizarse. La estructura actual de `Themes/` y `DesignTokens.xaml` debe reutilizarse antes de crear nuevos diccionarios.

`MainWindow.xaml` actúa como shell. Las áreas funcionales grandes deberían extraerse gradualmente a `UserControl` o vistas dedicadas cuando su tamaño dificulte mantenimiento, sin introducir un framework de navegación ni mover lógica de negocio a code-behind.

El code-behind puede manejar comportamiento propio de WPF —foco, apertura de ventanas, interacción visual, routing de teclado contextual— pero no reglas de dominio ni SQL.

---

## 20. Prohibiciones para agentes de UI

Un agente NO DEBE:

- rediseñar una pantalla aprobada sin autorización;
- imponer master-detail porque la jerarquía de dominio tenga varios niveles;
- inventar colores por pantalla;
- introducir tamaños arbitrarios repetidos;
- crear estilos inline duplicados;
- usar ancho fijo para todo;
- usar `Canvas` para layout normal;
- añadir librerías UI sin aprobación;
- usar globalmente atajos de una letra;
- esconder información crítica sólo en tooltip;
- usar placeholders como labels;
- eliminar foco visual;
- mostrar IDs internos;
- mostrar errores técnicos;
- colocar lógica de negocio en code-behind;
- crear SQL en ViewModels;
- cambiar reglas funcionales para mejorar la estética.

---

## 21. Proceso obligatorio para nuevas pantallas

### Paso 1 — Wireframe

Antes de escribir XAML, el agente debe presentar:

- jerarquía;
- layout;
- dimensiones relativas;
- controles;
- acciones;
- scroll;
- comportamiento al redimensionar;
- estado vacío;
- errores;
- navegación por teclado.

### Paso 2 — Aprobación

El wireframe se corrige hasta quedar aprobado.

### Paso 3 — Implementación

Implementar exactamente el wireframe aprobado y reutilizar recursos existentes.

### Paso 4 — Prueba funcional

Comprobar bindings, comandos, cambios pendientes, teclado y apertura real de la ventana.

### Paso 5 — Auditoría visual

Revisar la ventana real buscando alineación, densidad, jerarquía, recortes, espacios muertos, scroll incorrecto, inconsistencia, foco y escalado.

---

## 22. Prompt base para agentes

Antes de modificar WPF:

```text
Lee docs/UI-GUIDELINES.md completo.

Estas reglas son obligatorias.

No implementes todavía.

Primero presenta un wireframe textual con:
- jerarquía;
- layout;
- dimensiones relativas;
- espaciado;
- controles;
- estados;
- teclado;
- scroll;
- redimensionamiento;
- estado vacío;
- errores;
- acciones principales y secundarias.

No asumas master-detail.
Elige lista principal, ventana dedicada o pantalla especializada según la tarea.
Reutiliza los recursos y patrones existentes.
Espera aprobación antes de modificar XAML.
```

Después de aprobarlo:

```text
Implementa exactamente el wireframe aprobado y docs/UI-GUIDELINES.md.
No rediseñes.
No cambies reglas funcionales.
Centraliza estilos reutilizables.
Ejecuta build y pruebas.
Reporta cualquier aspecto visual que no pueda validarse automáticamente.
```

---

## 23. Checklist de auditoría visual

- [ ] ¿Se entiende en pocos segundos qué pantalla es y qué acción es principal?
- [ ] ¿Los controles están alineados y usan espaciado consistente?
- [ ] ¿No hay espacios muertos o zonas comprimidas sin justificación?
- [ ] ¿No hay texto truncado?
- [ ] ¿Controles equivalentes comparten tamaño y estilo?
- [ ] ¿Los campos editables parecen editables?
- [ ] ¿Las tablas muestran claramente al estudiante y sus estados?
- [ ] ¿`Tab` sigue un orden lógico?
- [ ] ¿`Ctrl+S` funciona donde corresponde?
- [ ] ¿Los atajos de letras no interfieren con escritura?
- [ ] ¿El foco es visible?
- [ ] ¿Hay estados vacío, error, guardado y cambios pendientes?
- [ ] ¿La ventana funciona al redimensionar?
- [ ] ¿Funciona razonablemente a 125–150 % de escalado?
- [ ] ¿Los temas claro, oscuro y alto contraste mantienen legibilidad?

---

## 24. Definition of Done de UI

Una pantalla no se considera terminada hasta que:

- [ ] existe wireframe aprobado;
- [ ] cumple esta guía;
- [ ] reutiliza estilos y tokens;
- [ ] no contiene bindings que fallen al abrir;
- [ ] la ventana puede abrirse realmente;
- [ ] teclado y foco funcionan;
- [ ] los atajos no interfieren con escritura;
- [ ] los cambios pendientes están protegidos;
- [ ] existen estados vacíos y de error;
- [ ] funciona al redimensionar;
- [ ] se revisó el escalado;
- [ ] no hay IDs ni errores técnicos visibles;
- [ ] build y pruebas pasan;
- [ ] se realizó auditoría visual manual.

---

## 25. Regla final

El diseño moderno del Sistema Docente NEM no depende de efectos visuales. Depende de jerarquía clara, interacción predecible, excelentes estados, espaciado consistente, accesibilidad y velocidad de trabajo.
