// CertApi.ArchitectureRules/InterfaceDesignTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public class InterfaceDesignTests : BaseArchitectureTest
{
    [Fact]
    public void Interfaces_Should_Follow_Naming_Convention()
    {
        var rule = Interfaces()
            .That().ResideInNamespace("CertApi.Abstractions")
            .Should().HaveNameStartingWith("I")
            .Because("Interface prefix 'I' is required for all interfaces");

        AssertArchRule(rule);
    }

    [Fact]
    public void Async_Methods_Should_Have_Async_Suffix_And_Return_Task()
    {
        var asyncMethods = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().ResideInNamespace("CertApi.Abstractions"))
            .And().HaveNameEndingWith("Async");

        var rule = asyncMethods
            .Should().HaveReturnType("Task", true)
            .Because("Async methods should return Task or Task<T>");

        AssertArchRule(rule);

        var methodsReturningTask = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().ResideInNamespace("CertApi.Abstractions"))
            .And().HaveReturnType("Task", true);

        var rule2 = methodsReturningTask
            .Should().HaveNameEndingWith("Async")
            .Because("Methods returning Task should have Async suffix");

        AssertArchRule(rule2);
    }

    [Fact]
    public void Interfaces_Should_Not_Have_Too_Many_Methods()
    {
        // Custom rule to validate interface segregation principle
        var rule = Interfaces()
            .That().ResideInNamespace("CertApi.Abstractions")
            .Should().FollowCustomCondition(t => t.GetMethodMembers().Count() <= 10, 
                "have no more than 10 methods", 
                "has too many methods")
            .Because("Interfaces should follow interface segregation principle");

        AssertArchRule(rule);
    }
}