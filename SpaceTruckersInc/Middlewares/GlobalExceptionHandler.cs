using SpaceTruckersInc.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace SpaceTruckersInc.Middlewares;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status = exception switch
        {
            EntityNotFoundException => HttpStatusCode.NotFound,
            ConcurrencyConflictException => HttpStatusCode.Conflict,
            DomainException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var payload = new
        {
            error = exception.GetType().Name,
            message = exception.Message
        };

        string json = JsonSerializer.Serialize(payload);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsync(json);
    }
}