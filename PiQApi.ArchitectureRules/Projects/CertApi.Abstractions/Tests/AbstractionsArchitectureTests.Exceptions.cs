// PiQApi.ArchitectureRules/Projects/PiQApi.Abstractions/Tests/AbstractionsArchitectureTests.Exceptions.cs
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace PiQApi.ArchitectureRules.Projects.PiQApi.Abstractions.Tests
{
    public class ExceptionTests : AbstractionsArchitectureTests
    {
        [Fact]
        public void Exceptions_Should_InheritFromCertServiceException()
        {
            var rule = Types()
                .That().ResideInNamespace("PiQApi.Abstractions")
                .And().AreAssignableTo(typeof(System.Exception))
                .And().AreNotInterfaces()
                .Should().BeAssignableTo("PiQApi.Abstractions.Exceptions.CertServiceException")
                .Because("all exceptions should inherit from CertServiceException");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void Exceptions_Should_BeSealed()
        {
            var rule = Types()
                .That().ResideInNamespace("PiQApi.Abstractions")
                .And().AreAssignableTo(typeof(System.Exception))
                .And().AreNotAbstract()
                .Should().BeSealed()
                .Because("exceptions should be sealed to prevent inheritance");

            rule.Check(ArchSystem);
        }

        [Fact]
        public void Exceptions_Should_BeSerializable()
        {
            var rule = Types()
                .That().ResideInNamespace("PiQApi.Abstractions")
                .And().AreAssignableTo(typeof(System.Exception))
                .Should().HaveAttribute(typeof(System.SerializableAttribute))
                .Because("exceptions must be serializable");

            rule.Check(ArchSystem);
        }
    }
}