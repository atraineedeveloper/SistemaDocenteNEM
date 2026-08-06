# Proposal: Accesibilidad y Calidad de Interfaz Transversal

## Summary
Implementar mejoras transversales de accesibilidad, usabilidad y rendimiento en toda la interfaz WPF del Sistema Docente NEM. Incluye navegación por teclado estructurada (`TabIndex`, atajos Alt y Enter/Escape), contraste de color adecuado con indicadores no dependientes únicamente del color, soporte para escalado de DPI en Windows, adaptabilidad en resoluciones compactas (800x600 y superiores) con barras de desplazamiento limpias, mensajes de error contextuales claros y pruebas de rendimiento visual con listas de 40 o más estudiantes.

## Intent
- Asegurar que el docente pueda operar todo el sistema utilizando exclusivamente el teclado sin depender del mouse.
- Garantizar que la interfaz sea utilizable en monitores escolares de baja resolución (1024x768 / 800x600) y con escalado de Windows al 125% o 150%.
- Ofrecer indicadores visuales claros (combinando colores accesibles de alto contraste con texto/iconos explicativos).
- Confirmar fluidez y rendimiento óptimo al gestionar grupos con listas de 40 estudiantes sin congelamientos ni desbordamientos de la ventana.

## Proposed Changes

### Capa Presentación y WPF (`SistemaDocente.App.Wpf`)
- **Navegación por Teclado y TabIndex:** Establecer una secuencia lógica de `TabIndex` e `IsTabStop` en todos los controles de `MainWindow.xaml`, `ExpedienteEstudianteWindow.xaml`, etc.
- **Atajos Teclado y Mnemónicos:** Agregar mnemónicos (`_Nombres`, `_Guardar`, etc.) y KeyBindings (e.g. `Escape` para cancelar edición, `Ctrl+N` para agregar estudiante, `F5` o `Enter` para guardar).
- **Etiquetas e Identificadores Accesibles:** Configurar `AutomationProperties.Name` y `Target` de etiquetas para lectores de pantalla.
- **Adaptabilidad a Resoluciones:** Envolver paneles principales en `ScrollViewer` con `CanContentScroll="True"` y definir `MinHeight` / `MinWidth` razonables.
- **Contraste e Indicadores Multimodales:** Asegurar un ratio de contraste >= 4.5:1 en textos e incluir símbolos/insignias textuales ("(Inactivo)", "(Falta)", "(Presente)") además de los colores de estado.
- **Estados de Carga y Error:** Mensajes de error específicos en banner contextual con iconos y atajo para descartar.

### Capa Pruebas (`SistemaDocente.App.Wpf.Tests`)
- Añadir pruebas automatizadas para verificar secuencias de tabulación y generación de datos de prueba para grupos de 40 estudiantes.

## System Capability Impact
- **Capabilities Added:** Soporte completo de navegación por teclado, accesibilidad de contraste/lectores de pantalla, adaptabilidad a pantallas pequeñas e indicadores multimodales.
- **User Experience:** Experiencia docente fluida, rápida, sin fatiga visual y operable al 100% con teclado.
