## Purpose

Define la composición modular del shell WPF y las fronteras entre navegación global, vistas principales y dependencias específicas de cada módulo.

## ADDED Requirements

### Requirement: MainWindow actúa como shell visual
`MainWindow` SHALL ensamblar el encabezado global, las vistas de módulos, el feedback global y el cierre, y SHALL evitar contener las grillas y formularios internos principales de Grupo, Asistencia, Proyectos o Evaluación.

#### Scenario: Construir la ventana principal
- **WHEN** se instancia `MainWindow` con sus dependencias de presentación
- **THEN** la ventana puede inicializar y realizar layout mientras las superficies de módulo permanecen en controles dedicados

### Requirement: Vistas principales especializadas
Grupo, Asistencia, Proyectos y Evaluación SHALL vivir en `UserControl` dedicados y SHALL recibir su frontera de presentación mediante `DataContext` o propiedades explícitas, sin consultar SQLite desde code-behind.

#### Scenario: Cargar GrupoView
- **WHEN** el shell muestra el módulo Grupo
- **THEN** `GrupoView` recibe `GestionGrupoViewModel` y sus dependencias visuales explícitas sin resolver el shell concreto

### Requirement: Frontera propia para Asistencia
Asistencia SHALL disponer de una frontera de presentación que agrupe sus modos diario y mensual y SHALL evitar depender del `MainWindowViewModel` completo dentro de `AsistenciaView`.

#### Scenario: Cambiar entre asistencia diaria y mensual
- **WHEN** el usuario cambia el modo dentro del módulo Asistencia
- **THEN** la frontera del módulo coordina qué vista se muestra sin cambiar la navegación global

### Requirement: Navegación global centralizada
`MainWindowViewModel` SHALL coordinar el cambio entre módulos y la confirmación de cambios pendientes del módulo actual antes de abandonar su contexto.

#### Scenario: Salir de un módulo con cambios pendientes
- **WHEN** el usuario solicita navegar a otro módulo
- **THEN** el ViewModel del módulo actual tiene oportunidad de guardar, descartar o cancelar antes de completar la navegación

### Requirement: Virtualización y teclado permanecen locales
Las grillas operativas SHALL conservar virtualización y SHALL procesar sus atajos simples únicamente cuando el foco pertenece a la superficie correspondiente.

#### Scenario: Tecla de captura fuera de la grilla
- **WHEN** el foco está en un control de texto u otra superficie externa
- **THEN** la grilla no interpreta esa tecla como una acción de captura

### Requirement: Recursos visuales compartidos
Las vistas extraídas SHALL consumir recursos semánticos compartidos para soportar los temas de la aplicación y SHALL evitar hardcodear colores físicos como contrato de estado.

#### Scenario: Cambiar tema
- **WHEN** el usuario cambia entre los temas soportados
- **THEN** el shell y las vistas extraídas actualizan sus superficies mediante recursos semánticos