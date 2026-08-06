using System.Runtime.CompilerServices;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionAsistenciaCasosUsoTests
{
    private static readonly DateOnly Fecha = new(2026, 8, 3);

    [Fact]
    public void PrepararDiaAusenteIncluyeSoloActivosPresentesSinGuardar()
    {
        var grupo = Grupo.Crear("Primero A");
        var activo = grupo.AgregarEstudiante("Ana", 2);
        var inactivo = grupo.AgregarEstudiante("Beto", 1);
        grupo.DesactivarEstudiante(inactivo.Id);
        var grupos = new GruposDoble(grupo);
        var asistencias = new AsistenciasDoble();
        var casosUso = new GestionAsistenciaCasosUso(grupos, asistencias);

        var resultado = casosUso.Preparar(grupo.Id, Fecha);

        Assert.False(resultado.EsPersistido);
        var fila = Assert.Single(resultado.Estudiantes);
        Assert.Equal(activo.Id, fila.EstudianteId);
        Assert.Equal(EstadoAsistencia.Presente, fila.Estado);
        Assert.Equal(0, asistencias.Guardados);
        Assert.Null(casosUso.Cargar(grupo.Id, Fecha));
        Assert.False(casosUso.Existe(grupo.Id, Fecha));
    }

    [Fact]
    public void DiaHistoricoIncluyeInactivoYNoAgregaEstudiantePosterior()
    {
        var grupo = Grupo.Crear("Primero A");
        var historico = grupo.AgregarEstudiante("Ana", 1);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(historico.Id, EstadoAsistencia.Falta)]);
        grupo.DesactivarEstudiante(historico.Id);
        var nuevo = grupo.AgregarEstudiante("Nuevo", 2);
        var casosUso = new GestionAsistenciaCasosUso(
            new GruposDoble(grupo),
            new AsistenciasDoble(asistencia));

        var resultado = casosUso.Preparar(grupo.Id, Fecha);

        var fila = Assert.Single(resultado.Estudiantes);
        Assert.Equal(historico.Id, fila.EstudianteId);
        Assert.False(fila.EstaActivoActualmente);
        Assert.DoesNotContain(resultado.Estudiantes, x => x.EstudianteId == nuevo.Id);
    }

    [Fact]
    public void ProyeccionOrdenaMaterializaYConservaIdentidades()
    {
        var grupoId = GrupoId.DesdeGuid(Guid.Parse("10000000-0000-0000-0000-000000000000"));
        var menor = EstudianteId.DesdeGuid(Guid.Parse("20000000-0000-0000-0000-000000000000"));
        var mayor = EstudianteId.DesdeGuid(Guid.Parse("30000000-0000-0000-0000-000000000000"));
        var grupo = Grupo.Rehidratar(
            grupoId,
            "Primero A",
            [new(mayor, "Ana", 1, false), new(menor, "Ana", 1, false)]);
        var asistencia = AsistenciaDiaria.Crear(
            grupoId,
            Fecha,
            [new(mayor, EstadoAsistencia.Retardo), new(menor, EstadoAsistencia.Justificada)]);
        var casosUso = new GestionAsistenciaCasosUso(
            new GruposDoble(grupo),
            new AsistenciasDoble(asistencia));

        var primero = casosUso.Preparar(grupoId, Fecha);
        var segundo = casosUso.Preparar(grupoId, Fecha);

        Assert.IsType<AsistenciaEstudianteDetalle[]>(primero.Estudiantes);
        Assert.NotSame(primero.Estudiantes, segundo.Estudiantes);
        Assert.Equal([menor, mayor], primero.Estudiantes.Select(x => x.EstudianteId));
        Assert.True(EsRecordInmutable(typeof(AsistenciaDiaDetalle)));
        Assert.True(EsRecordInmutable(typeof(AsistenciaEstudianteDetalle)));
    }

    [Fact]
    public void GuardarDiaNuevoExigePadronExactoYGuardaUnaVez()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var bea = grupo.AgregarEstudiante("Bea", 2);
        var asistencias = new AsistenciasDoble();
        var casosUso = new GestionAsistenciaCasosUso(new GruposDoble(grupo), asistencias);

        var resultado = casosUso.Guardar(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Falta), new(bea.Id, EstadoAsistencia.Presente)]);

        Assert.True(resultado.EsPersistido);
        Assert.Equal(1, asistencias.Guardados);
        Assert.Equal(EstadoAsistencia.Falta, asistencias.Cargar(grupo.Id, Fecha)!.Registros[0].Estado);
    }

    [Fact]
    public void EntradasFaltantesDuplicadasAjenasOInvalidasNoGuardan()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var ajeno = Grupo.Crear("Otro").AgregarEstudiante("Ajeno", 1);
        IReadOnlyCollection<EntradaEstadoAsistencia>[] entradasInvalidas =
        [
            [],
            [new(ana.Id, EstadoAsistencia.Presente), new(ana.Id, EstadoAsistencia.Falta)],
            [new(ajeno.Id, EstadoAsistencia.Presente)],
            [new(ana.Id, (EstadoAsistencia)99)],
        ];

        foreach (var entradas in entradasInvalidas)
        {
            var asistencias = new AsistenciasDoble();
            var casosUso = new GestionAsistenciaCasosUso(new GruposDoble(grupo), asistencias);
            Assert.ThrowsAny<Exception>(() => casosUso.Guardar(grupo.Id, Fecha, entradas));
            Assert.Equal(0, asistencias.Guardados);
        }
    }

    [Fact]
    public void ActualizarHistoricoEditaInactivoYGuardaUnaVezSinEliminar()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var bea = grupo.AgregarEstudiante("Bea", 2);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Presente), new(bea.Id, EstadoAsistencia.Presente)]);
        grupo.DesactivarEstudiante(bea.Id);
        var asistencias = new AsistenciasDoble(asistencia);
        var casosUso = new GestionAsistenciaCasosUso(new GruposDoble(grupo), asistencias);

        var resultado = casosUso.Guardar(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Retardo), new(bea.Id, EstadoAsistencia.Justificada)]);

        Assert.Equal(1, asistencias.Guardados);
        Assert.Equal(2, resultado.Estudiantes.Count);
        Assert.Equal(
            EstadoAsistencia.Justificada,
            resultado.Estudiantes.Single(x => x.EstudianteId == bea.Id).Estado);
    }

    [Fact]
    public void FalloDePersistenciaNoComparteInstanciaYRecargaEstadoAnterior()
    {
        var grupo = Grupo.Crear("Primero A");
        var ana = grupo.AgregarEstudiante("Ana", 1);
        var asistencia = AsistenciaDiaria.Crear(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Presente)]);
        var error = new ErrorPersistenciaAplicacionException("fallo", new IOException("causa"));
        var asistencias = new AsistenciasDoble(asistencia) { ErrorAlGuardar = error };
        var casosUso = new GestionAsistenciaCasosUso(new GruposDoble(grupo), asistencias);

        var recibido = Assert.Throws<ErrorPersistenciaAplicacionException>(() => casosUso.Guardar(
            grupo.Id,
            Fecha,
            [new(ana.Id, EstadoAsistencia.Falta)]));
        asistencias.ErrorAlGuardar = null;
        var posterior = casosUso.Cargar(grupo.Id, Fecha)!;

        Assert.Same(error, recibido);
        Assert.Equal(EstadoAsistencia.Presente, posterior.Estudiantes[0].Estado);
    }

    [Fact]
    public void GrupoAusenteProduceErrorYExisteNoConvierteFalloEnAusencia()
    {
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        var asistencias = new AsistenciasDoble();
        var casosUso = new GestionAsistenciaCasosUso(new GruposDoble(), asistencias);

        Assert.Throws<GrupoNoEncontradoException>(() => casosUso.Preparar(grupoId, Fecha));
        var error = new ErrorPersistenciaAplicacionException("fallo", new IOException());
        asistencias.ErrorAlExistir = error;
        Assert.Same(
            error,
            Assert.Throws<ErrorPersistenciaAplicacionException>(
                () => casosUso.Existe(grupoId, Fecha)));
    }

    [Fact]
    public void ApplicationNoReferenciaInfraestructuraNiPresentacion()
    {
        var referencias = typeof(GestionAsistenciaCasosUso).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToArray();

        Assert.DoesNotContain("SistemaDocente.Data", referencias);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", referencias);
        Assert.DoesNotContain("SistemaDocente.Presentation", referencias);
        Assert.DoesNotContain("PresentationFramework", referencias);
    }

    private static bool EsRecordInmutable(Type type) =>
        type.GetProperties().All(
            propiedad => propiedad.SetMethod is null
                || propiedad.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                    .Contains(typeof(IsExternalInit)));

    private sealed class GruposDoble : IAlmacenamientoGrupos
    {
        private readonly Dictionary<GrupoId, SnapshotGrupo> _datos = [];

        internal GruposDoble(params Grupo[] grupos)
        {
            foreach (var grupo in grupos)
            {
                _datos.Add(grupo.Id, SnapshotGrupo.Desde(grupo));
            }
        }

        public IReadOnlyList<Grupo> ListarTodos() => _datos.Values.Select(s => s.Rehidratar()).ToList();

        public Grupo? Cargar(GrupoId grupoId) =>
            _datos.TryGetValue(grupoId, out var dato) ? dato.Rehidratar() : null;

        public bool Existe(GrupoId grupoId) => _datos.ContainsKey(grupoId);

        public void Guardar(Grupo grupo) => _datos[grupo.Id] = SnapshotGrupo.Desde(grupo);
    }

    private sealed class AsistenciasDoble : IAlmacenamientoAsistencias
    {
        private readonly Dictionary<(GrupoId, DateOnly), SnapshotAsistencia> _datos = [];

        internal AsistenciasDoble(params AsistenciaDiaria[] asistencias)
        {
            foreach (var asistencia in asistencias)
            {
                _datos.Add((asistencia.GrupoId, asistencia.Fecha), SnapshotAsistencia.Desde(asistencia));
            }
        }

        internal int Guardados { get; private set; }

        internal ErrorPersistenciaAplicacionException? ErrorAlGuardar { get; set; }

        internal ErrorPersistenciaAplicacionException? ErrorAlExistir { get; set; }

        public AsistenciaDiaria? Cargar(GrupoId grupoId, DateOnly fecha) =>
            _datos.TryGetValue((grupoId, fecha), out var dato) ? dato.Rehidratar() : null;

        public bool Existe(GrupoId grupoId, DateOnly fecha)
        {
            if (ErrorAlExistir is not null)
            {
                throw ErrorAlExistir;
            }

            return _datos.ContainsKey((grupoId, fecha));
        }

        public IReadOnlyList<AsistenciaDiaria> CargarIntervalo(
            GrupoId grupoId,
            DateOnly desde,
            DateOnly hasta) =>
            _datos
                .Where(x => x.Key.Item1 == grupoId && x.Key.Item2 >= desde && x.Key.Item2 <= hasta)
                .OrderBy(x => x.Key.Item2)
                .Select(x => x.Value.Rehidratar())
                .ToArray();

        public void Guardar(AsistenciaDiaria asistencia)
        {
            Guardados++;
            if (ErrorAlGuardar is not null)
            {
                throw ErrorAlGuardar;
            }

            _datos[(asistencia.GrupoId, asistencia.Fecha)] = SnapshotAsistencia.Desde(asistencia);
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

        internal Grupo Rehidratar() => Grupo.Rehidratar(Id, Nombre, Estudiantes);
    }

    private sealed record SnapshotAsistencia(
        GrupoId GrupoId,
        DateOnly Fecha,
        DatosRegistroAsistenciaRehidratado[] Registros)
    {
        internal static SnapshotAsistencia Desde(AsistenciaDiaria asistencia) => new(
            asistencia.GrupoId,
            asistencia.Fecha,
            asistencia.Registros.Select(x => new DatosRegistroAsistenciaRehidratado(
                x.EstudianteId,
                x.Estado)).ToArray());

        internal AsistenciaDiaria Rehidratar() =>
            AsistenciaDiaria.Rehidratar(GrupoId, Fecha, Registros);
    }
}