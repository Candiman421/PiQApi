// CertApi.ArchitectureRules/Projects/CertApi.Abstractions/Tests/AbstractionsArchitectureTests.LayerDependencies.cs
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace CertApi.ArchitectureRules.Projects.CertApi.Abstractions.Tests
{
    public class LayerDependencyTests : AbstractionsArchitectureTests
    {
        [Fact]
        public void CoreLayer_Should_NotDependOnOtherLayers()
        {
            var rule = Types()
                .That().Are(CoreLayer)
                .Should().NotDependOnAny(Types().That().ResideInNamespace("CertApi.Exchange"))
                .AndShould(NotDependOnImplementationTypes())
                .Because("core layer should be independent of implementation details");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void ResultTypes_Should_BeImmutable()
        {
            var rule = Types()
                .That().Are(ResultsLayer)
                .Should(BeImmutable())
                .Because("result types should be immutable to ensure thread safety");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void AllLayers_Should_DependOnlyOnAllowedLayers()
        {
            var rule = Types()
                .That().ResideInNamespace("CertApi.Abstractions")
                .Should().OnlyDependOn(Types().That()
                    .ResideInNamespace("CertApi.Abstractions")
                    .Or().ResideInNamespace("System")
                    .Or().ResideInNamespace("Microsoft.Extensions"))
                .Because("abstractions layer should only depend on itself and core framework");

            rule.Check(ArchSystem);
        }
    }
}