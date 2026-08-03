using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionEstadoAsistencia(EstadoAsistencia Estado, string Texto);

public sealed class AsistenciaEstudianteVisual : ViewModelBase
{
    private EstadoAsistencia _estado;
    private readonly IReadOnlyList<OpcionEstadoAsistencia> _opcionesEstado = Opciones;

    public AsistenciaEstudianteVisual(
        EstudianteId estudianteId,
        string nombre,
        int numeroLista,
        EstadoAsistencia estado,
        bool estaActivoActualmente,
        Action cambio)
    {
        EstudianteId = estudianteId;
        Nombre = nombre;
        NumeroLista = numeroLista;
        _estado = estado;
        EstaActivoActualmente = estaActivoActualmente;
        Cambio = cambio;
    }

    public static IReadOnlyList<OpcionEstadoAsistencia> Opciones { get; } =
    [
        new(EstadoAsistencia.Presente, "Presente"),
        new(EstadoAsistencia.Falta, "Falta"),
        new(EstadoAsistencia.Retardo, "Retardo"),
        new(EstadoAsistencia.Justificada, "Falta justificada"),
    ];

    private Action Cambio { get; }

    internal EstudianteId EstudianteId { get; }

    public string Nombre { get; }

    public int NumeroLista { get; }

    public bool EstaActivoActualmente { get; }

    public string SituacionActual => EstaActivoActualmente ? "Activo actualmente" : "Inactivo actualmente";

    public IReadOnlyList<OpcionEstadoAsistencia> OpcionesEstado => _opcionesEstado;

    public EstadoAsistencia Estado
    {
        get => _estado;
        set
        {
            if (SetProperty(ref _estado, value))
            {
                Cambio();
            }
        }
    }
}