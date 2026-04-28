# Validation Framework Architecture

This document provides an overview of the validation framework architecture, explaining key components and their relationships.

## Core Components

### 1. Validation Rules

Validation rules are the fundamental building blocks of the validation system. They define how to validate specific types or properties.

**Class Hierarchy:**
- `PiQValidationRule<T>` - Base abstract class for all validation rules
  - `PiQBaseValidationRule<T>` - Adds common validation patterns
    - `PiQSyncValidationRule<T>` - Optimized for synchronous validation
    - `PiQAsyncValidationRule<T>` - Optimized for asynchronous validation
  - `PiQPropertyValidationRule<T>` - For validating specific properties
  - `PiQHeaderValidationRule<T>` - For validating headers
  - `PiQFileExtensionValidationRule<T>` - For validating file extensions
  - `PiQCompositeValidationRule<T>` - For composing multiple validation rules

### 2. Validation Context

Validation context provides information about the current validation operation.

- `PiQValidationContext` - Holds validation state, mode, options, and correlation information
- `PiQValidationContextFactory` - Creates and configures validation contexts

### 3. Validation Results

Validation results represent the outcome of validation operations.

- `PiQValidationResult` - Abstract base class for validation results
  - `PiQCoreValidationResult` - Concrete implementation
- `PiQValidationResultFactory` - Creates different types of validation results
- `PiQValidationResultBuilder` - Builds validation results with a fluent API

### 4. Validation Processor

The validation processor orchestrates the validation process.

- `PiQValidationProcessor` - Main processor that coordinates rule execution
  - `PiQValidationProcessor.Registration` - Handles rule registration
  - `PiQValidationProcessor.Sync` - Synchronous validation methods
  - `PiQValidationProcessor.Async` - Asynchronous validation methods
  - `PiQValidationProcessor.Helpers` - Helper methods for validation

### 5. Resources

Resources are used during validation operations.

- `PiQValidationRuleResource` - Represents a validation rule resource
- `PiQValidationRuleResourceBuilder` - Builds validation rule resources

## Architectural Principles

1. **Separation of Concerns**
   - Each component has a specific responsibility
   - Rules define validation logic, processors coordinate execution, context provides state

2. **Composition over Inheritance**
   - Composite validation rules combine multiple rules
   - Processors compose rule results

3. **Interface-Based Design**
   - All components implement interfaces for testability and extensibility
   - Factories create concrete implementations of interfaces

4. **Thread Safety**
   - Thread-safe collections and operations for concurrent validation
   - Immutable results and contexts where appropriate

5. **Async-First Approach**
   - First-class support for asynchronous validation
   - Synchronous operations provided for backward compatibility

6. **Resource Tracking**
   - Resources created during validation are tracked and cleaned up
   - Correlation IDs provide traceability

7. **Error Aggregation**
   - Errors can be collected and aggregated or fail-fast
   - Configurable validation modes (Standard, Strict)

## Usage Patterns

### Basic Validation

```csharp
// Create a validation context
var context = _validationContextFactory.CreateForService("1.0");

// Validate an entity
var result = _validationProcessor.ValidateAsync(entity, context);

// Check the result
if (!result.IsValid)
{
    // Handle validation errors
}
```

### Composite Validation

```csharp
// Create composite rule
var compositeRule = new PiQCompositeValidationRule<Entity>(
    new[] {
        new RequiredPropertyRule(),
        new FormatValidationRule(),
        new BusinessLogicRule()
    }
);

// Register the rule
_validationProcessor.RegisterRules(new[] { compositeRule });
```

### Custom Rule Creation

```csharp
public class MyCustomRule : PiQSyncValidationRule<MyEntity>
{
    public MyCustomRule(ILogger<MyCustomRule> logger) : base(logger)
    {
        RuleId = "MyCustomRule";
    }

    protected override IPiQValidationResult ValidateInternal(
        MyEntity entity,
        PiQValidationContext context)
    {
        var builder = CreateBuilder(context.CorrelationId);

        // Validation logic here
        if (!IsValid(entity))
        {
            builder.WithError("PropertyName", "Error message");
        }

        return builder.Build();
    }
}
```

## Best Practices

1. **Rule Registration**
   - Register rules at application startup
   - Use dependency injection for rule creation

2. **Resource Management**
   - Create resources with unique IDs
   - Dispose of resources after use

3. **Performance Optimization**
   - Use compiled expressions for type-specific validation
   - Cache validation results when appropriate

4. **Error Handling**
   - Provide clear error messages with property paths
   - Use appropriate error codes and severities

5. **Testing**
   - Test validation rules in isolation
   - Use mocks for dependencies
