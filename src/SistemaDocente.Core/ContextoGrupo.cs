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
        TimeOnly? horaSalida = null)
    {
        if (grupoId == default)
        {
            throw new DomainValidationException("La identidad del grupo es obligatoria.");
        }

        if (!Enum.IsDefined(etapaCognoscitiva))
        {
            throw new DomainValidationException("La etapa de desarrollo cognoscitivo no es válida.");
        }

        if (responsableDesde is not null && responsableHasta is not null && responsableHasta < responsableDesde)
        {
            throw new DomainValidationException("La fecha final de responsabilidad no puede ser anterior a la fecha inicial.");
        }

        return new ContextoGrupo(
            grupoId,
            Normalizar(cicloEscolar, 20, "El ciclo escolar"),
            Normalizar(nombreEscuela, 180, "El nombre de la escuela"),
            Normalizar(cct, 30, "La CCT"),
            Normalizar(entidadFederativa, 80, "La entidad federativa"),
            Normalizar(municipio, 120, "El municipio"),
            Normalizar(localidad, 120, "La localidad"),
            Normalizar(grado, 30, "El grado"),
            Normalizar(grupo, 30, "El grupo"),
            Normalizar(turno, 40, "El turno"),
            etapaCognoscitiva,
            Normalizar(docenteResponsable, 180, "El docente responsable"),
            responsableDesde,
            responsableHasta,
            horaEntrada,
            horaSalida);
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
