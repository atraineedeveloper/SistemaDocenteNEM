using System.Globalization;
using System.Text;

using SistemaDocente.Core;

namespace SistemaDocente.Application;

public sealed class ImportacionEstudiantesCasosUso
{
    private static readonly string[] FormatosFechaTexto =
    [
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy",
        "d-M-yyyy",
    ];

    private readonly IAlmacenamientoGrupos _grupos;
    private readonly IAlmacenamientoContextoGrupo _contextos;

    public ImportacionEstudiantesCasosUso(
        IAlmacenamientoGrupos grupos,
        IAlmacenamientoContextoGrupo contextos)
    {
        ArgumentNullException.ThrowIfNull(grupos);
        ArgumentNullException.ThrowIfNull(contextos);
        _grupos = grupos;
        _contextos = contextos;
    }

    public PreviaImportacionEstudiantes CrearPrevia(
        GrupoId grupoId,
        HojaTabular hoja,
        IReadOnlyCollection<MapeoColumnaImportacion> mapeo)
    {
        ArgumentNullException.ThrowIfNull(hoja);
        ArgumentNullException.ThrowIfNull(mapeo);
        ValidarMapeo(hoja, mapeo);

        var filas = hoja.Filas
            .Select(fila => MapearFila(fila, mapeo))
            .ToArray();

        return Revalidar(grupoId, filas);
    }

    public PreviaImportacionEstudiantes Revalidar(
        GrupoId grupoId,
        IReadOnlyCollection<FilaImportacionEstudiante> filas)
    {
        ArgumentNullException.ThrowIfNull(filas);
        var grupo = CargarGrupo(grupoId);
        var contexto = _contextos.Cargar(grupoId);
        return RevalidarContraSnapshot(grupo, contexto, filas);
    }

    public ResultadoImportacionEstudiantes Confirmar(
        GrupoId grupoId,
        IReadOnlyCollection<FilaImportacionEstudiante> filas)
    {
        ArgumentNullException.ThrowIfNull(filas);

        var grupoActual = CargarGrupo(grupoId);
        var contextoActual = _contextos.Cargar(grupoId);
        var previa = RevalidarContraSnapshot(grupoActual, contextoActual, filas);
        if (!previa.PuedeConfirmarse)
        {
            return new ResultadoImportacionEstudiantes(
                false,
                0,
                previa.Excluidas,
                Array.Empty<EstudianteId>(),
                previa);
        }

        var grupoTrabajo = ClonarGrupo(grupoActual);
        var creados = new List<EstudianteId>();

        foreach (var fila in previa.Filas.Where(fila => fila.Estado == EstadoFilaImportacion.Lista))
        {
            var estudiante = grupoTrabajo.AgregarEstudiante(
                fila.NombreVisible,
                fila.NumeroLista!.Value,
                fila.PrimerApellido,
                fila.SegundoApellido,
                fila.Nombres,
                fila.FechaNacimiento,
                fila.Genero,
                fila.FechaIngreso,
                fila.Observaciones,
                fila.Grado,
                preservarNombreVisible: !string.IsNullOrWhiteSpace(fila.NombreCompleto));
            creados.Add(estudiante.Id);
        }

        _grupos.Guardar(grupoTrabajo);
        return new ResultadoImportacionEstudiantes(
            true,
            creados.Count,
            previa.Excluidas,
            creados);
    }

    private static PreviaImportacionEstudiantes RevalidarContraSnapshot(
        Grupo grupo,
        ContextoGrupo? contexto,
        IReadOnlyCollection<FilaImportacionEstudiante> filas)
    {
        var gradosAtendidos = CatalogoNemPrimaria.NormalizarGrados(contexto?.GradosAtendidos);
        var normalizadas = filas
            .Select(fila => NormalizarFila(fila, gradosAtendidos))
            .ToArray();

        AplicarConflictosNumeroLista(normalizadas, grupo);
        AplicarDuplicadosProbables(normalizadas, grupo);
        AplicarValidacionDominio(normalizadas, grupo);

        return new PreviaImportacionEstudiantes(grupo.Id, normalizadas);
    }

    private static FilaImportacionEstudiante MapearFila(
        FilaTabular fila,
        IReadOnlyCollection<MapeoColumnaImportacion> mapeo)
    {
        string Valor(CampoImportacionEstudiante campo)
        {
            var origen = mapeo.SingleOrDefault(item => item.Campo == campo);
            return origen is null || origen.IndiceColumna >= fila.Celdas.Count
                ? string.Empty
                : fila.Celdas[origen.IndiceColumna].Texto;
        }

        return new FilaImportacionEstudiante(
            fila.NumeroOrigen,
            Valor(CampoImportacionEstudiante.NumeroLista),
            Valor(CampoImportacionEstudiante.NombreCompleto),
            Valor(CampoImportacionEstudiante.PrimerApellido),
            Valor(CampoImportacionEstudiante.SegundoApellido),
            Valor(CampoImportacionEstudiante.Nombres),
            Valor(CampoImportacionEstudiante.FechaNacimiento),
            Valor(CampoImportacionEstudiante.Genero),
            Valor(CampoImportacionEstudiante.FechaIngreso),
            Valor(CampoImportacionEstudiante.Grado),
            Valor(CampoImportacionEstudiante.Observaciones));
    }

    private static FilaImportacionEstudiante NormalizarFila(
        FilaImportacionEstudiante fila,
        IReadOnlyList<GradoPrimaria> gradosAtendidos)
    {
        if (fila.Excluida)
        {
            return fila with
            {
                Estado = EstadoFilaImportacion.Excluida,
                Problemas = Array.Empty<ProblemaImportacion>(),
            };
        }

        var problemas = new List<ProblemaImportacion>();
        var numeroLista = NormalizarNumeroLista(fila.NumeroListaTexto, problemas);
        var nombreVisible = ResolverNombreVisible(fila, problemas);
        var fechaNacimiento = NormalizarFecha(
            fila.FechaNacimientoTexto,
            CampoImportacionEstudiante.FechaNacimiento,
            problemas);
        var genero = NormalizarGenero(fila.GeneroTexto, problemas);
        var fechaIngreso = NormalizarFecha(
            fila.FechaIngresoTexto,
            CampoImportacionEstudiante.FechaIngreso,
            problemas);
        var (grado, predeterminado) = NormalizarGrado(fila, gradosAtendidos, problemas);

        var estado = CalcularEstado(problemas);
        return fila with
        {
            NumeroLista = numeroLista,
            NombreVisible = nombreVisible,
            FechaNacimiento = fechaNacimiento,
            Genero = genero,
            FechaIngreso = fechaIngreso,
            Grado = grado,
            GradoPredeterminadoPorGrupo = predeterminado,
            Estado = estado,
            Problemas = problemas,
        };
    }

    private static int? NormalizarNumeroLista(
        string texto,
        List<ProblemaImportacion> problemas)
    {
        var valor = texto.Trim();
        if (int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var entero) && entero > 0)
        {
            return entero;
        }

        if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValor) &&
            decimalValor == decimal.Truncate(decimalValor) &&
            decimalValor is > 0 and <= int.MaxValue)
        {
            return decimal.ToInt32(decimalValor);
        }

        problemas.Add(new ProblemaImportacion(
            CampoImportacionEstudiante.NumeroLista,
            "invalid-list-number",
            "El número de lista debe ser un entero mayor que cero.",
            SeveridadProblemaImportacion.Invalido));
        return null;
    }

    private static string ResolverNombreVisible(
        FilaImportacionEstudiante fila,
        List<ProblemaImportacion> problemas)
    {
        var nombreExplicito = fila.NombreCompleto.Trim();
        if (nombreExplicito.Length > 0)
        {
            return nombreExplicito;
        }

        var construido = string.Join(
            " ",
            new[] { fila.PrimerApellido, fila.SegundoApellido, fila.Nombres }
                .Select(valor => valor.Trim())
                .Where(valor => valor.Length > 0));

        if (construido.Length > 0)
        {
            return construido;
        }

        problemas.Add(new ProblemaImportacion(
            CampoImportacionEstudiante.NombreCompleto,
            "missing-name",
            "La fila necesita un nombre completo o suficientes campos de nombre estructurado.",
            SeveridadProblemaImportacion.Invalido));
        return string.Empty;
    }

    private static DateOnly? NormalizarFecha(
        string texto,
        CampoImportacionEstudiante campo,
        List<ProblemaImportacion> problemas)
    {
        var valor = texto.Trim();
        if (valor.Length == 0)
        {
            return null;
        }

        if (DateOnly.TryParseExact(
            valor,
            FormatosFechaTexto,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var fecha))
        {
            return fecha;
        }

        problemas.Add(new ProblemaImportacion(
            campo,
            "ambiguous-date",
            "La fecha no coincide con un formato admitido y requiere corrección.",
            SeveridadProblemaImportacion.Revision));
        return null;
    }

    private static GeneroEstudiante NormalizarGenero(
        string texto,
        List<ProblemaImportacion> problemas)
    {
        var valor = NormalizarClave(texto);
        if (valor.Length == 0)
        {
            return GeneroEstudiante.NoEspecificado;
        }

        if (valor is "f" or "femenino" or "mujer")
        {
            return GeneroEstudiante.Mujer;
        }

        if (valor is "h" or "hombre" or "masculino")
        {
            return GeneroEstudiante.Hombre;
        }

        problemas.Add(new ProblemaImportacion(
            CampoImportacionEstudiante.Genero,
            valor == "m" ? "ambiguous-gender" : "invalid-gender",
            valor == "m"
                ? "La abreviatura M es ambigua. Selecciona Hombre o Mujer explícitamente."
                : "El género no coincide con un valor admitido y requiere corrección.",
            SeveridadProblemaImportacion.Revision));
        return GeneroEstudiante.NoEspecificado;
    }

    private static (GradoPrimaria Grado, bool Predeterminado) NormalizarGrado(
        FilaImportacionEstudiante fila,
        IReadOnlyList<GradoPrimaria> gradosAtendidos,
        List<ProblemaImportacion> problemas)
    {
        var texto = fila.GradoTexto.Trim();
        if (texto.Length == 0)
        {
            if (gradosAtendidos.Count == 1)
            {
                var gradoActual = gradosAtendidos[0];
                if (fila.GradoPredeterminadoPorGrupo &&
                    CatalogoNemPrimaria.EsGradoReal(fila.Grado) &&
                    fila.Grado != gradoActual)
                {
                    problemas.Add(new ProblemaImportacion(
                        CampoImportacionEstudiante.Grado,
                        "group-grade-changed",
                        "La configuración de grado del grupo cambió desde la vista previa. Revisa el grado antes de importar.",
                        SeveridadProblemaImportacion.Revision));
                }

                return (gradoActual, true);
            }

            problemas.Add(new ProblemaImportacion(
                CampoImportacionEstudiante.Grado,
                gradosAtendidos.Count > 1 ? "missing-multigrade-grade" : "missing-grade",
                gradosAtendidos.Count > 1
                    ? "En un grupo multigrado debes seleccionar el grado de esta fila."
                    : "No hay un único grado configurado que pueda asignarse automáticamente.",
                SeveridadProblemaImportacion.Revision));
            return (GradoPrimaria.NoEspecificado, false);
        }

        if (!CatalogoNemPrimaria.TryParseGradoLegacy(texto, out var grado))
        {
            problemas.Add(new ProblemaImportacion(
                CampoImportacionEstudiante.Grado,
                "invalid-grade",
                "El grado no puede interpretarse de forma determinista como 1.º a 6.º de primaria.",
                SeveridadProblemaImportacion.Revision));
            return (GradoPrimaria.NoEspecificado, false);
        }

        if (gradosAtendidos.Count > 0 && !gradosAtendidos.Contains(grado))
        {
            problemas.Add(new ProblemaImportacion(
                CampoImportacionEstudiante.Grado,
                "grade-outside-group",
                "El grado indicado no forma parte de los grados atendidos por este grupo.",
                SeveridadProblemaImportacion.Revision));
        }

        return (grado, false);
    }

    private static void AplicarConflictosNumeroLista(
        FilaImportacionEstudiante[] filas,
        Grupo grupo)
    {
        var numerosActivos = grupo.Estudiantes
            .Where(estudiante => estudiante.EstaActivo)
            .Select(estudiante => estudiante.NumeroLista)
            .ToHashSet();

        foreach (var fila in filas.Where(PuedeRecibirProblemas))
        {
            if (fila.NumeroLista is { } numero && numerosActivos.Contains(numero))
            {
                AgregarProblema(
                    filas,
                    fila,
                    new ProblemaImportacion(
                        CampoImportacionEstudiante.NumeroLista,
                        "active-list-number-conflict",
                        $"El número de lista {numero} ya pertenece a un estudiante activo.",
                        SeveridadProblemaImportacion.Invalido));
            }
        }

        var duplicados = filas
            .Where(PuedeRecibirProblemas)
            .Where(fila => fila.NumeroLista is not null)
            .GroupBy(fila => fila.NumeroLista!.Value)
            .Where(grupoFilas => grupoFilas.Count() > 1)
            .SelectMany(grupoFilas => grupoFilas)
            .ToArray();

        foreach (var fila in duplicados)
        {
            AgregarProblema(
                filas,
                fila,
                new ProblemaImportacion(
                    CampoImportacionEstudiante.NumeroLista,
                    "duplicate-import-list-number",
                    $"El número de lista {fila.NumeroLista} se repite entre las filas incluidas.",
                    SeveridadProblemaImportacion.Invalido));
        }
    }

    private static void AplicarDuplicadosProbables(
        FilaImportacionEstudiante[] filas,
        Grupo grupo)
    {
        foreach (var fila in filas.Where(PuedeRecibirProblemas))
        {
            if (fila.ImportarDuplicadoProbableComoNuevo || fila.NombreVisible.Length == 0)
            {
                continue;
            }

            var coincidencia = grupo.Estudiantes.FirstOrDefault(
                estudiante => EsDuplicadoProbable(fila, estudiante));
            if (coincidencia is null)
            {
                continue;
            }

            var evidenciaFecha = fila.FechaNacimiento is not null &&
                coincidencia.FechaNacimiento == fila.FechaNacimiento;
            AgregarProblema(
                filas,
                fila,
                new ProblemaImportacion(
                    CampoImportacionEstudiante.NombreCompleto,
                    "probable-duplicate",
                    evidenciaFecha
                        ? "Existe un estudiante con el mismo nombre normalizado y fecha de nacimiento. Revisa antes de importarlo como nuevo."
                        : "Existe un estudiante con datos de nombre coincidentes. Revisa antes de importarlo como nuevo.",
                    SeveridadProblemaImportacion.Revision));
        }
    }

    private static void AplicarValidacionDominio(
        FilaImportacionEstudiante[] filas,
        Grupo grupo)
    {
        var prueba = ClonarGrupo(grupo);
        foreach (var fila in filas.Where(fila => fila.Estado == EstadoFilaImportacion.Lista))
        {
            try
            {
                prueba.AgregarEstudiante(
                    fila.NombreVisible,
                    fila.NumeroLista!.Value,
                    fila.PrimerApellido,
                    fila.SegundoApellido,
                    fila.Nombres,
                    fila.FechaNacimiento,
                    fila.Genero,
                    fila.FechaIngreso,
                    fila.Observaciones,
                    fila.Grado,
                    preservarNombreVisible: !string.IsNullOrWhiteSpace(fila.NombreCompleto));
            }
            catch (Exception exception) when (exception is DomainValidationException or DomainConflictException)
            {
                AgregarProblema(
                    filas,
                    fila,
                    new ProblemaImportacion(
                        null,
                        "domain-validation",
                        exception.Message,
                        SeveridadProblemaImportacion.Invalido));
            }
        }
    }

    private static bool EsDuplicadoProbable(
        FilaImportacionEstudiante fila,
        Estudiante estudiante)
    {
        if (NormalizarIdentidad(fila.NombreVisible) == NormalizarIdentidad(estudiante.NombreVisible))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(fila.PrimerApellido) || string.IsNullOrWhiteSpace(fila.Nombres))
        {
            return false;
        }

        return NormalizarIdentidad(fila.PrimerApellido) == NormalizarIdentidad(estudiante.PrimerApellido) &&
               NormalizarIdentidad(fila.SegundoApellido) == NormalizarIdentidad(estudiante.SegundoApellido) &&
               NormalizarIdentidad(fila.Nombres) == NormalizarIdentidad(estudiante.Nombres);
    }

    private static void AgregarProblema(
        FilaImportacionEstudiante[] filas,
        FilaImportacionEstudiante fila,
        ProblemaImportacion problema)
    {
        var indice = Array.FindIndex(filas, candidata => ReferenceEquals(candidata, fila));
        if (indice < 0)
        {
            return;
        }

        var problemas = fila.Problemas.Append(problema).ToArray();
        filas[indice] = fila with
        {
            Problemas = problemas,
            Estado = CalcularEstado(problemas),
        };
    }

    private static EstadoFilaImportacion CalcularEstado(IEnumerable<ProblemaImportacion> problemas)
    {
        var snapshot = problemas.ToArray();
        if (snapshot.Any(problema => problema.Severidad == SeveridadProblemaImportacion.Invalido))
        {
            return EstadoFilaImportacion.Invalida;
        }

        return snapshot.Any(problema => problema.Severidad == SeveridadProblemaImportacion.Revision)
            ? EstadoFilaImportacion.RequiereRevision
            : EstadoFilaImportacion.Lista;
    }

    private static bool PuedeRecibirProblemas(FilaImportacionEstudiante fila) =>
        fila.Estado != EstadoFilaImportacion.Excluida;

    private static Grupo ClonarGrupo(Grupo grupo) =>
        Grupo.Rehidratar(
            grupo.Id,
            grupo.NombreVisible,
            grupo.Estudiantes.Select(estudiante => new DatosEstudianteRehidratado(
                estudiante.Id,
                estudiante.NombreVisible,
                estudiante.PrimerApellido,
                estudiante.SegundoApellido,
                estudiante.Nombres,
                estudiante.FechaNacimiento,
                estudiante.Genero,
                estudiante.FechaIngreso,
                estudiante.Observaciones,
                estudiante.NumeroLista,
                estudiante.EstaActivo,
                estudiante.Grado)).ToArray());

    private Grupo CargarGrupo(GrupoId grupoId) =>
        _grupos.Cargar(grupoId)
        ?? throw new GrupoNoEncontradoException($"No existe el grupo {grupoId}.");

    private static void ValidarMapeo(
        HojaTabular hoja,
        IReadOnlyCollection<MapeoColumnaImportacion> mapeo)
    {
        if (mapeo.Any(item => item.IndiceColumna < 0 || item.IndiceColumna >= hoja.Encabezados.Count))
        {
            throw new ArgumentException("El mapeo contiene un índice de columna fuera de rango.", nameof(mapeo));
        }

        var repetidos = mapeo
            .Where(item => item.Campo != CampoImportacionEstudiante.Ignorar)
            .GroupBy(item => item.Campo)
            .Any(grupo => grupo.Count() > 1);
        if (repetidos)
        {
            throw new ArgumentException("Un campo de estudiante no puede mapearse desde más de una columna.", nameof(mapeo));
        }
    }

    private static string NormalizarIdentidad(string valor)
    {
        var descompuesto = valor.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(descompuesto.Length);
        var ultimoFueEspacio = false;

        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(caracter))
            {
                if (!ultimoFueEspacio && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                ultimoFueEspacio = true;
                continue;
            }

            builder.Append(char.ToUpperInvariant(caracter));
            ultimoFueEspacio = false;
        }

        return builder.ToString().Trim();
    }

    private static string NormalizarClave(string valor) =>
        NormalizarIdentidad(valor).ToLowerInvariant();
}