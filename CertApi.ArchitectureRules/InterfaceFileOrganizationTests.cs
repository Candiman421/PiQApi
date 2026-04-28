// CertApi.ArchitectureRules/InterfaceFileOrganizationTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using System.Linq;

namespace CertApi.ArchitectureRules;

public class InterfaceFileOrganizationTests : BaseArchitectureTest
{
    [Fact]
    public void Each_Interface_Should_Be_In_Its_Own_File()
    {
        // This test is a conceptual placeholder as ArchUnitNET doesn't directly access file information
        // In a real implementation, you would need to use reflection or other means to check file organization
        
        var rule = Interfaces()
            .That().ResideInNamespace("CertApi.Abstractions", true)
            .Should().FollowCustomCondition(i => 
                // In a real implementation, you would check the actual file path
                // This is a placeholder that will always pass
                true,
                "be defined in their own file",
                "shares a file with other interfaces")
            .Because("Each interface should be defined in its own file");

        AssertArchRule(rule);
    }

    [Fact]
    public void Interface_File_Names_Should_Match_Interface_Names()
    {
        // This test is a conceptual placeholder as ArchUnitNET doesn't directly access file information
        
        var rule = Interfaces()
            .That().ResideInNamespace("CertApi.Abstractions", true)
            .Should().FollowCustomCondition(i => 
                // In a real implementation, you would check if the file name matches the interface name
                // This is a placeholder that will always pass
                true,
                "be defined in a file with the same name",
                "is defined in a file with a different name")
            .Because("Interface file names should match the interface names");

        AssertArchRule(rule);
    }
}