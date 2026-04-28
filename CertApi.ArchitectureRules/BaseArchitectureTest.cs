// CertApi.ArchitectureRules/BaseArchitectureTest.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules;

public abstract class BaseArchitectureTest
{
    protected readonly Architecture Architecture;

    protected BaseArchitectureTest()
    {
        Architecture = TestArchitecture.Architecture;
    }

    protected void AssertArchRule(IArchRule rule)
    {
        rule.Check(Architecture);
    }
}