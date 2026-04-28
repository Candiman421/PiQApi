// CertApi.ArchitectureRules/Projects/CertApi.Abstractions/Tests/Extensions/MemberExtensions.cs
using ArchUnitNET.Domain;

namespace CertApi.ArchitectureRules.Projects.CertApi.Abstractions.Tests.Extensions
{
    public static class MemberExtensions  
    {
        public static bool CanBeReassigned(this IMember member)
        {
            if (member is IFieldMember field)
            {
                return !field.IsReadOnly;
            }
            
            if (member is IPropertyMember property)
            {
                return property.CanWrite && !property.IsInitOnly;
            }

            return false;
        }
    }
}