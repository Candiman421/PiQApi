// CertApi.ArchitectureRules/CommonRules.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public static class CommonRules
{
    // Project namespace constants
    public const string AbstractionsNamespace = "CertApi.Abstractions";
    public const string CoreNamespace = "CertApi.Exchange.Core";
    public const string ServiceNamespace = "CertApi.Exchange.Service";
    public const string OperationsNamespace = "CertApi.Exchange.Operations";
    public const string ClientNamespace = "CertApi.Exchange.Client";

    // Layer definitions
    public static IObjectProvider<IType> AbstractionsLayer => Types().That().ResideInNamespace(AbstractionsNamespace, true);
    public static IObjectProvider<IType> CoreLayer => Types().That().ResideInNamespace(CoreNamespace, true);
    public static IObjectProvider<IType> ServiceLayer => Types().That().ResideInNamespace(ServiceNamespace, true);
    public static IObjectProvider<IType> OperationsLayer => Types().That().ResideInNamespace(OperationsNamespace, true);
    public static IObjectProvider<IType> ClientLayer => Types().That().ResideInNamespace(ClientNamespace, true);

    // Common rules
    public static IArchRule InterfacesShouldStartWithI =>
        Interfaces()
            .Should().HaveNameStartingWith("I")
            .Because("Interfaces should follow the naming convention of starting with capital I");

    // Layer dependency rules
    public static IArchRule AbstractionsShouldNotDependOnAnyOtherLayer =>
        Types().That().Are(AbstractionsLayer)
            .Should().NotDependOnAny(CoreLayer)
            .AndShould().NotDependOnAny(ServiceLayer)
            .AndShould().NotDependOnAny(OperationsLayer)
            .AndShould().NotDependOnAny(ClientLayer)
            .Because("Abstractions layer should be independent of all other layers");

    public static IArchRule CoreShouldOnlyDependOnAbstractions =>
        Types().That().Are(CoreLayer)
            .Should().NotDependOnAny(ServiceLayer)
            .AndShould().NotDependOnAny(OperationsLayer)
            .AndShould().NotDependOnAny(ClientLayer)
            .Because("Core layer should only depend on Abstractions layer");

    public static IArchRule ServiceShouldOnlyDependOnCoreAndAbstractions =>
        Types().That().Are(ServiceLayer)
            .Should().NotDependOnAny(OperationsLayer)
            .AndShould().NotDependOnAny(ClientLayer)
            .Because("Service layer should only depend on Core and Abstractions layers");

    public static IArchRule OperationsShouldNotDependOnClient =>
        Types().That().Are(OperationsLayer)
            .Should().NotDependOnAny(ClientLayer)
            .Because("Operations layer should not depend on Client layer");
}