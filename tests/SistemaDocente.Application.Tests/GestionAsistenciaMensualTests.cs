using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionAsistenciaMensualTests
{
    [Theory]
    [InlineData(2026, 8, 3, true)]
    [InlineData(2026, 8, 8, false)]
    [InlineData(2026, 8, 9, false)]
    public void CalendarioPredeterminadoConsideraLunesAViernesLectivos(
        int anio,
        int mes,
        int dia,
        bool esperado)
    {
        var calendario = new CalendarioLectivoLunesAViernes();

        Assert.Equal(esperado, calendario.EsLaborable(new DateOnly(anio, mes, dia)));
    }

    private static readonly int[] DiasDesordenados = [3, 1, 2];

    [Theory]
    [InlineData(2025, 2, 20, 3)]
    [InlineData(2024, 2, 21, 4)]
    [InlineData(2026, 4, 22, 4)]
    [InlineData(2026, 8, 21, 4)]
    public void CargarMesGeneraSoloDiasLectivosYSeparacionesReales(
        int anio,
        int mes,
        int cantidad,
        int separaciones)
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Ana", 1);
        var casos = Crear(grupo, out _);

        var resultado = casos.CargarMes(grupo.Id, anio, mes);

        Assert.Equal(cantidad, resultado.Dias.Count);
        Assert.All(resultado.Dias, x => Assert.True(x.Fecha.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday));
        Assert.All(resultado.Dias, x => Assert.Contains(x.AbreviaturaDiaSemana, "LMMJV"));
        Assert.Equal(separaciones, resultado.Dias.Count(x => x.EsCierreSemana));
        Assert.All(resultado.Dias.Where(x => x.EsCierreSemana), x => Assert.Equal(DayOfWeek.Friday, x.Fecha.DayOfWeek));
        Assert.False(resultado.Dias[^1].EsCierreSemana);
    }

    [Theory]
    [InlineData(2025, 2, 3, 28)]
    [InlineData(2026, 5, 1, 29)]
    [InlineData(2026, 4, 1, 30)]
    public void RespetaPrimerYUltimoDiaLectivoReal(int anio, int mes, int primero, int ultimo)
    {
        var grupo = Grupo.Crear("Primero A");
        var resultado = Crear(grupo, out _).CargarMes(grupo.Id, anio, mes);

        Assert.Equal(primero, resultado.Dias[0].NumeroDia);
        Assert.Equal(ultimo, resultado.Dias[^1].NumeroDia);
    }

    [Fact]
    public void CargarMesUneActivosEHistoricosYCalculaPorcentaje()
    {
        var grupo = Grupo.Crear("Primero A");
        var activo = grupo.AgregarEstudiante("Activo", 2);
        var historico = grupo.AgregarEstudiante("Histórico", 1);
        var almacenamiento = new AsistenciasDoble();
        almacenamiento.Dias.Add(AsistenciaDiaria.Crear(
            grupo.Id,
            new DateOnly(2026, 8, 3),
            [new(historico.Id, EstadoAsistencia.Retardo)]));
        grupo.DesactivarEstudiante(historico.Id);
        var casos = new GestionAsistenciaCasosUso(
            new GruposDoble(grupo), almacenamiento);

        var resultado = casos.CargarMes(grupo.Id, 2026, 8);

        Assert.Equal([historico.Id, activo.Id], resultado.Estudiantes.Select(x => x.EstudianteId));
        var fila = resultado.Estudiantes[0];
        Assert.False(fila.EstaActivoActualmente);
        Assert.Equal(100d, fila.PorcentajeConfirmado);
        Assert.Equal(TipoCeldaAsistencia.NoAplicable, fila.Estados[3].Tipo);
        Assert.Equal(TipoCeldaAsistencia.Borrador, resultado.Estudiantes[1].Estados[3].Tipo);
        Assert.False(almacenamiento.Guardo);
    }

    [Fact]
    public void PorcentajeAusenteSinDiasConfirmados()
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Ana", 1);
        var resultado = Crear(grupo, out _).CargarMes(grupo.Id, 2026, 8);

        Assert.Null(Assert.Single(resultado.Estudiantes).PorcentajeConfirmado);
    }

    [Fact]
    public void RegistroPersistidoEnFinDeSemanaNoParticipaEnConteosNiPorcentaje()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var almacenamiento = new AsistenciasDoble();
        almacenamiento.Dias.Add(AsistenciaDiaria.Crear(
            grupo.Id,
            new DateOnly(2026, 8, 8),
            [new(estudiante.Id, EstadoAsistencia.Presente)]));
        var casos = new GestionAsistenciaCasosUso(new GruposDoble(grupo), almacenamiento);

        var fila = Assert.Single(casos.CargarMes(grupo.Id, 2026, 8).Estudiantes);

        Assert.Equal(0, fila.Presentes);
        Assert.Null(fila.PorcentajeConfirmado);
    }

    [Fact]
    public void GuardarMesOrdenaYDetieneAnteFallo()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var almacenamiento = new AsistenciasDoble { FallarEnGuardado = 2 };
        var casos = new GestionAsistenciaCasosUso(new GruposDoble(grupo), almacenamiento);
        var entradas = DiasDesordenados.Select(dia => new EntradaDiaAsistencia(
            new DateOnly(2026, 8, dia),
            [new(estudiante.Id, EstadoAsistencia.Presente)])).ToArray();

        var error = Assert.Throws<GuardadoMesInterrumpidoException>(
            () => casos.GuardarMes(grupo.Id, entradas));

        Assert.Equal(new DateOnly(2026, 8, 1), Assert.Single(error.FechasGuardadas));
        Assert.Equal(new DateOnly(2026, 8, 2), error.FechaFallida);
        Assert.Equal(2, almacenamiento.Intentos);
    }

    [Fact]
    public void ConsultaMaterializaArreglosYNoComparteEstado()
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Ana", 1);
        var casos = Crear(grupo, out _);

        var uno = casos.CargarMes(grupo.Id, 2026, 8);
        var dos = casos.CargarMes(grupo.Id, 2026, 8);

        Assert.IsType<AsistenciaDiaColumnaDetalle[]>(uno.Dias);
        Assert.IsType<AsistenciaEstudianteMesDetalle[]>(uno.Estudiantes);
        Assert.NotSame(uno.Dias, dos.Dias);
    }

    private static GestionAsistenciaCasosUso Crear(Grupo grupo, out AsistenciasDoble asistencias)
    {
        asistencias = new AsistenciasDoble();
        return new(new GruposDoble(grupo), asistencias);
    }

    private sealed class GruposDoble(Grupo grupo) : IAlmacenamientoGrupos
    {
        public Grupo? Cargar(GrupoId grupoId) => grupoId == grupo.Id
            ? Grupo.Rehidratar(
                grupo.Id,
                grupo.NombreVisible,
                grupo.Estudiantes.Select(x => new DatosEstudianteRehidratado(
                    x.Id, x.NombreVisible, x.NumeroLista, x.EstaActivo)).ToArray())
            : null;
        public bool Existe(GrupoId grupoId) => grupoId == grupo.Id;
        public void Guardar(Grupo valor) => throw new NotSupportedException();
        public IReadOnlyList<Grupo> ListarTodos() => [grupo];
    }

    private sealed class AsistenciasDoble : IAlmacenamientoAsistencias
    {
        internal List<AsistenciaDiaria> Dias { get; } = [];
        internal int? FallarEnGuardado { get; set; }
        internal int Intentos { get; private set; }
        internal bool Guardo => Intentos > 0;

        public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha) =>
            Dias.SingleOrDefault(x => x.GrupoId == grupoId && x.Fecha == fecha);
        public bool Existe(GrupoId grupoId, DateOnly fecha) => Cargar(grupoId, fecha) is not null;
        public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(
            GrupoId grupoId, DateOnly desde, DateOnly hasta) =>
            Dias.Where(x => x.GrupoId == grupoId && x.Fecha >= desde && x.Fecha <= hasta).ToArray();
        public void Guardar(AsistenciaDiaria asistencia)
        {
            Intentos++;
            if (FallarEnGuardado == Intentos)
            {
                throw new ErrorPersistenciaAplicacionException("fallo", new IOException());
            }

            Dias.RemoveAll(x => x.GrupoId == asistencia.GrupoId && x.Fecha == asistencia.Fecha);
            Dias.Add(asistencia);
        }
    }
}