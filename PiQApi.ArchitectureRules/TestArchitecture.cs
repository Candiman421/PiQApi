// PiQApi.ArchitectureRules/TestArchitecture.cs
using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.Domain;

namespace PiQApi.ArchitectureRules;

public static class TestArchitecture
{
    private static readonly Lazy<Architecture> LazyArchitecture = new(() => CreateArchitecture(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static Architecture Architecture => LazyArchitecture.Value;

    private static Architecture CreateArchitecture()
    {
        // Load the PiQApi assemblies
        var abstractionsAssembly = typeof(PiQApi.Abstractions.Results.ICertResult).Assembly;
        var coreAssembly = typeof(PiQApi.Exchange.Core.Results.ExchangeResult).Assembly;
        var serviceAssembly = typeof(PiQApi.Exchange.Service.ServiceWrapper).Assembly;
        var operationsAssembly = typeof(PiQApi.Exchange.Operations.OperationBase).Assembly;
        var clientAssembly = typeof(PiQApi.Exchange.Client.ExchangeClient).Assembly;

        // Create and return the architecture
        return new ArchLoader()
            .LoadAssemblies(
                abstractionsAssembly,
                coreAssembly,
                serviceAssembly,
                operationsAssembly,
                clientAssembly)
            .Build();
    }
}