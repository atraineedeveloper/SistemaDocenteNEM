using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public enum FiltroAsistenciaMensual
{
    Todos,
    ConIncidencias,
    SoloActivos,
    ActivosEInactivosHistoricos,
}

public sealed record MesVisual(int Numero, string Nombre);

public sealed class AsistenciaCeldaVisual : ViewModelBase
{
    private EstadoAsistencia? _estado;

    public AsistenciaCeldaVisual(
        DateOnly fecha,
        EstadoAsistencia? estado,
        bool esEditable,
        bool esConfirmada)
    {
        Fecha = fecha;
        _estado = estado;
        EsEditable = esEditable;
        EsConfirmada = esConfirmada;
    }

    public DateOnly Fecha { get; }
    public bool EsEditable { get; }
    public bool EsConfirmada { get; internal set; }
    public EstadoAsistencia? Estado
    {
        get => _estado;
        internal set
        {
            if (SetProperty(ref _estado, value)) OnPropertyChanged(nameof(Texto));
        }
    }
    public string Texto => Estado switch
    {
        EstadoAsistencia.Presente => "P",
        EstadoAsistencia.Falta => "F",
        EstadoAsistencia.Retardo => "R",
        EstadoAsistencia.Justificada => "J",
        _ => "—",
    };
}

public sealed class AsistenciaEstudianteMesVisual : ViewModelBase
{
    internal AsistenciaEstudianteMesVisual(
        EstudianteId id,
        int numero,
        string nombre,
        bool activo,
        IReadOnlyList<AsistenciaCeldaVisual> celdas,
        double? porcentaje)
    {
        Id = id;
        NumeroLista = numero;
        Nombre = nombre;
        EstaActivoActualmente = activo;
        Celdas = celdas;
        PorcentajeConfirmado = porcentaje;
    }

    internal EstudianteId Id { get; }
    public int NumeroLista { get; }
    public string Nombre { get; }
    public bool EstaActivoActualmente { get; }
    public string SituacionActual => EstaActivoActualmente ? "Activo actualmente" : "Inactivo actualmente";
    public IReadOnlyList<AsistenciaCeldaVisual> Celdas { get; }
    public int Presentes => Celdas.Count(x => x.Estado == EstadoAsistencia.Presente);
    public int Faltas => Celdas.Count(x => x.Estado == EstadoAsistencia.Falta);
    public int Retardos => Celdas.Count(x => x.Estado == EstadoAsistencia.Retardo);
    public int Justificadas => Celdas.Count(x => x.Estado == EstadoAsistencia.Justificada);
    public double? PorcentajeConfirmado { get; }
    public string PorcentajeTexto => PorcentajeConfirmado is null ? "—" : $"{PorcentajeConfirmado:0.#} %";
    public bool TieneIncidencias => Faltas + Retardos + Justificadas > 0;

    internal void NotificarConteos()
    {
        OnPropertyChanged(nameof(Presentes));
        OnPropertyChanged(nameof(Faltas));
        OnPropertyChanged(nameof(Retardos));
        OnPropertyChanged(nameof(Justificadas));
        OnPropertyChanged(nameof(TieneIncidencias));
    }
}