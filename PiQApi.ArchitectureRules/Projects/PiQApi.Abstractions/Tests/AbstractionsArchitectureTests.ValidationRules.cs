// PiQApi.ArchitectureRules/Projects/PiQApi.Abstractions/Tests/AbstractionsArchitectureTests.ValidationRules.cs
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace PiQApi.ArchitectureRules.Projects.PiQApi.Abstractions.Tests
{
    public class ValidationRuleTests : AbstractionsArchitectureTests
    {
        [Fact]
        public void ValidationRules_Should_BeStateless()
        {
            var rule = Types()
                .That().Are(ValidationLayer)
                .And().HaveNameEndingWith("Rule")
                .Should(BeStateless())
                .Because("validation rules must be stateless and thread-safe");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void ValidationContext_Should_BeImmutable()
        {
            var rule = Types()
                .That().Are(ValidationLayer)
                .And().HaveNameEndingWith("Context")
                .Should(BeImmutable())
                .Because("validation context should be immutable once created");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void ValidationResults_Should_NotExposeMutableCollections()
        {
            var rule = Types()
                .That().Are(ValidationLayer)
                .And().HaveNameEndingWith("Result")
                .Should().HavePropertyMembersThat()
                .AreNotTypeOf(typeof(System.Collections.Generic.ICollection<>))
                .AndShould(BeImmutable())
                .Because("validation results must expose read-only collections");

            rule.Check(ArchSystem);
        }
    }
}