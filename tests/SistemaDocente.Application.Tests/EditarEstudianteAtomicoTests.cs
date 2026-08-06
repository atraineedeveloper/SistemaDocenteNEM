using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class EditarEstudianteAtomicoTests
{
    [Fact]
    public void CambiosValidosCarganYGuardanUnaVezConIdentidadesEstables()
    {
        var escenario = Escenario.Crear();

        var resultado = escenario.CasosUso.EditarEstudiante(
            escenario.GrupoId,
            escenario.EstudianteId,
            "Ana María",
            7);

        Assert.Equal(1, escenario.Almacenamiento.Cargas);
        Assert.Equal(1, escenario.Almacenamiento.Guardados);
        Assert.Equal(escenario.EstudianteId, resultado.EstudianteId);
        Assert.Equal("Ana María", resultado.NombreVisible);
        Assert.Equal(7, resultado.NumeroLista);
        var persistido = escenario.Almacenamiento.Cargar(escenario.GrupoId)!;
        Assert.Equal(escenario.GrupoId, persistido.Id);
        var estudiante = persistido.Estudiantes.Single(x => x.Id == escenario.EstudianteId);
        Assert.Equal("Ana María", estudiante.NombreVisible);
        Assert.Equal(7, estudiante.NumeroLista);
    }

    [Fact]
    public void NombreInvalidoNoGuardaYConservaNumeroPersistido()
    {
        var escenario = Escenario.Crear();

        Assert.Throws<DomainValidationException>(() => escenario.CasosUso.EditarEstudiante(
            escenario.GrupoId,
            escenario.EstudianteId,
            " ",
            9));

        Assert.Equal(0, escenario.Almacenamiento.Guardados);
        var persistido = escenario.Almacenamiento.Cargar(escenario.GrupoId)!;
        var estudiante = persistido.Estudiantes.Single(x => x.Id == escenario.EstudianteId);
        Assert.Equal("Ana", estudiante.NombreVisible);
        Assert.Equal(1, estudiante.NumeroLista);
    }

    [Theory]
    [InlineData(0, typeof(DomainValidationException))]
    [InlineData(2, typeof(DomainConflictException))]
    public void NumeroRechazadoDespuesDeNombreValidoNoGuarda(
        int numero,
        Type tipoExcepcion)
    {
        var escenario = Escenario.Crear(conSegundoEstudiante: true);

        var error = Record.Exception(() => escenario.CasosUso.EditarEstudiante(
            escenario.GrupoId,
            escenario.EstudianteId,
            "Nombre temporal",
            numero));

        Assert.IsType(tipoExcepcion, error);
        Assert.Equal(0, escenario.Almacenamiento.Guardados);
        var persistido = escenario.Almacenamiento.Cargar(escenario.GrupoId)!;
        var estudiante = persistido.Estudiantes.Single(x => x.Id == escenario.EstudianteId);
        Assert.Equal("Ana", estudiante.NombreVisible);
        Assert.Equal(1, estudiante.NumeroLista);
    }

    [Fact]
    public void FalloDeGuardadoNoDevuelveExitoYCargaPosteriorVeEstadoAnterior()
    {
        var escenario = Escenario.Crear();
        escenario.Almacenamiento.FallarGuardado = true;

        Assert.Throws<ErrorPersistenciaAplicacionException>(() => escenario.CasosUso.EditarEstudiante(
            escenario.GrupoId,
            escenario.EstudianteId,
            "No persistido",
            8));

        escenario.Almacenamiento.FallarGuardado = false;
        var posterior = escenario.CasosUso.CargarGrupo(escenario.GrupoId);
        var estudiante = posterior.Estudiantes.Single(x => x.EstudianteId == escenario.EstudianteId);
        Assert.Equal("Ana", estudiante.NombreVisible);
        Assert.Equal(1, estudiante.NumeroLista);
        Assert.Equal(2, escenario.Almacenamiento.Cargas);
        Assert.Equal(1, escenario.Almacenamiento.Guardados);
    }

    [Fact]
    public void GrupoInexistenteNoGuarda()
    {
        var almacenamiento = new AlmacenamientoDoble();
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        Assert.Throws<GrupoNoEncontradoException>(() => casosUso.EditarEstudiante(
            GrupoId.DesdeGuid(Guid.NewGuid()),
            EstudianteId.DesdeGuid(Guid.NewGuid()),
            "Ana",
            1));

        Assert.Equal(1, almacenamiento.Cargas);
        Assert.Equal(0, almacenamiento.Guardados);
    }

    private sealed record Escenario(
        GestionGrupoCasosUso CasosUso,
        AlmacenamientoDoble Almacenamiento,
        GrupoId GrupoId,
        EstudianteId EstudianteId)
    {
        internal static Escenario Crear(bool conSegundoEstudiante = false)
        {
            var grupo = Grupo.Crear("Primero A");
            var estudiante = grupo.AgregarEstudiante("Ana", 1);
            if (conSegundoEstudiante)
            {
                grupo.AgregarEstudiante("Beto", 2);
            }

            var almacenamiento = new AlmacenamientoDoble(grupo);
            return new(new GestionGrupoCasosUso(almacenamiento), almacenamiento, grupo.Id, estudiante.Id);
        }
    }

    private sealed class AlmacenamientoDoble : IAlmacenamientoGrupos
    {
        private readonly Dictionary<GrupoId, SnapshotGrupo> _persistidos = [];

        internal AlmacenamientoDoble(params Grupo[] grupos)
        {
            foreach (var grupo in grupos)
            {
                _persistidos[grupo.Id] = SnapshotGrupo.Desde(grupo);
            }
        }

        internal int Cargas { get; private set; }
        internal int Guardados { get; private set; }

        public IReadOnlyList<Grupo> ListarTodos() => _persistidos.Values.Select(s => s.Rehidratar()).ToList();
        internal bool FallarGuardado { get; set; }

        public Grupo? Cargar(GrupoId grupoId)
        {
            Cargas++;
            return _persistidos.TryGetValue(grupoId, out var snapshot) ? snapshot.Rehidratar() : null;
        }

        public bool Existe(GrupoId grupoId) => _persistidos.ContainsKey(grupoId);

        public void Guardar(Grupo grupo)
        {
            Guardados++;
            if (FallarGuardado)
            {
                throw new ErrorPersistenciaAplicacionException("fallo", new IOException());
            }

            _persistidos[grupo.Id] = SnapshotGrupo.Desde(grupo);
        }
    }

    private sealed record SnapshotGrupo(
        GrupoId Id,
        string Nombre,
        DatosEstudianteRehidratado[] Estudiantes)
    {
        internal static SnapshotGrupo Desde(Grupo grupo) => new(
            grupo.Id,
            grupo.NombreVisible,
            grupo.Estudiantes.Select(x => new DatosEstudianteRehidratado(
                x.Id,
                x.NombreVisible,
                x.NumeroLista,
                x.EstaActivo)).ToArray());

        internal Grupo Rehidratar() => Grupo.Rehidratar(Id, Nombre, [.. Estudiantes]);
    }
}