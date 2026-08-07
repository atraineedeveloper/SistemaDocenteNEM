using SistemaDocente.Core;

namespace SistemaDocente.Presentation;

public sealed record OpcionMetodologiaProyectoNem(
    MetodologiaProyectoNem Valor,
    string Texto);

public sealed record OpcionCampoFormativoNem(
    CampoFormativoNem Valor,
    string Texto);

public sealed class SeleccionGradoPlaneacion : ViewModelBase
{
    private bool _seleccionado;

    public SeleccionGradoPlaneacion(GradoPrimaria grado, bool seleccionado = false)
    {
        if (!CatalogoNemPrimaria.EsGradoReal(grado))
        {
            throw new ArgumentOutOfRangeException(nameof(grado));
        }

        Grado = grado;
        Texto = CatalogoNemPrimaria.FormatearGrado(grado);
        _seleccionado = seleccionado;
    }

    public GradoPrimaria Grado { get; }
    public string Texto { get; }

    public bool Seleccionado
    {
        get => _seleccionado;
        set => SetProperty(ref _seleccionado, value);
    }
}