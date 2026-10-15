using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.Exceptions;

namespace Products.API.Middleware;

/// <summary>
/// Converts well known exceptions into RFC 7807 problem responses.
/// Stack traces and internal details are never leaked outside the Development environment.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception) when (!context.Response.HasStarted)
        {
            logger.LogWarning(
                "Request validation failed for {Path}: {Errors}",
                context.Request.Path,
                JsonSerializer.Serialize(exception.Errors.Select(error => error.ErrorMessage)));

            var errors = exception.Errors
                .GroupBy(error => ToCamelCase(error.PropertyName ?? string.Empty))
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Detail = "The request contains invalid values.",
                Instance = context.Request.Path.Value
            };

            await WriteProblemAsync(context, problem);
        }
        catch (DomainException exception) when (!context.Response.HasStarted)
        {
            logger.LogWarning(
                "Domain rule violated for {Path}: {Message}",
                context.Request.Path,
                exception.Message);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "A business rule was violated.",
                Detail = exception.Message,
                Instance = context.Request.Path.Value
            };

            await WriteProblemAsync(context, problem);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An internal server error occurred.",
                Detail = environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path.Value
            };

            await WriteProblemAsync(context, problem);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            problem,
            problem.GetType(),
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }

    private static string ToCamelCase(string value)
        => string.IsNullOrEmpty(value) || char.IsLower(value[0])
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
}
