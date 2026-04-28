// CertApi.ArchitectureRules/LayerDependencyTests.cs
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public class LayerDependencyTests : BaseArchitectureTest
{
    [Fact]
    public void Abstractions_Should_Not_Depend_On_Any_Other_Layer()
    {
        AssertArchRule(CommonRules.AbstractionsShouldNotDependOnAnyOtherLayer);
    }

    [Fact]
    public void Core_Should_Only_Depend_On_Abstractions()
    {
        AssertArchRule(CommonRules.CoreShouldOnlyDependOnAbstractions);
    }

    [Fact]
    public void Service_Should_Only_Depend_On_Core_And_Abstractions()
    {
        AssertArchRule(CommonRules.ServiceShouldOnlyDependOnCoreAndAbstractions);
    }

    [Fact]
    public void Operations_Should_Not_Depend_On_Client()
    {
        AssertArchRule(CommonRules.OperationsShouldNotDependOnClient);
    }
}