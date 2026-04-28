// PiQApi.ArchitectureRules/ModelDesignTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class ModelDesignTests : BaseArchitectureTest
{
    [Fact]
    public void Request_Models_Should_Be_Immutable()
    {
        var requestModels = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions.*.Requests", true)
            .Or().HaveNameEndingWith("Request");

        var rule = requestModels
            .Should().FollowCustomCondition(c => c.GetPropertyMembers().All(p => !p.HasPublicSetter()),
                "have only properties without public setters",
                "has properties with public setters")
            .Because("Request models should be immutable");

        AssertArchRule(rule);
    }

    [Fact]
    public void Response_Models_Should_Indicate_Success_Or_Failure()
    {
        var responseModels = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions.*.Responses", true)
            .Or().HaveNameEndingWith("Response");

        var rule = responseModels
            .Should().FollowCustomCondition(c =>
                c.GetPropertyMembers().Any(p => p.Name == "IsSuccess") ||
                c.ImplementsInterface(i => i.Name.Contains("IPiQResult")),
                "indicate success or failure",
                "does not indicate success or failure")
            .Because("Response models should clearly indicate success or failure");

        AssertArchRule(rule);
    }

    [Fact]
    public void Response_Models_Should_Have_Error_Details_When_Failed()
    {
        var responseModels = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions.*.Responses", true)
            .Or().HaveNameEndingWith("Response");

        var rule = responseModels
            .Should().FollowCustomCondition(c =>
                c.GetPropertyMembers().Any(p => p.Name == "Error") ||
                c.ImplementsInterface(i => i.Name.Contains("IPiQResult")),
                "provide error details",
                "does not provide error details")
            .Because("Response models should provide error details when failed");

        AssertArchRule(rule);
    }
}
