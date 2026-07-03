using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Services;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.Inf.Common;

public static class RepositoryExecutor
{
    public static ExceptionHandlingContext CreateContext(
        string operationName,
        string entityType) =>
        new()
        {
            OperationName = operationName,
            EntityType = entityType
        };

    public static async Task<TResult> ExecuteAsync<TResult>(
        IExceptionHandler exceptionHandler,
        ILogger logger,
        ExceptionHandlingContext context,
        string operationLabel,
        Func<Task<TResult>> operation)
    {
        async Task<TResult> fallback()
        {
            logger.LogError("Failed to {Operation}", context.OperationName);
            throw new DatabaseException(operationLabel, context.EntityType, null);
        }

        return await exceptionHandler.ExecuteWithHybridStrategyAsync(
            operation,
            fallback,
            "Database",
            context);
    }
}
