// PiQApi.Core/Documentation/OperationPatternsGuide.json
```json
{
  "documentPath": "PiQApi.Core/Documentation/OperationPatternsGuide.json",
  "version": "1.0.0",
  "lastUpdated": "2025-04-06",
  "purpose": "Define standardized patterns for implementing operations in the piqification API",
  "guide_type": "pattern",
  "development_contexts": ["core", "exchange.core", "exchange.service", "testing", "all"],
  "applies_to": ["PiQApi.Core", "PiQApi.Service"],

  "key_principles": {
    "lifecycle_management": {
      "description": "Operations follow a well-defined lifecycle (Initialize → Execute → Cleanup → Dispose)",
      "rationale": "Predictable lifecycle simplifies state management and resource tracking"
    },
    "execution_tracking": {
      "description": "All operation executions are tracked with metrics, logging, and error handling",
      "rationale": "Comprehensive tracking enables operational visibility and diagnostics"
    },
    "thread_safety": {
      "description": "Operations are thread-safe through the use of async locks",
      "rationale": "Thread-safety prevents race conditions and ensures data integrity"
    },
    "testability": {
      "description": "Operations accept interfaces for time providers, loggers, and other dependencies",
      "rationale": "Interface-based dependencies enable mocking and unit testing"
    }
  },

  "components": {
    "PiQOperationBase": {
      "interface": "IPiQOperationBase",
      "purpose": "Base implementation for all operations with lifecycle management",
      "key_properties": [
        "CorrelationId - String - Unique identifier for correlating logs and metrics",
        "OperationId - String - Unique identifier for the operation instance",
        "IsActive - Boolean - Indicates if operation is currently active",
        "IsReady - Boolean - Indicates if operation has been initialized"
      ],
      "key_methods": [
        "InitializeAsync(CancellationToken) - Task - Prepares the operation for execution",
        "ValidateStateAsync(CancellationToken) - Task - Validates operation state",
        "IsOperationalAsync(CancellationToken) - Task<bool> - Checks if operation can be executed",
        "CleanupAsync(CancellationToken) - Task - Releases resources",
        "DisposeAsync() - ValueTask - Asynchronously disposes the operation"
      ],
      "implementation": "PiQOperationBase",
      "implementation_details": [
        "Implements IAsyncDisposable for async resource cleanup",
        "Tracks operation timing, status, and metrics",
        "Thread-safe through IPiQAsyncLock",
        "Comprehensive logging with LoggerMessage delegates"
      ]
    },
    "PiQOperationBase<TResult>": {
      "interface": "IPiQOperation<TResult>",
      "purpose": "Base implementation for operations that return a specific result type",
      "key_properties": [
        "Inherits all properties from PiQOperationBase"
      ],
      "key_methods": [
        "ExecuteAsync(CancellationToken) - Task<TResult> - Executes the operation and returns result",
        "ExecuteWithTrackingAsync(Func<CancellationToken, Task<TResult>>, string, CancellationToken) - Task<TResult> - Helper for tracked execution"
      ],
      "implementation": "PiQOperationBase<TResult>",
      "implementation_details": [
        "Extends PiQOperationBase with strongly-typed result handling",
        "Provides abstraction for executing operations with proper tracking"
      ]
    },
    "PiQValidationProcessor": {
      "interface": "IPiQValidationProcessor",
      "purpose": "Processes validation rules against entities",
      "key_methods": [
        "RegisterRules<T>(IEnumerable<IPiQValidationRule<T>>) - void - Registers validation rules for type T",
        "Validate<T>(T, PiQValidationContext) - IPiQValidationResult - Validates entity synchronously",
        "ValidateAsync<T>(T, PiQValidationContext, CancellationToken) - Task<IPiQValidationResult> - Validates entity asynchronously"
      ],
      "implementation": "PiQValidationProcessor",
      "implementation_details": [
        "Thread-safe rule registration and execution",
        "Performance optimization with expression compilation",
        "Comprehensive metrics tracking",
        "Support for synchronous and asynchronous validation"
      ]
    }
  },

  "implementation_patterns": {
    "operation_execution": {
      "pattern": "Execute operations with tracking and error handling",
      "example_code": "protected async Task<T> ExecuteOperationAsync<T>(\n    Func<CancellationToken, Task<T>> operation,\n    string operationName,\n    CancellationToken cancellationToken)\n{\n    // Ensure initialized\n    if (!IsReady) {\n        await InitializeAsync(cancellationToken);\n    }\n    \n    // Track metrics\n    Context.Metrics.StartTimer(operationName);\n    \n    try {\n        // Execute operation\n        return await operation(cancellationToken);\n    }\n    catch (Exception ex) {\n        // Log error and record metrics\n        await LogErrorAsync(ex);\n        throw;\n    }\n    finally {\n        // Record timing\n        Context.Metrics.StopTimer(operationName);\n    }\n}",
      "key_aspects": [
        "Always initialize before execution",
        "Track operation timing and metrics",
        "Structured error handling with correlation IDs",
        "Resource cleanup in finally block"
      ],
      "when_to_use": "When implementing new operation classes that need proper tracking and error handling"
    },
    "async_validation": {
      "pattern": "Validate entities asynchronously with cancellation support",
      "example_code": "public async Task<IPiQValidationResult> ValidateAsync<T>(\n    T entity,\n    PiQValidationContext context, \n    CancellationToken cancellationToken)\n{\n    // Get rules\n    var rules = GetRulesForType<T>().ToList();\n    \n    // Execute rules\n    var tasks = rules.Select(rule => \n        rule.ValidateAsync(entity, context, cancellationToken));\n    var results = await Task.WhenAll(tasks);\n    \n    // Combine results\n    return _factory.Combine(results);\n}",
      "key_aspects": [
        "Parallel rule execution with Task.WhenAll",
        "Proper cancellation support",
        "Result aggregation",
        "Thread-safe rule resolution"
      ],
      "when_to_use": "When implementing validation that needs to be efficient and cancellable"
    }
  },

  "common_pitfalls": {
    "missing_initialization": {
      "issue": "Using operations without calling InitializeAsync first",
      "solution": "Always call InitializeAsync before first use, or use ExecuteWithTrackingAsync which handles initialization"
    },
    "improper_disposal": {
      "issue": "Not properly disposing operations, leading to resource leaks",
      "solution": "Use 'await using' with all operation instances or explicitly call DisposeAsync"
    },
    "insufficient_error_handling": {
      "issue": "Not capturing correlation IDs and operation context in exceptions",
      "solution": "Use LogErrorAsync method which enriches exceptions with context information"
    },
    "thread_safety_issues": {
      "issue": "Not properly synchronizing access to shared state",
      "solution": "Use the IPiQAsyncLock provided in the constructor for all state modifications"
    }
  },

  "architectural_standards": {
    "operation_composition": {
      "rule": "Complex operations should be composed of simpler operations",
      "rationale": "Promotes reusability and separation of concerns",
      "examples": {
        "correct": "// Correct\npublic async Task<PiQResult> ExecuteAsync()\n{\n    var subResult1 = await _subOperation1.ExecuteAsync();\n    var subResult2 = await _subOperation2.ExecuteAsync();\n    return CombineResults(subResult1, subResult2);\n}",
        "incorrect": "// Incorrect\npublic async Task<PiQResult> ExecuteAsync()\n{\n    // Implementing all functionality in one large method\n    // instead of using smaller operations\n    var data1 = await _repository1.GetDataAsync();\n    // Many more direct data access and processing calls\n    return new PiQResult();\n}"
      }
    },
    "context_propagation": {
      "rule": "Always propagate the operation context through the call chain",
      "rationale": "Ensures correlation ID and metrics are preserved",
      "examples": {
        "correct": "// Correct\nawait _subOperation.ExecuteAsync(_context, cancellationToken);",
        "incorrect": "// Incorrect\nawait _subOperation.ExecuteAsync(new PiQOperationContext(), cancellationToken);"
      }
    }
  }
}
```
