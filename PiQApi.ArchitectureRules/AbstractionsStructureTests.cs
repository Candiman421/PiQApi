// PiQApi.ArchitectureRules/AbstractionsStructureTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class AbstractionsStructureTests : BaseArchitectureTest
{
    [Fact]
    public void Key_Result_Interfaces_Should_Exist()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Results")
            .And().HaveAnyNameOf("IPiQResult", "IPiQResult`1", "IResultError", "IResultFactory")
            .Should().Exist()
            .Because("Core result interfaces are required in the Abstractions layer");

        AssertArchRule(rule);
    }

    [Fact]
    public void Key_Validation_Interfaces_Should_Exist()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Validation")
            .And().HaveAnyNameOf("IValidationRule`1")
            .Should().Exist()
            .Because("Core validation interfaces are required in the Abstractions layer");

        AssertArchRule(rule);
    }

    [Fact]
    public void Core_Enums_Should_Have_Type_Suffix()
    {
        var rule = Types()
            .That().AreEnums()
            .And().ResideInNamespace("PiQApi.Abstractions.Enums")
            .Should().HaveNameEndingWith("Type")
            .Because("Enum types in Abstractions should have the 'Type' suffix");

        AssertArchRule(rule);
    }

    [Fact]
    public void Required_Enums_Should_Exist()
    {
        var rule = Types()
            .That().AreEnums()
            .And().ResideInNamespace("PiQApi.Abstractions.Enums")
            .And().HaveAnyNameOf(
                "ValidationModeType",
                "ServiceOperationStatusType",
                "ConnectionStateType",
                "ResourceStatusType")
            .Should().Exist()
            .Because("Required enum types must exist in the Abstractions layer");

        AssertArchRule(rule);
    }
}
