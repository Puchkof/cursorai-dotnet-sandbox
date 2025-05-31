using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HeroBoxAI.WebApi.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = exception switch
        {
            ValidationException => CreateErrorResponse(HttpStatusCode.BadRequest, exception.Message),
            UserAlreadyExistsException => CreateErrorResponse(HttpStatusCode.Conflict, exception.Message),
            ConflictException => CreateErrorResponse(HttpStatusCode.Conflict, exception.Message),
            InvalidCredentialsException => CreateErrorResponse(HttpStatusCode.Unauthorized, exception.Message),
            UserNotFoundException => CreateErrorResponse(HttpStatusCode.NotFound, exception.Message),
            ForbiddenException => CreateErrorResponse(HttpStatusCode.Forbidden, exception.Message),
            _ => CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = response.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

        return true;
    }

    private ProblemDetails CreateErrorResponse(HttpStatusCode status, string detail)
    {
        return new ProblemDetails
        {
            Status = (int)status,
            Title = status.ToString(),
            Detail = detail
        };
    }
} 