using System.Net;
using System.Text.Json;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;

namespace JobPortal.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (HttpStatusCode.BadRequest, CreateValidationFailureResponse(validationException)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ApiResponse<object>.FailResponse(exception.Message)),
            ForbiddenException forbiddenException => (HttpStatusCode.Forbidden, ApiResponse<object>.FailResponse(forbiddenException.Message)),
            NotFoundException notFoundException => (HttpStatusCode.NotFound, ApiResponse<object>.FailResponse(notFoundException.Message)),
            ConflictException conflictException => (HttpStatusCode.Conflict, ApiResponse<object>.FailResponse(conflictException.Message)),
            BusinessException businessException => (HttpStatusCode.BadRequest, ApiResponse<object>.FailResponse(businessException.Message)),
            ApiException apiException => ((HttpStatusCode)apiException.StatusCode, ApiResponse<object>.FailResponse(apiException.Message)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.FailResponse("An unexpected error occurred."))
        };

        if (_environment.IsDevelopment() && exception is not ValidationException && exception is not ApiException)
        {
            response.Errors.Add(new DTOs.Common.ApiErrorItem
            {
                Field = string.Empty,
                Message = exception.Message
            });
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static ApiResponse<object> CreateValidationFailureResponse(ValidationException validationException)
    {
        var errors = validationException.Errors
            .Select(error => new ApiErrorItem
            {
                Field = error.PropertyName,
                Message = error.ErrorMessage
            })
            .ToList();

        return ApiResponse<object>.FailResponse("Validation failed.", errors);
    }
}
