using System.Net;
using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FluentValidation;

namespace FitMetrics.API.Middleware;

/// <summary>
/// Uygulama exception'larını tutarlı JSON yanıtlara ve doğru HTTP durum kodlarına dönüştürür.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, message, errors) = ex switch
        {
            ValidationException ve => (
                (int)HttpStatusCode.BadRequest,
                "Doğrulama hatası.",
                ve.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToArray()),
            NotFoundException nf => ((int)HttpStatusCode.NotFound, nf.Message, Array.Empty<string>()),
            ConflictException cf => ((int)HttpStatusCode.Conflict, cf.Message, Array.Empty<string>()),
            UnauthorizedException ua => ((int)HttpStatusCode.Unauthorized, ua.Message, Array.Empty<string>()),
            _ => ((int)HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.", Array.Empty<string>())
        };

        if (status == (int)HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "İşlenmeyen istisna oluştu");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = JsonSerializer.Serialize(
            new { status, message, errors },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(payload);
    }
}
