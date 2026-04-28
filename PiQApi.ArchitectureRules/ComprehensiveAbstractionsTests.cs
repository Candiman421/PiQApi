// PiQApi.ArchitectureRules/ComprehensiveAbstractionsTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class ComprehensiveAbstractionsTests : BaseArchitectureTest
{
    [Fact]
    public void Abstractions_Should_Follow_Clean_Architecture_Principles()
    {
        // Combined rule to verify multiple architectural aspects
        var interfaceRule = Interfaces()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().HaveNameStartingWith("I")
            .AndShould().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Core")
            );

        var interfaceRule2 = Interfaces()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Service")
            );

        var interfaceRule3 = Interfaces()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Operations")
            );

        var interfaceRule4 = Interfaces()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Client")
            );

        var classRule = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().BePublic()
            .AndShould().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Core")
            );

        var classRule2 = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Service")
            );

        var classRule3 = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Operations")
            );

        var classRule4 = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace("PiQApi.Exchange.Client")
            );

        var enumRule = Types()
            .That().AreEnums()
            .And().ResideInNamespace("PiQApi.Abstractions.Enums")
            .Should().HaveNameEndingWith("Type");

        var combinedRule = interfaceRule
            .And(interfaceRule2)
            .And(interfaceRule3)
            .And(interfaceRule4)
            .And(classRule)
            .And(classRule2)
            .And(classRule3)
            .And(classRule4)
            .And(enumRule);

        AssertArchRule(combinedRule);
    }

    [Fact]
    public void Abstractions_Internal_Dependencies_Should_Be_Acyclic()
    {
        // Verify no cyclic dependencies within abstractions layer
        var rule = SliceRuleDefinition.Slices()
            .Matching("PiQApi.Abstractions.(*)")
            .Should()
            .BeFreeOfCycles();

        // Use rule.Check() directly instead of AssertArchRule
        rule.Check(Architecture);
    }

    [Fact]
    public void Abstractions_Classes_Should_Be_Sealed_Unless_Designed_For_Inheritance()
    {
        // Classes should be sealed unless they're specifically designed for inheritance
        var rule = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .And().AreNotAbstract()
            .And().DoNotHaveNameEndingWith("Base")
            .Should().BeSealed()
            .Because("Classes should be sealed unless designed for inheritance");

        AssertArchRule(rule);
    }

    [Fact]
    public void Abstractions_Should_Not_Contain_Implementation_Details()
    {
        // No implementations of external interfaces
        var rule = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotImplementInterface("Microsoft.*")
            .Because("Abstractions should not contain implementations of external interfaces");

        AssertArchRule(rule);
    }
}