// PiQApi.Core/Validation/PiQValidationProcessor.Registration.cs
using PiQApi.Abstractions.Validation;
using PiQApi.Abstractions.Validation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiQApi.Core.Validation;

// This partial implementation contains all rule registration logic
public partial class PiQValidationProcessor
{
    // Dictionary to store rules by entity type
    private readonly Dictionary<Type, List<object>> _rules = new();

    /// <summary>
    /// Gets the rules for a type
    /// </summary>
    private IEnumerable<IPiQValidationRule<T>> GetRulesForType<T>() where T : class
    {
        var type = typeof(T);
        if (_rules.TryGetValue(type, out var rules))
        {
            return rules.Cast<IPiQValidationRule<T>>();
        }

        return Enumerable.Empty<IPiQValidationRule<T>>();
    }
}