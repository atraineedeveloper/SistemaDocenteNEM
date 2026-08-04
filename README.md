# Sistema Docente Local

Aplicación local de escritorio para apoyar la operación cotidiana
de un docente de educación primaria.

## Estado actual

Solo está implementada la fundación técnica de la solución. Las funciones docentes descritas como objetivo siguen pendientes de diseño e implementación.

## Objetivo inicial

Ofrecer una experiencia sencilla para:

- administrar el grupo;
- pasar lista;
- crear actividades;
- registrar evaluación formativa;
- consultar seguimientos;
- generar reportes y respaldos.

## Tecnología prevista

- C#
- .NET 10
- WPF para Windows
- SQLite
- xUnit
- OpenSpec
- Git

## Forma de trabajo

El proyecto utiliza desarrollo guiado por especificaciones:

1. explorar la necesidad;
2. crear una propuesta;
3. revisar requisitos, diseño y tareas;
4. aprobar;
5. implementar;
6. ejecutar pruebas;
7. revisar el código;
8. archivar el cambio.

No se almacenarán datos reales de estudiantes en el repositorio.

## Documentación

La documentación completa del proyecto está disponible en el directorio `docs/`:

| Documento | Descripción |
|-----------|-------------|
| [Arquitectura](docs/architecture.md) | Visión general de la arquitectura en capas, proyectos y flujos principales |
| [Referencia de API](docs/api-reference.md) | Documentación detallada de clases, métodos y componentes por capa |
| [Guía de Desarrollo](docs/development-guide.md) | Instrucciones para configurar el entorno, convenciones y flujo de trabajo |
| [Diagramas](docs/diagrams.md) | Diagramas de arquitectura, agregados, flujos y base de datos |
| [Esquema de Base de Datos](docs/database-schema.md) | Documentación completa del esquema SQLite, tablas y migraciones |
| [Lista de Tareas Pendientes](docs/todo-list.md) | Funcionalidades pendientes, pruebas manuales y deuda técnica |

## Estructura del Proyecto

```
├── src/                          # Código fuente
│   ├── SistemaDocente.Core/      # Dominio
│   ├── SistemaDocente.Application/ # Casos de uso
│   ├── SistemaDocente.Data/      # Persistencia SQLite
│   ├── SistemaDocente.Presentation/ # MVVM portable
│   ├── SistemaDocente.Reporting/ # Reportes (pendiente)
│   └── SistemaDocente.App.Wpf/   # Interfaz WPF
├── tests/                        # Pruebas unitarias
├── openspec/                     # Especificaciones
└── docs/                         # Documentación
```

## Inicio Rápido

```bash
# Restaurar dependencias
dotnet restore SistemaDocente.sln

# Compilar
dotnet build SistemaDocente.sln

# Ejecutar pruebas
dotnet test SistemaDocente.sln

# Ejecutar aplicación (Windows)
dotnet run --project src/SistemaDocente.App.Wpf
```

Para más detalles, consulta la [Guía de Desarrollo](docs/development-guide.md).
