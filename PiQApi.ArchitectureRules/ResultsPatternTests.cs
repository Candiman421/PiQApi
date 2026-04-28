// PiQApi.ArchitectureRules/ResultsPatternTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class ResultsPatternTests : BaseArchitectureTest
{
    [Fact]
    public void ICertResult_Should_Have_Required_Properties()
    {
        var rule = Interfaces()
            .That().HaveFullName("PiQApi.Abstractions.Results.ICertResult")
            .Should().HavePropertyMemberWithName("IsSuccess")
            .AndShould().HavePropertyMemberWithName("Error")
            .Because("ICertResult must have IsSuccess and Error properties");

        AssertArchRule(rule);
    }

    [Fact]
    public void Generic_ICertResult_Should_Extend_Base_ICertResult()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Results.ICertResult`1")
            .Should().BeAssignableTo("PiQApi.Abstractions.Results.ICertResult")
            .Because("Generic ICertResult<T> must inherit from ICertResult");

        AssertArchRule(rule);
    }

    [Fact]
    public void Generic_ICertResult_Should_Have_Value_Property()
    {
        var rule = Interfaces()
            .That().HaveFullNameContaining("PiQApi.Abstractions.Results.ICertResult`1")
            .Should().HavePropertyMemberWithName("Value")
            .Because("Generic ICertResult<T> must have a Value property");

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