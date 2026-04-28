// CertApi.ArchitectureRules/Projects/CertApi.Abstractions/Tests/AbstractionsArchitectureTests.Base.cs
using System.Linq;
using System.Reflection;
using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Fluent.Syntax;
using ArchUnitNET.Fluent.Syntax.Elements;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Fluent.Syntax.Elements.Members;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using CertApi.Abstractions.Core.Interfaces;

namespace CertApi.ArchitectureRules.Projects.CertApi.Abstractions.Tests
{
    public abstract class AbstractionsArchitectureTests
    {
        protected static readonly Architecture ArchSystem = new ArchLoader()
            .LoadAssemblies(typeof(ICorrelationContext).Assembly)
            .Build();

        protected static IObjectProvider<IType> CoreLayer =>
            Types().That().ResideInNamespace("CertApi.Abstractions.Core");

        protected static IObjectProvider<IType> ValidationLayer =>
            Types().That().ResideInNamespace("CertApi.Abstractions.Validation");

        protected static IObjectProvider<IType> ResultsLayer =>
            Types().That().ResideInNamespace("CertApi.Abstractions.Results");

        protected static IArchCondition<IType> BeImmutable()
        {
            return new ArchCondition<IType>("be immutable",
                type => !type.GetMemberFields().Any(f => !f.IsReadOnly) &&
                       !type.GetMemberProperties().Any(p => p.CanWrite && !p.IsInitOnly));
        }

        protected static IArchCondition<IType> BeStateless()
        {
            return new ArchCondition<IType>("be stateless",
                type => !type.GetMemberFields().Any() && !type.GetMemberProperties().Any());
        }

        protected static IArchCondition<IType> NotDependOnImplementationTypes()
        {
            return new ArchCondition<IType>("not depend on implementation types",
                type => !type.GetDependencies().Any(d => 
                    d.TargetType.FullName.Contains(".Services.") ||
                    d.TargetType.FullName.Contains(".Implementation.")));
        }
    }
}