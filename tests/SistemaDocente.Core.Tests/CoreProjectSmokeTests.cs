namespace SistemaDocente.Core.Tests;

public sealed class CoreProjectSmokeTests
{
    [Fact]
    public void CoreAssemblyIsDiscoveredAndLoadedWithoutWpf()
    {
        var assembly = System.Reflection.Assembly.Load("SistemaDocente.Core");

        Assert.Equal("SistemaDocente.Core", assembly.GetName().Name);
    }
}