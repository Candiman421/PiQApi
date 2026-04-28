// CertApi.ArchitectureRules/AbstractionsDependencyTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public class AbstractionsDependencyTests : BaseArchitectureTest
{
    [Fact]
    public void Abstractions_Should_Not_Depend_On_External_Libraries()
    {
        // This rule prevents dependency on external libraries except standard .NET
        var rule = Types()
            .That().ResideInNamespace("CertApi.Abstractions", true)
            .Should().OnlyDependOnTypesThat()
            .ResideInAnyNamespace(
                "System", "System.*", // System namespace and sub-namespaces
                "CertApi.Abstractions", "CertApi.Abstractions.*" // Our abstractions
            )
            .Because("Abstractions should only depend on System namespaces and itself");

        AssertArchRule(rule);
    }

    [Fact]
    public void Abstractions_Should_Not_Have_Concrete_Framework_Dependencies()
    {
        // Check that abstractions don't depend on specific frameworks
        var prohibitedFrameworks = new[] {
            "Microsoft.Extensions", "Microsoft.Extensions.*",
            "Newtonsoft.Json", "Newtonsoft.Json.*",
            "Microsoft.AspNetCore", "Microsoft.AspNetCore.*",
            "Microsoft.Exchange", "Microsoft.Exchange.*"
        };

        var rule = Types()
            .That().ResideInNamespace("CertApi.Abstractions", true)
            .Should().NotDependOnAny(Types().That().ResideInAnyNamespace(prohibitedFrameworks))
            .Because("Abstractions should not depend on specific implementation frameworks");

        AssertArchRule(rule);
    }

    [Fact]
    public void Abstractions_Should_Not_Expose_External_Types_In_Public_API()
    {
        // This rule ensures that public methods/properties don't expose external types
        var publicMethods = MethodMembers()
            .That().AreDeclaredIn(Types().That().ResideInNamespace("CertApi.Abstractions", true))
            .And().ArePublic();

        var rule = publicMethods
            .Should().FollowCustomCondition(m => 
                !m.ReturnType.FullName.StartsWith("Microsoft.") &&
                !m.Parameters.Any(p => p.Type.FullName.StartsWith("Microsoft.")),
                "not expose external framework types",
                "exposes external framework types")
            .Because("Public API should not expose external implementation types");

        AssertArchRule(rule);
    }
}