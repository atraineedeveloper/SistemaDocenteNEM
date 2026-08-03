namespace SistemaDocente.Data.Tests;

public sealed class DataProjectSmokeTests
{
    [Fact]
    public void DataAndCoreAssembliesAreDiscoveredAndLoadedWithoutWpf()
    {
        var dataAssembly = System.Reflection.Assembly.Load("SistemaDocente.Data");
        var coreAssembly = System.Reflection.Assembly.Load("SistemaDocente.Core");

        Assert.Equal("SistemaDocente.Data", dataAssembly.GetName().Name);
        Assert.Equal("SistemaDocente.Core", coreAssembly.GetName().Name);
    }
}