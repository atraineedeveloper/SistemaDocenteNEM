using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class ConfiguracionGrupoWizardTests
{
    [Fact]
    public void ConfiguracionInicialPermiteTodosLosCamposOpcionalesVacios()
    {
        var grupos = new GruposDoble();
        var contextos = new ContextosDoble();
        var grupo = Grupo.Crear("Grupo nuevo");
        grupos.Guardar(grupo);
        var viewModel = CrearViewModel(grupos, contextos);

        viewModel.PrepararNuevoGrupo();
        var guardado = viewModel.GuardarOpcionalParaNuevoGrupo(grupo.Id);

        Assert.True(guardado);
        Assert.NotNull(contextos.Ultimo);
        Assert.Equal(grupo.Id, contextos.Ultimo!.GrupoId);
        Assert.Empty(contextos.Ultimo.GradosAtendidos);
        Assert.Equal(string.Empty, contextos.Ultimo.NombreEscuela);
        Assert.Equal(string.Empty, contextos.Ultimo.EntidadFederativa);
        Assert.Equal(string.Empty, contextos.Ultimo.Municipio);
    }

    [Fact]
    public void ConfiguracionInicialPersisteGradosEscuelaYUbicacionCapturados()
    {
        var grupos = new GruposDoble();
        var contextos = new ContextosDoble();
        var grupo = Grupo.Crear("Quinto B");
        grupos.Guardar(grupo);
        var viewModel = CrearViewModel(grupos, contextos);
        viewModel.PrepararNuevoGrupo();

        viewModel.QuintoGrado = true;
        viewModel.NombreEscuela = "Primaria de prueba";
        viewModel.Cct = "27DPR0000X";
        viewModel.CicloEscolar = "2026-2027";
        viewModel.Turno = "Matutino";
        viewModel.EntidadFederativa = viewModel.EntidadesFederativas.First();
        viewModel.Municipio = viewModel.MunicipiosDisponibles.First();
        viewModel.Localidad = "Localidad de prueba";

        var guardado = viewModel.GuardarOpcionalParaNuevoGrupo(grupo.Id);

        Assert.True(guardado);
        Assert.NotNull(contextos.Ultimo);
        Assert.Equal(new[] { GradoPrimaria.Quinto }, contextos.Ultimo!.GradosAtendidos);
        Assert.Equal("Primaria de prueba", contextos.Ultimo.NombreEscuela);
        Assert.Equal("27DPR0000X", contextos.Ultimo.Cct);
        Assert.Equal("2026-2027", contextos.Ultimo.CicloEscolar);
        Assert.Equal("Matutino", contextos.Ultimo.Turno);
        Assert.Equal(viewModel.EntidadFederativa, contextos.Ultimo.EntidadFederativa);
        Assert.Equal(viewModel.Municipio, contextos.Ultimo.Municipio);
        Assert.Equal("Localidad de prueba", contextos.Ultimo.Localidad);
    }

    [Fact]
    public void PrepararNuevoGrupoLimpiaBorradorAnterior()
    {
        var grupos = new GruposDoble();
        var contextos = new ContextosDoble();
        var viewModel = CrearViewModel(grupos, contextos);
        viewModel.NombreEscuela = "Temporal";
        viewModel.SextoGrado = true;
        viewModel.EntidadFederativa = viewModel.EntidadesFederativas.First();
        viewModel.Municipio = viewModel.MunicipiosDisponibles.First();

        viewModel.PrepararNuevoGrupo();

        Assert.Equal(string.Empty, viewModel.NombreEscuela);
        Assert.Equal(string.Empty, viewModel.EntidadFederativa);
        Assert.Equal(string.Empty, viewModel.Municipio);
        Assert.Empty(viewModel.MunicipiosDisponibles);
        Assert.Empty(viewModel.ObtenerGradosConfigurados());
    }

    private static ConfiguracionGrupoViewModel CrearViewModel(
        IAlmacenamientoGrupos grupos,
        IAlmacenamientoContextoGrupo contextos) =>
        new(new GestionContextoGrupoCasosUso(grupos, contextos));

    private sealed class GruposDoble : IAlmacenamientoGrupos
    {
        private readonly Dictionary<GrupoId, Grupo> _grupos = [];

        public Grupo? Cargar(GrupoId grupoId) =>
            _grupos.TryGetValue(grupoId, out var grupo) ? grupo : null;

        public bool Existe(GrupoId grupoId) => _grupos.ContainsKey(grupoId);

        public void Guardar(Grupo grupo) => _grupos[grupo.Id] = grupo;

        public IReadOnlyList<Grupo> ListarTodos() => _grupos.Values.ToArray();
    }

    private sealed class ContextosDoble : IAlmacenamientoContextoGrupo
    {
        internal ContextoGrupo? Ultimo { get; private set; }

        public ContextoGrupo? Cargar(GrupoId grupoId) =>
            Ultimo?.GrupoId == grupoId ? Ultimo : null;

        public void Guardar(ContextoGrupo contexto) => Ultimo = contexto;
    }
}