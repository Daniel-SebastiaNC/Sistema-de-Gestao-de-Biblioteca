using Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Handlers;

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
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Requisição Inválida"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflito no Sistema"),
            RegraNegocioException => (StatusCodes.Status409Conflict, "Conflito no Sistema"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro interno não tratado ao processar requisição: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Falha na requisição ({StatusCode} - {Title}): {Message}", statusCode, title, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}