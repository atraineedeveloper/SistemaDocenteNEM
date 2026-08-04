using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed class EntregaActividadVisual : ViewModelBase
{
    private EstadoEntrega _estado;
    private string _observacion;
    private bool _seleccionada;

    internal EntregaActividadVisual(EntregaActividadDetalle detalle)
    {
        EstudianteId = detalle.EstudianteId; NumeroLista = detalle.NumeroLista; Nombre = detalle.NombreVisible;
        EstaActivoActualmente = detalle.EstaActivoActualmente; _estado = detalle.Estado; _observacion = detalle.Observacion;
    }

    internal EstudianteId EstudianteId { get; }
    public int NumeroLista { get; }
    public string Nombre { get; }
    public bool EstaActivoActualmente { get; }
    public string SituacionActual => EstaActivoActualmente ? "Activo actualmente" : "Inactivo actualmente";
    public EstadoEntrega Estado { get => _estado; set => SetProperty(ref _estado, value); }
    public string Observacion { get => _observacion; set => SetProperty(ref _observacion, value); }
    public bool Seleccionada { get => _seleccionada; set => SetProperty(ref _seleccionada, value); }
}

public enum FiltroProyecto { Todos, Borrador, EnCurso, Finalizado }
public enum FiltroEntrega { Todos, Pendientes, Entregadas, NoEntregadas, SoloIncidencias, SoloActivos, ActivosEInactivosHistoricos }