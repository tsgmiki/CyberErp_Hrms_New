using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using CyberErp.Hrms.App.Common.Exceptions;

namespace CyberErp.Hrms.Api.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        {
            context.Response.ContentType = "application/json";

            // Map exceptions to appropriate HTTP Status Codes
            var statusCode = exception switch
            {
                // Handle the specific Login error from your trace
                KeyNotFoundException => (int)HttpStatusCode.Unauthorized,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                NotFoundException => (int)HttpStatusCode.NotFound,
                ValidationException => (int)HttpStatusCode.BadRequest,
                HrmsException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            // Handled business rejections (4xx) are expected — a short warning is enough.
            // Only genuine server faults deserve a full error + stack trace in the log.
            if (statusCode >= 500)
                logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            else
                logger.LogWarning("Request rejected ({StatusCode}): {Message}", statusCode, exception.Message);

            context.Response.StatusCode = statusCode;

            // Validation failures include the per-field errors so clients can show which
            // field is wrong instead of a generic message.
            object response = exception is ValidationException validationException
                ? new { message = exception.Message, errors = validationException.Errors }
                : new { message = exception.Message };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}