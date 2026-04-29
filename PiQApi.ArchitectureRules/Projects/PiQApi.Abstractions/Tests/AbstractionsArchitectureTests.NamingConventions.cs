// PiQApi.ArchitectureRules/Projects/PiQApi.Abstractions/Tests/AbstractionsArchitectureTests.NamingConventions.cs
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace PiQApi.ArchitectureRules.Projects.PiQApi.Abstractions.Tests
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
        public void ValidationTypes_Should_UsePiQPrefix()
        {
            var rule = Types()
                .That().ResideInNamespace("PiQApi.Abstractions.Validation")
                .And().AreNotInterfaces()
                .Should().HaveNameStartingWith("PiQ")
                .Because("validation types must use PiQ prefix to avoid framework collisions");

            rule.Check(ArchSystem);
        }
    }
}
