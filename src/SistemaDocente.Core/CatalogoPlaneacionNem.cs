namespace SistemaDocente.Core;

public static class CatalogoPlaneacionNem
{
    private static readonly MetodologiaProyectoNem[] Metodologias =
    [
        MetodologiaProyectoNem.ProyectosComunitarios,
        MetodologiaProyectoNem.IndagacionSteam,
        MetodologiaProyectoNem.AprendizajeBasadoEnProblemas,
        MetodologiaProyectoNem.AprendizajeServicio,
    ];

    private static readonly CampoFormativoNem[] Campos =
    [
        CampoFormativoNem.Lenguajes,
        CampoFormativoNem.SaberesPensamientoCientifico,
        CampoFormativoNem.EticaNaturalezaSociedades,
        CampoFormativoNem.DeLoHumanoYLoComunitario,
    ];

    public static IReadOnlyList<MetodologiaProyectoNem> MetodologiasProyecto { get; } =
        Array.AsReadOnly(Metodologias);

    public static IReadOnlyList<CampoFormativoNem> CamposFormativos { get; } =
        Array.AsReadOnly(Campos);

    public static bool EsMetodologiaValida(MetodologiaProyectoNem metodologia) =>
        Enum.IsDefined(metodologia);

    public static bool EsMetodologiaEspecifica(MetodologiaProyectoNem metodologia) =>
        metodologia is >= MetodologiaProyectoNem.ProyectosComunitarios
            and <= MetodologiaProyectoNem.AprendizajeServicio;

    public static bool EsCampoValido(CampoFormativoNem campo) =>
        Enum.IsDefined(campo);

    public static bool EsCampoEspecifico(CampoFormativoNem campo) =>
        campo is >= CampoFormativoNem.Lenguajes
            and <= CampoFormativoNem.DeLoHumanoYLoComunitario;

    public static string FormatearMetodologia(MetodologiaProyectoNem metodologia) => metodologia switch
    {
        MetodologiaProyectoNem.ProyectosComunitarios => "Aprendizaje Basado en Proyectos Comunitarios",
        MetodologiaProyectoNem.IndagacionSteam => "Aprendizaje Basado en Indagación · STEAM como enfoque",
        MetodologiaProyectoNem.AprendizajeBasadoEnProblemas => "Aprendizaje Basado en Problemas (ABP)",
        MetodologiaProyectoNem.AprendizajeServicio => "Aprendizaje Servicio (AS)",
        _ => "No especificada",
    };

    public static string FormatearCampo(CampoFormativoNem campo) => campo switch
    {
        CampoFormativoNem.Lenguajes => "Lenguajes",
        CampoFormativoNem.SaberesPensamientoCientifico => "Saberes y Pensamiento Científico",
        CampoFormativoNem.EticaNaturalezaSociedades => "Ética, Naturaleza y Sociedades",
        CampoFormativoNem.DeLoHumanoYLoComunitario => "De lo Humano y lo Comunitario",
        _ => "No especificado",
    };

    public static IReadOnlyList<GradoPrimaria> NormalizarGradosObjetivo(
        IEnumerable<GradoPrimaria>? grados,
        bool permitirVacio = true)
    {
        if (grados is null)
        {
            return permitirVacio
                ? Array.Empty<GradoPrimaria>()
                : throw new DomainValidationException("Selecciona al menos un grado objetivo.");
        }

        var snapshot = grados.ToArray();
        if (snapshot.Any(grado => !CatalogoNemPrimaria.EsGradoReal(grado)))
        {
            throw new DomainValidationException("Los grados objetivo deben pertenecer a primaria, del 1.º al 6.º.");
        }

        var normalizados = snapshot
            .Distinct()
            .OrderBy(grado => (int)grado)
            .ToArray();

        if (!permitirVacio && normalizados.Length == 0)
        {
            throw new DomainValidationException("Selecciona al menos un grado objetivo.");
        }

        return normalizados;
    }

    public static void ValidarGradosActividadDentroDelProyecto(
        IEnumerable<GradoPrimaria>? gradosProyecto,
        IEnumerable<GradoPrimaria>? gradosActividad)
    {
        var proyecto = NormalizarGradosObjetivo(gradosProyecto);
        var actividad = NormalizarGradosObjetivo(gradosActividad);
        if (proyecto.Count == 0 || actividad.Count == 0)
        {
            return;
        }

        var permitidos = proyecto.ToHashSet();
        if (actividad.Any(grado => !permitidos.Contains(grado)))
        {
            throw new DomainValidationException(
                "Los grados objetivo de la actividad deben estar incluidos en los grados objetivo del proyecto.");
        }
    }
}