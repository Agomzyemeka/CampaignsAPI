using System.Net;
using System.Text.Json;
using CampaignsAPI.DTOs;

namespace CampaignsAPI.Middleware
{
    /// <summary>
    /// Global Exception Handling Middleware
    /// Purpose: Centralized exception handling for all API requests
    /// Interview Notes:
    /// - Catches all unhandled exceptions
    /// - Returns consistent error responses
    /// - Logs exceptions for monitoring
    /// - Prevents sensitive error details from leaking to clients
    /// - Different responses for development vs production
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handle exception and return appropriate response
        /// Interview Note: Returns different detail levels based on environment
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while processing your request."
            };

            // Set status code based on exception type
            switch (exception)
            {
                case ArgumentException:
                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access.";
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Resource not found.";
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // Include stack trace in development environment
            // Interview Note: Never expose internal details in production
            if (_environment.IsDevelopment())
            {
                response.Errors = new List<string>
                {
                    exception.Message,
                    exception.StackTrace ?? "No stack trace available"
                };
            }
            else
            {
                response.Errors = new List<string> { "Please contact support if the problem persists." };
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Extension method to register middleware
    /// Interview Note: Follows ASP.NET Core middleware pattern
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
