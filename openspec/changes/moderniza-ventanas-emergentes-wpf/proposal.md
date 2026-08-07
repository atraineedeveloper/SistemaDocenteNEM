# Proposal: Modernizar ventanas emergentes WPF

## Problema

Las superficies principales del Sistema Docente NEM ya comparten un lenguaje visual moderno, pero las ventanas secundarias conservan patrones anteriores, colores físicos aislados y formularios con jerarquía inconsistente. En `EditorEstudianteWindow` los labels declarados mediante `FormField` pueden no mostrarse porque el contenido externo compite con `UserControl.Content`.

## Objetivo

Modernizar las ventanas secundarias sin cambiar reglas de dominio ni flujos funcionales, y corregir el patrón reusable de campos para que todos los formularios tengan labels visibles, foco claro, espaciado consistente y controles legibles.

## Alcance

- `EditorEstudianteWindow`.
- `DetalleProyectoWindow`.
- `DetalleActividadWindow`.
- `ExpedienteEstudianteWindow`.
- `EditarEvaluacionCeldaWindow`.
- `DialogoMensajeWindow`.
- `Controls/FormField`.
- estilos WPF compartidos para ventanas secundarias.
- pruebas estructurales/regresión de App.Wpf.

No se rediseñan `MainWindow` ni las vistas principales de Grupo, Asistencia, Proyectos o Evaluación.

## Criterios de aceptación

- todo campo de formulario tiene label visible y no depende de placeholder/tooltip;
- `FormField` conserva su chrome interno y recibe el control editable mediante una propiedad de contenido propia;
- las seis ventanas usan recursos semánticos y no colores hexadecimales locales;
- existe una sola acción primaria visual por contexto;
- acciones destructivas se distinguen sin depender sólo del color;
- `Esc` cancela/cierra cuando corresponde y `Ctrl+S` guarda en editores que persisten cambios;
- los formularios largos usan un único scroll vertical razonable;
- las ventanas mantienen `Owner`, `CenterOwner`, `ShowInTaskbar=False` y redimensionamiento cuando el contenido lo justifica;
- Claro, Oscuro y Alto contraste siguen resolviendo colores mediante `DynamicResource`;
- build y pruebas deben pasar en Windows antes del merge.
