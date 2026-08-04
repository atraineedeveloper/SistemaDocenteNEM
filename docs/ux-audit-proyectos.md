# Auditoría UI/UX - Módulo de Proyectos Didácticos

## Resumen Ejecutivo

La interfaz actual del módulo de proyectos presenta **problemas críticos de usabilidad** que dificultan la gestión eficiente de proyectos didácticos, actividades y evaluaciones. El diseño actual es funcional pero carece de principios básicos de UX para docentes de primaria.

---

## 1. Problemas Críticos Identificados

### 1.1 Layout y Distribución del Espacio

**Ubicación:** `MainWindow.xaml` líneas 134-197

#### Problemas:
- **Tres paneles horizontales fijos** (240px | 310px | *) con GridSplitters que obligan al usuario a redimensionar manualmente
- **Espacio mal distribuido**: El panel de actividades (centro, 310px) es muy estrecho para mostrar información completa
- **Scroll vertical anidado**: El panel central tiene ScrollViewer interno creando experiencia de desplazamiento confusa
- **Altura fija de ListBox**: `Height="220"` para actividades (línea 169) corta listas prematuramente

#### Impacto:
- Docentes pierden tiempo ajustando paneles en cada sesión
- Información importante queda oculta requiring scrolling constante
- Experiencia inconsistente entre diferentes resoluciones de pantalla

---

### 1.2 Jerarquía Visual Deficiente

#### Problemas:
- **Títulos del mismo tamaño**: "Proyectos" (24px), "Proyecto didáctico" (21px), "Actividad y entregas" (21px) compiten visualmente
- **Sin separación clara entre secciones**: Todo está apilado verticalmente sin agrupamiento semántico
- **Colores inconsistentes**: 
  - Warning de duración usa `#9A6700` (ámbar)
  - Otros mensajes usan `#B42318` (rojo)
  - Sin patrón claro de semántica de color

#### Ejemplo Código (líneas 156-165):
```xml
<TextBlock Text="Proyecto didáctico" FontSize="21" FontWeight="SemiBold"/>
<TextBlock Text="Nombre"/><TextBox .../>
<TextBlock Text="Descripción"/><TextBox .../>
<!-- 8 campos apilados sin agrupamiento -->
<TextBlock Foreground="#9A6700" .../> <!-- Advertencia aislada -->
```

---

### 1.3 Formularios Sobrecargados

#### Panel Central - Creación/Edición de Proyecto:
- **11 elementos verticales** sin paginación o tabs
- Campos críticos mezclados con secundarios
- **Botones de acción masivos**: 5 botones alineados horizontalmente (Guardar, Iniciar, Finalizar, Reabrir, Eliminar)
- **Sin confirmación visual de estado**: El usuario no sabe rápidamente en qué estado está el proyecto

#### Panel Derecho - Gestión de Actividades:
- **DataGrid con 6 columnas** incluyendo checkbox manual ("Sel.")
- **Estados representados como texto**: "E/N/P" requiere memorización
- **8 botones de acción** divididos en dos WrapPanels (línea 193)

---

### 1.4 Flujo de Trabajo Confuso

#### Problemas de Navegación:
1. **Selección en cascada poco clara**: 
   - Primero seleccionas proyecto (izquierda)
   - Luego actividad (centro) 
   - Luego entregas (derecha)
   - Sin breadcrumbs ni indicador de contexto

2. **Acciones destructivas accesibles fácilmente**:
   - "Eliminar proyecto" y "Eliminar actividad" están al mismo nivel que "Guardar"
   - Sin diálogo de confirmación visible en el código

3. **Estados no evidentes**:
   - No hay indicador visual prominente del estado actual (Borrador/EnCurso/Finalizado)
   - La advertencia de duración atípica es fácil de ignorar

---

### 1.5 Accesibilidad y Usabilidad

#### Problemas Detectados:
- **Sin atajos de teclado documentados** excepto Ctrl+S
- **Tab index no optimizado**: Salta entre paneles sin lógica clara
- **ToolTips insuficientes**: Solo 2 ToolTips en toda la interfaz
- **Contraste de color cuestionable**: Texto gris `#52606D` sobre fondos blancos
- **Tamaño de áreas clickeables**: Botones con padding mínimo (14,7)

#### DataGrid de Entregas (líneas 183-192):
```xml
<DataGridCheckBoxColumn Header="Sel." Binding="{Binding Seleccionada}" Width="45"/>
<!-- Checkbox manual para selección múltiple en 2024 -->
<DataGridTextColumn Header="Estado E/N/P" .../>
<!-- Abreviaturas crípticas para estados -->
```

---

## 2. Análisis por Componente

### 2.1 Panel Izquierdo - Lista de Proyectos

**Actual:**
```xml
<ListBox ItemsSource="{Binding ProyectosVisibles}" SelectedItem="{Binding ProyectoSeleccionado}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Margin="4">
                <TextBlock Text="{Binding Nombre}" FontWeight="SemiBold" TextWrapping="Wrap"/>
                <TextBlock Text="{Binding Estado}" Foreground="#52606D"/>
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

**Problemas:**
- Sin iconos de estado visual (solo texto)
- Sin información contextual (fechas, número de actividades)
- ComboBox de filtros arriba pero sin indicador de filtro activo

---

### 2.2 Panel Central - Detalle de Proyecto

**Campo de Fechas (líneas 159-162):**
```xml
<Grid>
    <StackPanel><TextBlock Text="Inicio"/><TextBox Text="{Binding FechaInicio}"/></StackPanel>
    <StackPanel Grid.Column="2"><TextBlock Text="Término"/><TextBox Text="{Binding FechaTermino}"/></StackPanel>
</Grid>
```

**Problemas:**
- **TextBox para fechas** en lugar de DatePicker → error prone
- Sin validación inline de formato
- La advertencia de duración viene DESPUÉS de los campos, no durante la edición

**Lista de Actividades (línea 169):**
```xml
<ListBox Height="220" ItemsSource="{Binding Actividades}" ...>
```

**Problemas:**
- Altura fija arbitraria
- Sin lazy loading para muchas actividades
- Búsqueda (`BusquedaActividad`) está DEBAJO de la lista, no arriba

---

### 2.3 Panel Derecho - Entregas y Evaluación

**DataGrid de Entregas:**
```xml
<DataGrid x:Name="GrillaEntregas" ...>
    <DataGrid.Columns>
        <DataGridCheckBoxColumn Header="Sel." Width="45"/>
        <DataGridTextColumn Header="Núm." Width="55"/>
        <DataGridTextColumn Header="Nombre" Width="*"/>
        <DataGridTextColumn Header="Situación" Width="130"/>
        <DataGridTextColumn Header="Estado E/N/P" Width="105"/>
        <DataGridTextColumn Header="Observación" Width="180"/>
    </DataGrid.Columns>
</DataGrid>
```

**Problemas Críticos:**
1. **Checkbox manual anticuado**: WPF tiene selección múltiple nativa
2. **"Estado E/N/P"**: Header confuso, debería ser "Estado Entrega"
3. **Columna Observación editable inline**: Sin validación, sin guardar automático
4. **Sin ordenamiento**: `CanUserSortColumns` no está habilitado
5. **Anchos fijos**: En diferentes resoluciones, el nombre del estudiante puede truncarse

**Botonera Inferior (línea 193):**
```xml
<WrapPanel DockPanel.Dock="Left">
    <Button Content="E Entregada"/>
    <Button Content="N No entregada"/>
    <Button Content="P Pendiente"/>
    <Button Content="Todos entregada"/>
</WrapPanel>
<WrapPanel DockPanel.Dock="Right">
    <Button Content="Anular"/>
    <Button Content="Eliminar"/>
    <Button Content="Descartar"/>
    <Button Content="Guardar (Ctrl+S)"/>
</WrapPanel>
```

**Problemas:**
- **8 botones primarios** sin jerarquía visual
- Etiquetas redundantes ("E Entregada" cuando podría ser solo "Marcar como Entregada")
- Acciones destructivas (Eliminar, Anular) junto a acciones cotidianas (Guardar)
- Sin confirmación para acciones masivas ("Todos entregada")

---

## 3. Principios UX Violados

| Principio | Violación | Severidad |
|-----------|-----------|-----------|
| **Ley de Fitts** | Botones pequeños, áreas de clic reducidas | Alta |
| **Jerarquía Visual** | Todo compite por atención | Alta |
| **Progresiva Revelación** | Todos los campos visibles siempre | Media |
| **Feedback Inmediato** | Estados no son evidentes | Alta |
| **Prevención de Errores** | TextBox para fechas, sin validación inline | Alta |
| **Consistencia** | Patrones diferentes entre paneles | Media |
| **Accesibilidad** | Sin soporte keyboard-first, contraste bajo | Media |

---

## 4. Recomendaciones Prioritarias

### 🔴 CRÍTICO (Sprint 1)

1. **Reemplazar TextBox de fechas con DatePicker**
   ```xml
   <DatePicker SelectedDate="{Binding FechaInicio}"/>
   ```

2. **Implementar selección múltiple nativa en DataGrid**
   ```xml
   <DataGrid SelectionMode="Extended" SelectionUnit="FullRow">
   ```

3. **Agrupar botones por contexto y peligro**
   - Separar acciones de guardado de acciones destructivas
   - Usar colores semánticos (rojo solo para eliminar)

4. **Añadir indicadores de estado visuales**
   - Iconos + color de fondo para estados de proyecto
   - Badges para contar actividades/entregas

### 🟡 ALTA PRIORIDAD (Sprint 2)

5. **Rediseñar layout con Tabs o Master-Detail**
   - Tab 1: Lista de proyectos
   - Tab 2: Detalle de proyecto + actividades
   - Tab 3: Gestión de entregas (solo cuando hay actividad seleccionada)

6. **Mejorar jerarquía tipográfica**
   ```xml
   <!-- Título principal -->
   <TextBlock FontSize="28" FontWeight="Bold" .../>
   <!-- Subtítulo de sección -->
   <TextBlock FontSize="18" FontWeight="SemiBold" .../>
   <!-- Labels de campo -->
   <TextBlock FontSize="13" FontWeight="Medium" .../>
   ```

7. **Mover búsqueda arriba de listas**
   - Patrón consistente: Filtro → Búsqueda → Lista

8. **Añadir breadcrumbs o header contextual**
   ```
   Grupo 3°A > Proyectos > [Nombre Proyecto] > [Nombre Actividad]
   ```

### 🟢 MEDIA PRIORIDAD (Sprint 3)

9. **Implementar atajos de teclado documentados**
   - Ctrl+N: Nuevo proyecto/actividad
   - Ctrl+E: Editar seleccionado
   - Delete: Eliminar (con confirmación)
   - F5: Actualizar lista

10. **Añadir ToolTips informativos**
    - Explicar estados
    - Mostrar fechas completas en hover
    - Explicar acciones masivas

11. **Validación inline con feedback visual**
    - Borde rojo en campos inválidos
    - Mensaje de error debajo del campo
    - Deshabilitar botón Guardar hasta que sea válido

12. **Confirmación para acciones destructivas**
    - Diálogo modal para Eliminar/Anular
    - Undo temporal después de eliminar

---

## 5. Propuesta de Rediseño (Wireframe Textual)

```
┌─────────────────────────────────────────────────────────────────┐
│  Sistema Docente Local                                          │
│  [Grupo] [Asistencia] [● Proyectos]                            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  PROYECTOS DIDÁCTICOS                                           │
│  Grupo: 3°A ▸ [▼ Selector de Proyecto]         [+ Nuevo]       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─ DETALLE DEL PROYECTO ───────────────────────────────────┐  │
│  │  [📊 Borrador]  "Proyecto de Lectura"                    │  │
│  │  📅 01/09/2024 - 20/09/2024 (20 días) ✓                  │  │
│  │                                                           │  │
│  │  Descripción:                                             │  │
│  │  [___________________________________________]            │  │
│  │                                                           │  │
│  │  [💾 Guardar]  [▶ Iniciar]  [⋮ Más acciones▼]           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─ ACTIVIDADES (3) ──────────────  [🔍 Buscar] [+ Nueva]     │  │
│  │  ┌─────────────────────────────────────────────────────┐   │  │
│  │  │ 📝 Actividad 1: Comprensión lectora                 │   │  │
│  │  │    📅 05/09/2024  ● Activa  |  25 estudiantes       │   │  │
│  │  └─────────────────────────────────────────────────────┘   │  │
│  │  ┌─────────────────────────────────────────────────────┐   │  │
│  │  │ 📝 Actividad 2: Vocabulario                         │   │  │
│  │  │    📅 10/09/2024  ● Activa  |  25 estudiantes       │   │  │
│  │  └─────────────────────────────────────────────────────┘   │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─ ENTREGAS: Actividad 1 ──────────────────────────────────┐  │
│  │  Filtro: [Todas ▼]  │  Total: 25 | ✓20 | ⏳3 | ✗2       │  │
│  │                                                           │  │
│  │  ┌────┬───────┬──────────────┬─────────┬─────────────┐   │  │
│  │  │ #  │ Estudiante │ Estado   │ Fecha   │ Observación │   │  │
│  │  ├────┼───────┼──────────────┼─────────┼─────────────┤   │  │
│  │  │ 1  │ Ana G. │ ✓ Entregada│ 05/09   │ [_______]   │   │  │
│  │  │ 2  │ Luis R.│ ⏳ Pendiente│ --      │ [_______]   │   │  │
│  │  └────┴───────┴──────────────┴─────────┴─────────────┘   │  │
│  │                                                           │  │
│  │  [Marcar Seleccionados▼]  [Exportar]                     │  │
│  │                               [💾 Guardar Cambios]       │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 6. Métricas de Éxito Post-Rediseño

| Métrica | Actual | Objetivo |
|---------|--------|----------|
| Tiempo para crear proyecto | ~2 min | < 45 seg |
| Clics para marcar entrega | 4-5 | 2 |
| Errores de fecha | Frecuentes | 0 |
| Satisfacción docente (SUS) | Desconocido | > 75 |
| Acciones destructivas accidentales | Posibles | 0 |

---

## 7. Archivos a Modificar

1. **`/workspace/src/SistemaDocente.App.Wpf/MainWindow.xaml`**
   - Líneas 134-197: Rediseño completo del Grid de Proyectos

2. **`/workspace/src/SistemaDocente.Presentation/GestionProyectosViewModel.cs`**
   - Añadir propiedades para UI mejorada
   - Comandos para acciones rápidas
   - Validación reactiva

3. **Nuevos archivos sugeridos:**
   - `ProyectosView.xaml`: UserControl dedicado
   - `ProyectoCardControl.xaml`: Template para items
   - `EntregaRowControl.xaml`: Template personalizado

---

## Conclusión

La interfaz actual **funciona pero no es usable**. Con las mejoras propuestas (especialmente las críticas del Sprint 1), se puede transformar en una herramienta intuitiva que reduzca la carga cognitiva de los docentes y prevenga errores comunes.

**Prioridad recomendada**: Comenzar con cambios incrementales (DatePicker, botones semánticos, indicadores de estado) antes de un rediseño completo del layout.
