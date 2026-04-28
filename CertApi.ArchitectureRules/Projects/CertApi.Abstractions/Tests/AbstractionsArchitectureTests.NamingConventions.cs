// CertApi.ArchitectureRules/Projects/CertApi.Abstractions/Tests/AbstractionsArchitectureTests.NamingConventions.cs
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace CertApi.ArchitectureRules.Projects.CertApi.Abstractions.Tests
{
    public class NamingConventionTests : AbstractionsArchitectureTests
    {
        [Fact]
        public void Interfaces_Should_StartWith_I()
        {
            var rule = Interfaces()
                .Should()
                .HaveNameStartingWith("I")
                .Because("interfaces must be clearly identified");
            
            rule.Check(ArchSystem);
        }

        [Fact]
        public void Enums_Should_EndWith_Type()
        {
            var rule = Types()
                .That().AreEnums()
                .Should().HaveNameEndingWith("Type")
                .Because("enums must use Type suffix to avoid collisions");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void ValidationTypes_Should_UseCertPrefix()
        {
            var rule = Types()
                .That().ResideInNamespace("CertApi.Abstractions.Validation")
                .And().AreNotInterfaces()
                .Should().HaveNameStartingWith("Cert")
                .Because("validation types must use Cert prefix to avoid framework collisions");

            rule.Check(ArchSystem);
        }
    }
}