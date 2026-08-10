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
                var statusCode = exception switch
                {
                    FluentValidation.ValidationException => StatusCodes.Status400BadRequest,

                    AppException appException => GetStatusCode(appException),

                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;

                var problemDetails = CreateProblemDetails(
                    context,
                    exception,
                    statusCode);

                await problemDetailsService.WriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = problemDetails
                    });
            }
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context,Exception exception, int statusCode)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Validation Error",
                    StatusCodes.Status401Unauthorized => "Unauthorized",
                    StatusCodes.Status403Forbidden => "Forbidden",
                    StatusCodes.Status404NotFound => "Not Found",
                    _ => "An error occurred"
                },
                Instance = context.Request.Path
            };

            if (exception is FluentValidation.ValidationException validationException)
            {
                problemDetails.Extensions["errors"] =
                    validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group
                                .Select(error => error.ErrorMessage)
                                .ToArray());

                return problemDetails;
            }
            problemDetails.Detail = "An unexpected error occurred.";

            if (exception is AppException appException) problemDetails.Detail = appException.Message;

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
