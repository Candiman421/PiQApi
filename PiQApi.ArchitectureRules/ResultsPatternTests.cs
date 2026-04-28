// PiQApi.ArchitectureRules/ResultsPatternTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class ResultsPatternTests : BaseArchitectureTest
{
    [Fact]
    public void IPiQResult_Should_Have_Required_Properties()
    {
        var rule = Interfaces()
            .That().HaveFullName("PiQApi.Abstractions.Results.IPiQResult")
            .Should().HavePropertyMemberWithName("IsSuccess")
            .AndShould().HavePropertyMemberWithName("Error")
            .Because("IPiQResult must have IsSuccess and Error properties");

        AssertArchRule(rule);
    }

    [Fact]
    public void Generic_IPiQResult_Should_Extend_Base_IPiQResult()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Results.IPiQResult`1")
            .Should().BeAssignableTo("PiQApi.Abstractions.Results.IPiQResult")
            .Because("Generic IPiQResult<T> must inherit from IPiQResult");

        AssertArchRule(rule);
    }

    [Fact]
    public void Generic_IPiQResult_Should_Have_Value_Property()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Results.IPiQResult`1")
            .Should().HavePropertyMemberWithName("Value")
            .Because("Generic IPiQResult<T> must have a Value property");

        AssertArchRule(rule);
    }

    [Fact]
    public void IResultError_Should_Have_Required_Properties()
    {
        var rule = Interfaces()
            .That().HaveFullName("PiQApi.Abstractions.Results.IResultError")
            .Should().HavePropertyMemberWithName("Code")
            .AndShould().HavePropertyMemberWithName("Message")
            .AndShould().HavePropertyMemberWithName("CorrelationId")
            .AndShould().HavePropertyMemberWithName("Context")
            .Because("IResultError must have all required error properties");

        AssertArchRule(rule);
    }
}
