using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.Presentation.Tests;

public sealed class GestionAsistenciaViewModelTests
{
    private static readonly GrupoId GrupoId = Grupo.Crear("Primero A").Id;
    private static readonly DateOnly Hoy = new(2026, 8, 3);

    [Fact]
    public void InicializarUsaFechaLocalYDiaNuevoPermanecePendiente()
    {
        var gestion = new GestionDoble { Preparado = CrearDetalle(false, 35) };
        var viewModel = CrearViewModel(gestion);

        viewModel.Inicializar(GrupoId);

        Assert.Equal(Hoy.ToDateTime(TimeOnly.MinValue), viewModel.FechaSeleccionada);
        Assert.False(viewModel.EsPersistido);
        Assert.True(viewModel.TieneCambios);
        Assert.Equal("Sin guardar", viewModel.EstadoGuardado);
        Assert.True(viewModel.GuardarCommand.CanExecute(null));
        Assert.Equal(35, viewModel.Total);
        Assert.Equal(35, viewModel.Presentes);
        Assert.Equal(0, gestion.Guardados);
    }

    [Fact]
    public void DiaGuardadoSinCambiosDeshabilitaGuardarEIncluyeInactivo()
    {
        var detalle = CrearDetalle(true, 2, inactivoUltimo: true);
        var gestion = new GestionDoble { Preparado = detalle };
        var viewModel = CrearViewModel(gestion);

        viewModel.Inicializar(GrupoId);

        Assert.True(viewModel.EsPersistido);
        Assert.False(viewModel.TieneCambios);
        Assert.False(viewModel.GuardarCommand.CanExecute(null));
        Assert.Equal("Inactivo actualmente", viewModel.Estudiantes[1].SituacionActual);
        Assert.Equal(detalle.Estudiantes.Select(x => x.NombreVisible), viewModel.Estudiantes.Select(x => x.Nombre));
    }

    [Fact]
    public void EditarYMarcarTodosActualizaConteosDeTodasLasFilas()
    {
        var gestion = new GestionDoble { Preparado = CrearDetalle(true, 3) };
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);

        viewModel.Estudiantes[0].Estado = EstadoAsistencia.Justificada;

        Assert.Equal(2, viewModel.Presentes);
        Assert.Equal(1, viewModel.Justificadas);
        Assert.True(viewModel.TieneCambios);
        Assert.Contains(
            AsistenciaEstudianteVisual.Opciones,
            x => x.Estado == EstadoAsistencia.Justificada && x.Texto == "Falta justificada");

        viewModel.MarcarTodosPresentesCommand.Execute(null);

        Assert.Equal(3, viewModel.Presentes);
        Assert.Equal(0, viewModel.Justificadas);
    }

    [Fact]
    public void GuardadoExitosoUsaUnaOperacionYActualizaConfirmado()
    {
        var gestion = new GestionDoble { Preparado = CrearDetalle(false, 2) };
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);
        viewModel.Estudiantes[0].Estado = EstadoAsistencia.Falta;

        viewModel.GuardarCommand.Execute(null);

        Assert.Equal(1, gestion.Guardados);
        Assert.True(viewModel.EsPersistido);
        Assert.False(viewModel.TieneCambios);
        Assert.False(viewModel.GuardarCommand.CanExecute(null));
        Assert.Equal(EstadoAsistencia.Falta, gestion.UltimasEntradas![0].Estado);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ErrorDeDominioOPersistenciaConservaEdicionYSnapshot(bool dominio)
    {
        var gestion = new GestionDoble
        {
            Preparado = CrearDetalle(true, 1),
            ErrorAlGuardar = dominio
                ? new DomainConflictException("conflicto")
                : new ErrorPersistenciaAplicacionException("fallo", new IOException()),
        };
        var mensajes = new MensajesDoble();
        var viewModel = CrearViewModel(gestion, mensajes: mensajes);
        viewModel.Inicializar(GrupoId);
        viewModel.Estudiantes[0].Estado = EstadoAsistencia.Falta;

        viewModel.GuardarCommand.Execute(null);

        Assert.Equal(EstadoAsistencia.Falta, viewModel.Estudiantes[0].Estado);
        Assert.True(viewModel.TieneCambios);
        Assert.Equal("Cambios sin guardar", viewModel.EstadoGuardado);
        Assert.NotEmpty(mensajes.Errores);
    }

    [Fact]
    public void FechaInvalidaConservaFechaYPresentaMensaje()
    {
        var mensajes = new MensajesDoble();
        var viewModel = CrearViewModel(
            new GestionDoble { Preparado = CrearDetalle(true, 1) },
            mensajes: mensajes);
        viewModel.Inicializar(GrupoId);

        viewModel.FechaSeleccionada = null;

        Assert.Equal(Hoy.ToDateTime(TimeOnly.MinValue), viewModel.FechaSeleccionada);
        Assert.NotEmpty(mensajes.Errores);
    }

    [Theory]
    [InlineData(DecisionCambiosPendientes.Descartar, true)]
    [InlineData(DecisionCambiosPendientes.Cancelar, false)]
    public void CambiarFechaRespetaDescartarOCancelar(
        DecisionCambiosPendientes decision,
        bool cambia)
    {
        var dialogo = new DialogoDoble(decision);
        var gestion = new GestionDoble { Preparado = CrearDetalle(false, 1) };
        var viewModel = CrearViewModel(gestion, dialogo);
        viewModel.Inicializar(GrupoId);

        viewModel.FechaSeleccionada = Hoy.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var esperada = cambia ? Hoy.AddDays(1) : Hoy;
        Assert.Equal(esperada.ToDateTime(TimeOnly.MinValue), viewModel.FechaSeleccionada);
        Assert.Equal(0, gestion.Guardados);
    }

    [Fact]
    public void GuardarAntesDeCambiarFechaContinuaSoloTrasExito()
    {
        var dialogo = new DialogoDoble(
            DecisionCambiosPendientes.Guardar,
            DecisionCambiosPendientes.Guardar);
        var gestion = new GestionDoble { Preparado = CrearDetalle(false, 1) };
        var viewModel = CrearViewModel(gestion, dialogo);
        viewModel.Inicializar(GrupoId);

        viewModel.FechaSeleccionada = Hoy.AddDays(1).ToDateTime(TimeOnly.MinValue);

        Assert.Equal(1, gestion.Guardados);
        Assert.Equal(Hoy.AddDays(1).ToDateTime(TimeOnly.MinValue), viewModel.FechaSeleccionada);

        gestion.ErrorAlGuardar = new ErrorPersistenciaAplicacionException("fallo", new IOException());
        viewModel.Estudiantes[0].Estado = EstadoAsistencia.Falta;
        viewModel.FechaSeleccionada = Hoy.AddDays(2).ToDateTime(TimeOnly.MinValue);
        Assert.Equal(Hoy.AddDays(1).ToDateTime(TimeOnly.MinValue), viewModel.FechaSeleccionada);
    }

    [Fact]
    public void NavegarYCerrarReutilizanConfirmacion()
    {
        var dialogo = new DialogoDoble(
            DecisionCambiosPendientes.Cancelar,
            DecisionCambiosPendientes.Descartar);
        var viewModel = CrearViewModel(
            new GestionDoble { Preparado = CrearDetalle(false, 1) },
            dialogo);
        viewModel.Inicializar(GrupoId);

        Assert.False(viewModel.SolicitarNavegarAGrupo());
        Assert.True(viewModel.SolicitarCerrar());
        Assert.Equal(2, dialogo.Llamadas);
    }

    [Fact]
    public void PresentationNoReferenciaDataSqliteNiWpf()
    {
        var referencias = typeof(GestionAsistenciaViewModel).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToArray();

        Assert.DoesNotContain("SistemaDocente.Data", referencias);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", referencias);
        Assert.DoesNotContain("PresentationFramework", referencias);
    }

    private static GestionAsistenciaViewModel CrearViewModel(
        GestionDoble gestion,
        DialogoDoble? dialogo = null,
        MensajesDoble? mensajes = null) =>
        new(
            gestion,
            new RelojDoble(Hoy),
            dialogo ?? new DialogoDoble(DecisionCambiosPendientes.Cancelar),
            mensajes ?? new MensajesDoble());

    private static AsistenciaDiaDetalle CrearDetalle(
        bool persistido,
        int cantidad,
        bool inactivoUltimo = false)
    {
        var filas = Enumerable.Range(1, cantidad)
            .Select(indice => new AsistenciaEstudianteDetalle(
                EstudianteId.DesdeGuid(Guid.Parse($"{indice:X8}-0000-0000-0000-000000000000")),
                $"Estudiante {indice:00}",
                indice,
                EstadoAsistencia.Presente,
                !inactivoUltimo || indice != cantidad))
            .ToArray();
        return new(GrupoId, Hoy, persistido, filas);
    }

    private sealed class GestionDoble : IGestionAsistenciaPresentacion
    {
        internal required AsistenciaDiaDetalle Preparado { get; set; }

        internal int Guardados { get; private set; }

        internal EntradaEstadoAsistencia[]? UltimasEntradas { get; private set; }

        internal Exception? ErrorAlGuardar { get; set; }

        public AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha) =>
            Preparado with { GrupoId = grupoId, Fecha = fecha };

        public AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int mes) =>
            throw new NotSupportedException();

        public AsistenciaDiaDetalle Guardar(
            GrupoId grupoId,
            DateOnly fecha,
            IReadOnlyCollection<EntradaEstadoAsistencia> entradas)
        {
            Guardados++;
            UltimasEntradas = entradas.ToArray();
            if (ErrorAlGuardar is not null)
            {
                throw ErrorAlGuardar;
            }

            Preparado = new AsistenciaDiaDetalle(
                grupoId,
                fecha,
                true,
                Preparado.Estudiantes.Select(x => x with
                {
                    Estado = UltimasEntradas.Single(e => e.EstudianteId == x.EstudianteId).Estado,
                }).ToArray());
            return Preparado;
        }

        public ResultadoGuardadoMes GuardarMes(
            GrupoId grupoId,
            IReadOnlyCollection<EntradaDiaAsistencia> dias) =>
            throw new NotSupportedException();
    }

    private sealed record RelojDoble(DateOnly Hoy) : IRelojLocal;

    private sealed class DialogoDoble(params DecisionCambiosPendientes[] decisiones)
        : IDialogoCambiosPendientes
    {
        private readonly Queue<DecisionCambiosPendientes> _decisiones = new(decisiones);

        internal int Llamadas { get; private set; }

        public DecisionCambiosPendientes ConfirmarCambiosPendientes()
        {
            Llamadas++;
            return _decisiones.Dequeue();
        }
    }

    private sealed class MensajesDoble : IServicioMensajes
    {
        internal List<string> Errores { get; } = [];

        public void MostrarError(string mensaje) => Errores.Add(mensaje);
    }
}