using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class ImportacionEstudiantesViewModelTests
{
    [Fact]
    public void CargarArchivoSugiereMapeoYAvanzaAColumnas()
    {
        var contexto = new ContextoPrueba();
        contexto.ViewModel.Inicializar(contexto.Grupo.Id);

        var cargado = contexto.ViewModel.CargarArchivo("alumnos.xlsx");

        Assert.True(cargado);
        Assert.Equal(PasoImportacionEstudiantes.Columnas, contexto.ViewModel.Paso);
        Assert.Equal("alumnos.xlsx", contexto.ViewModel.NombreArchivo);
        Assert.Equal(4, contexto.ViewModel.Columnas.Count);
        Assert.Equal(CampoImportacionEstudiante.NumeroLista, contexto.ViewModel.Columnas[0].Campo);
        Assert.Equal(CampoImportacionEstudiante.NombreCompleto, contexto.ViewModel.Columnas[1].Campo);
        Assert.Equal(CampoImportacionEstudiante.Grado, contexto.ViewModel.Columnas[2].Campo);
        Assert.Equal(CampoImportacionEstudiante.Observaciones, contexto.ViewModel.Columnas[3].Campo);
        Assert.True(contexto.ViewModel.GenerarPreviaCommand.CanExecute(null));
    }

    [Fact]
    public void PreviaUnigradoQuedaListaParaConfirmar()
    {
        var contexto = new ContextoPrueba();
        contexto.ViewModel.Inicializar(contexto.Grupo.Id);
        contexto.ViewModel.CargarArchivo("alumnos.xlsx");

        contexto.ViewModel.GenerarPreviaCommand.Execute(null);

        Assert.Equal(PasoImportacionEstudiantes.Previa, contexto.ViewModel.Paso);
        Assert.Equal(1, contexto.ViewModel.Listas);
        Assert.Equal(0, contexto.ViewModel.RequierenRevision);
        Assert.Equal("4.º", Assert.Single(contexto.ViewModel.Filas).GradoResuelto);
        Assert.True(contexto.ViewModel.PrepararConfirmacionCommand.CanExecute(null));
    }

    [Fact]
    public void FilaInvalidaPuedeExcluirseYDejaDeBloquear()
    {
        var contexto = new ContextoPrueba(incluirFilaInvalida: true);
        contexto.ViewModel.Inicializar(contexto.Grupo.Id);
        contexto.ViewModel.CargarArchivo("alumnos.xlsx");
        contexto.ViewModel.GenerarPreviaCommand.Execute(null);
        Assert.Equal(1, contexto.ViewModel.Invalidas);

        contexto.ViewModel.FilaSeleccionada = Assert.Single(
            contexto.ViewModel.Filas,
            fila => fila.Estado == EstadoFilaImportacion.Invalida);
        contexto.ViewModel.AlternarExclusionCommand.Execute(null);

        Assert.Equal(1, contexto.ViewModel.Excluidas);
        Assert.Equal(0, contexto.ViewModel.Invalidas);
        Assert.True(contexto.ViewModel.PrepararConfirmacionCommand.CanExecute(null));
    }

    [Fact]
    public void ConfirmarImportacionMuestraResultadoYGuardaUnaVez()
    {
        var contexto = new ContextoPrueba();
        contexto.ViewModel.Inicializar(contexto.Grupo.Id);
        contexto.ViewModel.CargarArchivo("alumnos.xlsx");
        contexto.ViewModel.GenerarPreviaCommand.Execute(null);
        contexto.ViewModel.PrepararConfirmacionCommand.Execute(null);

        Assert.Equal(PasoImportacionEstudiantes.Confirmacion, contexto.ViewModel.Paso);
        Assert.True(contexto.ViewModel.ConfirmarCommand.CanExecute(null));

        contexto.ViewModel.ConfirmarCommand.Execute(null);

        Assert.Equal(PasoImportacionEstudiantes.Resultado, contexto.ViewModel.Paso);
        Assert.Equal(1, contexto.ViewModel.Importados);
        Assert.Equal(1, contexto.Grupos.GuardarLlamadas);
        Assert.Single(contexto.Grupos.Persistido.Estudiantes);
    }

    [Fact]
    public void ErrorDeLecturaPermaneceEnArchivoSinExponerRutaTecnica()
    {
        var contexto = new ContextoPrueba();
        contexto.Lector.Error = new ImportacionTabularException("El archivo seleccionado no es válido.");
        contexto.ViewModel.Inicializar(contexto.Grupo.Id);

        var cargado = contexto.ViewModel.CargarArchivo("C:\\privado\\alumnos.xlsx");

        Assert.False(cargado);
        Assert.Equal(PasoImportacionEstudiantes.Archivo, contexto.ViewModel.Paso);
        Assert.Equal("El archivo seleccionado no es válido.", contexto.ViewModel.Mensaje);
        Assert.DoesNotContain("C:\\privado", contexto.ViewModel.Mensaje, StringComparison.Ordinal);
    }

    private sealed class ContextoPrueba
    {
        internal ContextoPrueba(bool incluirFilaInvalida = false)
        {
            Grupo = Grupo.Crear("4.º A");
            Grupos = new AlmacenamientoGruposDoble(Grupo);
            Contextos = new AlmacenamientoContextoDoble(
                ContextoGrupo.Crear(Grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
            Lector = new LectorDoble(CrearDocumento(incluirFilaInvalida));
            ViewModel = new ImportacionEstudiantesViewModel(
                Lector,
                new ImportacionEstudiantesCasosUso(Grupos, Contextos));
        }

        internal Grupo Grupo { get; }

        internal AlmacenamientoGruposDoble Grupos { get; }

        internal AlmacenamientoContextoDoble Contextos { get; }

        internal LectorDoble Lector { get; }

        internal ImportacionEstudiantesViewModel ViewModel { get; }

        private static DocumentoTabular CrearDocumento(bool incluirFilaInvalida)
        {
            var filas = new List<FilaTabular>
            {
                new(
                    2,
                    [
                        CeldaTabular.DesdeTexto("1"),
                        CeldaTabular.DesdeTexto("Ana López"),
                        CeldaTabular.Vacia,
                        CeldaTabular.DesdeTexto("Sin observaciones"),
                    ]),
            };

            if (incluirFilaInvalida)
            {
                filas.Add(new FilaTabular(
                    3,
                    [
                        CeldaTabular.DesdeTexto("0"),
                        CeldaTabular.DesdeTexto("Luis Pérez"),
                        CeldaTabular.Vacia,
                        CeldaTabular.Vacia,
                    ]));
            }

            return new DocumentoTabular(
                "alumnos.xlsx",
                [
                    new HojaTabular(
                        "Alumnos",
                        [
                            CeldaTabular.DesdeTexto("No."),
                            CeldaTabular.DesdeTexto("Nombre completo"),
                            CeldaTabular.DesdeTexto("Grado"),
                            CeldaTabular.DesdeTexto("Observaciones"),
                        ],
                        filas),
                ]);
        }
    }

    private sealed class LectorDoble : ILectorImportacionTabular
    {
        private readonly DocumentoTabular _documento;

        internal LectorDoble(DocumentoTabular documento)
        {
            _documento = documento;
        }

        internal ImportacionTabularException? Error { get; set; }

        public DocumentoTabular Leer(string rutaArchivo) =>
            Error is not null ? throw Error : _documento;
    }

    private sealed class AlmacenamientoContextoDoble : IAlmacenamientoContextoGrupo
    {
        private ContextoGrupo _contexto;

        internal AlmacenamientoContextoDoble(ContextoGrupo contexto)
        {
            _contexto = contexto;
        }

        public ContextoGrupo? Cargar(GrupoId grupoId) =>
            grupoId == _contexto.GrupoId ? _contexto : null;

        public void Guardar(ContextoGrupo contexto)
        {
            _contexto = contexto;
        }
    }

    private sealed class AlmacenamientoGruposDoble : IAlmacenamientoGrupos
    {
        internal AlmacenamientoGruposDoble(Grupo grupo)
        {
            Persistido = Clonar(grupo);
        }

        internal Grupo Persistido { get; private set; }

        internal int GuardarLlamadas { get; private set; }

        public Grupo? Cargar(GrupoId grupoId) =>
            grupoId == Persistido.Id ? Clonar(Persistido) : null;

        public bool Existe(GrupoId grupoId) => grupoId == Persistido.Id;

        public void Guardar(Grupo grupo)
        {
            GuardarLlamadas++;
            Persistido = Clonar(grupo);
        }

        public IReadOnlyList<Grupo> ListarTodos() => [Clonar(Persistido)];

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
    }
}
