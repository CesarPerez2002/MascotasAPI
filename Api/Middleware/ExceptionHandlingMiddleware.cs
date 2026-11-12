using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(
                exception,
                "Ocurrió un error al procesar la solicitud.");

            await ManejarExcepcionAsync(context, exception);
        }
    }

    private static async Task ManejarExcepcionAsync(
        HttpContext context,
        Exception exception)
    {
        int statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        string titulo = statusCode switch
        {
            400 => "Solicitud incorrecta",
            409 => "Conflicto",
            _ => "Error interno del servidor"
        };

        string detalle = statusCode == 500
            ? "Ocurrió un error inesperado en el servidor."
            : exception.Message;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = detalle,
            Instance = context.Request.Path
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
