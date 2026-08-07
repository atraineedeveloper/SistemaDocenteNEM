using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class EntregaActividadVisual : ViewModelBase
{
    private EstadoEntregaActividad _estadoEntrega;
    private NivelLogro _nivelLogro;
    private string _observacion;
    private bool _seleccionada;

    internal EntregaActividadVisual(EntregaActividadDetalle detalle)
    {
        EstudianteId = detalle.EstudianteId;
        NumeroLista = detalle.NumeroLista;
        Nombre = detalle.NombreVisible;
        EstaActivoActualmente = detalle.EstaActivoActualmente;
        _estadoEntrega = detalle.EstadoEntrega;
        _nivelLogro = detalle.NivelLogro;
        _observacion = detalle.Observacion;
    }

    internal EstudianteId EstudianteId { get; }
    public int NumeroLista { get; }
    public string Nombre { get; }
    public bool EstaActivoActualmente { get; }
    public string SituacionActual => EstaActivoActualmente ? "Activo actualmente" : "Inactivo actualmente";

    public EstadoEntregaActividad EstadoEntrega
    {
        get => _estadoEntrega;
        set
        {
            if (!Enum.IsDefined(value)) return;
            var nivelAnterior = _nivelLogro;
            if (value == EstadoEntregaActividad.NoEntregada)
            {
                _nivelLogro = NivelLogro.Pendiente;
            }

            if (SetProperty(ref _estadoEntrega, value))
            {
                OnPropertyChanged(nameof(EtiquetaEntrega));
                OnPropertyChanged(nameof(EtiquetaNivel));
            }
            if (nivelAnterior != _nivelLogro)
            {
                OnPropertyChanged(nameof(NivelLogro));
                OnPropertyChanged(nameof(EtiquetaNivel));
            }
        }
    }

    public string EtiquetaEntrega => _estadoEntrega switch
    {
        EstadoEntregaActividad.Entregada => "Entregada",
        EstadoEntregaActividad.NoEntregada => "No entregada",
        _ => "Pendiente",
    };

    public NivelLogro NivelLogro
    {
        get => _nivelLogro;
        set
        {
            if (!Enum.IsDefined(value)) return;
            var estadoAnterior = _estadoEntrega;
            var nivelNormalizado = value;
            if (value == NivelLogro.NoEntrego)
            {
                _estadoEntrega = EstadoEntregaActividad.NoEntregada;
                nivelNormalizado = NivelLogro.Pendiente;
            }
            else if (value != NivelLogro.Pendiente)
            {
                _estadoEntrega = EstadoEntregaActividad.Entregada;
            }

            if (SetProperty(ref _nivelLogro, nivelNormalizado))
            {
                OnPropertyChanged(nameof(EtiquetaNivel));
            }
            if (estadoAnterior != _estadoEntrega)
            {
                OnPropertyChanged(nameof(EstadoEntrega));
                OnPropertyChanged(nameof(EtiquetaEntrega));
                OnPropertyChanged(nameof(EtiquetaNivel));
            }
        }
    }

    public string EtiquetaNivel
    {
        get
        {
            if (_estadoEntrega == EstadoEntregaActividad.NoEntregada) return "NE";
            return _nivelLogro switch
            {
                NivelLogro.Domina => "D",
                NivelLogro.Suficiente => "S",
                NivelLogro.EnProceso => "EP",
                NivelLogro.RequiereApoyo => "RA",
                _ => "—",
            };
        }
    }

    public string Observacion { get => _observacion; set => SetProperty(ref _observacion, value); }
    public bool Seleccionada { get => _seleccionada; set => SetProperty(ref _seleccionada, value); }
}

public enum FiltroProyecto { Todos, Borrador, EnCurso, Finalizado }
public enum FiltroEntrega
{
    Todos, Pendientes,
    Domina, Suficiente, EnProceso, RequiereApoyo, NoEntrego,
    SoloIncidencias, SoloActivos, ActivosEInactivosHistoricos
}