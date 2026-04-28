// CertApi.ArchitectureRules/Projects/CertApi.Abstractions/Tests/ArchitectureTestHelper.cs
using System;
using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Fluent.Syntax.Elements;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using ArchUnitNET.Fluent.Syntax.Elements.Members;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CertApi.ArchitectureRules.Projects.CertApi.Abstractions.Tests
{
    public static class ArchitectureTestHelper
    {
        public static IObjectProvider<IType> GetLayer(string namespacePattern) =>
            Types().That().ResideInNamespace(namespacePattern);

        public static IArchRule BeThreadSafe()
        {
            return Classes()
                .That()
                .Are(Classes())
                .Should()
                .BeSealed()
                .OrShould()
                .BeImmutable()
                .Because("classes should be thread safe");
        }

        public static IArchRule HaveValidationAttributes()
        {
            return Classes()
                .That()
                .AreNotInterfaces()
                .Should()
                .HaveAnyAttributes()
                .Because("validation classes must have validation attributes");
        }

        public static IArchRule FollowNamingConvention(string pattern)
        {
            return Classes()
                .That()
                .AreNot(Classes().That().AreEnums())
                .Should()
                .HaveNameContaining(pattern)
                .Because($"classes should follow naming pattern {pattern}");
        }

        public static IArchRule OnlyDependOnLayer(string targetNamespace)
        {
            return Types()
                .That()
                .Are(Types())
                .Should()
                .OnlyDependOnTypesThat()
                .ResideInNamespace(targetNamespace)
                .Because($"classes should only depend on {targetNamespace}");
        }

        public static IArchRule HaveNoMutableMembers()
        {
            return Classes()
                .That()
                .Are(Classes())
                .Should()
                .BeImmutable()
                .Because("classes should not have mutable state");
        }
    }
}