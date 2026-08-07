using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Data;

namespace SistemaDocente.App.Wpf.Demo;

internal static class DemoDataSeeder
{
    private const string NombreGrupoPrincipal = "4.º A · Demostración";

    internal static GrupoId AsegurarDatos(
        PersistenciaGrupoSqlite grupos,
        PersistenciaAsistenciaSqlite asistencias,
        PersistenciaProyectosSqlite proyectos,
        PersistenciaExpedienteSqlite expedientes)
    {
        ArgumentNullException.ThrowIfNull(grupos);
        ArgumentNullException.ThrowIfNull(asistencias);
        ArgumentNullException.ThrowIfNull(proyectos);
        ArgumentNullException.ThrowIfNull(expedientes);

        grupos.Inicializar();
        var existente = grupos.ListarTodos()
            .FirstOrDefault(x => string.Equals(x.NombreVisible, NombreGrupoPrincipal, StringComparison.Ordinal));
        if (existente is not null) return existente.Id;

        var grupo = CrearGrupoPrincipal();
        grupos.Guardar(grupo);

        var gestionAsistencia = new GestionAsistenciaCasosUso(grupos, asistencias);
        var gestionProyectos = new GestionProyectosActividadesCasosUso(grupos, proyectos, proyectos);
        var gestionExpediente = new GestionExpedienteCasosUso(
            grupos, asistencias, proyectos, proyectos, expedientes);

        // Historia previa: todos los 30 estudiantes iniciales pertenecían al padrón.
        SembrarAsistencia(gestionAsistencia, grupo, new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 17), 1);
        CrearProyectoHistorico(gestionProyectos, grupos, grupo.Id);

        // Un estudiante queda inactivo pero debe seguir apareciendo en historia previa.
        grupo = grupos.Cargar(grupo.Id)!;
        var historicoInactivo = grupo.Estudiantes.Single(x => x.NumeroLista == 30);
        grupo.DesactivarEstudiante(historicoInactivo.Id);
        grupos.Guardar(grupo);

        // Proyecto actual: las primeras actividades nacen con 29 estudiantes activos.
        var proyectoActual = CrearProyectoActualInicial(gestionProyectos, grupos, grupo.Id);
        grupo = grupos.Cargar(grupo.Id)!;
        SembrarAsistencia(gestionAsistencia, grupo, new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 7), 7);

        // Alta posterior: permite comprobar las celdas "—" en actividades anteriores.
        grupo.AgregarEstudiante(
            "Ximena Torres Vidal",
            31,
            "Torres",
            "Vidal",
            "Ximena",
            new DateOnly(2016, 11, 18),
            GeneroEstudiante.Mujer,
            new DateOnly(2026, 8, 10),
            "Ingreso posterior al inicio del proyecto de demostración.");
        grupos.Guardar(grupo);

        CrearActividadesPosteriores(gestionProyectos, grupos, proyectoActual);
        grupo = grupos.Cargar(grupo.Id)!;
        SembrarAsistencia(gestionAsistencia, grupo, new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 21), 19);
        CrearProyectoBorrador(gestionProyectos, grupos, grupo.Id);
        SembrarExpedientes(gestionExpediente, grupo);
        CrearGrupoSecundario(grupos);

        return grupo.Id;
    }

    private static Grupo CrearGrupoPrincipal()
    {
        var grupo = Grupo.Crear(NombreGrupoPrincipal);
        var estudiantes = new (string Nombre, string PrimerApellido, string SegundoApellido, string Nombres, GeneroEstudiante Genero, DateOnly Nacimiento)[]
        {
            ("Ana Sofía Rivera López", "Rivera", "López", "Ana Sofía", GeneroEstudiante.Mujer, new(2016, 2, 14)),
            ("Mateo Hernández Cruz", "Hernández", "Cruz", "Mateo", GeneroEstudiante.Hombre, new(2016, 5, 3)),
            ("Valeria Martínez Gómez", "Martínez", "Gómez", "Valeria", GeneroEstudiante.Mujer, new(2016, 1, 27)),
            ("Santiago Pérez Luna", "Pérez", "Luna", "Santiago", GeneroEstudiante.Hombre, new(2015, 12, 11)),
            ("Camila Sánchez Ortiz", "Sánchez", "Ortiz", "Camila", GeneroEstudiante.Mujer, new(2016, 7, 9)),
            ("Leonardo García Reyes", "García", "Reyes", "Leonardo", GeneroEstudiante.Hombre, new(2016, 4, 21)),
            ("Renata Flores Díaz", "Flores", "Díaz", "Renata", GeneroEstudiante.Mujer, new(2016, 8, 30)),
            ("Emiliano Vargas Ruiz", "Vargas", "Ruiz", "Emiliano", GeneroEstudiante.Hombre, new(2016, 3, 17)),
            ("Regina Morales Castillo", "Morales", "Castillo", "Regina", GeneroEstudiante.Mujer, new(2016, 6, 6)),
            ("Sebastián Ramírez Soto", "Ramírez", "Soto", "Sebastián", GeneroEstudiante.Hombre, new(2015, 10, 23)),
            ("Mariana Torres Jiménez", "Torres", "Jiménez", "Mariana", GeneroEstudiante.Mujer, new(2016, 9, 12)),
            ("Diego Navarro Méndez", "Navarro", "Méndez", "Diego", GeneroEstudiante.Hombre, new(2016, 2, 8)),
            ("Victoria Aguilar Campos", "Aguilar", "Campos", "Victoria", GeneroEstudiante.Mujer, new(2016, 11, 2)),
            ("Daniel Romero Silva", "Romero", "Silva", "Daniel", GeneroEstudiante.Hombre, new(2016, 1, 19)),
            ("Natalia Mendoza León", "Mendoza", "León", "Natalia", GeneroEstudiante.Mujer, new(2016, 5, 25)),
            ("Alejandro Castro Mora", "Castro", "Mora", "Alejandro", GeneroEstudiante.Hombre, new(2016, 4, 2)),
            ("Luciana Reyes Peña", "Reyes", "Peña", "Luciana", GeneroEstudiante.Mujer, new(2016, 7, 28)),
            ("Gael Vázquez Ríos", "Vázquez", "Ríos", "Gael", GeneroEstudiante.Hombre, new(2016, 3, 4)),
            ("Isabella Contreras Gil", "Contreras", "Gil", "Isabella", GeneroEstudiante.Mujer, new(2015, 12, 30)),
            ("Rodrigo Salas Pineda", "Salas", "Pineda", "Rodrigo", GeneroEstudiante.Hombre, new(2016, 10, 16)),
            ("Fernanda Cabrera Solís", "Cabrera", "Solís", "Fernanda", GeneroEstudiante.Mujer, new(2016, 8, 5)),
            ("Nicolás Domínguez Vera", "Domínguez", "Vera", "Nicolás", GeneroEstudiante.Hombre, new(2016, 6, 19)),
            ("Paula Guerrero Acosta", "Guerrero", "Acosta", "Paula", GeneroEstudiante.Mujer, new(2016, 2, 26)),
            ("Ángel Molina Fuentes", "Molina", "Fuentes", "Ángel", GeneroEstudiante.Hombre, new(2016, 9, 7)),
            ("Elena Rojas Velasco", "Rojas", "Velasco", "Elena", GeneroEstudiante.Mujer, new(2016, 4, 13)),
            ("Bruno Espinosa Lara", "Espinosa", "Lara", "Bruno", GeneroEstudiante.Hombre, new(2016, 1, 6)),
            ("Sara Medina Córdova", "Medina", "Córdova", "Sara", GeneroEstudiante.Mujer, new(2016, 11, 29)),
            ("Iker Lozano Padilla", "Lozano", "Padilla", "Iker", GeneroEstudiante.Hombre, new(2016, 5, 15)),
            ("Julieta Miranda Trejo", "Miranda", "Trejo", "Julieta", GeneroEstudiante.Mujer, new(2016, 3, 31)),
            ("Hugo Valdés Neri", "Valdés", "Neri", "Hugo", GeneroEstudiante.Hombre, new(2015, 9, 26)),
        };

        for (var indice = 0; indice < estudiantes.Length; indice++)
        {
            var dato = estudiantes[indice];
            grupo.AgregarEstudiante(
                dato.Nombre,
                indice + 1,
                dato.PrimerApellido,
                dato.SegundoApellido,
                dato.Nombres,
                dato.Nacimiento,
                dato.Genero,
                new DateOnly(2026, 7, 1),
                indice % 9 == 0 ? "Dato ficticio para probar observaciones del expediente." : string.Empty);
        }

        return grupo;
    }

    private static void CrearProyectoHistorico(
        GestionProyectosActividadesCasosUso gestion,
        PersistenciaGrupoSqlite grupos,
        GrupoId grupoId)
    {
        var proyecto = gestion.CrearProyecto(
            grupoId,
            new EntradaProyecto(
                "Historias de nuestra comunidad",
                "Proyecto ficticio para comprobar historial y proyecto finalizado.",
                new DateOnly(2026, 7, 6),
                new DateOnly(2026, 7, 31),
                "Datos exclusivos del modo demostración."));
        proyecto = gestion.CambiarEstadoProyecto(proyecto.ProyectoId, proyecto.Version, EstadoProyecto.EnCurso);

        var actividades = new[]
        {
            ("Conversatorio de saberes familiares", new DateOnly(2026, 7, 7)),
            ("Mapa de lugares significativos", new DateOnly(2026, 7, 10)),
            ("Entrevista a una persona de la comunidad", new DateOnly(2026, 7, 15)),
            ("Relato ilustrado", new DateOnly(2026, 7, 21)),
            ("Galería y reflexión colectiva", new DateOnly(2026, 7, 27)),
        };

        for (var i = 0; i < actividades.Length; i++)
        {
            CrearActividadConPatron(gestion, grupos, proyecto.ProyectoId, actividades[i].Item1, actividades[i].Item2, i + 2);
        }

        var actualizado = gestion.ObtenerProyecto(proyecto.ProyectoId);
        gestion.CambiarEstadoProyecto(actualizado.ProyectoId, actualizado.Version, EstadoProyecto.Finalizado);
    }

    private static ProyectoDetalle CrearProyectoActualInicial(
        GestionProyectosActividadesCasosUso gestion,
        PersistenciaGrupoSqlite grupos,
        GrupoId grupoId)
    {
        var proyecto = gestion.CrearProyecto(
            grupoId,
            new EntradaProyecto(
                "Periódico mural: voces de nuestra escuela",
                "Proyecto ficticio en curso con suficientes actividades para probar la matriz de evaluación.",
                new DateOnly(2026, 8, 3),
                new DateOnly(2026, 8, 28),
                "La mitad de las actividades se creó antes del ingreso de Ximena."));
        proyecto = gestion.CambiarEstadoProyecto(proyecto.ProyectoId, proyecto.Version, EstadoProyecto.EnCurso);

        var tempranas = new[]
        {
            ("Exploramos periódicos y sus secciones", new DateOnly(2026, 8, 3)),
            ("Elegimos temas de interés escolar", new DateOnly(2026, 8, 5)),
            ("Investigamos fuentes confiables", new DateOnly(2026, 8, 7)),
            ("Escribimos el primer borrador", new DateOnly(2026, 8, 10)),
        };

        for (var i = 0; i < tempranas.Length; i++)
        {
            CrearActividadConPatron(gestion, grupos, proyecto.ProyectoId, tempranas[i].Item1, tempranas[i].Item2, i + 11);
        }

        return gestion.ObtenerProyecto(proyecto.ProyectoId);
    }

    private static void CrearActividadesPosteriores(
        GestionProyectosActividadesCasosUso gestion,
        PersistenciaGrupoSqlite grupos,
        ProyectoDetalle proyecto)
    {
        var posteriores = new[]
        {
            ("Revisamos claridad y ortografía", new DateOnly(2026, 8, 12)),
            ("Diseñamos títulos e ilustraciones", new DateOnly(2026, 8, 14)),
            ("Organizamos las secciones del mural", new DateOnly(2026, 8, 18)),
            ("Presentamos el periódico al grupo", new DateOnly(2026, 8, 21)),
            ("Reflexionamos sobre lo aprendido", new DateOnly(2026, 8, 25)),
        };

        for (var i = 0; i < posteriores.Length; i++)
        {
            CrearActividadConPatron(gestion, grupos, proyecto.ProyectoId, posteriores[i].Item1, posteriores[i].Item2, i + 23);
        }
    }

    private static void CrearProyectoBorrador(
        GestionProyectosActividadesCasosUso gestion,
        PersistenciaGrupoSqlite grupos,
        GrupoId grupoId)
    {
        var proyecto = gestion.CrearProyecto(
            grupoId,
            new EntradaProyecto(
                "Guardianes del agua",
                "Borrador ficticio para probar filtros y estados de proyectos.",
                new DateOnly(2026, 9, 7),
                new DateOnly(2026, 9, 25),
                string.Empty));

        CrearActividadConPatron(gestion, grupos, proyecto.ProyectoId, "¿Cómo usamos el agua?", new DateOnly(2026, 9, 8), 37);
        CrearActividadConPatron(gestion, grupos, proyecto.ProyectoId, "Registro de consumo y propuestas", new DateOnly(2026, 9, 14), 41);
    }

    private static void CrearActividadConPatron(
        GestionProyectosActividadesCasosUso gestion,
        PersistenciaGrupoSqlite grupos,
        ProyectoId proyectoId,
        string titulo,
        DateOnly fecha,
        int semilla)
    {
        var grupoId = gestion.ObtenerProyecto(proyectoId).GrupoId;
        var grupo = grupos.Cargar(grupoId)!;
        var entradas = grupo.EstudiantesActivos
            .Select(estudiante =>
            {
                var (estado, nivel) = SeguimientoPara(estudiante.NumeroLista, semilla);
                return new EntradaEntregaActividad(
                    estudiante.Id,
                    estado,
                    nivel,
                    ObservacionPara(estudiante.NumeroLista, semilla));
            })
            .ToArray();
        gestion.CrearActividad(
            proyectoId,
            new EntradaActividad(
                titulo,
                "Actividad ficticia para explorar estados, teclado, filtros y seguimiento.",
                fecha,
                string.Empty,
                entradas));
    }

    private static (EstadoEntregaActividad Estado, NivelLogro Nivel) SeguimientoPara(int numeroLista, int semilla)
    {
        var valor = (numeroLista * 3 + semilla) % 19;
        return valor switch
        {
            0 => (EstadoEntregaActividad.Pendiente, NivelLogro.Pendiente),
            1 => (EstadoEntregaActividad.Entregada, NivelLogro.Pendiente),
            2 or 3 => (EstadoEntregaActividad.Entregada, NivelLogro.RequiereApoyo),
            4 => (EstadoEntregaActividad.NoEntregada, NivelLogro.Pendiente),
            5 or 6 or 7 => (EstadoEntregaActividad.Entregada, NivelLogro.EnProceso),
            8 or 9 or 10 or 11 or 12 => (EstadoEntregaActividad.Entregada, NivelLogro.Suficiente),
            _ => (EstadoEntregaActividad.Entregada, NivelLogro.Domina),
        };
    }

    private static string ObservacionPara(int numeroLista, int semilla)
    {
        var valor = (numeroLista + semilla) % 13;
        return valor switch
        {
            0 => "Explica sus ideas con claridad y aporta ejemplos pertinentes.",
            1 => "Requiere organizar mejor la información antes de presentar su producto.",
            2 => "Conviene ofrecer una pregunta guía en la siguiente actividad.",
            _ => string.Empty,
        };
    }

    private static void SembrarAsistencia(
        GestionAsistenciaCasosUso gestion,
        Grupo grupo,
        DateOnly inicio,
        DateOnly termino,
        int semilla)
    {
        for (var fecha = inicio; fecha <= termino; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            var entradas = grupo.EstudiantesActivos.Select(estudiante =>
                new EntradaEstadoAsistencia(
                    estudiante.Id,
                    EstadoAsistenciaPara(estudiante.NumeroLista, fecha.Day, semilla))).ToArray();
            gestion.Guardar(grupo.Id, fecha, entradas);
        }
    }

    private static EstadoAsistencia EstadoAsistenciaPara(int numeroLista, int dia, int semilla)
    {
        var valor = (numeroLista * 5 + dia + semilla) % 31;
        return valor switch
        {
            0 or 1 => EstadoAsistencia.Falta,
            2 => EstadoAsistencia.Justificada,
            3 or 4 => EstadoAsistencia.Retardo,
            _ => EstadoAsistencia.Presente,
        };
    }

    private static void SembrarExpedientes(GestionExpedienteCasosUso gestion, Grupo grupo)
    {
        var ana = grupo.Estudiantes.Single(x => x.NumeroLista == 1);
        var mateo = grupo.Estudiantes.Single(x => x.NumeroLista == 2);
        var valeria = grupo.Estudiantes.Single(x => x.NumeroLista == 3);

        gestion.RegistrarNotaPedagogica(grupo.Id, ana.Id, TipoNotaPedagogica.Fortaleza,
            "Participa con iniciativa y suele relacionar las actividades con experiencias de su comunidad.");
        gestion.RegistrarNotaPedagogica(grupo.Id, ana.Id, TipoNotaPedagogica.ObservacionCronologica,
            "Durante el trabajo de periódico mural propuso entrevistar a integrantes de la comunidad escolar.");

        gestion.RegistrarNotaPedagogica(grupo.Id, mateo.Id, TipoNotaPedagogica.Dificultad,
            "En textos extensos necesita apoyo para organizar ideas principales y secundarias.");
        gestion.RegistrarNotaPedagogica(grupo.Id, mateo.Id, TipoNotaPedagogica.ApoyoAplicado,
            "Se utilizó una guía breve con preguntas y un organizador gráfico antes de redactar.");
        gestion.RegistrarAcuerdoTutor(
            grupo.Id,
            mateo.Id,
            "Dar continuidad al hábito de lectura y organización de tareas.",
            "Familia y docente revisarán dos veces por semana una lista breve de pendientes y lecturas.",
            new DateOnly(2026, 8, 6),
            new DateOnly(2026, 8, 20));

        gestion.RegistrarNotaPedagogica(grupo.Id, valeria.Id, TipoNotaPedagogica.Fortaleza,
            "Colabora de manera respetuosa, escucha a sus compañeras y compañeros y explica procedimientos.");
    }

    private static void CrearGrupoSecundario(PersistenciaGrupoSqlite grupos)
    {
        var grupo = Grupo.Crear("5.º B · Muestra");
        var nombres = new[]
        {
            "Abril Núñez Paz", "Carlos Mejía Roldán", "Diana Solano Pérez", "Erick Franco Ruiz",
            "Fátima Cuevas Luna", "Gerardo Ochoa León", "Jimena Pardo Silva", "Luis Castañeda Mora",
        };
        for (var i = 0; i < nombres.Length; i++)
        {
            grupo.AgregarEstudiante(
                nombres[i],
                i + 1,
                fechaNacimiento: new DateOnly(2015, (i % 12) + 1, Math.Min(20, i + 3)),
                genero: i % 2 == 0 ? GeneroEstudiante.Mujer : GeneroEstudiante.Hombre,
                fechaIngreso: new DateOnly(2026, 7, 1));
        }
        grupos.Guardar(grupo);
    }
}
