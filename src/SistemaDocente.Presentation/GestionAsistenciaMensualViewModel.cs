using System.Collections.ObjectModel;
using System.Globalization;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class GestionAsistenciaMensualViewModel : ViewModelBase
{
    private readonly IGestionAsistenciaPresentacion _gestion;
    private readonly IRelojLocal _reloj;
    private readonly IDialogoCambiosPendientes _dialogo;
    private readonly IServicioMensajes _mensajes;
    private readonly HashSet<DateOnly> _fechasModificadas = [];
    private readonly Dictionary<(EstudianteId, DateOnly), EstadoAsistencia?> _confirmados = [];
    private GrupoId? _grupoId;
    private int _anio;
    private int _mes;
    private DateOnly? _fechaSeleccionada;
    private string _busqueda = string.Empty;
    private FiltroAsistenciaMensual _filtro;
    private bool _estaOcupado;
    private AsistenciaMesDetalle? _mesConfirmado;
    private IReadOnlyList<AsistenciaDiaColumnaDetalle> _dias = [];
    private IReadOnlyList<AsistenciaEstudianteMesVisual> _filas = [];
    private IReadOnlyList<AsistenciaEstudianteMesVisual> _filasVisibles = [];

    public GestionAsistenciaMensualViewModel(
        IGestionAsistenciaPresentacion gestion,
        IRelojLocal reloj,
        IDialogoCambiosPendientes dialogo,
        IServicioMensajes mensajes)
    {
        _gestion = gestion;
        _reloj = reloj;
        _dialogo = dialogo;
        _mensajes = mensajes;
        Meses = new ReadOnlyCollection<MesVisual>(Enumerable.Range(1, 12)
            .Select(x => new MesVisual(x, CultureInfo.GetCultureInfo("es-MX").DateTimeFormat.GetMonthName(x)))
            .ToArray());
        MesAnteriorCommand = new RelayCommand(() => CambiarMes(-1), () => !EstaOcupado);
        MesSiguienteCommand = new RelayCommand(() => CambiarMes(1), () => !EstaOcupado);
        IrMesActualCommand = new RelayCommand(IrMesActual, () => !EstaOcupado);
        GuardarDiaCommand = new RelayCommand(() => GuardarDia(), PuedeGuardarDia);
        GuardarMesCommand = new RelayCommand(() => GuardarMes(), () => !EstaOcupado && _fechasModificadas.Count > 0);
        DescartarDiaCommand = new RelayCommand(DescartarDia, () => !EstaOcupado && FechaSeleccionada is { } f && _fechasModificadas.Contains(f));
        DescartarMesCommand = new RelayCommand(DescartarMes, () => !EstaOcupado && _fechasModificadas.Count > 0);
        MarcarDiaPresenteCommand = new RelayCommand(MarcarDiaPresente, () => !EstaOcupado && DiaSeleccionado?.EsLaborable == true);
    }

    public IReadOnlyList<MesVisual> Meses { get; }
    public IReadOnlyList<FiltroAsistenciaMensual> Filtros { get; } = Enum.GetValues<FiltroAsistenciaMensual>();
    public RelayCommand MesAnteriorCommand { get; }
    public RelayCommand MesSiguienteCommand { get; }
    public RelayCommand IrMesActualCommand { get; }
    public RelayCommand GuardarDiaCommand { get; }
    public RelayCommand GuardarMesCommand { get; }
    public RelayCommand DescartarDiaCommand { get; }
    public RelayCommand DescartarMesCommand { get; }
    public RelayCommand MarcarDiaPresenteCommand { get; }
    public int AnioSeleccionado { get => _anio; set { if (value is >= 1 and <= 9999 && value != _anio) IntentarCargar(value, _mes); } }
    public int MesSeleccionado { get => _mes; set { if (value is >= 1 and <= 12 && value != _mes) IntentarCargar(_anio, value); } }
    public IReadOnlyList<AsistenciaDiaColumnaDetalle> Dias { get => _dias; private set => SetProperty(ref _dias, value); }
    public IReadOnlyList<AsistenciaEstudianteMesVisual> FilasVisibles { get => _filasVisibles; private set => SetProperty(ref _filasVisibles, value); }
    public IReadOnlySet<DateOnly> FechasModificadas => _fechasModificadas;
    public DateOnly? FechaSeleccionada { get => _fechaSeleccionada; private set { if (SetProperty(ref _fechaSeleccionada, value)) NotificarEstado(); } }
    public string FechaSeleccionadaTexto => FechaSeleccionada?.ToDateTime(TimeOnly.MinValue).ToString("D", CultureInfo.GetCultureInfo("es-MX")) ?? "Selecciona un día";
    public string Busqueda { get => _busqueda; set { if (SetProperty(ref _busqueda, value)) AplicarFiltro(); } }
    public FiltroAsistenciaMensual Filtro { get => _filtro; set { if (SetProperty(ref _filtro, value)) AplicarFiltro(); } }
    public bool EstaOcupado { get => _estaOcupado; private set { if (SetProperty(ref _estaOcupado, value)) NotificarComandos(); } }
    public string EstadoMes => _fechasModificadas.Count > 0 ? "Cambios sin guardar" : Dias.Count(x => x.ExisteRegistroPersistido) switch { 0 => "Sin registros", var n when n < Dias.Count(x => x.EsLaborable) => "Mes parcialmente guardado", _ => "Guardado" };
    public bool IncluyeBorradores => _fechasModificadas.Count > 0;
    public int AlumnosVisibles => FilasVisibles.Count;
    public int DiasGuardados => Dias.Count(x => x.ExisteRegistroPersistido);
    public int Presentes => FilasVisibles.Sum(x => x.Presentes);
    public int Faltas => FilasVisibles.Sum(x => x.Faltas);
    public int Retardos => FilasVisibles.Sum(x => x.Retardos);
    public int Justificadas => FilasVisibles.Sum(x => x.Justificadas);
    private AsistenciaDiaColumnaDetalle? DiaSeleccionado => FechaSeleccionada is { } f ? Dias.SingleOrDefault(x => x.Fecha == f) : null;

    public void Inicializar(GrupoId grupoId)
    {
        _grupoId = grupoId;
        Cargar(_reloj.Hoy.Year, _reloj.Hoy.Month);
    }

    public void SeleccionarCelda(EstudianteId estudianteId, DateOnly fecha) => FechaSeleccionada = fecha;

    public void SeleccionarCelda(AsistenciaEstudianteMesVisual estudiante, DateOnly fecha)
    {
        ArgumentNullException.ThrowIfNull(estudiante);
        SeleccionarCelda(estudiante.Id, fecha);
    }

    public bool AsignarEstado(
        AsistenciaEstudianteMesVisual estudiante,
        DateOnly fecha,
        EstadoAsistencia estado)
    {
        ArgumentNullException.ThrowIfNull(estudiante);
        return AsignarEstado(estudiante.Id, fecha, estado);
    }

    public bool AsignarEstado(EstudianteId estudianteId, DateOnly fecha, EstadoAsistencia estado)
    {
        var fila = _filas.SingleOrDefault(x => x.Id == estudianteId);
        var celda = fila?.Celdas.SingleOrDefault(x => x.Fecha == fecha);
        if (celda?.EsEditable != true) return false;
        celda.Estado = estado;
        fila!.NotificarConteos();
        ActualizarModificacion(fecha);
        AplicarFiltro();
        NotificarEstado();
        return true;
    }

    public DateOnly? ObtenerFechaLectivaSiguiente(DateOnly fecha)
    {
        var indice = Array.FindIndex(Dias.ToArray(), x => x.Fecha == fecha);
        return indice >= 0 && indice + 1 < Dias.Count ? Dias[indice + 1].Fecha : null;
    }

    public bool SolicitarSalir() => ConfirmarPendientes();

    private void Cargar(int anio, int mes)
    {
        if (_grupoId is null) return;
        Ejecutar(() => Aplicar(_gestion.CargarMes(_grupoId.Value, anio, mes)));
    }

    private void Aplicar(AsistenciaMesDetalle detalle)
    {
        _mesConfirmado = detalle;
        _anio = detalle.Anio; _mes = detalle.Mes;
        OnPropertyChanged(nameof(AnioSeleccionado)); OnPropertyChanged(nameof(MesSeleccionado));
        Dias = detalle.Dias.ToArray();
        _confirmados.Clear();
        _filas = detalle.Estudiantes.Select(e => new AsistenciaEstudianteMesVisual(
            e.EstudianteId, e.NumeroLista, e.NombreVisible, e.EstaActivoActualmente,
            e.Estados.Select(c => { _confirmados[(e.EstudianteId, c.Fecha)] = c.Estado; return new AsistenciaCeldaVisual(c.Fecha, c.Estado, c.Tipo != TipoCeldaAsistencia.NoAplicable, c.Tipo == TipoCeldaAsistencia.Confirmada); }).ToArray(),
            e.PorcentajeConfirmado)).ToArray();
        _fechasModificadas.Clear();
        FechaSeleccionada = Dias.FirstOrDefault(x => x.EsLaborable)?.Fecha;
        AplicarFiltro(); NotificarEstado();
    }

    private void CambiarMes(int delta)
    {
        var fecha = new DateOnly(_anio, _mes, 1).AddMonths(delta);
        IntentarCargar(fecha.Year, fecha.Month);
    }

    private void IrMesActual() => IntentarCargar(_reloj.Hoy.Year, _reloj.Hoy.Month);
    private void IntentarCargar(int anio, int mes) { if (ConfirmarPendientes()) Cargar(anio, mes); }
    private bool ConfirmarPendientes() => _fechasModificadas.Count == 0 || _dialogo.ConfirmarCambiosPendientes() switch { DecisionCambiosPendientes.Guardar => GuardarMes(), DecisionCambiosPendientes.Descartar => true, _ => false };

    private bool GuardarDia()
    {
        if (_grupoId is null || FechaSeleccionada is not { } fecha || DiaSeleccionado?.EsLaborable != true) return false;
        var exito = false;
        Ejecutar(() => { _gestion.Guardar(_grupoId.Value, fecha, CrearEntradas(fecha)); ConfirmarFecha(fecha); exito = true; });
        return exito;
    }

    private bool GuardarMes()
    {
        if (_grupoId is null || _fechasModificadas.Count == 0) return true;
        var exito = false;
        Ejecutar(() =>
        {
            try
            {
                var dias = _fechasModificadas.OrderBy(x => x).Select(x => new EntradaDiaAsistencia(x, CrearEntradas(x))).ToArray();
                var resultado = _gestion.GuardarMes(_grupoId.Value, dias);
                foreach (var fecha in resultado.FechasGuardadas) ConfirmarFecha(fecha);
                exito = true;
            }
            catch (GuardadoMesInterrumpidoException exception)
            {
                foreach (var fecha in exception.FechasGuardadas) ConfirmarFecha(fecha);
                _mensajes.MostrarError($"No fue posible guardar el {exception.FechaFallida:dd/MM/yyyy}. Los días anteriores indicados sí quedaron guardados.");
            }
        });
        return exito;
    }

    private EntradaEstadoAsistencia[] CrearEntradas(DateOnly fecha) => _filas
        .Select(f => (Fila: f, Celda: f.Celdas.Single(c => c.Fecha == fecha)))
        .Where(x => x.Celda.EsEditable && x.Celda.Estado is not null)
        .Select(x => new EntradaEstadoAsistencia(x.Fila.Id, x.Celda.Estado!.Value)).ToArray();

    private void ConfirmarFecha(DateOnly fecha)
    {
        if (_mesConfirmado is null) return;

        foreach (var fila in _filas)
        {
            var celda = fila.Celdas.Single(x => x.Fecha == fecha);
            if (celda.EsEditable) { _confirmados[(fila.Id, fecha)] = celda.Estado; celda.EsConfirmada = true; }
        }

        var diasConfirmados = _mesConfirmado.Dias
            .Select(dia => dia.Fecha == fecha ? dia with { ExisteRegistroPersistido = true } : dia)
            .ToArray();
        var estudiantesConfirmados = _mesConfirmado.Estudiantes
            .Select(estudiante => ConfirmarEstudiante(estudiante, fecha))
            .ToArray();

        _mesConfirmado = _mesConfirmado with
        {
            Dias = diasConfirmados,
            Estudiantes = estudiantesConfirmados,
        };
        Dias = diasConfirmados;
        _fechasModificadas.Remove(fecha);
        OnPropertyChanged(nameof(FechasModificadas));
        NotificarEstado();
    }

    private AsistenciaEstudianteMesDetalle ConfirmarEstudiante(
        AsistenciaEstudianteMesDetalle estudiante,
        DateOnly fecha)
    {
        var estados = estudiante.Estados
            .Select(celda => celda.Fecha == fecha && celda.Tipo != TipoCeldaAsistencia.NoAplicable
                ? celda with
                {
                    Estado = _confirmados[(estudiante.EstudianteId, fecha)],
                    Tipo = TipoCeldaAsistencia.Confirmada,
                }
                : celda)
            .ToArray();
        var confirmados = estados.Where(celda => celda.Tipo == TipoCeldaAsistencia.Confirmada).ToArray();
        var presentes = confirmados.Count(celda => celda.Estado == EstadoAsistencia.Presente);
        var faltas = confirmados.Count(celda => celda.Estado == EstadoAsistencia.Falta);
        var retardos = confirmados.Count(celda => celda.Estado == EstadoAsistencia.Retardo);
        var justificadas = confirmados.Count(celda => celda.Estado == EstadoAsistencia.Justificada);
        var porcentaje = confirmados.Length == 0
            ? null
            : (double?)(presentes + retardos) / confirmados.Length * 100;

        return estudiante with
        {
            Estados = estados,
            Presentes = presentes,
            Faltas = faltas,
            Retardos = retardos,
            FaltasJustificadas = justificadas,
            PorcentajeConfirmado = porcentaje,
        };
    }

    private void MarcarDiaPresente()
    {
        if (FechaSeleccionada is not { } fecha) return;
        foreach (var fila in _filas) AsignarEstado(fila.Id, fecha, EstadoAsistencia.Presente);
    }

    private void DescartarDia()
    {
        if (FechaSeleccionada is not { } fecha) return;
        foreach (var fila in _filas)
        {
            fila.Celdas.Single(x => x.Fecha == fecha).Estado = _confirmados[(fila.Id, fecha)];
            fila.NotificarConteos();
        }
        _fechasModificadas.Remove(fecha); NotificarEstado();
    }
    private void DescartarMes() { foreach (var fecha in _fechasModificadas.ToArray()) { FechaSeleccionada = fecha; DescartarDia(); } }
    private void ActualizarModificacion(DateOnly fecha)
    {
        var difiere = _filas.Any(f => f.Celdas.Single(c => c.Fecha == fecha).Estado != _confirmados[(f.Id, fecha)]);
        if (difiere) _fechasModificadas.Add(fecha); else _fechasModificadas.Remove(fecha);
        OnPropertyChanged(nameof(FechasModificadas));
    }
    private void AplicarFiltro()
    {
        FilasVisibles = _filas.Where(x => (string.IsNullOrWhiteSpace(Busqueda) || x.Nombre.Contains(Busqueda, StringComparison.CurrentCultureIgnoreCase)) && Filtro switch { FiltroAsistenciaMensual.ConIncidencias => x.TieneIncidencias, FiltroAsistenciaMensual.SoloActivos => x.EstaActivoActualmente, _ => true }).ToArray();
        NotificarEstado();
    }
    private void Ejecutar(Action accion)
    {
        if (EstaOcupado) return; EstaOcupado = true;
        try { accion(); }
        catch (DomainValidationException e) { _mensajes.MostrarError(e.Message); }
        catch (DomainConflictException e) { _mensajes.MostrarError(e.Message); }
        catch (ErrorPersistenciaAplicacionException) { _mensajes.MostrarError("No fue posible guardar o cargar la asistencia. Intenta nuevamente."); }
        finally { EstaOcupado = false; }
    }
    private bool PuedeGuardarDia()
    {
        if (EstaOcupado || FechaSeleccionada is not { } fecha || _mesConfirmado is null) return false;

        var diaConfirmado = _mesConfirmado.Dias.SingleOrDefault(dia => dia.Fecha == fecha);
        return diaConfirmado?.EsLaborable == true
            && (!diaConfirmado.ExisteRegistroPersistido || _fechasModificadas.Contains(fecha));
    }
    private void NotificarEstado()
    {
        OnPropertyChanged(nameof(FechaSeleccionadaTexto)); OnPropertyChanged(nameof(EstadoMes)); OnPropertyChanged(nameof(IncluyeBorradores));
        OnPropertyChanged(nameof(AlumnosVisibles)); OnPropertyChanged(nameof(DiasGuardados)); OnPropertyChanged(nameof(Presentes)); OnPropertyChanged(nameof(Faltas)); OnPropertyChanged(nameof(Retardos)); OnPropertyChanged(nameof(Justificadas)); NotificarComandos();
    }
    private void NotificarComandos() { MesAnteriorCommand.NotifyCanExecuteChanged(); MesSiguienteCommand.NotifyCanExecuteChanged(); IrMesActualCommand.NotifyCanExecuteChanged(); GuardarDiaCommand.NotifyCanExecuteChanged(); GuardarMesCommand.NotifyCanExecuteChanged(); DescartarDiaCommand.NotifyCanExecuteChanged(); DescartarMesCommand.NotifyCanExecuteChanged(); MarcarDiaPresenteCommand.NotifyCanExecuteChanged(); }
}