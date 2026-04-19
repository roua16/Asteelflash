namespace ITStockM.WebApi.Middleware;

using FluentValidation;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using BusinessRuleViolationException = ITStockM.Domain.Exceptions.BusinessRuleViolationException;
using DomainInvalidOperationException = ITStockM.Domain.Exceptions.InvalidOperationException;
using EntityNotFoundException = ITStockM.Domain.Exceptions.EntityNotFoundException;

/// <summary>
/// Comprehensive global exception handling middleware for standardized error responses.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Success = false,
            Message = exception.Message,
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };

        return exception switch
        {
            EntityNotFoundException => HandleEntityNotFoundException(context, (EntityNotFoundException)exception, response),
            BusinessRuleViolationException => HandleBusinessRuleViolationException(context, (BusinessRuleViolationException)exception, response),
            DomainInvalidOperationException => HandleDomainInvalidOperationException(context, (DomainInvalidOperationException)exception, response),
            ValidationException => HandleValidationException(context, (ValidationException)exception, response),
            ArgumentNullException => HandleArgumentNullException(context, (ArgumentNullException)exception, response),
            ArgumentException => HandleArgumentException(context, (ArgumentException)exception, response),
            UnauthorizedAccessException => HandleUnauthorizedException(context, response),
            KeyNotFoundException => HandleNotFoundException(context, response),
            System.InvalidOperationException => HandleInvalidOperationException(context, response),
            _ => HandleGenericException(context, response)
        };
    }

    private static Task HandleEntityNotFoundException(HttpContext context, EntityNotFoundException exception, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Message = exception.Message;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleBusinessRuleViolationException(HttpContext context, BusinessRuleViolationException exception, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        response.Message = exception.Message;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleDomainInvalidOperationException(HttpContext context, DomainInvalidOperationException exception, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        response.Message = exception.Message;
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleValidationException(HttpContext context, ValidationException exception, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.Message = "Validation failed";
        response.Details = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleArgumentNullException(HttpContext context, ArgumentNullException exception, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.Message = $"Required parameter '{exception.ParamName}' is null";
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleArgumentException(HttpContext context, ArgumentException exception, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.Message = $"Invalid argument: {exception.Message}";
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleUnauthorizedException(HttpContext context, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        response.Message = "Unauthorized access";
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleNotFoundException(HttpContext context, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Message = "Resource not found";
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleInvalidOperationException(HttpContext context, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.Message = response.Message ?? "Invalid operation";
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleGenericException(HttpContext context, ErrorResponse response)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.Message = "An unexpected error occurred";
        return context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Standardized error response format for all API errors.
    /// </summary>
    private sealed class ErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("traceId")]
        public required string TraceId { get; set; }

        [JsonPropertyName("details")]
        public object? Details { get; set; }
    }
}
