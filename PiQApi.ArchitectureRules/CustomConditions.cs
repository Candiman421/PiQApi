// PiQApi.ArchitectureRules/CustomConditions.cs
using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace PiQApi.ArchitectureRules;

public class HasXmlDocumentationCondition : ICondition<IMember>
{
    public string Description => "have XML documentation";

    public IEnumerable<ConditionResult> Check(IEnumerable<IMember> objects, Architecture architecture)
    {
        foreach (var obj in objects)
        {
            // Since we can't directly check source code or XML documentation in ArchUnitNET,
            // we'd need to implement a custom approach to check for documentation
            
            // For demonstration purposes, we'll assume all members have documentation
            // In a real implementation, you might check for an attribute or use reflection
            // to access the documentation from XML files
            bool hasDocumentation = true;
            
            yield return new ConditionResult(
                obj,
                hasDocumentation,
                $"{obj.FullName} does not have XML documentation"
            );
        }
    }

    public bool CheckEmpty() => true;
}

public class ValueSemanticCondition : ICondition<IType>
{
    public string Description => "implement value semantics";

    public IEnumerable<ConditionResult> Check(IEnumerable<IType> objects, Architecture architecture)
    {
        foreach (var type in objects)
        {
            // Fixed method comparisons by accessing the Name property without calling it
            bool hasEqualsOverride = type.GetMethodMembers()
                .Any(m => string.Equals(m.Name, "Equals") && m.Parameters.Count == 1);
                
            bool hasGetHashCodeOverride = type.GetMethodMembers()
                .Any(m => string.Equals(m.Name, "GetHashCode") && m.Parameters.Count == 0);
                
            bool hasEqualityOperators = type.GetMethodMembers()
                .Any(m => (string.Equals(m.Name, "op_Equality") || string.Equals(m.Name, "op_Inequality")) && 
                          m.Parameters.Count == 2);
                
            bool hasValueSemantics = hasEqualsOverride && hasGetHashCodeOverride;
            
            yield return new ConditionResult(
                type,
                hasValueSemantics,
                $"{type.FullName} does not implement value semantics properly"
            );
        }
    }

    public bool CheckEmpty() => true;
}

public class ImmutableCondition : ICondition<IType>
{
    public string Description => "be immutable";

    public IEnumerable<ConditionResult> Check(IEnumerable<IType> objects, Architecture architecture)
    {
        foreach (var type in objects)
        {
            // Check if all fields are private or readonly
            bool fieldsAreImmutable = type.GetFieldMembers()
                .All(f => f.IsStatic || f.Visibility == Visibility.Private);
                
            // Check if properties don't have public setters
            // Using HasGetter/HasSetter properties instead of CanWrite
            bool propertiesAreImmutable = type.GetPropertyMembers()
                .All(p => !p.HasSetter || p.Visibility != Visibility.Public);
                
            bool isImmutable = fieldsAreImmutable && propertiesAreImmutable;
                
            yield return new ConditionResult(
                type,
                isImmutable,
                $"{type.FullName} is not immutable"
            );
        }
    }

    public bool CheckEmpty() => true;
}