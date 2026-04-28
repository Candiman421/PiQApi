// CertApi.ArchitectureRules/AsyncPatternTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public class AsyncPatternTests : BaseArchitectureTest
{
    [Fact]
    public void Async_Methods_Should_Have_Cancellation_Token_Parameter()
    {
        var asyncMethods = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().ResideInNamespace("CertApi.Abstractions", true))
            .And().HaveNameEndingWith("Async")
            .And().AreNotDeclaredIn(Classes().That().ImplementInterface("IDisposable")); // Exclude Dispose methods

        var rule = asyncMethods
            .Should().FollowCustomCondition(m => 
                m.Parameters.Any(p => p.Type.Name.Contains("CancellationToken")),
                "accept a CancellationToken parameter",
                "doesn't accept a CancellationToken parameter")
            .Because("Async methods should support cancellation with CancellationToken");

        AssertArchRule(rule);
    }

    [Fact]
    public void Async_Methods_Should_Return_Task_Or_ValueTask()
    {
        var asyncMethods = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().ResideInNamespace("CertApi.Abstractions", true))
            .And().HaveNameEndingWith("Async");

        var rule = asyncMethods
            .Should().HaveReturnType("Task", true)
            .OrShould().HaveReturnType("ValueTask", true)
            .Because("Async methods should return Task or ValueTask");

        AssertArchRule(rule);
    }

    [Fact]
    public void Task_Returning_Methods_Should_Have_Async_Suffix()
    {
        var taskReturningMethods = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().ResideInNamespace("CertApi.Abstractions", true))
            .And().HaveReturnType("Task", true);

        var rule = taskReturningMethods
            .Should().HaveNameEndingWith("Async")
            .Because("Methods returning Task should have Async suffix");

        AssertArchRule(rule);
    }
}