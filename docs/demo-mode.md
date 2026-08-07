# Modo de demostración

El modo de demostración permite revisar la interfaz y las funciones del Sistema Docente NEM con un conjunto rico de datos ficticios sin escribir en la base de datos real del docente.

## Ejecutar

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo
```

La primera ejecución crea los datos de demostración. Las modificaciones hechas durante la sesión se conservan para poder probar edición, guardado e historial.

## Reiniciar los datos ficticios

```powershell
dotnet run --project .\src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj -- --demo-reset
```

`--demo-reset` elimina exclusivamente el almacenamiento de demostración y vuelve a sembrarlo. No puede ejecutar el borrado sobre rutas de producción.

## Aislamiento de almacenamiento

Producción:

```text
%LOCALAPPDATA%\SistemaDocenteNEM\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM\data\app-state.json
```

Demostración:

```text
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\sistema-docente.db
%LOCALAPPDATA%\SistemaDocenteNEM-Demo\data\app-state.json
```

El shell muestra un distintivo `DEMO` y el título de la ventana también indica el modo.

## Dataset

El conjunto ficticio incluye:

- `4.º A · Demostración` con 30 estudiantes activos actuales, un estudiante histórico inactivo y una alta posterior al inicio de un proyecto;
- `5.º B · Muestra` para comprobar el selector de grupos;
- asistencia de julio y agosto de 2026 con presentes, faltas, retardos y faltas justificadas;
- un proyecto histórico finalizado;
- un proyecto en curso con nueve actividades y padrones históricos distintos;
- un proyecto borrador;
- niveles de logro variados: Pendiente, Domina, Suficiente, En proceso, Requiere apoyo y No entregó;
- observaciones de evaluación;
- notas pedagógicas y un acuerdo ficticio con tutor.

Los nombres, observaciones y acuerdos son ficticios y existen exclusivamente para probar la aplicación.

## Caso clave de la matriz de evaluación

Las primeras actividades del proyecto `Periódico mural: voces de nuestra escuela` se crean antes de la incorporación de `Ximena Torres Vidal`. Las actividades posteriores sí la incluyen. Por ello, en Evaluación su fila debe mostrar `—` en las primeras columnas y niveles editables en las posteriores.

Ese escenario permite validar que la nueva matriz respeta el padrón histórico de cada actividad y no agrega estudiantes retroactivamente.