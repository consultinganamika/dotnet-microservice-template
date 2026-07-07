using Employee.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Employee.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ErrorHandlingMiddleware> logger)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = notFoundEx.Message;
                    response.Type = "Not Found";
                    logger.LogWarning(notFoundEx, "Not found exception");
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Validation failed";
                    response.Type = "Bad Request";
                    response.Details = validationEx.Errors;
                    logger.LogWarning(validationEx, "Validation exception");
                    break;

                case UnauthorizedException unauthorizedEx:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = unauthorizedEx.Message;
                    response.Type = "Unauthorized";
                    logger.LogWarning(unauthorizedEx, "Unauthorized exception");
                    break;

                case ForbiddenException forbiddenEx:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = forbiddenEx.Message;
                    response.Type = "Forbidden";
                    logger.LogWarning(forbiddenEx, "Forbidden exception");
                    break;

                case ConflictException conflictEx:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response.Message = conflictEx.Message;
                    response.Type = "Conflict";
                    logger.LogWarning(conflictEx, "Conflict exception");
                    break;

                case DownstreamServiceException downstreamEx:
                    context.Response.StatusCode = StatusCodes.Status502BadGateway;
                    response.Message = "External service error";
                    response.Type = "Bad Gateway";
                    logger.LogError(downstreamEx, "Downstream service exception");
                    break;

                case NotImplementedException notImplementedEx:
                    context.Response.StatusCode = StatusCodes.Status501NotImplemented;
                    response.Message = notImplementedEx.Message;
                    response.Type = "Not Implemented";
                    logger.LogWarning(notImplementedEx, "Not implemented exception");
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = "An error occurred processing your request";
                    response.Type = "Internal Server Error";
                    logger.LogError(exception, "Unhandled exception");
                    break;
            }

            response.TraceId = context.TraceIdentifier;
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsJsonAsync(response, options);
        }
    }

    public class ErrorResponse
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }
        public object Details { get; set; }
    }
}
