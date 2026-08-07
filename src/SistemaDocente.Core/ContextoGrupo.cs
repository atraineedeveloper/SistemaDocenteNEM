namespace SistemaDocente.Core;

public sealed record ContextoGrupo(
    GrupoId GrupoId,
    string CicloEscolar,
    string NombreEscuela,
    string Cct,
    string EntidadFederativa,
    string Municipio,
    string Localidad,
    string Grado,
    string Grupo,
    string Turno,
    EtapaDesarrolloCognoscitivo EtapaCognoscitiva,
    string DocenteResponsable,
    DateOnly? ResponsableDesde,
    DateOnly? ResponsableHasta,
    TimeOnly? HoraEntrada,
    TimeOnly? HoraSalida)
{
    public OrganizacionEscolar OrganizacionEscolar { get; init; } = OrganizacionEscolar.NoEspecificada;

    public IReadOnlyList<GradoPrimaria> GradosAtendidos { get; init; } = Array.Empty<GradoPrimaria>();

    public IReadOnlyList<FaseNem> FasesNem => CatalogoNemPrimaria.ObtenerFases(GradosAtendidos);

    public bool EsMultigrado => GradosAtendidos.Count > 1;

    public string ModalidadGrupo => GradosAtendidos.Count switch
    {
        0 => "Sin grados configurados",
        1 => "Unigrado",
        _ => "Multigrado",
    };

    public string GradosTexto => GradosAtendidos.Count == 0
        ? Grado
        : CatalogoNemPrimaria.FormatearGrados(GradosAtendidos);

    public string FasesNemTexto => CatalogoNemPrimaria.FormatearFases(GradosAtendidos);

    public string ReferenciaDesarrolloTexto => CatalogoNemPrimaria.DescribirReferenciaPiaget(GradosAtendidos);

    public IReadOnlyList<EtapaDesarrolloCognoscitivo> EtapasCognoscitivasReferencia =>
        CatalogoNemPrimaria.ObtenerReferenciaPiaget(GradosAtendidos);

    public static ContextoGrupo Crear(
        GrupoId grupoId,
        string? cicloEscolar = null,
        string? nombreEscuela = null,
        string? cct = null,
        string? entidadFederativa = null,
        string? municipio = null,
        string? localidad = null,
        string? grado = null,
        string? grupo = null,
        string? turno = null,
        EtapaDesarrolloCognoscitivo etapaCognoscitiva = EtapaDesarrolloCognoscitivo.NoEspecificada,
        string? docenteResponsable = null,
        DateOnly? responsableDesde = null,
        DateOnly? responsableHasta = null,
        TimeOnly? horaEntrada = null,
        TimeOnly? horaSalida = null,
        OrganizacionEscolar organizacionEscolar = OrganizacionEscolar.NoEspecificada,
        IReadOnlyCollection<GradoPrimaria>? gradosAtendidos = null)
    {
        if (grupoId == default)
        {
            throw new DomainValidationException("La identidad del grupo es obligatoria.");
        }

        if (!Enum.IsDefined(etapaCognoscitiva))
        {
            throw new DomainValidationException("La etapa de desarrollo cognoscitivo no es válida.");
        }

        if (!Enum.IsDefined(organizacionEscolar))
        {
            throw new DomainValidationException("La organización escolar no es válida.");
        }

        if (responsableDesde is not null && responsableHasta is not null && responsableHasta < responsableDesde)
        {
            throw new DomainValidationException("La fecha final de responsabilidad no puede ser anterior a la fecha inicial.");
        }

        var gradoLegacy = Normalizar(grado, 80, "El grado");
        var grados = ResolverGrados(gradosAtendidos, gradoLegacy);
        var etapaCompatible = ResolverEtapaCompatible(grados, etapaCognoscitiva);
        var gradoProyectado = grados.Count == 0
            ? gradoLegacy
            : CatalogoNemPrimaria.FormatearGrados(grados);

        return new ContextoGrupo(
            grupoId,
            Normalizar(cicloEscolar, 20, "El ciclo escolar"),
            Normalizar(nombreEscuela, 180, "El nombre de la escuela"),
            Normalizar(cct, 30, "La CCT"),
            Normalizar(entidadFederativa, 80, "La entidad federativa"),
            Normalizar(municipio, 120, "El municipio"),
            Normalizar(localidad, 120, "La localidad"),
            gradoProyectado,
            Normalizar(grupo, 30, "El grupo"),
            Normalizar(turno, 40, "El turno"),
            etapaCompatible,
            Normalizar(docenteResponsable, 180, "El docente responsable"),
            responsableDesde,
            responsableHasta,
            horaEntrada,
            horaSalida)
        {
            OrganizacionEscolar = organizacionEscolar,
            GradosAtendidos = grados,
        };
    }

    private static IReadOnlyList<GradoPrimaria> ResolverGrados(
        IReadOnlyCollection<GradoPrimaria>? gradosAtendidos,
        string gradoLegacy)
    {
        var grados = CatalogoNemPrimaria.NormalizarGrados(gradosAtendidos);
        if (grados.Count > 0) return grados;

        return CatalogoNemPrimaria.TryParseGradoLegacy(gradoLegacy, out var grado)
            ? new[] { grado }
            : Array.Empty<GradoPrimaria>();
    }

    private static EtapaDesarrolloCognoscitivo ResolverEtapaCompatible(
        IReadOnlyList<GradoPrimaria> grados,
        EtapaDesarrolloCognoscitivo valorLegacy)
    {
        if (grados.Count == 0) return valorLegacy;
        var referencias = CatalogoNemPrimaria.ObtenerReferenciaPiaget(grados);
        return referencias.Count == 1
            ? referencias[0]
            : EtapaDesarrolloCognoscitivo.NoEspecificada;
    }

    private static string Normalizar(string? valor, int maximo, string campo)
    {
        var texto = string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        if (texto.Length > maximo)
        {
            throw new DomainValidationException($"{campo} no puede exceder {maximo} caracteres.");
        }

        return texto;
    }
}