using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Common.Services
{
    public class ExceptionHandlingResult
    {
        public bool IsHandled { get; set; }
        public bool ShouldRetry { get; set; }
        public bool IsTransient { get; set; }
        public string? ErrorCode { get; set; }
        public string? UserMessage { get; set; }
        public Exception? OriginalException { get; set; }
        public object? FallbackValue { get; set; }

        public static ExceptionHandlingResult Success() => new() { IsHandled = true };
        public static ExceptionHandlingResult Failure(Exception ex) => new() { IsHandled = false, OriginalException = ex };
        public static ExceptionHandlingResult Retryable(Exception ex, bool isTransient = true) => new() { IsHandled = true, ShouldRetry = true, IsTransient = isTransient, OriginalException = ex };
        public static ExceptionHandlingResult Fallback(Exception ex, object? fallbackValue) => new() { IsHandled = true, FallbackValue = fallbackValue, OriginalException = ex };
    }

    public class ExceptionHandlingContext
    {
        public string OperationName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public object? FallbackValue { get; set; }
        public IDictionary<string, object?> Metadata { get; set; } = new Dictionary<string, object?>();
    }

    public interface IExceptionHandler
    {
        ExceptionHandlingResult Handle(Exception exception, ExceptionHandlingContext? context = null);

        T ExecuteWithHandling<T>(Func<T> operation, Func<Exception, T>? fallback = null, ExceptionHandlingContext? context = null);

        Task<T> ExecuteWithHandlingAsync<T>(Func<Task<T>> operation, Func<Exception, T>? fallback = null, ExceptionHandlingContext? context = null);

        Task<T> ExecuteWithHybridStrategyAsync<T>(
            Func<Task<T>> operation,
            Func<Task<T>>? fallback,
            string circuitType,
            ExceptionHandlingContext? context);

        Task ExecuteWithHybridStrategyAsync(
            Func<Task> operation,
            Func<Task>? fallback,
            string circuitType,
            ExceptionHandlingContext? context);
    }
}
