using Microsoft.AspNetCore.Diagnostics;
using Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
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
            _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor")
        };

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