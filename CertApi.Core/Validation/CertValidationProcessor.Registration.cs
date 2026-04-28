// CertApi.Core/Validation/CertValidationProcessor.Registration.cs
using CertApi.Abstractions.Validation;
using CertApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CertApi.Core.Validation;

// This partial implementation contains all rule registration logic
public partial class CertValidationProcessor
{
    // Dictionary to store rules by entity type
    private readonly Dictionary<Type, List<object>> _rules = new();

    /// <summary>
    /// Gets the rules for a type
    /// </summary>
    private IEnumerable<ICertValidationRule<T>> GetRulesForType<T>() where T : class
    {
        var type = typeof(T);
        if (_rules.TryGetValue(type, out var rules))
        {
            return rules.Cast<ICertValidationRule<T>>();
        }

        return Enumerable.Empty<ICertValidationRule<T>>();
    }
}