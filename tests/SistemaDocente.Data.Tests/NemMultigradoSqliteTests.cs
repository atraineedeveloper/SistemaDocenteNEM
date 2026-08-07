using SistemaDocente.Core;
using SistemaDocente.Data;

namespace SistemaDocente.Data.Tests;

public sealed class NemMultigradoSqliteTests
{
    [Fact]
    public void ContextoUnigradoPersisteYAsignaGradoDeterministicoAEstudiantes()
    {
        var carpeta = CrearCarpetaTemporal();
        try
        {
            var ruta = Path.Combine(carpeta, "unigrado.db");
            var grupos = new PersistenciaGrupoSqlite(ruta);
            grupos.Inicializar();

            var grupo = Grupo.Crear("Grupo de prueba");
            grupo.AgregarEstudiante("Ana Ejemplo", 1);
            grupo.AgregarEstudiante("Luis Ejemplo", 2);
            grupos.Guardar(grupo);

            var contextos = new PersistenciaContextoGrupoSqlite(ruta);
            contextos.Guardar(ContextoGrupo.Crear(
                grupo.Id,
                cicloEscolar: "2026-2027",
                nombreEscuela: "Escuela de prueba",
                entidadFederativa: "Tabasco",
                municipio: "Centro",
                localidad: "Villahermosa",
                grupo: "A",
                turno: "Matutino",
                organizacionEscolar: OrganizacionEscolar.Completa,
                gradosAtendidos: [GradoPrimaria.Cuarto]));

            var contexto = contextos.Cargar(grupo.Id);
            var recargado = grupos.Cargar(grupo.Id);

            Assert.NotNull(contexto);
            Assert.Equal(OrganizacionEscolar.Completa, contexto.OrganizacionEscolar);
            Assert.Equal([GradoPrimaria.Cuarto], contexto.GradosAtendidos);
            Assert.Equal([FaseNem.Fase4], contexto.FasesNem);
            Assert.False(contexto.EsMultigrado);
            Assert.NotNull(recargado);
            Assert.All(recargado.Estudiantes, estudiante => Assert.Equal(GradoPrimaria.Cuarto, estudiante.Grado));
        }
        finally
        {
            EliminarCarpeta(carpeta);
        }
    }

    [Fact]
    public void ContextoMultigradoConservaVariosGradosYFases()
    {
        var carpeta = CrearCarpetaTemporal();
        try
        {
            var ruta = Path.Combine(carpeta, "multigrado.db");
            var grupos = new PersistenciaGrupoSqlite(ruta);
            grupos.Inicializar();

            var grupo = Grupo.Crear("Multigrado");
            var ana = grupo.AgregarEstudiante("Ana Ejemplo", 1, grado: GradoPrimaria.Segundo);
            var luis = grupo.AgregarEstudiante("Luis Ejemplo", 2, grado: GradoPrimaria.Tercero);
            grupos.Guardar(grupo);

            var contextos = new PersistenciaContextoGrupoSqlite(ruta);
            contextos.Guardar(ContextoGrupo.Crear(
                grupo.Id,
                entidadFederativa: "Tabasco",
                municipio: "Tacotalpa",
                localidad: "Localidad de prueba",
                grupo: "Único",
                organizacionEscolar: OrganizacionEscolar.Bidocente,
                gradosAtendidos: [GradoPrimaria.Segundo, GradoPrimaria.Tercero]));

            var contexto = contextos.Cargar(grupo.Id);
            var recargado = grupos.Cargar(grupo.Id);

            Assert.NotNull(contexto);
            Assert.True(contexto.EsMultigrado);
            Assert.Equal("Multigrado", contexto.ModalidadGrupo);
            Assert.Equal([GradoPrimaria.Segundo, GradoPrimaria.Tercero], contexto.GradosAtendidos);
            Assert.Equal([FaseNem.Fase3, FaseNem.Fase4], contexto.FasesNem);
            Assert.Equal(OrganizacionEscolar.Bidocente, contexto.OrganizacionEscolar);
            Assert.NotNull(recargado);
            Assert.Equal(GradoPrimaria.Segundo, recargado.Estudiantes.Single(x => x.Id == ana.Id).Grado);
            Assert.Equal(GradoPrimaria.Tercero, recargado.Estudiantes.Single(x => x.Id == luis.Id).Grado);
        }
        finally
        {
            EliminarCarpeta(carpeta);
        }
    }

    private static string CrearCarpetaTemporal()
    {
        var ruta = Path.Combine(Path.GetTempPath(), "SistemaDocenteNEM-NEM-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ruta);
        return ruta;
    }

    private static void EliminarCarpeta(string ruta)
    {
        if (Directory.Exists(ruta)) Directory.Delete(ruta, recursive: true);
    }
}