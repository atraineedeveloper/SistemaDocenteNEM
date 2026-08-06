# Auditoría Exhaustiva de Diseño UX/UI y Mejores Prácticas
## Sistema Docente Local - Aplicación de Gestión de Aulas de Primaria (NEM)

**Fecha de Auditoría:** Agosto 2025  
**Versión del Sistema:** Fundación Técnica Completa  
**Tecnología:** WPF (.NET 10), C#, SQLite  

---

## Executive Summary

La aplicación presenta una **base técnica sólida** con arquitectura limpia (MVVM, separación de capas, dominio independiente). El diseño visual sigue patrones Fluent Design modernos y existe conciencia de accesibilidad básica. Sin embargo, se identifican **oportunidades significativas de mejora** en experiencia de usuario, jerarquía visual, consistencia de patrones de diseño y alineación con el modelo pedagógico NEM (Nueva Escuela Mexicana).

---

## 1. EVALUACIÓN DE ARQUITECTURA DE INFORMACIÓN Y JERARQUÍA VISUAL

### 1.1 Estructura de Navegación Principal

**✅ ASPECTOS POSITIVOS:**
- Header fijo con navegación por pestañas claras (Grupo, Asistencia, Proyectos, Evaluación)
- Selector de grupo visible en todo momento
- Breadcrumbs implícitos mediante títulos de sección

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 1.1.1: Jerarquía Visual del Header
```xml
<!-- Actual: Todos los elementos compiten visualmente -->
<TextBlock Text="Sistema Docente Local" FontSize="18" FontWeight="SemiBold"/>
<ComboBox Width="180" .../>
<Button Content="+ Nuevo grupo"/>
<Button Content="_Grupo"/>
<Button Content="_Asistencia"/>
<Button Content="_Proyectos"/>
<Button Content="_Evaluación"/>
```

**Recomendación:**
```xml
<!-- Propuesta: Jerarquía clara con agrupamiento visual -->
<Border Background="#173F5F" Padding="20,12">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/> <!-- Logo + Título -->
            <ColumnDefinition Width="*"/>   <!-- Espaciador -->
            <ColumnDefinition Width="Auto"/> <!-- Selector Grupo -->
            <ColumnDefinition Width="Auto"/> <!-- Navegación -->
        </Grid.ColumnDefinitions>
        
        <!-- Columna 0: Branding -->
        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
            <Border Background="White" CornerRadius="6" Padding="8,6" Margin="0,0,16,0">
                <TextBlock Text="📚" FontSize="20"/>
            </Border>
            <StackPanel>
                <TextBlock Text="Sistema Docente Local" 
                          Foreground="White" FontSize="16" FontWeight="Bold"/>
                <TextBlock Text="Gestión de Aula - NEM" 
                          Foreground="#B0C4DE" FontSize="11"/>
            </StackPanel>
        </StackPanel>
        
        <!-- Columna 2: Selector de Grupo con etiqueta -->
        <StackPanel Grid.Column="2" Orientation="Horizontal" VerticalAlignment="Center">
            <TextBlock Text="Grupo:" Foreground="#B0C4DE" 
                      VerticalAlignment="Center" Margin="0,0,8,0"/>
            <ComboBox Width="200" .../>
            <Button Content="+ Nuevo" ... Margin="8,0,0,0"/>
        </StackPanel>
        
        <!-- Columna 3: Navegación principal -->
        <StackPanel Grid.Column="3" Orientation="Horizontal" Margin="24,0,0,0">
            <!-- Botones de navegación con iconos -->
        </StackPanel>
    </Grid>
</Border>
```

#### Problema 1.1.2: Falta de Indicador de Pestaña Activa
Los botones de navegación no muestran claramente cuál está activo.

**Recomendación:**
```xml
<Style x:Key="NavTabButton" TargetType="Button">
    <!-- Agregar estado visual para IsEnabled/IsActive -->
    <Setter Property="Opacity" Value="0.7"/>
    <Style.Triggers>
        <DataTrigger Binding="{Binding IsViewActive}" Value="True">
            <Setter Property="Opacity" Value="1.0"/>
            <Setter Property="Background" Value="#255177"/>
            <Setter Property="BorderThickness" Value="0,0,0,3"/>
            <Setter Property="BorderBrush" Value="#60A5FA"/>
        </DataTrigger>
    </Style.Triggers>
</Style>
```

---

### 1.2 Vista de Bienvenida / Onboarding

**✅ ASPECTOS POSITIVOS:**
- Mensaje claro de propósito
- Flujo simple de creación de grupo

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 1.2.1: Falta de Contexto Pedagógico NEM
El mensaje "Comienza escribiendo el nombre del grupo que administras" es genérico.

**Recomendación:**
```xml
<StackPanel MaxWidth="520">
    <Border Background="#EFF8FF" CornerRadius="8" Padding="16" Margin="0,0,0,20">
        <StackPanel>
            <TextBlock Text="🎯 Nueva Escuela Mexicana" 
                      FontWeight="Bold" Foreground="#175CD3" Margin="0,0,0,4"/>
            <TextBlock Text="Esta herramienta te ayuda a gestionar tu grupo de primaria 
                            bajo el modelo NEM, facilitando el seguimiento pedagógico 
                            y la evaluación formativa." 
                      TextWrapping="Wrap" Foreground="#344054" FontSize="13"/>
        </StackPanel>
    </Border>
    
    <TextBlock Text="¿Qué grupo vas a administrar hoy?" 
              FontSize="20" FontWeight="Bold" Foreground="#101828" Margin="0,0,0,8"/>
    <TextBlock Text="Ejemplo: &quot;3° A&quot;, &quot;Quinto Grado - Grupo B&quot;, 
                  &quot;Primaria Benito Juárez - 4°C&quot;" 
              Foreground="#64748B" FontSize="12" FontStyle="Italic" Margin="0,0,0,16"/>
    
    <!-- Resto del formulario... -->
</StackPanel>
```

#### Problema 1.2.2: Botón "Olvidar referencia" sin contexto
No está claro qué hace este botón para un usuario nuevo.

**Recomendación:**
```xml
<WrapPanel>
    <Button Content="_Crear grupo" IsDefault="True" 
            Style="{StaticResource PrimaryButton}"/>
    <Button Content="_Usar otro grupo" Command="{Binding OlvidarReferenciaCommand}"
            ToolTip="Seleccionar un grupo diferente de los guardados previamente"/>
</WrapPanel>
```

---

## 2. AUDITORÍA DE GESTIÓN DE GRUPO Y ESTUDIANTES

### 2.1 DataGrid de Estudiantes

**✅ ASPECTOS POSITIVOS:**
- Virtualización activada para rendimiento
- Estados visuales diferenciados (activo/inactivo)
- ContextMenu con acciones relevantes

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 2.1.1: Columnas sin Alineación Pedagógica NEM
Las columnas actuales son genéricas. Faltan datos relevantes para NEM.

**Columnas Actuales:**
```
Núm. | Nombre del Estudiante | Género | Edad | Estado
```

**Recomendación - Columnas Enriquecidas NEM:**
```
Nº Lista | Estudiante | Género | Edad | ADE (¿?) | Observaciones Clave | Estado
```

Donde ADE = Alumno con Diversidad Étnica o Necesidades Específicas (según NEM)

```xml
<DataGrid.Columns>
    <DataGridTextColumn Header="Nº Lista" Binding="{Binding NumeroLista}" Width="70"/>
    <DataGridTemplateColumn Header="Estudiante" Width="280">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <StackPanel>
                    <TextBlock Text="{Binding NombreCompleto}" FontWeight="SemiBold"/>
                    <TextBlock Text="{Binding CURP}" FontSize="10" Foreground="#64748B"
                              Visibility="{Binding MostrarCURP, Converter={StaticResource BoolToVisibility}}"/>
                </StackPanel>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
    <DataGridTextColumn Header="Género" Binding="{Binding GeneroTexto}" Width="90"/>
    <DataGridTextColumn Header="Edad" Binding="{Binding Edad}" Width="60"/>
    <DataGridTemplateColumn Header="ADE" Width="70" ToolTip="Alumno con Diversidad/Necesidades Específicas">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <Border Background="{Binding ColorADE}" CornerRadius="4" Padding="6,2"
                        Visibility="{Binding EsADE, Converter={StaticResource BoolToVisibility}}">
                    <TextBlock Text="ADE" FontSize="10" FontWeight="Bold" Foreground="White"/>
                </Border>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
    <DataGridTemplateColumn Header="Observaciones" Width="180">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding ResumenObservaciones}" 
                          TextTrimming="CharacterEllipsis" 
                          ToolTip="{Binding ObservacionesCompleto}"/>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
    <DataGridTextColumn Header="Estado" Binding="{Binding Estado}" Width="100"/>
</DataGrid.Columns>
```

#### Problema 2.1.2: Acciones Masivas Ocultas
Las acciones como "Desactivar" o "Reactivar" requieren selección individual.

**Recomendación:**
```xml
<WrapPanel Grid.Row="2" Margin="0,14,0,6">
    <!-- Acciones Individuales (requieren selección) -->
    <Button Content="_Ver Expediente" Command="{Binding VerExpedienteCommand}" 
            ToolTip="Ver expediente del estudiante seleccionado"/>
    <Button Content="_Agregar estudiante" Command="{Binding AbrirAgregarEstudianteCommand}"/>
    <Button Content="_Editar estudiante" Command="{Binding AbrirEditarEstudianteCommand}"
            ToolTip="Editar estudiante seleccionado"/>
    
    <Separator Width="1" Height="24" Margin="8,0" Background="#D0D5DD"/>
    
    <!-- Acciones Masivas -->
    <Menu Background="Transparent" VerticalAlignment="Center">
        <MenuItem Header="⚙️ Acciones _Masivas ▾">
            <MenuItem Header="📥 Exportar lista a CSV" Command="{Binding ExportarListaCommand}"/>
            <MenuItem Header="🖨️ Imprimir lista de asistencia" Command="{Binding ImprimirListaCommand}"/>
            <Separator/>
            <MenuItem Header="🚫 Desactivar seleccionados" Command="{Binding DesactivarSeleccionadosCommand}"/>
            <MenuItem Header="🔄 Reactivar seleccionados" Command="{Binding ReactivarSeleccionadosCommand}"/>
        </MenuItem>
    </Menu>
    
    <Button Content="📋 Seleccionar _todos" Command="{Binding SeleccionarTodosCommand}" Margin="4,0"/>
</WrapPanel>
```

---

### 2.2 Editor de Estudiantes (Formulario)

**✅ ASPECTOS POSITIVOS:**
- Layout en grid de 3 columnas eficiente
- Soporte de teclado (Enter/Escape)
- Observaciones con salto de línea

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 2.2.1: Campos Esenciales NEM Faltantes
El formulario actual no captura información relevante para el modelo NEM.

**Campos Actuales:**
```
Primer apellido | Segundo apellido | Nombres
Número de lista | Género | Fecha de nacimiento
Observaciones particulares
```

**Recomendación - Formulario Enriquecido NEM:**
```xml
<StackPanel>
    <TextBlock Text="{Binding TituloEditorEstudiante}" 
              Foreground="#101828" FontSize="19" FontWeight="SemiBold"/>
    
    <!-- Sección 1: Datos Básicos -->
    <TextBlock Text="Datos Personales" FontWeight="Bold" Margin="0,16,0,8" Foreground="#344054"/>
    <Border Background="#F8F9FA" CornerRadius="6" Padding="12" Margin="0,0,0,12">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="16"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="16"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <StackPanel Grid.Column="0">
                <TextBlock Text="Primer apellido *" Foreground="#344054" FontWeight="SemiBold"/>
                <TextBox x:Name="PrimerApellidoEdicion" TabIndex="60" 
                        Text="{Binding PrimerApellidoEdicion, UpdateSourceTrigger=PropertyChanged}"/>
            </StackPanel>
            
            <StackPanel Grid.Column="2">
                <TextBlock Text="Segundo apellido" Foreground="#344054" FontWeight="SemiBold"/>
                <TextBox TabIndex="61" Text="{Binding SegundoApellidoEdicion, UpdateSourceTrigger=PropertyChanged}"/>
            </StackPanel>
            
            <StackPanel Grid.Column="4">
                <TextBlock Text="Nombres *" Foreground="#344054" FontWeight="SemiBold"/>
                <TextBox TabIndex="62" Text="{Binding NombresEdicion, UpdateSourceTrigger=PropertyChanged}"/>
            </StackPanel>
        </Grid>
    </Border>
    
    <!-- Sección 2: Datos Escolares -->
    <TextBlock Text="Datos Escolares" FontWeight="Bold" Margin="0,8,0,8" Foreground="#344054"/>
    <Border Background="#F8F9FA" CornerRadius="6" Padding="12" Margin="0,0,0,12">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="16"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="16"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <StackPanel Grid.Column="0">
                <TextBlock Text="Número de lista *" Foreground="#344054" FontWeight="SemiBold"/>
                <TextBox TabIndex="63" Text="{Binding NumeroListaEdicion, UpdateSourceTrigger=PropertyChanged}"/>
            </StackPanel>
            
            <StackPanel Grid.Column="2">
                <TextBlock Text="Género *" Foreground="#344054" FontWeight="SemiBold"/>
                <ComboBox TabIndex="64" SelectedIndex="{Binding GeneroIndexEdicion, Mode=TwoWay}">
                    <ComboBoxItem Content="No especificado"/>
                    <ComboBoxItem Content="Hombre"/>
                    <ComboBoxItem Content="Mujer"/>
                </ComboBox>
            </StackPanel>
            
            <StackPanel Grid.Column="4">
                <TextBlock Text="Fecha de nacimiento *" Foreground="#344054" FontWeight="SemiBold"/>
                <DatePicker TabIndex="65" SelectedDate="{Binding FechaNacimientoEdicion, Mode=TwoWay}"/>
            </StackPanel>
        </Grid>
    </Border>
    
    <!-- Sección 3: Información NEM -->
    <TextBlock Text="Información para NEM" FontWeight="Bold" Margin="0,8,0,8" Foreground="#344054"/>
    <Border Background="#EFF8FF" CornerRadius="6" Padding="12" Margin="0,0,0,12" BorderBrush="#B2DDFF" BorderThickness="1">
        <StackPanel>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="16"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <StackPanel Grid.Column="0">
                    <TextBlock Text="¿Requiere adecuaciones curriculares?" Foreground="#175CD3" FontWeight="SemiBold"/>
                    <ComboBox TabIndex="66" SelectedIndex="{Binding RequiereAdecuaciones, Mode=TwoWay}">
                        <ComboBoxItem Content="No"/>
                        <ComboBoxItem Content="Sí - Temporal"/>
                        <ComboBoxItem Content="Sí - Permanente"/>
                    </ComboBox>
                </StackPanel>
                
                <StackPanel Grid.Column="2">
                    <TextBlock Text="Lengua materna" Foreground="#175CD3" FontWeight="SemiBold"/>
                    <ComboBox TabIndex="67" Text="{Binding LenguaMaterna, UpdateSourceTrigger=PropertyChanged}">
                        <ComboBoxItem Content="Español"/>
                        <ComboBoxItem Content="Náhuatl"/>
                        <ComboBoxItem Content="Maya"/>
                        <ComboBoxItem Content="Mixteco"/>
                        <ComboBoxItem Content="Zapoteco"/>
                        <ComboBoxItem Content="Otro"/>
                    </ComboBox>
                </StackPanel>
            </Grid>
            
            <TextBlock Text="Campo Formativo Prioritario" Foreground="#175CD3" FontWeight="SemiBold" Margin="0,10,0,4"/>
            <WrapPanel>
                <CheckBox Content="Lenguajes" IsChecked="{Binding CampoLenguajes}" Margin="0,0,12,4"/>
                <CheckBox Content="Saberes Científicos" IsChecked="{Binding CampoCientificos}" Margin="0,0,12,4"/>
                <CheckBox Content="Ética, Naturaleza y Sociedades" IsChecked="{Binding CampoEtica}" Margin="0,0,12,4"/>
                <CheckBox Content="De lo Humano y Comunitario" IsChecked="{Binding CampoHumano}" Margin="0,0,12,4"/>
            </WrapPanel>
        </StackPanel>
    </Border>
    
    <!-- Sección 4: Observaciones -->
    <TextBlock Text="Observaciones Particulares (cualitativo/pedagógico)" 
              Foreground="#344054" FontWeight="SemiBold"/>
    <TextBox TabIndex="68" Text="{Binding ObservacionesEdicion, UpdateSourceTrigger=PropertyChanged}" 
            Height="70" TextWrapping="Wrap" AcceptsReturn="True"
            ToolTip="Registro de observaciones relevantes para el seguimiento pedagógico"/>
    
    <TextBlock Foreground="#B42318" Text="{Binding MensajeEdicion}" Margin="0,4,0,4"/>
    
    <WrapPanel Margin="0,12,0,0">
        <Button Content="_Guardar estudiante" Command="{Binding GuardarEstudianteCommand}" 
                Style="{StaticResource PrimaryButton}"/>
        <Button Content="_Cancelar" Command="{Binding CancelarEdicionCommand}"/>
    </WrapPanel>
</StackPanel>
```

#### Problema 2.2.2: Validación en Tiempo Real Ausente
No hay feedback visual de validación mientras se escribe.

**Recomendación:**
```xml
<!-- Agregar validación visual con templates -->
<Window.Resources>
    <Style TargetType="TextBox">
        <Setter Property="Validation.ErrorTemplate">
            <Setter.Value>
                <ControlTemplate>
                    <StackPanel>
                        <AdornedElementPlaceholder/>
                        <TextBlock Text="{Binding [0].ErrorContent}" 
                                  Foreground="#B42318" FontSize="11" Margin="0,2,0,0"/>
                    </StackPanel>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
        <Style.Triggers>
            <Trigger Property="Validation.HasError" Value="True">
                <Setter Property="BorderBrush" Value="#B42318"/>
                <Setter Property="Background" Value="#FEF3F2"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>
```

---

## 3. AUDITORÍA DE MÓDULO DE ASISTENCIA

### 3.1 Vista Diaria de Asistencia

**✅ ASPECTOS POSITIVOS:**
- Contadores visuales claros (Total, Presentes, Faltas, etc.)
- Atajo de teclado Ctrl+S documentado
- Opción "Marcar todos presentes" eficiente

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 3.1.1: Estados de Asistencia No Alineados con NEM
Los estados actuales pueden no reflejar las categorías oficiales NEM.

**Recomendación:**
```xml
<!-- En lugar de solo ComboBox básico -->
<DataGridTemplateColumn Header="Estado" Width="240">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Border Background="White" BorderBrush="#D0D5DD" BorderThickness="1" CornerRadius="6">
                <ComboBox ItemsSource="{Binding OpcionesEstado}" 
                         DisplayMemberPath="Texto" 
                         SelectedValuePath="Estado" 
                         SelectedValue="{Binding Estado, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                         Style="{StaticResource FlatComboBox}">
                    <ComboBox.ItemContainerStyle>
                        <Style TargetType="ComboBoxItem">
                            <Setter Property="Padding" Value="8,6"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Estado}" Value="Presente">
                                    <Setter Property="Background" Value="#ECFDF3"/>
                                    <Setter Property="Foreground" Value="#027A48"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Estado}" Value="Falta">
                                    <Setter Property="Background" Value="#FEF3F2"/>
                                    <Setter Property="Foreground" Value="#B42318"/>
                                </DataTrigger>
                                <!-- etc... -->
                            </Style.Triggers>
                        </Style>
                    </ComboBox.ItemContainerStyle>
                </ComboBox>
            </Border>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

#### Problema 3.1.2: Falta Contexto de Día Escolar
No se muestra si es día lectivo, festivo, o tipo de sesión.

**Recomendación:**
```xml
<Border Grid.Row="1" Margin="0,0,0,14" Padding="16,12" Background="White" 
        CornerRadius="8" BorderBrush="#EAECF0" BorderThickness="1">
    <DockPanel>
        <StackPanel DockPanel.Dock="Left">
            <TextBlock Foreground="#101828" FontWeight="SemiBold" Text="{Binding EstadoGuardado}"/>
            <TextBlock Foreground="#64748B" FontSize="12" 
                      Text="{Binding InformacionDia}" 
                      Visibility="{Binding MostrarInfoDia, Converter={StaticResource BoolToVisibility}}"/>
        </StackPanel>
        
        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal" VerticalAlignment="Center">
            <!-- Badge de tipo de día -->
            <Border Background="#F8F9FA" CornerRadius="6" Padding="10,6" Margin="4,0">
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding TipoDiaIcono}" FontSize="14" Margin="0,0,6,0"/>
                    <TextBlock Foreground="#101828" FontWeight="SemiBold" Text="{Binding TipoDiaTexto}"/>
                </StackPanel>
            </Border>
            
            <!-- Contadores existentes -->
            <Border Background="#F8F9FA" CornerRadius="6" Padding="10,6" Margin="4,0">
                <TextBlock Foreground="#101828" FontWeight="SemiBold" Text="{Binding Total, StringFormat=Total: {0}}"/>
            </Border>
            <!-- ... resto de contadores -->
        </StackPanel>
    </DockPanel>
</Border>
```

---

### 3.2 Vista Mensual de Asistencia

**✅ ASPECTOS POSITIVOS:**
- Navegación por meses intuitiva
- Filtros y búsqueda implementados
- Leyenda clara de estados (P, F, R, J)

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 3.2.1: Grilla Mensual Puede Ser Confusa
Muchas columnas, difícil seguimiento visual.

**Recomendaciones:**
1. **Congelar primera columna con nombres** (ya implementado con `FrozenColumnCount="2"`)
2. **Alternar color de filas más pronunciado**
3. **Agregar líneas verticales cada semana**
4. **Highlight de días con cambios no guardados**

```xml
<DataGrid.RowStyle>
    <Style TargetType="DataGridRow">
        <Setter Property="Height" Value="32"/>
        <Style.Triggers>
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#E0EAFF"/>
                <Setter Property="Foreground" Value="#101828"/>
            </Trigger>
            <DataTrigger Binding="{Binding TieneCambiosPendientes}" Value="True">
                <Setter Property="Background" Value="#FFFBEB"/>
                <Setter Property="FontWeight" Value="SemiBold"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</DataGrid.RowStyle>

<!-- Agregar separadores semanales -->
<DataGrid.ColumnHeaderStyle>
    <Style TargetType="DataGridColumnHeader">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="DataGridColumnHeader">
                    <Border Background="{TemplateBinding Background}" 
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Padding="{TemplateBinding Padding}">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <ContentPresenter HorizontalAlignment="Center"/>
                            <!-- Separador semanal -->
                            <Border Grid.Column="1" Width="1" Background="#F2F4F7" 
                                   Visibility="{Binding EsViernes, Converter={StaticResource BoolToVisibility}}"/>
                        </Grid>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</DataGrid.ColumnHeaderStyle>
```

#### Problema 3.2.2: Métricas de Mes Poco Accionables
Los contadores muestran datos pero no insights.

**Recomendación:**
```xml
<WrapPanel Grid.Row="1" Margin="0,0,0,14">
    <!-- Métricas básicas -->
    <Border Background="White" Padding="14,10" Margin="0,0,10,0" CornerRadius="8" 
            BorderBrush="#EAECF0" BorderThickness="1">
        <StackPanel>
            <TextBlock Foreground="#64748B" FontSize="11" Text="Días lectivos"/>
            <TextBlock Foreground="#101828" Text="{Binding DiasLectivos}" FontWeight="Bold" FontSize="18"/>
        </StackPanel>
    </Border>
    
    <Border Background="White" Padding="14,10" Margin="0,0,10,0" CornerRadius="8" 
            BorderBrush="#EAECF0" BorderThickness="1">
        <StackPanel>
            <TextBlock Foreground="#64748B" FontSize="11" Text="Días registrados"/>
            <TextBlock Foreground="#101828" Text="{Binding DiasGuardados}" FontWeight="Bold" FontSize="18"/>
        </StackPanel>
    </Border>
    
    <!-- Métrica de progreso -->
    <Border Background="#EFF8FF" Padding="14,10" Margin="0,0,10,0" CornerRadius="8" 
            BorderBrush="#B2DDFF" BorderThickness="1">
        <StackPanel>
            <TextBlock Foreground="#175CD3" FontSize="11" Text="Progreso del mes"/>
            <StackPanel Orientation="Horizontal">
                <TextBlock Foreground="#175CD3" Text="{Binding PorcentajeProgreso, StringFormat={}{0:F0}%}" 
                          FontWeight="Bold" FontSize="18"/>
                <TextBlock Foreground="#175CD3" Text=" completado" FontSize="12" VerticalAlignment="Center"/>
            </StackPanel>
        </StackPanel>
    </Border>
    
    <!-- Alerta de borradores -->
    <Border Background="#FFFAEB" Padding="14,10" CornerRadius="8" 
            BorderBrush="#FEF0C7" BorderThickness="1"
            Visibility="{Binding IncluyeBorradores, Converter={StaticResource BoolToVisibility}}">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="⚠️" FontSize="14" Margin="0,0,6,0"/>
            <StackPanel>
                <TextBlock Foreground="#B54708" FontWeight="Bold" Text="Borradores pendientes"/>
                <TextBlock Foreground="#B54708" FontSize="12" 
                          Text="{Binding CantidadBorradores, StringFormat={}{0} días sin guardar}}"/>
            </StackPanel>
        </StackPanel>
    </Border>
</WrapPanel>
```

---

## 4. AUDITORÍA DE MÓDULO DE PROYECTOS DIDÁCTICOS

### 4.1 Lista de Proyectos

**✅ ASPECTOS POSITIVOS:**
- Filtro por estado útil
- Doble clic para editar (patrón estándar)
- Columnas informativas (nombre, estado, fechas, actividades)

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 4.1.1: Falta Alineación con Campos Formativos NEM
Los proyectos NEM deben estar vinculados a campos formativos específicos.

**Recomendación:**
```xml
<DataGrid.Columns>
    <DataGridTemplateColumn Header="Proyecto" Width="280">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <StackPanel>
                    <TextBlock Text="{Binding Nombre}" FontWeight="SemiBold" 
                              TextWrapping="Wrap"/>
                    <WrapPanel Margin="0,4,0,0">
                        <Border Background="#F2F4F7" CornerRadius="4" Padding="4,2" Margin="0,0,4,0">
                            <TextBlock Text="{Binding CampoFormativo}" FontSize="10" 
                                      Foreground="#475467"/>
                        </Border>
                        <Border Background="{Binding ColorFase}" CornerRadius="4" Padding="4,2">
                            <TextBlock Text="{Binding FaseNEM}" FontSize="10" Foreground="White"/>
                        </Border>
                    </WrapPanel>
                </StackPanel>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
    
    <DataGridTemplateColumn Header="Estado" Width="120">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <Border Background="{Binding ColorEstado}" CornerRadius="6" Padding="8,4">
                    <StackPanel Orientation="Horizontal">
                        <TextBlock Text="{Binding IconoEstado}" FontSize="12" Margin="0,0,6,0"/>
                        <TextBlock Text="{Binding Estado}" FontWeight="SemiBold" 
                                  Foreground="{Binding ColorTextoEstado}"/>
                    </StackPanel>
                </Border>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
    
    <DataGridTextColumn Header="Inicio" Binding="{Binding FechaInicio, StringFormat=dd/MM/yyyy}" Width="90"/>
    <DataGridTextColumn Header="Término" Binding="{Binding FechaTermino, StringFormat=dd/MM/yyyy}" Width="90"/>
    
    <DataGridTemplateColumn Header="Actividades" Width="100">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding NumeroActividades}" FontWeight="SemiBold"/>
                    <TextBlock Text="/" Foreground="#64748B" Margin="4,0"/>
                    <TextBlock Text="{Binding TotalActividadesPlaneadas}" Foreground="#64748B"/>
                </StackPanel>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
    
    <DataGridTemplateColumn Header="Avance" Width="100">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <Grid Width="80">
                    <ProgressBar Value="{Binding PorcentajeAvance}" Height="6" 
                                Background="#F2F4F7" Foreground="#175CD3"/>
                    <TextBlock Text="{Binding PorcentajeAvance, StringFormat={}{0:F0}%}" 
                              HorizontalAlignment="Center" FontSize="10" 
                              Foreground="#344054" FontWeight="SemiBold"/>
                </Grid>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
</DataGrid.Columns>
```

---

### 4.2 Detalle de Proyecto (Ventana Emergente)

**✅ ASPECTOS POSITIVOS:**
- Advertencia de duración atípica
- Separación clara entre info del proyecto y actividades
- CRUD completo de actividades

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 4.2.1: Campos NEM Faltantes en Proyecto
No se capturan elementos esenciales del modelo NEM.

**Recomendación - Secciones Enriquecidas:**
```xml
<ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto" Margin="0,0,0,10">
    <StackPanel>
        <!-- Sección 1: Información Básica -->
        <Border Background="White" CornerRadius="8" Padding="16" Margin="0,0,0,14" 
                BorderBrush="#E2E8F0" BorderThickness="1">
            <StackPanel>
                <TextBlock Text="Información General" FontSize="16" FontWeight="SemiBold" 
                          Foreground="#1E293B" Margin="0,0,0,10"/>
                
                <!-- Campos existentes + nuevos -->
                <TextBlock Text="Nombre del Proyecto *" FontWeight="SemiBold"/>
                <TextBox Text="{Binding NombreProyecto, UpdateSourceTrigger=PropertyChanged}"/>
                
                <TextBlock Text="Campo Formativo Principal *" FontWeight="SemiBold"/>
                <ComboBox SelectedItem="{Binding CampoFormativoPrincipal}">
                    <ComboBoxItem Content="Lenguajes"/>
                    <ComboBoxItem Content="Saberes Científicos"/>
                    <ComboBoxItem Content="Ética, Naturaleza y Sociedades"/>
                    <ComboBoxItem Content="De lo Humano y Comunitario"/>
                </ComboBox>
                
                <TextBlock Text="Contenidos Curriculares" FontWeight="SemiBold"/>
                <TextBox Text="{Binding ContenidosCurriculares, UpdateSourceTrigger=PropertyChanged}" 
                        AcceptsReturn="True" Height="65" TextWrapping="Wrap"
                        ToolTip="Describir los contenidos curriculares abordados"/>
                
                <TextBlock Text="Problemática a Resolver" FontWeight="SemiBold"/>
                <TextBox Text="{Binding Problematica, UpdateSourceTrigger=PropertyChanged}" 
                        AcceptsReturn="True" Height="55" TextWrapping="Wrap"/>
                
                <!-- Fechas y duración -->
                <Grid Margin="0,8,0,0">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="12"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="12"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    
                    <StackPanel Grid.Column="0">
                        <TextBlock Text="Fecha Inicio *" FontWeight="SemiBold"/>
                        <TextBox Text="{Binding FechaInicio}"/>
                    </StackPanel>
                    
                    <StackPanel Grid.Column="2">
                        <TextBlock Text="Fecha Término *" FontWeight="SemiBold"/>
                        <TextBox Text="{Binding FechaTermino}"/>
                    </StackPanel>
                    
                    <StackPanel Grid.Column="4">
                        <TextBlock Text="Duración estimada" FontWeight="SemiBold"/>
                        <Border Background="#F8F9FA" CornerRadius="4" Padding="8,6">
                            <TextBlock Text="{Binding DuracionDias, StringFormat={}{0} días}" 
                                      FontWeight="SemiBold"/>
                        </Border>
                    </StackPanel>
                </Grid>
                
                <!-- Alerta de duración -->
                <Border Background="#FFFAEB" BorderBrush="#FEF0C7" BorderThickness="1" 
                        CornerRadius="6" Padding="12,8" Margin="0,4,0,12"
                        Visibility="{Binding DuracionAtipica, Converter={StaticResource BoolToVisibility}}">
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <TextBlock Text="⚠️ " Foreground="#B54708" FontSize="14"/>
                        <TextBlock Foreground="#B54708" FontWeight="Medium" TextWrapping="Wrap"
                                  Text="Duración fuera del rango estándar (14–31 días). 
                                        Puedes guardar si es intencional."/>
                    </StackPanel>
                </Border>
                
                <TextBlock Text="Observaciones" FontWeight="SemiBold"/>
                <TextBox Text="{Binding ObservacionesProyecto, UpdateSourceTrigger=PropertyChanged}" 
                        AcceptsReturn="True" Height="55" TextWrapping="Wrap"/>
                
                <!-- Acciones -->
                <WrapPanel Margin="0,8,0,0">
                    <Button Style="{StaticResource PrimaryButton}" Content="Guardar cambios" 
                            Command="{Binding GuardarProyectoCommand}"/>
                    <Button Content="Iniciar" Command="{Binding IniciarProyectoCommand}"/>
                    <Button Content="Finalizar" Command="{Binding FinalizarProyectoCommand}"/>
                    <Button Content="Reabrir" Command="{Binding ReabrirProyectoCommand}"/>
                    <Button Content="Eliminar borrador" Command="{Binding EliminarProyectoCommand}"/>
                </WrapPanel>
            </StackPanel>
        </Border>
        
        <!-- Sección 2: Elementos NEM -->
        <Border Background="White" CornerRadius="8" Padding="16" Margin="0,0,0,14" 
                BorderBrush="#E2E8F0" BorderThickness="1"
                Visibility="{Binding MostrarElementosNEM, Converter={StaticResource BoolToVisibility}}">
            <StackPanel>
                <TextBlock Text="Elementos NEM" FontSize="16" FontWeight="SemiBold" 
                          Foreground="#1E293B" Margin="0,0,0,10"/>
                
                <TextBlock Text="Ejes Articuladores (seleccionar al menos uno)" FontWeight="SemiBold"/>
                <WrapPanel Margin="0,4,0,12">
                    <CheckBox Content="Pensamiento Crítico" IsChecked="{Binding EjePensamientoCritico}" 
                             Margin="0,0,12,4"/>
                    <CheckBox Content="Aprendizaje Situado" IsChecked="{Binding EjeAprendizajeSituado}" 
                             Margin="0,0,12,4"/>
                    <CheckBox Content="Interculturalidad" IsChecked="{Binding EjeInterculturalidad}" 
                             Margin="0,0,12,4"/>
                    <CheckBox Content="Inclusión" IsChecked="{Binding EjeInclusion}" Margin="0,0,12,4"/>
                    <CheckBox Content="Igualdad de Género" IsChecked="{Binding EjeIgualdadGenero}" 
                             Margin="0,0,12,4"/>
                </WrapPanel>
                
                <TextBlock Text="Productos Esperados" FontWeight="SemiBold"/>
                <TextBox Text="{Binding ProductosEsperados, UpdateSourceTrigger=PropertyChanged}" 
                        AcceptsReturn="True" Height="65" TextWrapping="Wrap"/>
                
                <TextBlock Text="Recursos Didácticos" FontWeight="SemiBold"/>
                <TextBox Text="{Binding RecursosDidacticos, UpdateSourceTrigger=PropertyChanged}" 
                        AcceptsReturn="True" Height="55" TextWrapping="Wrap"/>
            </StackPanel>
        </Border>
        
        <!-- Sección 3: Actividades -->
        <Border Background="White" CornerRadius="8" Padding="16" 
                BorderBrush="#E2E8F0" BorderThickness="1">
            <StackPanel>
                <DockPanel Margin="0,0,0,10">
                    <TextBlock DockPanel.Dock="Left" Text="Actividades del Proyecto" 
                              FontSize="16" FontWeight="SemiBold" Foreground="#1E293B" 
                              VerticalAlignment="Center"/>
                    <Button DockPanel.Dock="Right" HorizontalAlignment="Right" 
                            Style="{StaticResource PrimaryButton}" Content="+ Nueva Actividad" 
                            Click="OnNuevaActividadClic"/>
                </DockPanel>
                
                <TextBox Text="{Binding BusquedaActividad, UpdateSourceTrigger=PropertyChanged}" 
                        ToolTip="Buscar actividad..." Margin="0,0,0,10"/>
                
                <!-- Lista de actividades con más contexto -->
                <ListBox x:Name="ListaActividades" Height="200" 
                        ItemsSource="{Binding Actividades}" 
                        SelectedItem="{Binding ActividadSeleccionada}" 
                        MouseDoubleClick="OnActividadDobleClic">
                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <Border Background="#F8FAFC" CornerRadius="6" Padding="10,8" Margin="0,2">
                                <DockPanel>
                                    <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
                                        <Border Background="{Binding ColorEstado}" CornerRadius="4" 
                                                Padding="6,2" VerticalAlignment="Center">
                                            <TextBlock Text="{Binding Estado}" Foreground="White" 
                                                      FontSize="11" FontWeight="SemiBold"/>
                                        </Border>
                                        <Border Background="#E2E8F0" CornerRadius="4" Padding="6,2" 
                                                VerticalAlignment="Center" Margin="8,0,0,0">
                                            <StackPanel Orientation="Horizontal">
                                                <TextBlock Text="📝" FontSize="10" Margin="0,0,4,0"/>
                                                <TextBlock Text="{Binding EntregasTotales}" 
                                                          FontSize="11" Foreground="#475569"/>
                                            </StackPanel>
                                        </Border>
                                    </StackPanel>
                                    
                                    <StackPanel DockPanel.Dock="Left">
                                        <TextBlock Text="{Binding Titulo}" FontWeight="SemiBold" 
                                                  FontSize="14" Foreground="#0F172A" 
                                                  TextWrapping="Wrap"/>
                                        <WrapPanel Margin="0,4,0,0">
                                            <TextBlock Text="{Binding FechaRealizacion, 
                                                     StringFormat=📅 {0:dd/MM/yyyy}}" 
                                                      Foreground="#64748B" FontSize="11"/>
                                            <TextBlock Text=" • " Foreground="#CBD5E1" FontSize="11"/>
                                            <TextBlock Text="{Binding DuracionEstimada, 
                                                     StringFormat={}{0} min}" 
                                                      Foreground="#64748B" FontSize="11"/>
                                        </WrapPanel>
                                        <TextBlock Text="{Binding DescripcionCorta}" 
                                                  Foreground="#475569" FontSize="12" 
                                                  TextTrimming="CharacterEllipsis" 
                                                  TextWrapping="Wrap"/>
                                    </StackPanel>
                                </DockPanel>
                            </Border>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>
                
                <WrapPanel Margin="0,8,0,0" HorizontalAlignment="Right">
                    <Button Content="✏️ Editar actividad" Click="OnEditarActividadClic"
                           ToolTip="Editar actividad seleccionada"/>
                    <Button Content="🗑️ Eliminar actividad" Command="{Binding EliminarActividadCommand}"
                           ToolTip="Eliminar actividad seleccionada"/>
                </WrapPanel>
            </StackPanel>
        </Border>
    </StackPanel>
</ScrollViewer>
```

---

## 5. AUDITORÍA DE MÓDULO DE EVALUACIÓN NEM

### 5.1 Interfaz de Evaluación de Actividades

**✅ ASPECTOS POSITIVOS:**
- Escala de logro NEM implementada (Domina, Suficiente, En Proceso, Requiere Apoyo)
- Captura rápida con botones dedicados
- Menú contextual para acciones masivas
- Código de colores consistente

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 5.1.1: Falta Contexto de la Actividad a Evaluar
El usuario debe recordar los detalles de la actividad mientras evalúa.

**Recomendación:**
```xml
<!-- Agregar panel de contexto de actividad -->
<Border Grid.Row="1" Background="#F8FAFC" CornerRadius="8" Padding="16" 
        Margin="0,0,0,14" BorderBrush="#E2E8F0" BorderThickness="1">
    <StackPanel>
        <DockPanel Margin="0,0,0,12">
            <StackPanel DockPanel.Dock="Left">
                <TextBlock Text="Actividad a Evaluar" Foreground="#64748B" FontSize="12" 
                          FontWeight="SemiBold"/>
                <TextBlock Text="{Binding ActividadSeleccionada.Titulo}" 
                          Foreground="#101828" FontSize="18" FontWeight="Bold"/>
            </StackPanel>
            
            <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
                <Border Background="White" CornerRadius="6" Padding="10,6" Margin="4,0">
                    <StackPanel Orientation="Horizontal">
                        <TextBlock Text="📅" FontSize="14" Margin="0,0,6,0"/>
                        <TextBlock Text="{Binding ActividadSeleccionada.Fecha, 
                                         StringFormat={}{0:dd/MM/yyyy}}" 
                                  FontWeight="SemiBold"/>
                    </StackPanel>
                </Border>
                
                <Border Background="White" CornerRadius="6" Padding="10,6" Margin="4,0">
                    <StackPanel Orientation="Horizontal">
                        <TextBlock Text="⏱️" FontSize="14" Margin="0,0,6,0"/>
                        <TextBlock Text="{Binding ActividadSeleccionada.Duracion}" 
                                  FontWeight="SemiBold"/>
                    </StackPanel>
                </Border>
            </StackPanel>
        </DockPanel>
        
        <Border Background="White" CornerRadius="6" Padding="12" BorderBrush="#EAECF0" BorderThickness="1">
            <StackPanel>
                <TextBlock Text="Descripción de la Actividad" FontWeight="SemiBold" 
                          Foreground="#344054" Margin="0,0,0,4"/>
                <TextBlock Text="{Binding ActividadSeleccionada.Descripcion}" 
                          Foreground="#475467" TextWrapping="Wrap" FontSize="13"/>
                
                <TextBlock Text="Criterios de Evaluación" FontWeight="SemiBold" 
                          Foreground="#344054" Margin="0,12,0,4"/>
                <ItemsControl ItemsSource="{Binding ActividadSeleccionada.CriteriosEvaluacion}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,4">
                                <TextBlock Text="• " Foreground="#64748B"/>
                                <TextBlock Text="{Binding}" Foreground="#475467" FontSize="13"/>
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </Border>
    </StackPanel>
</Border>
```

#### Problema 5.1.2: Observaciones por Estudiante Poco Visibles
El campo de observaciones está en la grilla pero puede pasar desapercibido.

**Recomendación:**
```xml
<!-- Expandir fila para mostrar observaciones completas -->
<DataGrid.RowStyle>
    <Style TargetType="DataGridRow">
        <Setter Property="Height" Value="Auto"/>
        <Setter Property="MinHeight" Value="40"/>
        <Style.Triggers>
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#E0EAFF"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</DataGrid.RowStyle>

<DataGrid.Columns>
    <!-- Columna de observaciones expandida -->
    <DataGridTemplateColumn Header="Observación" Width="280" MinWidth="200">
        <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
                <Grid>
                    <TextBox Text="{Binding Observacion, UpdateSourceTrigger=PropertyChanged}" 
                            Height="60" TextWrapping="Wrap" AcceptsReturn="True"
                            Background="Transparent" BorderThickness="0"
                            FontSize="12" Padding="4"
                            ToolTip="Doble clic para expandir"/>
                    
                    <!-- Highlight si tiene observación importante -->
                    <Border BorderBrush="#FDB022" BorderThickness="2" CornerRadius="4"
                            Visibility="{Binding EsObservacionImportante, 
                                     Converter={StaticResource BoolToVisibility}}"/>
                </Grid>
            </DataTemplate>
        </DataGridTemplateColumn.CellTemplate>
    </DataGridTemplateColumn>
</DataGrid.Columns>
```

#### Problema 5.1.3: Métricas de Evaluación No Accionables
Los contadores muestran números pero no ayudan a tomar decisiones pedagógicas.

**Recomendación:**
```xml
<WrapPanel DockPanel.Dock="Right" HorizontalAlignment="Right" VerticalAlignment="Center">
    <!-- Métricas con contexto -->
    <Border Background="#F8F9FA" CornerRadius="6" Padding="10,6" Margin="4,0" BorderBrush="#EAECF0" BorderThickness="1">
        <StackPanel>
            <TextBlock Foreground="#64748B" FontSize="10" Text="Total estudiantes"/>
            <TextBlock Foreground="#101828" FontWeight="Bold" Text="{Binding Total}"/>
        </StackPanel>
    </Border>
    
    <Border Background="#F2F4F7" CornerRadius="6" Padding="10,6" Margin="4,0">
        <StackPanel>
            <TextBlock Foreground="#64748B" FontSize="10" Text="Pendientes"/>
            <TextBlock Foreground="#344054" FontWeight="Bold" Text="{Binding Pendientes}"/>
        </StackPanel>
    </Border>
    
    <!-- Dominio del contenido -->
    <Border Background="#ECFDF3" CornerRadius="6" Padding="10,6" Margin="4,0">
        <StackPanel>
            <TextBlock Foreground="#047857" FontSize="10" FontWeight="SemiBold" Text="Dominan"/>
            <StackPanel Orientation="Horizontal">
                <TextBlock Foreground="#027A48" FontWeight="Bold" FontSize="16" Text="{Binding Domina}"/>
                <TextBlock Foreground="#047857" FontSize="10" Margin="4,0,0,0" VerticalAlignment="Bottom">
                    <Run Text="("/><Run Text="{Binding PorcentajeDomina, StringFormat={}{0:F0}%}"/><Run Text=")"/>
                </TextBlock>
            </StackPanel>
        </StackPanel>
    </Border>
    
    <!-- Suficiente -->
    <Border Background="#EFF8FF" CornerRadius="6" Padding="10,6" Margin="4,0">
        <StackPanel>
            <TextBlock Foreground="#175CD3" FontSize="10" FontWeight="SemiBold" Text="Suficiente"/>
            <StackPanel Orientation="Horizontal">
                <TextBlock Foreground="#175CD3" FontWeight="Bold" FontSize="16" Text="{Binding Suficiente}"/>
                <TextBlock Foreground="#175CD3" FontSize="10" Margin="4,0,0,0" VerticalAlignment="Bottom">
                    <Run Text="("/><Run Text="{Binding PorcentajeSuficiente, StringFormat={}{0:F0}%}"/><Run Text=")"/>
                </TextBlock>
            </StackPanel>
        </StackPanel>
    </Border>
    
    <!-- En proceso - Alerta -->
    <Border Background="#FFFAEB" CornerRadius="6" Padding="10,6" Margin="4,0">
        <StackPanel>
            <TextBlock Foreground="#B54708" FontSize="10" FontWeight="SemiBold" Text="En proceso"/>
            <StackPanel Orientation="Horizontal">
                <TextBlock Foreground="#B54708" FontWeight="Bold" FontSize="16" Text="{Binding EnProceso}"/>
                <TextBlock Foreground="#B54708" FontSize="10" Margin="4,0,0,0" VerticalAlignment="Bottom">
                    <Run Text="("/><Run Text="{Binding PorcentajeEnProceso, StringFormat={}{0:F0}%}"/><Run Text=")"/>
                </TextBlock>
            </StackPanel>
        </StackPanel>
    </Border>
    
    <!-- Requiere apoyo - Crítico -->
    <Border Background="#FEF3F2" CornerRadius="6" Padding="10,6" Margin="4,0">
        <StackPanel>
            <TextBlock Foreground="#B42318" FontSize="10" FontWeight="SemiBold" Text="Req. apoyo"/>
            <StackPanel Orientation="Horizontal">
                <TextBlock Foreground="#B42318" FontWeight="Bold" FontSize="16" Text="{Binding RequiereApoyo}"/>
                <TextBlock Foreground="#B42318" FontSize="10" Margin="4,0,0,0" VerticalAlignment="Bottom">
                    <Run Text="("/><Run Text="{Binding PorcentajeRequiereApoyo, StringFormat={}{0:F0}%}"/><Run Text=")"/>
                </TextBlock>
            </StackPanel>
        </StackPanel>
    </Border>
    
    <!-- No entregó -->
    <Border Background="#F2F4F7" CornerRadius="6" Padding="10,6" Margin="4,0">
        <StackPanel>
            <TextBlock Foreground="#475467" FontSize="10" FontWeight="SemiBold" Text="No entregó"/>
            <TextBlock Foreground="#475467" FontWeight="Bold" Text="{Binding NoEntrego}"/>
        </StackPanel>
    </Border>
    
    <!-- Insight accionable -->
    <Border Background="#173F5F" CornerRadius="6" Padding="10,6" Margin="8,0,0,0">
        <StackPanel>
            <TextBlock Foreground="#B0C4DE" FontSize="10" Text="Recomendación"/>
            <TextBlock Foreground="White" FontWeight="SemiBold" FontSize="12" 
                      Text="{Binding RecomendacionPedagogica}" TextWrapping="Wrap" Width="140"/>
        </StackPanel>
    </Border>
</WrapPanel>
```

---

## 6. AUDITORÍA DE EXPEDIENTE INDIVIDUAL DEL ESTUDIANTE

### 6.1 Ventana de Expediente

**✅ ASPECTOS POSITIVOS:**
- Organización por pestañas lógica
- Historial de entregas visible
- Acuerdos con tutores documentados

**⚠️ ÁREAS DE OPORTUNIDAD:**

#### Problema 6.1.1: Resumen Pedagógico Poco Visual
La información está presente pero no destaca insights importantes.

**Recomendación:**
```xml
<!-- Pestaña 1: Dashboard Visual del Estudiante -->
<TabItem Header="📊 Resumen">
    <ScrollViewer VerticalScrollBarVisibility="Auto" Padding="16">
        <StackPanel DataContext="{Binding Expediente}">
            <!-- KPI Cards -->
            <WrapPanel Margin="0,0,0,16">
                <Border Background="#EFF8FF" CornerRadius="8" Padding="16" Margin="0,0,10,10" Width="180">
                    <StackPanel>
                        <TextBlock Foreground="#175CD3" FontSize="11" FontWeight="SemiBold" Text="Asistencia Global"/>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Foreground="#175CD3" FontSize="24" FontWeight="Bold" 
                                      Text="{Binding PorcentajeAsistencia, StringFormat={}{0:F0}%}"/>
                            <TextBlock Foreground="#175CD3" FontSize="11" Margin="4,0,0,0" 
                                      VerticalAlignment="Bottom">%
                            </TextBlock>
                        </StackPanel>
                        <ProgressBar Value="{Binding PorcentajeAsistencia}" Height="4" 
                                    Background="#DBEAFE" Foreground="#175CD3" Margin="0,6,0,0"/>
                    </StackPanel>
                </Border>
                
                <Border Background="#ECFDF3" CornerRadius="8" Padding="16" Margin="0,0,10,10" Width="180">
                    <StackPanel>
                        <TextBlock Foreground="#047857" FontSize="11" FontWeight="SemiBold" Text="Actividades Dominadas"/>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Foreground="#027A48" FontSize="24" FontWeight="Bold" 
                                      Text="{Binding ActividadesDominadas}"/>
                            <TextBlock Foreground="#047857" FontSize="11" Margin="4,0,0,0" 
                                      VerticalAlignment="Bottom">/
                                <Run Text="{Binding TotalActividades}"/>
                            </TextBlock>
                        </StackPanel>
                    </StackPanel>
                </Border>
                
                <Border Background="#FFFAEB" CornerRadius="8" Padding="16" Margin="0,0,10,10" Width="180">
                    <StackPanel>
                        <TextBlock Foreground="#B54708" FontSize="11" FontWeight="SemiBold" Text="En Proceso"/>
                        <TextBlock Foreground="#B54708" FontSize="24" FontWeight="Bold" 
                                  Text="{Binding ActividadesEnProceso}"/>
                    </StackPanel>
                </Border>
                
                <Border Background="#FEF3F2" CornerRadius="8" Padding="16" Margin="0,0,10,10" Width="180">
                    <StackPanel>
                        <TextBlock Foreground="#B42318" FontSize="11" FontWeight="SemiBold" Text="Requiere Apoyo"/>
                        <TextBlock Foreground="#B42318" FontSize="24" FontWeight="Bold" 
                                  Text="{Binding ActividadesRequiereApoyo}"/>
                    </StackPanel>
                </Border>
            </WrapPanel>
            
            <!-- Alertas Pedagógicas -->
            <TextBlock Text="⚠️ Alertas Pedagógicas Formativas" FontSize="16" FontWeight="Bold" 
                      Foreground="#1E293B" Margin="0,8,0,8"/>
            <ItemsControl ItemsSource="{Binding AlertasPedagogicas}" Margin="0,0,0,16">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Border Background="#FEF3C7" BorderBrush="#F59E0B" BorderThickness="1" 
                                CornerRadius="6" Padding="12,8" Margin="0,3">
                            <DockPanel>
                                <TextBlock DockPanel.Dock="Left" Text="⚠️" FontSize="16" Margin="0,0,8,0"/>
                                <StackPanel>
                                    <TextBlock Text="{Binding TipoAlerta}" FontWeight="Bold" 
                                              Foreground="#92400E" FontSize="13"/>
                                    <TextBlock Text="{Binding Mensaje}" Foreground="#92400E" 
                                              TextWrapping="Wrap" Margin="0,2,0,0"/>
                                    <TextBlock Text="{Binding FechaGeneracion, StringFormat={}{0:dd/MM/yyyy}}" 
                                              Foreground="#B45309" FontSize="11" Margin="0,4,0,0"/>
                                </StackPanel>
                            </DockPanel>
                        </Border>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
            
            <!-- Gráfica de Progreso (placeholder para futura implementación) -->
            <Border Background="White" CornerRadius="8" Padding="16" Margin="0,0,0,16" 
                    BorderBrush="#E2E8F0" BorderThickness="1">
                <StackPanel>
                    <TextBlock Text="Progreso por Campo Formativo" FontSize="16" FontWeight="Bold" 
                              Foreground="#1E293B" Margin="0,0,0,8"/>
                    <Border Background="#F8FAFC" CornerRadius="6" Padding="16" Height="150">
                        <TextBlock Text="[Gráfica de barras por campo formativo - Próximamente]" 
                                  Foreground="#64748B" HorizontalAlignment="Center" 
                                  VerticalAlignment="Center"/>
                    </Border>
                </StackPanel>
            </Border>
            
            <!-- Historial Reciente -->
            <TextBlock Text="📝 Historial Reciente de Entregas" FontSize="16" FontWeight="Bold" 
                      Foreground="#1E293B" Margin="0,8,0,8"/>
            <DataGrid ItemsSource="{Binding UltimasEntregas}" AutoGenerateColumns="False" 
                     IsReadOnly="True" HeadersVisibility="Column" GridLinesVisibility="Horizontal" 
                     Height="180">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Fecha" Binding="{Binding Fecha, StringFormat=dd/MM/yyyy}" Width="90"/>
                    <DataGridTextColumn Header="Proyecto" Binding="{Binding NombreProyecto}" Width="180"/>
                    <DataGridTextColumn Header="Actividad" Binding="{Binding TituloActividad}" Width="*"/>
                    <DataGridTemplateColumn Header="Nivel" Width="110">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <Border Background="{Binding ColorNivel}" CornerRadius="4" Padding="6,2">
                                    <TextBlock Text="{Binding NivelLogro}" FontWeight="Bold" 
                                              Foreground="White" HorizontalAlignment="Center"/>
                                </Border>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                </DataGrid.Columns>
            </DataGrid>
        </StackPanel>
    </ScrollViewer>
</TabItem>
```

#### Problema 6.1.2: Fortalezas y Dificultades Sin Priorización
Todas las fortalezas/dificultades se ven igual, sin indicar cuáles son críticas.

**Recomendación:**
```xml
<TabItem Header="💪 Fortalezas y Dificultades">
    <Grid Margin="16">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="16"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <!-- Fortalezas -->
        <StackPanel Grid.Column="0">
            <DockPanel Margin="0,0,0,8">
                <TextBlock DockPanel.Dock="Left" Text="🌟 Fortalezas" FontSize="16" FontWeight="Bold" 
                          Foreground="#065F46" VerticalAlignment="Center"/>
                <Button DockPanel.Dock="Right" Content="+ Agregar" 
                       Command="{Binding AgregarFortalezaCommand}"/>
            </DockPanel>
            
            <!-- Selector de prioridad -->
            <Border Background="#ECFDF3" CornerRadius="6" Padding="10" Margin="0,0,0,8">
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="Nueva fortaleza:" Foreground="#047857" FontWeight="SemiBold" 
                              VerticalAlignment="Center" Margin="0,0,8,0"/>
                    <TextBox Text="{Binding NuevaFortaleza, UpdateSourceTrigger=PropertyChanged}" 
                            Width="*" ToolTip="Escriba una fortaleza pedagógica..."/>
                    <ComboBox SelectedItem="{Binding PrioridadNuevaFortaleza}" Width="100" Margin="8,0,0,0">
                        <ComboBoxItem Content="Normal"/>
                        <ComboBoxItem Content="Alta"/>
                        <ComboBoxItem Content="Clave"/>
                    </ComboBox>
                    <Button Content="Agregar" Command="{Binding AgregarFortalezaCommand}" 
                           Margin="8,0,0,0"/>
                </StackPanel>
            </Border>
            
            <!-- Lista con priorización visual -->
            <ListBox ItemsSource="{Binding Expediente.Fortalezas}" Height="350">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <Border CornerRadius="6" Padding="10,8" Margin="0,2">
                            <Border.Style>
                                <Style TargetType="Border">
                                    <Setter Property="Background" Value="#F0FDF4"/>
                                    <Setter Property="BorderBrush" Value="#BBF7D0"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding Prioridad}" Value="Alta">
                                            <Setter Property="Background" Value="#DCFCE7"/>
                                            <Setter Property="BorderBrush" Value="#86EFAC"/>
                                        </DataTrigger>
                                        <DataTrigger Binding="{Binding Prioridad}" Value="Clave">
                                            <Setter Property="Background" Value="#16A34A"/>
                                            <Setter Property="BorderBrush" Value="#15803D"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Border.Style>
                            
                            <DockPanel>
                                <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
                                    <TextBlock Text="{Binding FechaRegistro, StringFormat={}{0:dd/MM/yyyy}}" 
                                              Foreground="#64748B" FontSize="11" VerticalAlignment="Center"/>
                                    <Border Background="{Binding ColorPrioridad}" CornerRadius="4" 
                                           Padding="4,2" Margin="8,0,0,0">
                                        <TextBlock Text="{Binding Prioridad}" FontSize="10" 
                                                  Foreground="White" FontWeight="SemiBold"/>
                                    </Border>
                                </StackPanel>
                                
                                <StackPanel>
                                    <TextBlock Text="{Binding Contenido}" TextWrapping="Wrap" 
                                              FontWeight="SemiBold" Foreground="#064E3B"/>
                                    <TextBlock Text="{Binding Contexto}" TextWrapping="Wrap" 
                                              Foreground="#059669" FontSize="12" Margin="0,4,0,0"/>
                                </StackPanel>
                            </DockPanel>
                        </Border>
                    </DataTemplate>
                </ListBox.ItemTemplate>
            </ListBox>
        </StackPanel>
        
        <!-- Separador vertical -->
        <Border Grid.Column="1" Background="#E2E8F0" Margin="0,4"/>
        
        <!-- Dificultades (mismo patrón pero con colores rojos/naranjas) -->
        <StackPanel Grid.Column="2">
            <DockPanel Margin="0,0,0,8">
                <TextBlock DockPanel.Dock="Left" Text="⚠️ Dificultades" FontSize="16" FontWeight="Bold" 
                          Foreground="#991B1B" VerticalAlignment="Center"/>
                <Button DockPanel.Dock="Right" Content="+ Agregar" 
                       Command="{Binding AgregarDificultadCommand}"/>
            </DockPanel>
            
            <!-- Similar estructura que fortalezas pero con esquema de colores rojo -->
            <!-- ... -->
        </StackPanel>
    </Grid>
</TabItem>
```

---

## 7. RECOMENDACIONES GENERALES DE UX

### 7.1 Patrones de Diseño Consistentes

**Problema Identificado:**
- Algunos botones usan `Style="{StaticResource PrimaryButton}"` y otros no
- Los títulos de sección tienen tamaños inconsistentes
- Los mensajes de error/alerta varían en formato

**Recomendación - Guía de Estilo Unificada:**
```xml
<Application.Resources>
    <!-- Tipografía -->
    <Style x:Key="Heading1" TargetType="TextBlock">
        <Setter Property="FontSize" Value="24"/>
        <Setter Property="FontWeight" Value="Bold"/>
        <Setter Property="Foreground" Value="#101828"/>
        <Setter Property="Margin" Value="0,0,0,16"/>
    </Style>
    
    <Style x:Key="Heading2" TargetType="TextBlock">
        <Setter Property="FontSize" Value="18"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="#101828"/>
        <Setter Property="Margin" Value="0,0,0,12"/>
    </Style>
    
    <Style x:Key="Heading3" TargetType="TextBlock">
        <Setter Property="FontSize" Value="16"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="#344054"/>
        <Setter Property="Margin" Value="0,0,0,8"/>
    </Style>
    
    <Style x:Key="Label" TargetType="TextBlock">
        <Setter Property="FontSize" Value="13"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="#344054"/>
        <Setter Property="Margin" Value="0,0,0,4"/>
    </Style>
    
    <Style x:Key="Caption" TargetType="TextBlock">
        <Setter Property="FontSize" Value="11"/>
        <Setter Property="Foreground" Value="#64748B"/>
    </Style>
    
    <!-- Alertas -->
    <Style x:Key="AlertError" TargetType="TextBlock">
        <Setter Property="Foreground" Value="#B42318"/>
        <Setter Property="Background" Value="#FEF3F2"/>
        <Setter Property="Padding" Value="12,8"/>
        <Setter Property="CornerRadius" Value="6"/>
    </Style>
    
    <Style x:Key="AlertWarning" TargetType="TextBlock">
        <Setter Property="Foreground" Value="#B54708"/>
        <Setter Property="Background" Value="#FFFAEB"/>
        <Setter Property="Padding" Value="12,8"/>
        <Setter Property="CornerRadius" Value="6"/>
    </Style>
    
    <Style x:Key="AlertSuccess" TargetType="TextBlock">
        <Setter Property="Foreground" Value="#027A48"/>
        <Setter Property="Background" Value="#ECFDF3"/>
        <Setter Property="Padding" Value="12,8"/>
        <Setter Property="CornerRadius" Value="6"/>
    </Style>
</Application.Resources>
```

---

### 7.2 Feedback y Estados del Sistema

**Problema Identificado:**
- El indicador "Procesando…" es genérico
- No hay confirmación visual después de guardar exitosamente
- Los errores no siempre explican cómo resolverlos

**Recomendación:**
```xml
<!-- Toast Notification System -->
<Grid Grid.Row="0">
    <ItemsControl ItemsSource="{Binding Notificaciones}" HorizontalAlignment="Right" 
                  VerticalAlignment="Top" Margin="0,80,20,0">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Border Background="{Binding ColorFondo}" CornerRadius="8" Padding="16,12" 
                        Margin="0,0,0,8" BorderBrush="{Binding ColorBorde}" BorderThickness="1">
                    <Border.Effect>
                        <DropShadowEffect BlurRadius="8" ShadowDepth="2" Opacity="0.3"/>
                    </Border.Effect>
                    <StackPanel Width="320">
                        <DockPanel Margin="0,0,0,8">
                            <TextBlock DockPanel.Dock="Left" Text="{Binding Icono}" 
                                      FontSize="18" Margin="0,0,8,0"/>
                            <TextBlock DockPanel.Dock="Right" Text="✕" Cursor="Hand" 
                                      MouseLeftButtonDown="OnCerrarNotificacion" 
                                      Foreground="#64748B" FontSize="14"/>
                            <TextBlock Text="{Binding Titulo}" FontWeight="Bold" 
                                      Foreground="{Binding ColorTexto}"/>
                        </DockPanel>
                        <TextBlock Text="{Binding Mensaje}" TextWrapping="Wrap" 
                                  Foreground="{Binding ColorTexto}" FontSize="13"/>
                        <ProgressBar IsIndeterminate="True" Height="2" Margin="0,8,0,0" 
                                    Visibility="{Binding MostrarProgreso, Converter={StaticResource BoolToVisibility}}"/>
                    </StackPanel>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</Grid>
```

---

### 7.3 Accesibilidad Mejorada

**Problema Identificado:**
- No todos los controles tienen `AutomationProperties.Name`
- El orden de tabulación podría optimizarse
- Faltan tooltips en varios elementos

**Recomendación:**
```xml
<!-- Ejemplo de botón accesible -->
<Button Content="_Guardar estudiante" 
        Command="{Binding GuardarEstudianteCommand}"
        AutomationProperties.Name="Guardar los cambios del estudiante"
        AutomationProperties.HelpText="Presione Enter para guardar o Escape para cancelar"
        ToolTip="Guardar estudiante (Ctrl+G)"/>

<!-- Mejorar orden de tabulación -->
<Grid>
    <!-- Definir TabIndex explícito y lógico -->
    <TextBox x:Name="PrimerApellido" TabIndex="1"/>
    <TextBox x:Name="SegundoApellido" TabIndex="2"/>
    <TextBox x:Name="Nombres" TabIndex="3"/>
    <ComboBox x:Name="Genero" TabIndex="4"/>
    <DatePicker x:Name="FechaNacimiento" TabIndex="5"/>
    <Button x:Name="Guardar" Content="_Guardar" TabIndex="6" IsDefault="True"/>
    <Button x:Name="Cancelar" Content="_Cancelar" TabIndex="7" IsCancel="True"/>
</Grid>
```

---

### 7.4 Rendimiento y Optimización

**Problema Identificado:**
- Múltiples `DataContext` anidados pueden causar re-evaluaciones innecesarias
- Algunas grillas no tienen `EnableColumnVirtualization`

**Recomendación:**
```xml
<!-- Optimizar DataGrid -->
<DataGrid EnableRowVirtualization="True" 
          EnableColumnVirtualization="True"
          VirtualizingStackPanel.IsVirtualizing="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling"
          ScrollViewer.CanContentScroll="True"
          ItemsSource="{Binding Estudiantes, Mode=OneTime}">
    
    <!-- Congelar columnas clave -->
    <DataGrid.FrozenColumnCount>2</DataGrid.FrozenColumnCount>
</DataGrid>

<!-- Usar OneTime binding cuando sea posible -->
<TextBlock Text="{Binding Titulo, Mode=OneTime}"/>
```

---

## 8. CHECKLIST DE IMPLEMENTACIÓN PRIORIZADA

### 🔴 CRÍTICO (Implementar Inmediatamente)

- [ ] **8.1.1** Agregar campos NEM en formulario de estudiantes (ADE, lengua materna, adecuaciones)
- [ ] **8.1.2** Implementar validación en tiempo real con feedback visual
- [ ] **8.1.3** Agregar indicador visual de pestaña activa en navegación
- [ ] **8.1.4** Mejorar contraste de colores para accesibilidad WCAG AA
- [ ] **8.1.5** Agregar confirmación visual después de guardar (toast notifications)

### 🟠 ALTA PRIORIDAD (Próximo Sprint)

- [ ] **8.2.1** Enriquecer DataGrid de estudiantes con columnas NEM
- [ ] **8.2.2** Agregar contexto de actividad en vista de evaluación
- [ ] **8.2.3** Implementar dashboard visual en expediente del estudiante
- [ ] **8.2.4** Agregar campos NEM en proyecto didáctico (ejes articuladores, campos formativos)
- [ ] **8.2.5** Mejorar métricas de evaluación con porcentajes y recomendaciones

### 🟡 MEDIA PRIORIDAD (Mejoras Continuas)

- [ ] **8.3.1** Unificar estilos de tipografía con recursos de aplicación
- [ ] **8.3.2** Agregar tooltips y propiedades de automatización
- [ ] **8.3.3** Optimizar bindings con `Mode=OneTime` donde aplique
- [ ] **8.3.4** Agregar separadores visuales semanales en grilla mensual
- [ ] **8.3.5** Implementar priorización en fortalezas/dificultades

### 🟢 BAJA PRIORIDAD (Nice to Have)

- [ ] **8.4.1** Agregar gráficas de progreso por campo formativo
- [ ] **8.4.2** Implementar exportación a CSV de listas
- [ ] **8.4.3** Agregar modo oscuro (opcional futuro)
- [ ] **8.4.4** Animaciones sutiles en transiciones

---

## 9. CONSIDERACIONES ESPECÍFICAS NEM

### 9.1 Campos Formativos
La aplicación debe soportar los 4 campos formativos de NEM:
1. **Lenguajes** - Comunicación oral, escrita, lectura
2. **Saberes Científicos** - Pensamiento matemático, científico
3. **Ética, Naturaleza y Sociedades** - Formación cívica, historia, geografía
4. **De lo Humano y Comunitario** - Artes, educación física, vida saludable

### 9.2 Ejes Articuladores
Cada proyecto/actividad debería poder vincularse con:
- Pensamiento Crítico
- Aprendizaje Situado
- Interculturalidad Crítica
- Inclusión
- Igualdad de Género

### 9.3 Evaluación Formativa
La escala de logro debe reflejar:
- **Domina (D)** - Comprende y aplica autónomamente
- **Suficiente (S)** - Logra lo esencial con mínima guía
- **En Proceso (EP)** - Requiere apoyo parcial
- **Requiere Apoyo (RA)** - Necesita intervención significativa

### 9.4 Atención a la Diversidad
El sistema debe facilitar:
- Identificación de alumnos ADE
- Registro de adecuaciones curriculares
- Seguimiento diferenciado
- Documentación de apoyos específicos

---

## 10. CONCLUSIÓN

La aplicación **Sistema Docente Local** tiene una base técnica excelente con arquitectura limpia y separación de responsabilidades bien definida. El diseño visual sigue patrones modernos y existe conciencia de principios de accesibilidad.

**Fortalezas Principales:**
- ✅ Arquitectura MVVM bien implementada
- ✅ Separación clara de capas (Core, Application, Data, Presentation)
- ✅ Conciencia de accesibilidad básica (TabIndex, contrastes)
- ✅ Rendimiento considerado (virtualización)
- ✅ Atajos de teclado implementados

**Áreas de Mejora Críticas:**
- 🔴 Falta alineación completa con modelo pedagógico NEM
- 🔴 Campos esenciales para educación primaria no capturados
- 🔴 Feedback visual insuficiente en operaciones
- 🔴 Métricas presentes pero no accionables
- 🔴 Consistencia de estilos mejorable

**Impacto Esperado de Mejoras:**
Al implementar las recomendaciones de este reporte, la aplicación pasará de ser una herramienta funcional a una **solución pedagógicamente alineada** que realmente apoye la práctica docente bajo el modelo NEM, reduciendo la carga administrativa y mejorando la calidad del seguimiento estudiantil.

---

**Documento elaborado para:** Sistema Docente Local  
**Propósito:** Auditoría de UX/UI y Mejores Prácticas de Diseño  
**Próxima revisión sugerida:** Después de implementar mejoras críticas (4-6 semanas)
