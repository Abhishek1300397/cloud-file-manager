using CloudStorage.Application.Common.Enums;
using CloudStorage.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.Api.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IProblemDetailsService problemDetailsService)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "An unhandled exception occurred while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                var statusCode = exception is AppException appException ? GetStatusCode(appException) : StatusCodes.Status500InternalServerError;

                context.Response.StatusCode = statusCode;

                var problemDetails = CreateProblemDetails(
                    context,
                    exception,
                    statusCode);

                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails
                });
            }
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, int statusCode)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = exception is AppException appException ? appException.Title : "Internal Server Error",
                Type = $"https://httpstatuses.com/{statusCode}",
                Detail = statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            return problemDetails;
        }


    

        private static int GetStatusCode(AppException exception)
        {
            return exception.ErrorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
