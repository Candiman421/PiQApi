// CertApi.ArchitectureRules/TestArchitecture.cs
using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.Domain;

namespace CertApi.ArchitectureRules;

public static class TestArchitecture
{
    private static readonly Lazy<Architecture> LazyArchitecture = new(() => CreateArchitecture(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static Architecture Architecture => LazyArchitecture.Value;

    private static Architecture CreateArchitecture()
    {
        // Load the CertApi assemblies
        var abstractionsAssembly = typeof(CertApi.Abstractions.Results.ICertResult).Assembly;
        var coreAssembly = typeof(CertApi.Exchange.Core.Results.ExchangeResult).Assembly;
        var serviceAssembly = typeof(CertApi.Exchange.Service.ServiceWrapper).Assembly;
        var operationsAssembly = typeof(CertApi.Exchange.Operations.OperationBase).Assembly;
        var clientAssembly = typeof(CertApi.Exchange.Client.ExchangeClient).Assembly;

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