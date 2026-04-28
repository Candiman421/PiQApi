// PiQApi.ArchitectureRules/NamingConventionTests.cs
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class NamingConventionTests : BaseArchitectureTest
{
    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        AssertArchRule(CommonRules.InterfacesShouldStartWithI);
    }

    [Fact]
    public void Result_Classes_Should_End_With_Result()
    {
        var rule = Classes()
            .That().ResideInNamespace("PiQApi.Exchange.Core.Results")
            .And().ImplementInterface("ICertResult")
            .Should().HaveNameEndingWith("Result")
            .Because("Result implementation classes should follow naming convention");

        AssertArchRule(rule);
    }

    [Fact]
    public void Validation_Rules_Should_End_With_Rule()
    {
        var rule = Classes()
            .That().ImplementInterface("IValidationRule")
            .Should().HaveNameEndingWith("Rule")
            .Because("Validation rule classes should follow naming convention");

        AssertArchRule(rule);
    }
}