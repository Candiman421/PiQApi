// CertApi.ArchitectureRules/DocumentationTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public class DocumentationTests : BaseArchitectureTest
{
    [Fact]
    public void Interfaces_Should_Have_XML_Documentation()
    {
        // This test requires custom implementation to check for XML documentation
        var rule = Interfaces()
            .That().ResideInNamespace("CertApi.Abstractions", true)
            .Should().FollowCustomCondition(new HasXmlDocumentationCondition())
            .Because("All interfaces should have XML documentation");

        AssertArchRule(rule);
    }

    [Fact]
    public void Public_Methods_Should_Have_XML_Documentation()
    {
        // This test requires custom implementation to check for XML documentation
        var publicMethods = MethodMembers()
            .That().AreDeclaredIn(Interfaces().That().ResideInNamespace("CertApi.Abstractions", true));

        var rule = publicMethods
            .Should().FollowCustomCondition(new HasXmlDocumentationCondition())
            .Because("All public methods should have XML documentation");

        AssertArchRule(rule);
    }
}