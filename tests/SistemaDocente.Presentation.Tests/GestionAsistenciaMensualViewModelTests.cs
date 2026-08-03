using SistemaDocente.Application;
using SistemaDocente.Core;
using SistemaDocente.Presentation;

namespace SistemaDocente.Presentation.Tests;

public sealed class GestionAsistenciaMensualViewModelTests
{
    private static readonly GrupoId GrupoId = Grupo.Crear("Primero A").Id;
    private static readonly DateOnly Hoy = new(2026, 8, 3);

    [Fact]
    public void InicializaMesActualCon21ColumnasLectivasY40FilasSinGuardar()
    {
        var gestion = new GestionDoble(CrearMes(2026, 8, 40));
        var viewModel = CrearViewModel(gestion);

        viewModel.Inicializar(GrupoId);

        Assert.Equal(21, viewModel.Dias.Count);
        Assert.Equal(40, viewModel.FilasVisibles.Count);
        Assert.Equal(new DateOnly(2026, 8, 3), viewModel.Dias[0].Fecha);
        Assert.Equal("L", viewModel.Dias[0].AbreviaturaDiaSemana);
        Assert.DoesNotContain(viewModel.Dias, x => x.Fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
        Assert.Equal(0, gestion.GuardadosMes);
    }

    [Theory]
    [InlineData(2025, 2, 20)]
    [InlineData(2024, 2, 21)]
    [InlineData(2026, 4, 22)]
    [InlineData(2026, 8, 21)]
    public void SelectorMensualAdmiteTodasLasLongitudes(int anio, int mes, int dias)
    {
        var gestion = new GestionDoble(CrearMes(anio, mes, 1));
        var viewModel = new GestionAsistenciaMensualViewModel(
            gestion,
            new RelojDoble(new DateOnly(anio, mes, 1)),
            new DialogoDoble(),
            new MensajesDoble());

        viewModel.Inicializar(GrupoId);

        Assert.Equal(dias, viewModel.Dias.Count);
    }

    [Fact]
    public void AtajoDeEstadoMarcaSoloFechaSeleccionadaYGuardarDiaConfirma()
    {
        var gestion = new GestionDoble(CrearMes(2026, 8, 2));
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);
        var fila = viewModel.FilasVisibles[0];
        var lunes = new DateOnly(2026, 8, 3);

        Assert.True(viewModel.AsignarEstado(fila, lunes, EstadoAsistencia.Falta));
        Assert.Contains(lunes, viewModel.FechasModificadas);
        Assert.Equal("F", fila.Celdas[0].Texto);

        viewModel.GuardarDiaCommand.Execute(null);

        Assert.Equal(1, gestion.GuardadosDia);
        Assert.DoesNotContain(lunes, viewModel.FechasModificadas);
        Assert.Equal(lunes, gestion.UltimoDiaGuardado);
    }

    [Fact]
    public void GuardarDiaSeHabilitaParaDiaNuevoAunqueConserveTodosPresentes()
    {
        var viewModel = CrearViewModel(new GestionDoble(CrearMes(2026, 8, 2)));
        viewModel.Inicializar(GrupoId);

        Assert.True(viewModel.GuardarDiaCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarDiaSeDeshabilitaParaDiaPersistidoSinCambios()
    {
        var lunes = new DateOnly(2026, 8, 3);
        var viewModel = CrearViewModel(new GestionDoble(CrearMes(2026, 8, 2, lunes)));
        viewModel.Inicializar(GrupoId);

        Assert.False(viewModel.GuardarDiaCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarDiaSeHabilitaParaDiaPersistidoModificado()
    {
        var lunes = new DateOnly(2026, 8, 3);
        var viewModel = CrearViewModel(new GestionDoble(CrearMes(2026, 8, 2, lunes)));
        viewModel.Inicializar(GrupoId);

        viewModel.AsignarEstado(viewModel.FilasVisibles[0], lunes, EstadoAsistencia.Falta);

        Assert.True(viewModel.GuardarDiaCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarDiaSeDeshabilitaMientrasEstaOcupado()
    {
        var gestion = new GestionDoble(CrearMes(2026, 8, 2));
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);
        bool? puedeGuardarDuranteOperacion = null;
        gestion.AlGuardarDia = () => puedeGuardarDuranteOperacion = viewModel.GuardarDiaCommand.CanExecute(null);

        viewModel.GuardarDiaCommand.Execute(null);

        Assert.False(puedeGuardarDuranteOperacion);
    }

    [Fact]
    public void GuardarDiaNuevoActualizaMesSinRecargarlo()
    {
        var lunes = new DateOnly(2026, 8, 3);
        var gestion = new GestionDoble(CrearMes(2026, 8, 2));
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);

        viewModel.GuardarDiaCommand.Execute(null);

        Assert.True(viewModel.Dias.Single(x => x.Fecha == lunes).ExisteRegistroPersistido);
        Assert.DoesNotContain(lunes, viewModel.FechasModificadas);
        Assert.False(viewModel.GuardarDiaCommand.CanExecute(null));
        Assert.Equal(1, gestion.CargasMes);
    }

    [Fact]
    public void GuardarDiaModificadoActualizaSnapshotYConservaOtroBorrador()
    {
        var lunes = new DateOnly(2026, 8, 3);
        var martes = new DateOnly(2026, 8, 4);
        var gestion = new GestionDoble(CrearMes(2026, 8, 2, lunes, martes));
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);
        var fila = viewModel.FilasVisibles[0];
        viewModel.AsignarEstado(fila, lunes, EstadoAsistencia.Falta);
        viewModel.AsignarEstado(fila, martes, EstadoAsistencia.Retardo);
        viewModel.SeleccionarCelda(fila, lunes);

        viewModel.GuardarDiaCommand.Execute(null);
        viewModel.AsignarEstado(fila, lunes, EstadoAsistencia.Justificada);
        viewModel.DescartarDiaCommand.Execute(null);

        Assert.Equal(EstadoAsistencia.Falta, fila.Celdas.Single(x => x.Fecha == lunes).Estado);
        Assert.DoesNotContain(lunes, viewModel.FechasModificadas);
        Assert.Contains(martes, viewModel.FechasModificadas);
    }

    [Fact]
    public void FalloAlGuardarDiaConservaPersistenciaEdicionYPendiente()
    {
        var lunes = new DateOnly(2026, 8, 3);
        var gestion = new GestionDoble(CrearMes(2026, 8, 2)) { FallarDia = true };
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);
        var fila = viewModel.FilasVisibles[0];
        viewModel.AsignarEstado(fila, lunes, EstadoAsistencia.Falta);

        viewModel.GuardarDiaCommand.Execute(null);

        Assert.False(viewModel.Dias.Single(x => x.Fecha == lunes).ExisteRegistroPersistido);
        Assert.Contains(lunes, viewModel.FechasModificadas);
        Assert.Equal(EstadoAsistencia.Falta, fila.Celdas.Single(x => x.Fecha == lunes).Estado);
        Assert.True(viewModel.GuardarDiaCommand.CanExecute(null));
    }

    [Fact]
    public void CambiarColumnaRecalculaGuardarDiaSegunLaNuevaFecha()
    {
        var lunes = new DateOnly(2026, 8, 3);
        var martes = new DateOnly(2026, 8, 4);
        var viewModel = CrearViewModel(new GestionDoble(CrearMes(2026, 8, 2, lunes)));
        viewModel.Inicializar(GrupoId);

        Assert.False(viewModel.GuardarDiaCommand.CanExecute(null));

        viewModel.SeleccionarCelda(viewModel.FilasVisibles[0], martes);

        Assert.True(viewModel.GuardarDiaCommand.CanExecute(null));
    }

    [Fact]
    public void GuardarMesConFalloIntermedioConservaFechaPendiente()
    {
        var gestion = new GestionDoble(CrearMes(2026, 8, 2)) { FallarMes = true };
        var mensajes = new MensajesDoble();
        var viewModel = CrearViewModel(gestion, mensajes);
        viewModel.Inicializar(GrupoId);
        var fila = viewModel.FilasVisibles[0];
        var lunes = new DateOnly(2026, 8, 3);
        var martes = new DateOnly(2026, 8, 4);
        viewModel.AsignarEstado(fila, lunes, EstadoAsistencia.Falta);
        viewModel.AsignarEstado(fila, martes, EstadoAsistencia.Retardo);

        viewModel.GuardarMesCommand.Execute(null);

        Assert.DoesNotContain(lunes, viewModel.FechasModificadas);
        Assert.Contains(martes, viewModel.FechasModificadas);
        Assert.Single(mensajes.Errores);
    }

    [Fact]
    public void BusquedaYFiltroNoAlteranOrdenBase()
    {
        var gestion = new GestionDoble(CrearMes(2026, 2, 3));
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);

        viewModel.Busqueda = "02";
        Assert.Single(viewModel.FilasVisibles);
        Assert.Equal(2, viewModel.FilasVisibles[0].NumeroLista);

        viewModel.Busqueda = string.Empty;
        viewModel.Filtro = FiltroAsistenciaMensual.SoloActivos;
        Assert.Equal([1, 2, 3], viewModel.FilasVisibles.Select(x => x.NumeroLista));
    }

    [Fact]
    public void NavegacionLectivaSaltaDeViernesALunes()
    {
        var gestion = new GestionDoble(CrearMes(2026, 8, 1));
        var viewModel = CrearViewModel(gestion);
        viewModel.Inicializar(GrupoId);

        var viernes = new DateOnly(2026, 8, 7);

        Assert.True(viewModel.Dias.Single(x => x.Fecha == viernes).EsCierreSemana);
        Assert.Equal(new DateOnly(2026, 8, 10), viewModel.ObtenerFechaLectivaSiguiente(viernes));
        Assert.Null(viewModel.ObtenerFechaLectivaSiguiente(new DateOnly(2026, 8, 31)));
    }

    private static GestionAsistenciaMensualViewModel CrearViewModel(
        GestionDoble gestion,
        MensajesDoble? mensajes = null) =>
        new(gestion, new RelojDoble(Hoy), new DialogoDoble(), mensajes ?? new MensajesDoble());

    private static AsistenciaMesDetalle CrearMes(
        int anio,
        int mes,
        int estudiantes,
        params DateOnly[] fechasPersistidas)
    {
        var dias = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
            .Select(numero =>
            {
                var fecha = new DateOnly(anio, mes, numero);
                var abreviatura = fecha.DayOfWeek switch
                {
                    DayOfWeek.Monday => "L",
                    DayOfWeek.Tuesday => "M",
                    DayOfWeek.Wednesday => "M",
                    DayOfWeek.Thursday => "J",
                    _ => "V",
                };
                return (fecha, numero, abreviatura);
            })
            .Where(x => x.fecha.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday)
            .ToArray();
        var columnas = dias.Select((x, indice) => new AsistenciaDiaColumnaDetalle(
            x.fecha,
            x.numero,
            x.abreviatura,
            true,
            fechasPersistidas.Contains(x.fecha),
            x.fecha.DayOfWeek == DayOfWeek.Friday && indice < dias.Length - 1)).ToArray();
        var filas = Enumerable.Range(1, estudiantes).Select(numero => new AsistenciaEstudianteMesDetalle(
            EstudianteId.DesdeGuid(Guid.Parse($"{numero:X8}-0000-0000-0000-000000000000")),
            numero,
            $"Estudiante {numero:00}",
            true,
            columnas.Select(d => new AsistenciaCeldaDetalle(
                d.Fecha,
                EstadoAsistencia.Presente,
                fechasPersistidas.Contains(d.Fecha)
                    ? TipoCeldaAsistencia.Confirmada
                    : TipoCeldaAsistencia.Borrador)).ToArray(),
            0, 0, 0, 0, null)).ToArray();
        return new(GrupoId, anio, mes, columnas, filas);
    }

    private sealed class GestionDoble(AsistenciaMesDetalle mes) : IGestionAsistenciaPresentacion
    {
        public Action? AlGuardarDia { get; set; }
        public bool FallarDia { get; init; }
        public bool FallarMes { get; init; }
        public int CargasMes { get; private set; }
        public int GuardadosDia { get; private set; }
        public int GuardadosMes { get; private set; }
        public DateOnly? UltimoDiaGuardado { get; private set; }
        public AsistenciaDiaDetalle Preparar(GrupoId grupoId, DateOnly fecha) => throw new NotSupportedException();
        public AsistenciaMesDetalle CargarMes(GrupoId grupoId, int anio, int numeroMes)
        {
            CargasMes++;
            return mes;
        }
        public AsistenciaDiaDetalle Guardar(GrupoId grupoId, DateOnly fecha, IReadOnlyCollection<EntradaEstadoAsistencia> entradas)
        {
            GuardadosDia++;
            UltimoDiaGuardado = fecha;
            AlGuardarDia?.Invoke();
            if (FallarDia)
            {
                throw new ErrorPersistenciaAplicacionException("fallo", new InvalidOperationException());
            }
            return new(grupoId, fecha, true, entradas.Select((x, i) => new AsistenciaEstudianteDetalle(x.EstudianteId, $"E{i}", i + 1, x.Estado, true)).ToArray());
        }
        public ResultadoGuardadoMes GuardarMes(GrupoId grupoId, IReadOnlyCollection<EntradaDiaAsistencia> dias)
        {
            GuardadosMes++;
            var fechas = dias.Select(x => x.Fecha).OrderBy(x => x).ToArray();
            if (FallarMes)
            {
                throw new GuardadoMesInterrumpidoException(
                    fechas[1], [fechas[0]], fechas[1..], new ErrorPersistenciaAplicacionException("fallo", new InvalidOperationException()));
            }
            return new(fechas, []);
        }
    }

    private sealed record RelojDoble(DateOnly Hoy) : IRelojLocal;
    private sealed class DialogoDoble : IDialogoCambiosPendientes
    {
        public DecisionCambiosPendientes ConfirmarCambiosPendientes() => DecisionCambiosPendientes.Cancelar;
    }
    private sealed class MensajesDoble : IServicioMensajes
    {
        public List<string> Errores { get; } = [];
        public void MostrarError(string mensaje) => Errores.Add(mensaje);
    }
}