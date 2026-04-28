// CertApi.ArchitectureRules/DomainPrimitiveTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public class DomainPrimitiveTests : BaseArchitectureTest
{
    [Fact]
    public void Domain_Primitives_Should_Be_Immutable()
    {
        var domainPrimitives = Classes()
            .That().ResideInNamespace("CertApi.Abstractions.Core.Models")
            .And().HaveAnyNameOf("CorrelationId", "ResourceId", "OperationId");

        var rule = domainPrimitives
            .Should().FollowCustomCondition(c => c.GetPropertyMembers().All(p => !p.HasPublicSetter()),
                "have only properties without public setters",
                "has properties with public setters")
            .Because("Domain primitives should be immutable");

        AssertArchRule(rule);
    }

    [Fact]
    public void Domain_Primitives_Should_Override_Equality_Methods()
    {
        var domainPrimitives = Classes()
            .That().ResideInNamespace("CertApi.Abstractions.Core.Models")
            .And().HaveAnyNameOf("CorrelationId", "ResourceId", "OperationId");

        var rule = domainPrimitives
            .Should().FollowCustomCondition(c => 
                c.GetMethodMembers().Any(m => m.Name == "Equals" && m.Parameters.Count == 1) &&
                c.GetMethodMembers().Any(m => m.Name == "GetHashCode" && m.Parameters.Count == 0),
                "override Equals and GetHashCode",
                "does not override Equals and GetHashCode")
            .Because("Domain primitives should have value semantics with proper equality");

        AssertArchRule(rule);
    }

    [Fact]
    public void Domain_Primitives_Should_Have_Validation()
    {
        var domainPrimitives = Classes()
            .That().ResideInNamespace("CertApi.Abstractions.Core.Models")
            .And().HaveAnyNameOf("CorrelationId", "ResourceId", "OperationId");

        // This rule checks if constructor has validation logic
        var rule = domainPrimitives
            .Should().FollowCustomCondition(c => 
                c.GetMethodMembers().Any(m => m.IsConstructor() && m.Instructions.Any(i => i.ToString().Contains("throw"))),
                "have validation in constructor",
                "lacks validation in constructor")
            .Because("Domain primitives should validate their values");

        AssertArchRule(rule);
    }
}