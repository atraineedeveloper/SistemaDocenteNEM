# Design: Ventanas emergentes modernas WPF

## Principios

Se reutilizan `docs/UI-GUIDELINES.md`, `DesignTokens.xaml`, tipografía, botones y recursos semánticos existentes. No se agrega ninguna librería UI ni navegación nueva.

La estructura común de una ventana secundaria será:

```text
┌─────────────────────────────────────────────────────────────┐
│ CONTEXTO / EYEBROW                                          │
│ Título de la tarea                                          │
│ Ayuda breve o estado                                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ Sección                                               │  │
│  │ Label                                                 │  │
│  │ [ control                                            ]│  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│                   un único scroll cuando sea necesario      │
├─────────────────────────────────────────────────────────────┤
│                                      [Cancelar] [Primaria]  │
└─────────────────────────────────────────────────────────────┘
```

## FormField

`FormField` deja de utilizar `UserControl.Content` como contenido editable. Mantiene su `Grid` interno y expone una DP `FieldContent`, marcada como propiedad de contenido XAML mediante `ContentPropertyAttribute`.

Esto permite conservar la sintaxis:

```xml
<controls:FormField Header="Nombre">
    <TextBox ... />
</controls:FormField>
```

sin reemplazar visualmente el label y el mensaje de error.

## Estilos compartidos

Se crea `Styles/PopupStyles.xaml`, cargado localmente por las ventanas secundarias para no interferir con el mecanismo de `ThemeService`. Contiene únicamente estilos reutilizables de chrome de diálogo, encabezado, footer, badges, tabs, DatePicker y acciones destructivas. Todos los colores dependen de `DynamicResource`.

## EditorEstudianteWindow

- encabezado con contexto y título;
- cards para datos personales, escolares y observaciones;
- labels siempre visibles mediante `FormField`;
- DatePicker con altura/alineación consistente;
- footer fijo con `Cancelar` y `Guardar estudiante`;
- `Ctrl+S` enlazado al comando de guardado;
- `Esc` permanece mediante `IsCancel`;
- se elimina el guardado por Enter declarado individualmente en cada TextBox para evitar confirmaciones accidentales.

## DetalleProyectoWindow

- encabezado con nombre y estado;
- card de datos del proyecto;
- aviso de duración con recurso semántico;
- acciones de ciclo de vida separadas de la acción primaria de guardado;
- card de actividades con búsqueda etiquetada y lista;
- footer fijo con `Cerrar` + `Guardar cambios`;
- `Ctrl+S` guarda proyecto.

## DetalleActividadWindow

- contexto de proyecto y actividad;
- formulario en una card;
- acciones de ciclo de vida secundarias/destructivas separadas;
- footer fijo `Cerrar` + `Guardar actividad`;
- `Ctrl+S` guarda actividad.

## ExpedienteEstudianteWindow

- encabezado del estudiante usando recursos semánticos;
- tabs conservados, con presentación más limpia;
- fortalezas, dificultades, apoyos, observaciones y acuerdos usan labels visibles para captura;
- listas/alertas eliminan colores físicos y usan tokens semánticos;
- footer consistente.

## EditarEvaluacionCeldaWindow

- contexto de actividad y estudiante;
- card de edición con labels visibles;
- ayuda sobre persistencia local;
- acción primaria renombrada a `Aplicar a la matriz` para describir el resultado real;
- `Cancelar` mantiene la celda sin cambios confirmados por el diálogo.

## DialogoMensajeWindow

- mantiene su API y lógica;
- usa chrome compartido, icono semántico y footer consistente;
- acción afirmativa es la primaria;
- conserva `OK`, `OKCancel`, `YesNo` y `YesNoCancel`.

## No cambios funcionales

No se modifica Core, Application, Data, persistencia, reglas de proyecto/actividad/evaluación ni contratos de ViewModel. El cambio es de presentación, accesibilidad y regresión visual/estructural.
