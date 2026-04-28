// PiQApi.Abstractions/Validation/Interfaces/IPropertyValidationRule.cs
namespace PiQApi.Abstractions.Validation.Interfaces
{
    /// <summary>
    /// Defines a validation rule for a specific property of an entity
    /// </summary>
    /// <typeparam name="T">Type of entity to validate</typeparam>
    /// <typeparam name="TProperty">Type of the property</typeparam>
    public interface IPropertyValidationRule<T, TProperty> : IPiQValidationRule<T> where T : class
    {
        /// <summary>
        /// Gets the name of the property being validated
        /// </summary>
        string PropertyName { get; }
    }
}