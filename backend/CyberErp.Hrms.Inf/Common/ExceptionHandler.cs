using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Sockets;

namespace CyberErp.Hrms.Inf.Common
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public ExceptionHandlingResult Handle(Exception exception, ExceptionHandlingContext? context = null)
        {
            context ??= new ExceptionHandlingContext();
            var result = new ExceptionHandlingResult
            {
                OriginalException = exception,
                ErrorCode = "BB000"
            };

            switch (exception)
            {
                case NotFoundException:
                    result.IsHandled = true;
                    result.UserMessage = $"The requested {context.EntityType} was not found.";
                    result.ErrorCode = "BB001";
                    _logger.LogWarning("Resource not found: {Message}", exception.Message);
                    break;

                case DuplicateException:
                    result.IsHandled = true;
                    result.UserMessage = "A duplicate resource already exists.";
                    result.ErrorCode = "BB002";
                    _logger.LogWarning("Duplicate resource: {Message}", exception.Message);
                    break;

                case ValidationException validationEx:
                    result.IsHandled = true;
                    result.UserMessage = "Validation failed for one or more fields.";
                    result.ErrorCode = "BB003";
                    _logger.LogWarning("Validation failed: {Errors}",
                        string.Join(", ", validationEx.Errors.SelectMany(kv => kv.Value)));
                    break;

                case UnauthorizedException:
                    result.IsHandled = true;
                    result.UserMessage = "You are not authorized to perform this action.";
                    result.ErrorCode = "BB004";
                    _logger.LogWarning("Unauthorized access: {Message}", exception.Message);
                    break;

                case DbUpdateException:
                    result.IsHandled = false;
                    result.ErrorCode = "BB005";
                    _logger.LogError(exception, "Database update error during {Operation}", context.OperationName);
                    break;

                case TimeoutException:
                    result.IsHandled = true;
                    result.ShouldRetry = true;
                    result.IsTransient = true;
                    result.UserMessage = "The operation timed out. Please try again.";
                    result.ErrorCode = "BB006";
                    _logger.LogWarning("Timeout during {Operation}", context.OperationName);
                    break;

                case OperationCanceledException:
                    result.IsHandled = true;
                    result.UserMessage = "The operation was cancelled.";
                    result.ErrorCode = "BB007";
                    _logger.LogWarning("Operation cancelled: {Operation}", context.OperationName);
                    break;

                case CircuitBreakerOpenException cbEx:
                    result.IsHandled = true;
                    result.ShouldRetry = false;
                    result.UserMessage = $"Service is temporarily unavailable. Please try again after {cbEx.OpenUntil:HH:mm}.";
                    result.ErrorCode = "BB008";
                    _logger.LogWarning(cbEx, "Circuit breaker open: {CircuitName}", cbEx.CircuitName);
                    break;

                case HrmsException bbEx:
                    result.IsHandled = true;
                    result.ErrorCode = bbEx.ErrorCode;
                    _logger.LogError(bbEx, "Business exception during {Operation}", context.OperationName);
                    break;

                default:
                    result.IsHandled = false;
                    result.IsTransient = IsTransientException(exception);
                    result.ErrorCode = "BB999";
                    _logger.LogError(exception, "Unhandled exception during {Operation}", context.OperationName);
                    break;
            }

            return result;
        }

        public T ExecuteWithHandling<T>(Func<T> operation, Func<Exception, T>? fallback = null, ExceptionHandlingContext? context = null)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                var result = Handle(ex, context);

                if (fallback != null)
                {
                    try
                    {
                        return fallback(ex);
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Fallback also failed for {Operation}", context?.OperationName ?? "Unknown");
                        throw new AggregateException("Primary operation and fallback both failed", new[] { ex, fallbackEx });
                    }
                }

                if (!result.IsHandled)
                {
                    throw;
                }

                return default!;
            }
        }

        public async Task<T> ExecuteWithHandlingAsync<T>(Func<Task<T>> operation, Func<Exception, T>? fallback = null, ExceptionHandlingContext? context = null)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                var result = Handle(ex, context);
                if (result.FallbackValue != null)
                {
                    return (T)result.FallbackValue;
                }
                if (fallback != null)
                {
                    return fallback(ex);
                }
                throw;
            }
        }

        public async Task<T> ExecuteWithCircuitBreakerAsync<T>(
            Func<Task<T>> operation,
            string circuitName = "Default",
            bool recordSuccessOnCompletion = true)
        {
            return await operation();
        }

        public async Task<T> ExecuteWithFallbackAsync<T>(
            Func<Task<T>> operation,
            Func<Task<T>>? fallback,
            ExceptionHandlingContext? context = null)
        {
            if (fallback == null)
            {
                return await operation();
            }

            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary operation failed, executing fallback for {Operation}",
                    context?.OperationName ?? "Unknown");

                try
                {
                    return await fallback();
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback also failed for {Operation}",
                        context?.OperationName ?? "Unknown");
                    throw new AggregateException(
                        $"Both primary and fallback operations failed for {context?.OperationName ?? "Unknown"}",
                        new[] { ex, fallbackEx });
                }
            }
        }

        public async Task<T> ExecuteWithHybridStrategyAsync<T>(
            Func<Task<T>> operation,
            Func<Task<T>>? fallback = null,
            string circuitName = "Default",
            ExceptionHandlingContext? context = null)
        {
            context ??= new ExceptionHandlingContext
            {
                OperationName = "Unknown"
            };

            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Operation failed for {Operation}", context.OperationName);

                // Business/domain exceptions (auth, validation, not-found, ...) are meaningful
                // results, not infrastructure failures. Let them propagate to the mapping
                // middleware instead of masking them behind the DB fallback (which would turn
                // e.g. "Invalid username or password" into a 500).
                if (ex is HrmsException)
                {
                    throw;
                }

                if (fallback != null)
                {
                    try
                    {
                        return await fallback();
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Fallback also failed for {Operation}", context.OperationName);
                        throw new AggregateException(
                            $"Both primary and fallback operations failed for {context.OperationName}",
                            new[] { ex, fallbackEx });
                    }
                }

                throw;
            }
        }

        public async Task ExecuteWithHybridStrategyAsync(
            Func<Task> operation,
            Func<Task>? fallback = null,
            string circuitName = "Default",
            ExceptionHandlingContext? context = null)
        {
            context ??= new ExceptionHandlingContext
            {
                OperationName = "Unknown"
            };

            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Operation failed for {Operation}", context.OperationName);

                // Business/domain exceptions are meaningful results, not infrastructure
                // failures — let them propagate instead of masking them behind the fallback.
                if (ex is HrmsException)
                {
                    throw;
                }

                if (fallback != null)
                {
                    try
                    {
                        await fallback();
                        return;
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Fallback also failed for {Operation}", context.OperationName);
                        throw new AggregateException(
                            $"Both primary and fallback operations failed for {context.OperationName}",
                            new[] { ex, fallbackEx });
                    }
                }

                throw;
            }
        }

        private static bool IsTransientException(Exception exception)
        {
            return exception switch
            {
                DbUpdateException => true,
                TimeoutException => true,
                // NOT InvalidOperationException: domain state-machine guards throw it — retrying
                // a business-rule violation can never succeed (and may repeat side effects).
                HttpRequestException => true,
                SocketException => true,
                IOException => true,
                _ => exception.GetType().Name.Contains("SqlException", StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
