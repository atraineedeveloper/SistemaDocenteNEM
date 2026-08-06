using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class EntregaActividadVisual : ViewModelBase
{
    private NivelLogro _nivelLogro;
    private string _observacion;
    private bool _seleccionada;

    internal EntregaActividadVisual(EntregaActividadDetalle detalle)
    {
        EstudianteId = detalle.EstudianteId; NumeroLista = detalle.NumeroLista; Nombre = detalle.NombreVisible;
        EstaActivoActualmente = detalle.EstaActivoActualmente; _nivelLogro = detalle.NivelLogro; _observacion = detalle.Observacion;
    }

    internal EstudianteId EstudianteId { get; }
    public int NumeroLista { get; }
    public string Nombre { get; }
    public bool EstaActivoActualmente { get; }
    public string SituacionActual => EstaActivoActualmente ? "Activo actualmente" : "Inactivo actualmente";
    public NivelLogro NivelLogro { get => _nivelLogro; set { if (SetProperty(ref _nivelLogro, value)) OnPropertyChanged(nameof(EtiquetaNivel)); } }
    public string EtiquetaNivel => _nivelLogro switch
    {
        NivelLogro.Domina => "D",
        NivelLogro.Suficiente => "S",
        NivelLogro.EnProceso => "EP",
        NivelLogro.RequiereApoyo => "RA",
        NivelLogro.NoEntrego => "NE",
        _ => "—",
    };
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