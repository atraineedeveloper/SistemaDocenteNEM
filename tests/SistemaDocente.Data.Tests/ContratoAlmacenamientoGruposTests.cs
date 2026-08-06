using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Data.Tests;

public sealed class ContratoAlmacenamientoGruposTests : IDisposable
{
    private readonly BaseSqliteTemporal _base = new();

    [Fact]
    public void AdaptadorImplementaContratoConSqliteRealTemporal()
    {
        var grupo = Grupo.Crear("Primero A");

        Assert.IsAssignableFrom<IAlmacenamientoGrupos>(_base.Persistencia);
        ComprobarContrato(_base.Persistencia, grupo);
    }

    private static void ComprobarContrato(PersistenciaGrupoSqlite almacenamiento, Grupo grupo)
    {
        almacenamiento.Guardar(grupo);

        Assert.True(almacenamiento.Existe(grupo.Id));
        Assert.False(almacenamiento.Existe(GrupoId.DesdeGuid(Guid.NewGuid())));
        var cargado = almacenamiento.Cargar(grupo.Id);
        Assert.NotNull(cargado);
        Assert.Equal(grupo.Id, cargado.Id);
    }

    [Fact]
    public void ErrorDeDataSeTraduceYConservaComoCausa()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_base.Ruta)!);
        File.WriteAllText(_base.Ruta, "esto no es sqlite");
        var error = ObtenerErrorTraducido(_base.Persistencia);

        var causaData = Assert.IsAssignableFrom<DataAccessException>(error.InnerException);
        Assert.NotNull(causaData.InnerException);
    }

    [Fact]
    public void ErrorDeEsquemaSeTraduceYConservaComoCausa()
    {
        _base.Persistencia.Inicializar();
        using (var conexion = _base.AbrirConexion())
        using (var comando = conexion.CreateCommand())
        {
            comando.CommandText = "PRAGMA user_version = 5;";
            comando.ExecuteNonQuery();
        }

        var error = Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => _base.Persistencia.Cargar(GrupoId.DesdeGuid(Guid.NewGuid())));

        Assert.IsType<SchemaIncompatibleException>(error.InnerException);
    }

    private static ErrorPersistenciaAplicacionException ObtenerErrorTraducido(
        PersistenciaGrupoSqlite almacenamiento) =>
        Assert.Throws<ErrorPersistenciaAplicacionException>(
            () => almacenamiento.Existe(GrupoId.DesdeGuid(Guid.NewGuid())));

    public void Dispose() => _base.Dispose();
}