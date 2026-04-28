// PiQApi.ArchitectureRules/CleanArchitecturePrinciplesTests.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PiQApi.ArchitectureRules;

public class CleanArchitecturePrinciplesTests : BaseArchitectureTest
{
    [Fact]
    public void Abstractions_Classes_Must_Not_Depend_On_Implementation_Details()
    {
        var rule = Classes()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(Types().That().ResideInNamespace("System.Net.Http", true))
            .AndShould().NotDependOnAny(Types().That().ResideInNamespace("System.Net.WebClient", true))
            .AndShould().NotDependOnAny(Types().That().ResideInNamespace("System.Data", true))
            .Because("Abstractions layer must not depend on implementation technologies");

        AssertArchRule(rule);
    }

    [Fact]
    public void Abstractions_Should_Not_Reference_UI_Frameworks()
    {
        var uiFrameworks = new[] {
            "System.Windows",
            "System.Windows.*",
            "System.Web.UI",
            "System.Web.UI.*",
            "Microsoft.AspNetCore.Mvc",
            "Microsoft.AspNetCore.Mvc.*"
        };

        var rule = Types()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(Types().That().ResideInAnyNamespace(uiFrameworks))
            .Because("Abstractions layer should not reference UI frameworks");

        AssertArchRule(rule);
    }

    [Fact]
    public void Abstractions_Should_Not_Depend_On_Concrete_Logging_Frameworks()
    {
        var loggingFrameworks = new[] {
            "NLog",
            "NLog.*",
            "Serilog",
            "Serilog.*",
            "log4net",
            "log4net.*"
        };

        var rule = Types()
            .That().ResideInNamespace("PiQApi.Abstractions", true)
            .Should().NotDependOnAny(Types().That().ResideInAnyNamespace(loggingFrameworks))
            .Because("Abstractions should not depend on concrete logging frameworks");

        AssertArchRule(rule);
    }
}