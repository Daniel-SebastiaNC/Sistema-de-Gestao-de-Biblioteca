using System.Diagnostics;

namespace Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("Recebendo requisição HTTP {Method} {Path}", method, path);

        try
        {
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            _logger.LogInformation(
                "Requisição HTTP {Method} {Path} finalizada com Status {StatusCode} em {ElapsedMs}ms",
                method, path, statusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Exceção não tratada na requisição HTTP {Method} {Path} após {ElapsedMs}ms",
                method, path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
