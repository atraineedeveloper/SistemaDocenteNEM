using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void CrearDesdeResumenConservaGrupoHastaCancelar()
    {
        var contexto = new ContextoPrueba();
        contexto.IrAResumen();
        var grupoAnterior = contexto.Grupo.GrupoIdActual;

        contexto.Shell.CrearGrupoDesdeInicioCommand.Execute(null);

        Assert.True(contexto.Shell.MostrarCreacionGrupo);
        Assert.Equal(grupoAnterior, contexto.Grupo.GrupoIdActual);
        Assert.False(contexto.Shell.MostrarNavegacion);
        Assert.Equal(1, contexto.Shell.PasoCreacionGrupo);

        contexto.Shell.CancelarCreacionGrupoCommand.Execute(null);

        Assert.False(contexto.Shell.MostrarCreacionGrupo);
        Assert.Equal(grupoAnterior, contexto.Grupo.GrupoIdActual);
        Assert.True(contexto.Shell.MostrarGrupo);
        Assert.False(contexto.Shell.MostrarInicio);
    }

    [Fact]
    public void CancelarCreacionDesdeMisGruposRegresaAInicio()
    {
        var contexto = new ContextoPrueba();
        Assert.True(contexto.Shell.MostrarInicio);

        contexto.Shell.CrearGrupoDesdeInicioCommand.Execute(null);
        contexto.Grupo.NombreNuevoGrupo = "Borrador";
        contexto.Shell.CancelarCreacionGrupoCommand.Execute(null);

        Assert.True(contexto.Shell.MostrarInicio);
        Assert.False(contexto.Shell.MostrarCreacionGrupo);
        Assert.Equal(string.Empty, contexto.Grupo.NombreNuevoGrupo);
        Assert.Equal(1, contexto.Shell.PasoCreacionGrupo);
    }

    [Fact]
    public void PrimerPasoNoAvanzaSinNombre()
    {
        var contexto = new ContextoPrueba();
        contexto.IrAResumen();
        contexto.Shell.CrearGrupoDesdeInicioCommand.Execute(null);

        contexto.Shell.SiguienteCreacionGrupoCommand.Execute(null);

        Assert.Equal(1, contexto.Shell.PasoCreacionGrupo);
        Assert.True(contexto.Shell.MostrarPasoGrupo);
        Assert.Contains("nombre", contexto.Shell.MensajeCreacionGrupo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VolverConservaBorradorYLosPasosOpcionalesPuedenOmitirse()
    {
        var contexto = new ContextoPrueba();
        contexto.IrAResumen();
        contexto.Shell.CrearGrupoDesdeInicioCommand.Execute(null);
        contexto.Grupo.NombreNuevoGrupo = "Quinto B";

        contexto.Shell.SiguienteCreacionGrupoCommand.Execute(null);
        Assert.Equal(2, contexto.Shell.PasoCreacionGrupo);
        contexto.Shell.VolverCreacionGrupoCommand.Execute(null);

        Assert.Equal(1, contexto.Shell.PasoCreacionGrupo);
        Assert.Equal("Quinto B", contexto.Grupo.NombreNuevoGrupo);

        contexto.Shell.SiguienteCreacionGrupoCommand.Execute(null);
        contexto.Shell.OmitirPasoCreacionGrupoCommand.Execute(null);
        contexto.Shell.OmitirPasoCreacionGrupoCommand.Execute(null);
        contexto.Shell.OmitirPasoCreacionGrupoCommand.Execute(null);

        Assert.Equal(5, contexto.Shell.PasoCreacionGrupo);
        Assert.True(contexto.Shell.MostrarPasoConfirmar);
    }

    [Fact]
    public void CrearGrupoSoloConNombreCierraWizardYAbreResumenNuevo()
    {
        var contexto = new ContextoPrueba();
        contexto.IrAResumen();
        var grupoAnterior = contexto.Grupo.GrupoIdActual;
        contexto.Shell.CrearGrupoDesdeInicioCommand.Execute(null);
        contexto.Grupo.NombreNuevoGrupo = "Nuevo grupo";
        contexto.AvanzarHastaConfirmacionOmitiendoOpcionales();

        contexto.Shell.ConfirmarCreacionGrupoCommand.Execute(null);

        Assert.NotEqual(grupoAnterior, contexto.Grupo.GrupoIdActual);
        Assert.False(contexto.Shell.MostrarCreacionGrupo);
        Assert.False(contexto.Shell.MostrarInicio);
        Assert.True(contexto.Shell.MostrarGrupo);
        Assert.Contains("Resumen", contexto.Shell.TituloVentana, StringComparison.Ordinal);
    }

    private sealed class ContextoPrueba
    {
        internal ContextoPrueba()
        {
            Estado.Guardar(GestionGrupo.GrupoActual.GrupoId);
            Grupo = new GestionGrupoViewModel(GestionGrupo, Estado, Mensajes, Confirmacion);
            Grupo.Inicializar();

            var asistencia = new GestionAsistenciaViewModel(
                GestionAsistencia,
                Reloj,
                Dialogo,
                Mensajes);
            var asistenciaMensual = new GestionAsistenciaMensualViewModel(
                GestionAsistencia,
                Reloj,
                Dialogo,
                Mensajes);

            Shell = new MainWindowViewModel(Grupo, asistencia, asistenciaMensual);
        }

        internal GestionGrupoDoble GestionGrupo { get; } = new();
        internal GestionAsistenciaDoble GestionAsistencia { get; } = new();
        internal EstadoDoble Estado { get; } = new();
        internal MensajesDoble Mensajes { get; } = new();
        internal ConfirmacionDoble Confirmacion { get; } = new();
        internal DialogoDoble Dialogo { get; } = new();
        internal RelojDoble Reloj { get; } = new(new DateOnly(2026, 8, 11));
        internal GestionGrupoViewModel Grupo { get; }
        internal MainWindowViewModel Shell { get; }

        internal void IrAResumen()
        {
            Assert.NotNull(Grupo.GrupoIdActual);
            Assert.True(Shell.CambiarGrupo(Grupo.GrupoIdActual!.Value));
            Assert.True(Shell.MostrarGrupo);
        }

        internal void AvanzarHastaConfirmacionOmitiendoOpcionales()
        {
            Shell.SiguienteCreacionGrupoCommand.Execute(null);
            Shell.OmitirPasoCreacionGrupoCommand.Execute(null);
            Shell.OmitirPasoCreacionGrupoCommand.Execute(null);
            Shell.OmitirPasoCreacionGrupoCommand.Execute(null);
            Assert.True(Shell.MostrarPasoConfirmar);
        }
    }

    private sealed class EstadoDoble : IAlmacenamientoEstadoAplicacion
    {
        private GrupoId? _guardado;

        public ResultadoLecturaReferencia Cargar() => _guardado is { } id
            ? new(EstadoLecturaReferencia.Valida, id)
            : new(EstadoLecturaReferencia.Ausente);

        public void Guardar(GrupoId grupoId) => _guardado = grupoId;

        public void Olvidar() => _guardado = null;
    }

    private sealed class MensajesDoble : IServicioMensajes
    {
        public void MostrarError(string mensaje)
        {
        }
    }

    private sealed class ConfirmacionDoble : IServicioConfirmacion
    {
        public bool ConfirmarDesactivacion(string nombreEstudiante) => true;
    }

    private sealed class DialogoDoble : IDialogoCambiosPendientes
    {
        public DecisionCambiosPendientes ConfirmarCambiosPendientes() => DecisionCambiosPendientes.Descartar;
    }

    private sealed record RelojDoble(DateOnly Hoy) : IRelojLocal;

    private sealed class GestionAsistenciaDoble : IGestionAsistenciaPresentacion
    {
        public AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha) =>
            new(grupoId, fecha, false, []);

        public AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int mes) =>
            throw new NotSupportedException();

        public AsistenciaDiaDetalle Guardar(
            GrupoId grupoId,
            DateOnly fecha,
            IReadOnlyCollection<EntradaEstadoAsistencia> entradas) =>
            throw new NotSupportedException();

        public ResultadoGuardadoMes GuardarMes(
            GrupoId grupoId,
            IReadOnlyCollection<EntradaDiaAsistencia> dias) =>
            throw new NotSupportedException();
    }

    private sealed class GestionGrupoDoble : IGestionGrupoPresentacion
    {
        private readonly List<GrupoDetalle> _grupos = [];

        internal GestionGrupoDoble()
        {
            GrupoActual = CrearGrupoInterno("Grupo inicial");
        }

        internal GrupoDetalle GrupoActual { get; private set; }

        public GrupoDetalle CrearGrupo(string nombreVisible)
        {
            GrupoActual = CrearGrupoInterno(nombreVisible);
            return GrupoActual;
        }

        public GrupoDetalle CargarGrupo(GrupoId grupoId)
        {
            GrupoActual = _grupos.Single(x => x.GrupoId == grupoId);
            return GrupoActual;
        }

        public IReadOnlyList<GrupoDetalle> ListarGrupos() => _grupos.ToArray();

        public GrupoDetalle CambiarNombreGrupo(GrupoId grupoId, string nombreVisible)
        {
            var indice = _grupos.FindIndex(x => x.GrupoId == grupoId);
            var actualizado = _grupos[indice] with { NombreVisible = nombreVisible };
            _grupos[indice] = actualizado;
            GrupoActual = actualizado;
            return actualizado;
        }

        public EstudianteDetalle AgregarEstudiante(
            GrupoId grupoId,
            string nombreVisible,
            int numeroLista,
            string primerApellido = "",
            string segundoApellido = "",
            string nombres = "",
            DateOnly? fechaNacimiento = null,
            GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
            DateOnly? fechaIngreso = null,
            string observaciones = "") =>
            throw new NotSupportedException();

        public EstudianteDetalle RenombrarEstudiante(GrupoId grupoId, EstudianteId id, string nombre) =>
            throw new NotSupportedException();

        public EstudianteDetalle CambiarNumeroLista(GrupoId grupoId, EstudianteId id, int numero) =>
            throw new NotSupportedException();

        public EstudianteDetalle EditarEstudiante(
            GrupoId grupoId,
            EstudianteId id,
            string nombre,
            int numero,
            string primerApellido = "",
            string segundoApellido = "",
            string nombres = "",
            DateOnly? fechaNacimiento = null,
            GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
            DateOnly? fechaIngreso = null,
            string observaciones = "") =>
            throw new NotSupportedException();

        public EstudianteDetalle DesactivarEstudiante(GrupoId grupoId, EstudianteId id) =>
            throw new NotSupportedException();

        public EstudianteDetalle ReactivarEstudiante(GrupoId grupoId, EstudianteId id) =>
            throw new NotSupportedException();

        public IReadOnlyList<EstudianteDetalle> ObtenerTodosLosEstudiantes(GrupoId grupoId) => [];

        private GrupoDetalle CrearGrupoInterno(string nombre)
        {
            var grupo = new GrupoDetalle(GrupoId.DesdeGuid(Guid.NewGuid()), nombre, []);
            _grupos.Add(grupo);
            return grupo;
        }
    }
}