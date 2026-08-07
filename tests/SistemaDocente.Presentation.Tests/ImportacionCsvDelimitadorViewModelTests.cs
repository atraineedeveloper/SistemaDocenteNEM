using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class ImportacionCsvDelimitadorViewModelTests
{
    [Fact]
    public void CsvAmbiguoPermiteElegirDelimitadorYReintentar()
    {
        var grupo = Grupo.Crear("4.º A");
        var grupos = new AlmacenamientoGruposDoble(grupo);
        var contextos = new AlmacenamientoContextoDoble(
            ContextoGrupo.Crear(grupo.Id, gradosAtendidos: [GradoPrimaria.Cuarto]));
        var lector = new LectorCsvConfigurableDoble();
        var viewModel = new ImportacionEstudiantesViewModel(
            lector,
            new ImportacionEstudiantesCasosUso(grupos, contextos));
        viewModel.Inicializar(grupo.Id);

        var cargado = viewModel.CargarArchivo("alumnos.csv");

        Assert.False(cargado);
        Assert.Equal(PasoImportacionEstudiantes.Archivo, viewModel.Paso);
        Assert.True(viewModel.RequiereDelimitadorCsv);
        Assert.Equal(3, viewModel.OpcionesDelimitadoresCsv.Count);
        Assert.False(viewModel.ReintentarCsvCommand.CanExecute(null));

        viewModel.DelimitadorCsvSeleccionado = Assert.Single(
            viewModel.OpcionesDelimitadoresCsv,
            opcion => opcion.Delimitador == ';');

        Assert.True(viewModel.ReintentarCsvCommand.CanExecute(null));
        viewModel.ReintentarCsvCommand.Execute(null);

        Assert.Equal(';', lector.UltimoDelimitador);
        Assert.False(viewModel.RequiereDelimitadorCsv);
        Assert.Equal(PasoImportacionEstudiantes.Columnas, viewModel.Paso);
        Assert.Equal("alumnos.csv", viewModel.NombreArchivo);
        Assert.Equal(CampoImportacionEstudiante.NumeroLista, viewModel.Columnas[0].Campo);
        Assert.Equal(CampoImportacionEstudiante.NombreCompleto, viewModel.Columnas[1].Campo);
    }

    private sealed class LectorCsvConfigurableDoble : ILectorImportacionCsvConfigurable
    {
        public char? UltimoDelimitador { get; private set; }

        public DocumentoTabular Leer(string rutaArchivo) =>
            throw new ImportacionTabularException(
                "No fue posible detectar de forma unívoca el delimitador CSV. Selecciónalo explícitamente.",
                "csv-delimiter-ambiguous");

        public DocumentoTabular LeerCsv(string rutaArchivo, char delimitador)
        {
            UltimoDelimitador = delimitador;
            return new DocumentoTabular(
                "alumnos.csv",
                [
                    new HojaTabular(
                        "alumnos",
                        [
                            CeldaTabular.DesdeTexto("No."),
                            CeldaTabular.DesdeTexto("Nombre completo"),
                        ],
                        [
                            new FilaTabular(
                                2,
                                [
                                    CeldaTabular.DesdeTexto("1"),
                                    CeldaTabular.DesdeTexto("Ana López"),
                                ]),
                        ]),
                ]);
        }
    }

    private sealed class AlmacenamientoContextoDoble : IAlmacenamientoContextoGrupo
    {
        private ContextoGrupo _contexto;

        public AlmacenamientoContextoDoble(ContextoGrupo contexto)
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
        private readonly Grupo _grupo;

        public AlmacenamientoGruposDoble(Grupo grupo)
        {
            _grupo = grupo;
        }

        public Grupo? Cargar(GrupoId grupoId) =>
            grupoId == _grupo.Id ? _grupo : null;

        public bool Existe(GrupoId grupoId) => grupoId == _grupo.Id;

        public void Guardar(Grupo grupo)
        {
        }

        public IReadOnlyList<Grupo> ListarTodos() => [_grupo];
    }
}