using System.Reflection;
using System.Runtime.CompilerServices;

using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Application.Tests;

public sealed class GestionGrupoCasosUsoTests
{
    [Fact]
    public void CrearGrupoGuardaUnaVezYDevuelveDetalle()
    {
        var almacenamiento = new AlmacenamientoDoble();
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        var resultado = casosUso.CrearGrupo("  Primero   A  ");

        Assert.NotEqual(default, resultado.GrupoId);
        Assert.Equal("Primero A", resultado.NombreVisible);
        Assert.Empty(resultado.Estudiantes);
        Assert.Equal(1, almacenamiento.Guardados);
    }

    [Fact]
    public void CrearGrupoInvalidoNoGuarda()
    {
        var almacenamiento = new AlmacenamientoDoble();
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        Assert.Throws<DomainValidationException>(() => casosUso.CrearGrupo("   "));
        Assert.Equal(0, almacenamiento.Guardados);
    }

    [Fact]
    public void CargarYExisteNoGuardanYDistinguenAusencia()
    {
        var grupo = Grupo.Crear("Primero A");
        var almacenamiento = new AlmacenamientoDoble(grupo);
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        var cargado = casosUso.CargarGrupo(grupo.Id);

        Assert.Equal(grupo.Id, cargado.GrupoId);
        Assert.True(casosUso.Existe(grupo.Id));
        Assert.False(casosUso.Existe(GrupoId.DesdeGuid(Guid.NewGuid())));
        Assert.Throws<GrupoNoEncontradoException>(
            () => casosUso.CargarGrupo(GrupoId.DesdeGuid(Guid.NewGuid())));
        Assert.Equal(0, almacenamiento.Guardados);
    }

    [Fact]
    public void ComandosDeGrupoYEstudianteGuardanUnaVezYConservanIdentidades()
    {
        var grupo = Grupo.Crear("Primero A");
        var almacenamiento = new AlmacenamientoDoble(grupo);
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        var grupoRenombrado = casosUso.CambiarNombreGrupo(grupo.Id, "Segundo B");
        Assert.Equal(grupo.Id, grupoRenombrado.GrupoId);
        Assert.Equal(1, almacenamiento.Guardados);

        var agregado = casosUso.AgregarEstudiante(grupo.Id, "Ana", 1);
        Assert.NotEqual(default, agregado.EstudianteId);
        Assert.Equal(2, almacenamiento.Guardados);

        var renombrado = casosUso.RenombrarEstudiante(grupo.Id, agregado.EstudianteId, "Ana María");
        var renumerado = casosUso.CambiarNumeroLista(grupo.Id, agregado.EstudianteId, 7);
        var desactivado = casosUso.DesactivarEstudiante(grupo.Id, agregado.EstudianteId);
        var reactivado = casosUso.ReactivarEstudiante(grupo.Id, agregado.EstudianteId);

        Assert.All(
            [renombrado, renumerado, desactivado, reactivado],
            detalle => Assert.Equal(agregado.EstudianteId, detalle.EstudianteId));
        Assert.Equal("Ana María", renombrado.NombreVisible);
        Assert.Equal(7, renumerado.NumeroLista);
        Assert.False(desactivado.EstaActivo);
        Assert.True(reactivado.EstaActivo);
        Assert.Equal(6, almacenamiento.Guardados);
    }

    [Fact]
    public void ComandosIdempotentesGuardanUnaVezCadaUno()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var almacenamiento = new AlmacenamientoDoble(grupo);
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        casosUso.ReactivarEstudiante(grupo.Id, estudiante.Id);
        casosUso.DesactivarEstudiante(grupo.Id, estudiante.Id);
        casosUso.DesactivarEstudiante(grupo.Id, estudiante.Id);

        Assert.Equal(3, almacenamiento.Guardados);
    }

    [Fact]
    public void ErrorDeDominioEnCadaFamiliaDeComandoNoGuarda()
    {
        var grupo = Grupo.Crear("Primero A");
        var estudiante = grupo.AgregarEstudiante("Ana", 1);
        var almacenamiento = new AlmacenamientoDoble(grupo);
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        Assert.Throws<DomainValidationException>(() => casosUso.CambiarNombreGrupo(grupo.Id, " "));
        Assert.Throws<DomainConflictException>(() => casosUso.AgregarEstudiante(grupo.Id, "Luis", 1));
        Assert.Throws<DomainValidationException>(
            () => casosUso.RenombrarEstudiante(grupo.Id, estudiante.Id, " "));
        Assert.Throws<DomainValidationException>(
            () => casosUso.CambiarNumeroLista(grupo.Id, estudiante.Id, 0));
        Assert.Throws<DomainConflictException>(
            () => casosUso.DesactivarEstudiante(grupo.Id, EstudianteId.DesdeGuid(Guid.NewGuid())));

        Assert.Equal(0, almacenamiento.Guardados);
    }

    [Fact]
    public void ConsultasOrdenanMaterializanYNoGuardan()
    {
        var grupoId = GrupoId.DesdeGuid(Guid.Parse("10000000-0000-0000-0000-000000000000"));
        var idMayor = EstudianteId.DesdeGuid(Guid.Parse("30000000-0000-0000-0000-000000000000"));
        var idMenor = EstudianteId.DesdeGuid(Guid.Parse("20000000-0000-0000-0000-000000000000"));
        var grupo = Grupo.Rehidratar(
            grupoId,
            "Primero A",
            [
                new(idMayor, "Ana", 2, false),
                new(EstudianteId.DesdeGuid(Guid.Parse("40000000-0000-0000-0000-000000000000")), "Beto", 1, true),
                new(idMenor, "Ana", 2, false),
            ]);
        var almacenamiento = new AlmacenamientoDoble(grupo);
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        var todos = casosUso.ObtenerTodosLosEstudiantes(grupo.Id);
        var todosOtraVez = casosUso.ObtenerTodosLosEstudiantes(grupo.Id);
        var activos = casosUso.ObtenerEstudiantesActivos(grupo.Id);

        Assert.IsType<EstudianteDetalle[]>(todos);
        Assert.NotSame(todos, todosOtraVez);
        Assert.Equal([1, 2, 2], todos.Select(x => x.NumeroLista));
        Assert.Equal(idMenor, todos[1].EstudianteId);
        Assert.Equal(idMayor, todos[2].EstudianteId);
        Assert.Single(activos);
        Assert.Equal(0, almacenamiento.Guardados);
    }

    [Fact]
    public void GrupoDetalleContieneUnaMatrizNueva()
    {
        var grupo = Grupo.Crear("Primero A");
        grupo.AgregarEstudiante("Ana", 1);
        var casosUso = new GestionGrupoCasosUso(new AlmacenamientoDoble(grupo));

        var primero = casosUso.CargarGrupo(grupo.Id);
        var segundo = casosUso.CargarGrupo(grupo.Id);

        Assert.IsType<EstudianteDetalle[]>(primero.Estudiantes);
        Assert.NotSame(primero.Estudiantes, segundo.Estudiantes);
        Assert.True(EsRecordInmutable(typeof(GrupoDetalle)));
        Assert.True(EsRecordInmutable(typeof(EstudianteDetalle)));
        Assert.DoesNotContain(
            typeof(GrupoDetalle).GetProperties(),
            propiedad => propiedad.PropertyType == typeof(Grupo));
        Assert.DoesNotContain(
            typeof(EstudianteDetalle).GetProperties(),
            propiedad => propiedad.PropertyType == typeof(Estudiante));
    }

    [Fact]
    public void AusenciaAlModificarProduceErrorYNoGuarda()
    {
        var almacenamiento = new AlmacenamientoDoble();
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        Assert.Throws<GrupoNoEncontradoException>(
            () => casosUso.CambiarNombreGrupo(GrupoId.DesdeGuid(Guid.NewGuid()), "Nombre"));
        Assert.Equal(0, almacenamiento.Guardados);
    }

    [Fact]
    public void ErrorPersistenciaSePropagaSinNuevaEnvoltura()
    {
        var causa = new IOException("fallo");
        var error = new ErrorPersistenciaAplicacionException("persistencia", causa);
        var almacenamiento = new AlmacenamientoDoble { ErrorAlCargar = error };
        var casosUso = new GestionGrupoCasosUso(almacenamiento);
        var grupoId = GrupoId.DesdeGuid(Guid.NewGuid());

        var recibido = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => casosUso.CargarGrupo(grupoId));

        Assert.Same(error, recibido);
        Assert.Same(causa, recibido.InnerException);
    }

    [Fact]
    public void ErrorEnExisteNoSeConvierteEnAusencia()
    {
        var error = new ErrorPersistenciaAplicacionException("persistencia", new IOException());
        var almacenamiento = new AlmacenamientoDoble { ErrorAlExistir = error };
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        var recibido = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => casosUso.Existe(GrupoId.DesdeGuid(Guid.NewGuid())));

        Assert.Same(error, recibido);
    }

    [Fact]
    public void FalloAlGuardarDescartaInstanciaModificada()
    {
        var grupo = Grupo.Crear("Anterior");
        var almacenamiento = new AlmacenamientoDoble(grupo) { FallarSiguienteGuardado = true };
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => casosUso.CambiarNombreGrupo(grupo.Id, "Modificado"));

        var posterior = casosUso.CargarGrupo(grupo.Id);
        Assert.Equal("Anterior", posterior.NombreVisible);
        Assert.Equal(2, almacenamiento.Cargas);
    }

    [Fact]
    public void CadaComandoPropagaElFalloDeGuardadoSinDevolverExito()
    {
        var grupoBase = Grupo.Crear("Primero A");
        var estudiante = grupoBase.AgregarEstudiante("Ana", 1);
        Action<GestionGrupoCasosUso>[] comandos =
        [
            casosUso => casosUso.CambiarNombreGrupo(grupoBase.Id, "Segundo B"),
            casosUso => casosUso.AgregarEstudiante(grupoBase.Id, "Luis", 2),
            casosUso => casosUso.RenombrarEstudiante(grupoBase.Id, estudiante.Id, "Ana María"),
            casosUso => casosUso.CambiarNumeroLista(grupoBase.Id, estudiante.Id, 2),
            casosUso => casosUso.DesactivarEstudiante(grupoBase.Id, estudiante.Id),
            casosUso => casosUso.ReactivarEstudiante(grupoBase.Id, estudiante.Id),
        ];

        foreach (var comando in comandos)
        {
            var error = new ErrorPersistenciaAplicacionException("fallo", new IOException());
            var almacenamiento = new AlmacenamientoDoble(grupoBase)
            {
                ErrorAlGuardar = error,
            };
            var casosUso = new GestionGrupoCasosUso(almacenamiento);

            var recibido = Assert.Throws<ErrorPersistenciaAplicacionException>(
                () => comando(casosUso));

            Assert.Same(error, recibido);
            Assert.Equal(1, almacenamiento.Guardados);
        }
    }

    [Fact]
    public void CrearPropagaElFalloDeGuardadoSinNuevaEnvoltura()
    {
        var error = new ErrorPersistenciaAplicacionException("fallo", new IOException());
        var almacenamiento = new AlmacenamientoDoble { ErrorAlGuardar = error };
        var casosUso = new GestionGrupoCasosUso(almacenamiento);

        var recibido = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => casosUso.CrearGrupo("Primero A"));

        Assert.Same(error, recibido);
        Assert.Equal(1, almacenamiento.Guardados);
    }

    private static bool EsRecordInmutable(Type type) =>
        type.GetProperties().All(
            propiedad => propiedad.SetMethod is null
                || propiedad.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                    .Contains(typeof(IsExternalInit)));

    private sealed class AlmacenamientoDoble : IAlmacenamientoGrupos
    {
        private readonly Dictionary<GrupoId, SnapshotGrupo> _grupos = [];

        internal AlmacenamientoDoble(params Grupo[] grupos)
        {
            foreach (var grupo in grupos)
            {
                _grupos[grupo.Id] = SnapshotGrupo.Desde(grupo);
            }
        }

        internal int Cargas { get; private set; }

        internal int Guardados { get; private set; }

        internal bool FallarSiguienteGuardado { get; set; }

        internal ErrorPersistenciaAplicacionException? ErrorAlGuardar { get; set; }

        internal ErrorPersistenciaAplicacionException? ErrorAlCargar { get; set; }

        internal ErrorPersistenciaAplicacionException? ErrorAlExistir { get; set; }

        public IReadOnlyList<Grupo> ListarTodos() =>
            _grupos.Values.Select(s => s.Rehidratar()).ToList();

        public Grupo? Cargar(GrupoId grupoId)
        {
            Cargas++;
            if (ErrorAlCargar is not null)
            {
                throw ErrorAlCargar;
            }

            return _grupos.TryGetValue(grupoId, out var snapshot) ? snapshot.Rehidratar() : null;
        }

        public bool Existe(GrupoId grupoId)
        {
            if (ErrorAlExistir is not null)
            {
                throw ErrorAlExistir;
            }

            return _grupos.ContainsKey(grupoId);
        }

        public void Guardar(Grupo grupo)
        {
            Guardados++;
            if (ErrorAlGuardar is not null)
            {
                throw ErrorAlGuardar;
            }

            if (FallarSiguienteGuardado)
            {
                FallarSiguienteGuardado = false;
                throw new ErrorPersistenciaAplicacionException("fallo", new IOException());
            }

            _grupos[grupo.Id] = SnapshotGrupo.Desde(grupo);
        }
    }

    private sealed record SnapshotGrupo(
        GrupoId Id,
        string Nombre,
        DatosEstudianteRehidratado[] Estudiantes)
    {
        internal static SnapshotGrupo Desde(Grupo grupo) =>
            new(
                grupo.Id,
                grupo.NombreVisible,
                grupo.Estudiantes
                    .Select(x => new DatosEstudianteRehidratado(
                        x.Id,
                        x.NombreVisible,
                        x.NumeroLista,
                        x.EstaActivo))
                    .ToArray());

        internal Grupo Rehidratar() => Grupo.Rehidratar(Id, Nombre, [.. Estudiantes]);
    }
}