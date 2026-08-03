using Microsoft.Data.Sqlite;

namespace SistemaDocente.Data.Tests;

internal sealed class BaseSqliteTemporal : IDisposable
{
    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(),
        "SistemaDocenteNEM.Tests",
        Guid.NewGuid().ToString("N"));

    internal BaseSqliteTemporal()
    {
        Ruta = Path.Combine(_directorio, "sistema-docente.db");
        Persistencia = new PersistenciaGrupoSqlite(Ruta);
    }

    internal string Ruta { get; }

    internal PersistenciaGrupoSqlite Persistencia { get; }

    internal SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection($"Data Source={Ruta}");
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA foreign_keys = ON;";
        comando.ExecuteNonQuery();
        return conexion;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_directorio))
        {
            Directory.Delete(_directorio, true);
        }
    }
}