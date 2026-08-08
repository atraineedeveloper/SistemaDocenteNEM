using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class ImportacionEstudiantesCasosUsoTests
{
    [Fact]
    public void CrearPreviaUnigradoPredeterminaGradoYMapeaCampos()
    {
        var grupo = Grupo.Crear("4.º A");
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);
        var hoja = new HojaTabular(
            "Alumnos",
            [
                CeldaTabular.DesdeTexto("No."),
                CeldaTabular.DesdeTexto("Nombre completo"),
                CeldaTabular.DesdeTexto("Primer apellido"),
                CeldaTabular.DesdeTexto("Nombres"),
                CeldaTabular.DesdeTexto("Nacimiento"),
                CeldaTabular.DesdeTexto("Género"),
            ],
            [
                new FilaTabular(
                    2,
                    [
                        CeldaTabular.DesdeNumero(7, "7"),
                        CeldaTabular.DesdeTexto("Ana López"),
                        CeldaTabular.DesdeTexto("López"),
                        CeldaTabular.DesdeTexto("Ana María"),
                        CeldaTabular.DesdeFecha(new DateOnly(2017, 8, 15), "2017-08-15"),
                        CeldaTabular.DesdeTexto("F"),
                    ]),
            ]);
        MapeoColumnaImportacion[] mapeo =
        [
            new(0, CampoImportacionEstudiante.NumeroLista),
            new(1, CampoImportacionEstudiante.NombreCompleto),
            new(2, CampoImportacionEstudiante.PrimerApellido),
            new(3, CampoImportacionEstudiante.Nombres),
            new(4, CampoImportacionEstudiante.FechaNacimiento),
            new(5, CampoImportacionEstudiante.Genero),
        ];

        var previa = casosUso.CrearPrevia(grupo.Id, hoja, mapeo);

        var fila = Assert.Single(previa.Filas);
        Assert.Equal(EstadoFilaImportacion.Lista, fila.Estado);
        Assert.Equal(7, fila.NumeroLista);
        Assert.Equal("Ana López", fila.NombreVisible);
        Assert.Equal("López", fila.PrimerApellido);
        Assert.Equal("Ana María", fila.Nombres);
        Assert.Equal(new DateOnly(2017, 8, 15), fila.FechaNacimiento);
        Assert.Equal(GeneroEstudiante.Mujer, fila.Genero);
        Assert.Equal(GradoPrimaria.Cuarto, fila.Grado);
        Assert.True(fila.GradoPredeterminadoPorGrupo);
        Assert.True(previa.PuedeConfirmarse);
    }

    [Fact]
    public void MultigradoSinGradoRequiereRevision()
    {
        var grupo = Grupo.Crear("Multigrado");
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(
                grupo.Id,
                gradosAtendidos: [GradoPrimaria.Tercero, GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);

        var previa = casosUso.Revalidar(grupo.Id, [CrearFila(2, "1", "Ana López")]);

        var fila = Assert.Single(previa.Filas);
        Assert.Equal(EstadoFilaImportacion.RequiereRevision, fila.Estado);
        Assert.Contains(fila.Problemas, problema => problema.Codigo == "missing-multigrade-grade");
        Assert.False(previa.PuedeConfirmarse);
    }

    [Fact]
    public void ConflictosNumeroListaBloqueanImportacion()
    {
        var grupo = Grupo.Crear("4.º A");
        grupo.AgregarEstudiante("Existente", 3, grado: GradoPrimaria.Cuarto);
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);

        var previa = casosUso.Revalidar(
            grupo.Id,
            [
                CrearFila(2, "3", "Ana López"),
                CrearFila(3, "5", "Luis Pérez"),
                CrearFila(4, "5", "María Gómez"),
            ]);

        Assert.Equal(3, previa.Invalidas);
        Assert.Contains(
            previa.Filas[0].Problemas,
            problema => problema.Codigo == "active-list-number-conflict");
        Assert.Contains(
            previa.Filas[1].Problemas,
            problema => problema.Codigo == "duplicate-import-list-number");
        Assert.Contains(
            previa.Filas[2].Problemas,
            problema => problema.Codigo == "duplicate-import-list-number");
    }

    [Fact]
    public void DuplicadoProbableRequiereDecisionYNoReactivaExistente()
    {
        var grupo = Grupo.Crear("4.º A");
        var existente = grupo.AgregarEstudiante(
            "Ana López",
            1,
            fechaNacimiento: new DateOnly(2017, 8, 15),
            grado: GradoPrimaria.Cuarto);
        grupo.DesactivarEstudiante(existente.Id);
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);
        var original = CrearFila(2, "8", "Ana López") with
        {
            FechaNacimientoTexto = "2017-08-15",
        };

        var previa = casosUso.Revalidar(grupo.Id, [original]);
        var requiereRevision = Assert.Single(previa.Filas);
        Assert.Equal(EstadoFilaImportacion.RequiereRevision, requiereRevision.Estado);
        Assert.Contains(requiereRevision.Problemas, problema => problema.Codigo == "probable-duplicate");

        var autorizada = requiereRevision with
        {
            ImportarDuplicadoProbableComoNuevo = true,
        };
        var lista = casosUso.Revalidar(grupo.Id, [autorizada]);
        Assert.Equal(EstadoFilaImportacion.Lista, Assert.Single(lista.Filas).Estado);

        var resultado = casosUso.Confirmar(grupo.Id, lista.Filas);

        Assert.True(resultado.Completada);
        Assert.Equal(1, almacenamiento.GuardarLlamadas);
        var persistido = almacenamiento.CargarPersistido();
        Assert.Equal(2, persistido.Estudiantes.Count);
        Assert.False(Assert.Single(persistido.Estudiantes, estudiante => estudiante.Id == existente.Id).EstaActivo);
        Assert.True(Assert.Single(persistido.Estudiantes, estudiante => estudiante.Id != existente.Id).EstaActivo);
    }

    [Fact]
    public void ConfirmarGuardaUnaSolaVezYPreservaNombreVisibleExplicito()
    {
        var grupo = Grupo.Crear("4.º A");
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);
        var primera = CrearFila(2, "1", "Ana López") with
        {
            PrimerApellido = "López",
            Nombres = "Ana María",
        };
        var segunda = CrearFila(3, "2", "Luis Pérez");
        var previa = casosUso.Revalidar(grupo.Id, [primera, segunda]);

        var resultado = casosUso.Confirmar(grupo.Id, previa.Filas);

        Assert.True(resultado.Completada);
        Assert.Equal(2, resultado.Importados);
        Assert.Equal(1, almacenamiento.GuardarLlamadas);
        var persistido = almacenamiento.CargarPersistido();
        Assert.Equal(2, persistido.Estudiantes.Count);
        var ana = Assert.Single(persistido.Estudiantes, estudiante => estudiante.NumeroLista == 1);
        Assert.Equal("Ana López", ana.NombreVisible);
        Assert.Equal("López", ana.PrimerApellido);
        Assert.Equal("Ana María", ana.Nombres);
    }

    [Fact]
    public void ConfirmarRevalidaContraGrupoFrescoSinGuardarParcialmente()
    {
        var grupo = Grupo.Crear("4.º A");
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);
        var previa = casosUso.Revalidar(grupo.Id, [CrearFila(2, "5", "Ana López")]);
        Assert.True(previa.PuedeConfirmarse);

        almacenamiento.ModificarPersistido(
            actual => actual.AgregarEstudiante("Llegó después", 5, grado: GradoPrimaria.Cuarto));

        var resultado = casosUso.Confirmar(grupo.Id, previa.Filas);

        Assert.False(resultado.Completada);
        Assert.Equal(0, almacenamiento.GuardarLlamadas);
        var pendiente = Assert.IsType<PreviaImportacionEstudiantes>(resultado.PreviaPendiente);
        Assert.Contains(
            Assert.Single(pendiente.Filas).Problemas,
            problema => problema.Codigo == "active-list-number-conflict");
        Assert.Single(almacenamiento.CargarPersistido().Estudiantes);
    }

    [Fact]
    public void CambioGradoGrupoDespuesDePreviaExigeNuevaRevision()
    {
        var grupo = Grupo.Crear("Grupo");
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);
        var previa = casosUso.Revalidar(grupo.Id, [CrearFila(2, "1", "Ana López")]);
        Assert.Equal(GradoPrimaria.Cuarto, Assert.Single(previa.Filas).Grado);

        contextos.Contexto = ContextoGrupo.Crear(
            grupo.Id,
            gradosAtendidos: [GradoPrimaria.Quinto]);
        var resultado = casosUso.Confirmar(grupo.Id, previa.Filas);

        Assert.False(resultado.Completada);
        var pendiente = Assert.IsType<PreviaImportacionEstudiantes>(resultado.PreviaPendiente);
        var fila = Assert.Single(pendiente.Filas);
        Assert.Equal(EstadoFilaImportacion.RequiereRevision, fila.Estado);
        Assert.Contains(fila.Problemas, problema => problema.Codigo == "group-grade-changed");
        Assert.Equal(0, almacenamiento.GuardarLlamadas);
    }

    [Fact]
    public void GeneroAmbiguoEInvalidezImpidenPersistencia()
    {
        var grupo = Grupo.Crear("4.º A");
        var almacenamiento = new AlmacenamientoGruposPrueba(grupo);
        var contextos = new AlmacenamientoContextoPrueba(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var casosUso = new ImportacionEstudiantesCasosUso(almacenamiento, contextos);
        var ambigua = CrearFila(2, "1", "Ana López") with { GeneroTexto = "M" };
        var invalida = CrearFila(3, "0", "Luis Pérez");

        var previa = casosUso.Revalidar(grupo.Id, [ambigua, invalida]);
        var resultado = casosUso.Confirmar(grupo.Id, previa.Filas);

        Assert.Equal(1, previa.RequierenRevision);
        Assert.Equal(1, previa.Invalidas);
        Assert.False(resultado.Completada);
        Assert.Equal(0, almacenamiento.GuardarLlamadas);
        Assert.Empty(almacenamiento.CargarPersistido().Estudiantes);
    }

    private static FilaImportacionEstudiante CrearFila(
        int numeroOrigen,
        string numeroLista,
        string nombreCompleto,
        string grado = "") =>
        new(
            numeroOrigen,
            numeroLista,
            nombreCompleto,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            grado,
            string.Empty);

    private static Grupo Clonar(Grupo grupo) =>
        Grupo.Rehidratar(
            grupo.Id,
            grupo.NombreVisible,
            grupo.Estudiantes.Select(estudiante => new DatosEstudianteRehidratado(
                estudiante.Id,
                estudiante.NombreVisible,
                estudiante.PrimerApellido,
                estudiante.SegundoApellido,
                estudiante.Nombres,
                estudiante.FechaNacimiento,
                estudiante.Genero,
                estudiante.FechaIngreso,
                estudiante.Observaciones,
                estudiante.NumeroLista,
                estudiante.EstaActivo,
                estudiante.Grado)).ToArray());

    private sealed class AlmacenamientoGruposPrueba : IAlmacenamientoGrupos
    {
        private Grupo _persistido;

        public AlmacenamientoGruposPrueba(Grupo grupo)
        {
            _persistido = Clonar(grupo);
        }

        public int GuardarLlamadas { get; private set; }

        public Grupo? Cargar(GrupoId grupoId) =>
            grupoId == _persistido.Id ? Clonar(_persistido) : null;

        public bool Existe(GrupoId grupoId) => grupoId == _persistido.Id;

        public void Guardar(Grupo grupo)
        {
            GuardarLlamadas++;
            _persistido = Clonar(grupo);
        }

        public IReadOnlyList<Grupo> ListarTodos() => [Clonar(_persistido)];

        public Grupo CargarPersistido() => Clonar(_persistido);

        public void ModificarPersistido(Action<Grupo> accion)
        {
            var copia = Clonar(_persistido);
            accion(copia);
            _persistido = copia;
        }
    }

    private sealed class AlmacenamientoContextoPrueba : IAlmacenamientoContextoGrupo
    {
        public AlmacenamientoContextoPrueba(ContextoGrupo contexto)
        {
            Contexto = contexto;
        }

        public ContextoGrupo Contexto { get; set; }

        public ContextoGrupo? Cargar(GrupoId grupoId) =>
            grupoId == Contexto.GrupoId ? Contexto : null;

        public void Guardar(ContextoGrupo contexto)
        {
            Contexto = contexto;
        }
    }
}