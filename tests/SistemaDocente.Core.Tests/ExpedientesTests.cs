using SistemaDocente.Core;

namespace SistemaDocente.Core.Tests;

public sealed class ExpedientesTests
{
    [Fact]
    public void CrearNotaPedagogicaConDatosValidosFunciona()
    {
        var id = Guid.NewGuid();
        var fecha = DateTime.Now;
        var nota = new NotaPedagogica(id, TipoNotaPedagogica.Fortaleza, "Participativo en equipo", fecha);

        Assert.Equal(id, nota.NotaId);
        Assert.Equal(TipoNotaPedagogica.Fortaleza, nota.Tipo);
        Assert.Equal("Participativo en equipo", nota.Contenido);
        Assert.Equal(fecha, nota.FechaHoraRegistro);
    }

    [Fact]
    public void NotaPedagogicaSinContenidoLanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new NotaPedagogica(Guid.NewGuid(), TipoNotaPedagogica.Fortaleza, "   ", DateTime.Now));
    }

    [Fact]
    public void NotaPedagogicaConTipoInvalidoLanzaExcepcion()
    {
        Assert.Throws<DomainValidationException>(() => new NotaPedagogica(Guid.NewGuid(), (TipoNotaPedagogica)99, "Nota pedagógica", DateTime.Now));
    }

    [Theory]
    [InlineData("Diagnóstico de TDAH por el especialista.")]
    [InlineData("Presenta autismo leve en clase.")]
    [InlineData("Requiere receta médica para depresión.")]
    [InlineData("SínDromE neurológico en evaluación.")]
    public void NotaPedagogicaRechazaTerminosClinicosOMedicos(string textoClinico)
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            new NotaPedagogica(Guid.NewGuid(), TipoNotaPedagogica.Dificultad, textoClinico, DateTime.Now));
        Assert.Contains("términos de carácter médico o clínico", ex.Message);
    }

    [Fact]
    public void AcuerdoTutorRechazaFechaSeguimientoAnteriorAReunion()
    {
        var fechaReunion = new DateOnly(2026, 3, 10);
        var fechaSeguimientoAnterior = new DateOnly(2026, 3, 5);

        Assert.Throws<DomainValidationException>(() =>
            new AcuerdoTutor(Guid.NewGuid(), "Faltas continuas", "Asistencia diaria", fechaReunion, fechaSeguimientoAnterior));
    }

    [Fact]
    public void AcuerdoTutorRechazaTerminosClinicos()
    {
        var fechaReunion = new DateOnly(2026, 3, 10);
        Assert.Throws<DomainValidationException>(() =>
            new AcuerdoTutor(Guid.NewGuid(), "Trastorno de conducta", "Tratamiento psiquiátrico", fechaReunion, null));
    }

    [Fact]
    public void ExpedienteEstudianteRealizaCopiaDefensivaDeListas()
    {
        var estId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var nota = new NotaPedagogica(Guid.NewGuid(), TipoNotaPedagogica.Fortaleza, "Fortaleza pedagógica", DateTime.Now);

        var notasMutables = new List<NotaPedagogica> { nota };
        var expediente = new ExpedienteEstudiante(estId, grupoId, notasMutables);

        notasMutables.Clear();
        Assert.Single(expediente.Notas);
    }

    [Fact]
    public void ExpedienteEstudianteOrganizaNotasPorTipo()
    {
        var estId = EstudianteId.DesdeGuid(Guid.NewGuid());
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());

        var n1 = new NotaPedagogica(Guid.NewGuid(), TipoNotaPedagogica.Fortaleza, "Fortaleza 1", DateTime.Now.AddMinutes(-5));
        var n2 = new NotaPedagogica(Guid.NewGuid(), TipoNotaPedagogica.Fortaleza, "Fortaleza 2", DateTime.Now);
        var n3 = new NotaPedagogica(Guid.NewGuid(), TipoNotaPedagogica.Dificultad, "Dificultad 1", DateTime.Now);

        var expediente = new ExpedienteEstudiante(estId, grupoId, [n1, n2, n3]);

        var fortalezas = expediente.ObtenerNotasPorTipo(TipoNotaPedagogica.Fortaleza);
        Assert.Equal(2, fortalezas.Count);
        Assert.Equal("Fortaleza 2", fortalezas[0].Contenido); // Más reciente primero
    }
}