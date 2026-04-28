// PiQApi.Core/Validation/Framework/PiQValidationChain.Logging.cs
using PiQApi.Abstractions.Enums;
using Microsoft.Extensions.Logging;

namespace PiQApi.Core.Validation.Framework;

public partial class PiQValidationChain
{
    // LoggerMessage delegates for better performance
    private static readonly Func<ILogger, string, IDisposable?> _validationChainScope =
        LoggerMessage.DefineScope<string>("Validation Chain: {ChainName}");

    private static readonly Func<ILogger, string, IDisposable?> _asyncValidationChainScope =
        LoggerMessage.DefineScope<string>("Async Validation Chain: {ChainName}");

    private static readonly Action<ILogger, string, Exception?> _logStartingValidation =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, "StartingValidation"),
            "Starting validation for entity type {EntityType}");

    private static readonly Action<ILogger, string, Exception?> _logStartingAsyncValidation =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, "StartingAsyncValidation"),
            "Starting async validation for entity type {EntityType}");

    private static readonly Action<ILogger, Exception?> _logNoProcessors =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(3, "NoProcessors"),
            "No processors in validation chain");

    private static readonly Action<ILogger, Exception?> _logNoProcessorsAsync =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(4, "NoProcessorsAsync"),
            "No processors in async validation chain");

    private static readonly Action<ILogger, ValidationModeType, bool, Exception?> _logValidationMode =
        LoggerMessage.Define<ValidationModeType, bool>(
            LogLevel.Debug,
            new EventId(5, "ValidationMode"),
            "Validation mode: {ValidationMode}, AggregateAllErrors: {AggregateAllErrors}");

    private static readonly Action<ILogger, ValidationModeType, bool, Exception?> _logAsyncValidationMode =
        LoggerMessage.Define<ValidationModeType, bool>(
            LogLevel.Debug,
            new EventId(6, "AsyncValidationMode"),
            "Async validation mode: {ValidationMode}, AggregateAllErrors: {AggregateAllErrors}");

    private static readonly Action<ILogger, string, Exception?> _logExecutingProcessor =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(7, "ExecutingProcessor"),
            "Executing processor: {ProcessorName}");

    private static readonly Action<ILogger, string, Exception?> _logExecutingAsyncProcessor =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(8, "ExecutingAsyncProcessor"),
            "Executing async processor: {ProcessorName}");

    private static readonly Action<ILogger, int, Exception?> _logProcessorReturned =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(9, "ProcessorReturned"),
            "Processor returned with {ErrorCount} errors");

    private static readonly Action<ILogger, int, Exception?> _logAsyncProcessorReturned =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(10, "AsyncProcessorReturned"),
            "Async processor returned with {ErrorCount} errors");

    private static readonly Action<ILogger, Exception?> _logStoppingValidation =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(11, "StoppingValidation"),
            "Stopping validation due to errors in strict mode");

    private static readonly Action<ILogger, Exception?> _logStoppingAsyncValidation =
        LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(12, "StoppingAsyncValidation"),
            "Stopping async validation due to errors in strict mode");

    private static readonly Action<ILogger, int, Exception?> _logValidationComplete =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(13, "ValidationComplete"),
            "Validation complete with {ErrorCount} errors");

    private static readonly Action<ILogger, int, Exception?> _logAsyncValidationComplete =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(14, "AsyncValidationComplete"),
            "Async validation complete with {ErrorCount} errors");

    private static readonly Action<ILogger, Exception?> _logValidationCancelled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(15, "ValidationCancelled"),
            "Validation cancelled before execution");

    private static readonly Action<ILogger, Exception?> _logValidationCancelledDuringExecution =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(16, "ValidationCancelledDuringExecution"),
            "Validation cancelled during execution");
}