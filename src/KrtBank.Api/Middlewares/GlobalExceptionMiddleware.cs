using FluentValidation;
using KrtBank.Domain.Exceptions;
using Serilog;
using System.Net;
using System.Text.Json;

namespace KrtBank.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate next; 
    private readonly IHostEnvironment env;

    public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        this.next = next;
        this.env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        Log.Error(exception, "KrtBank API Error: {Message}", exception.Message);

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Ocorreu um erro interno no servidor do KRT Bank.";

        switch (exception)
        {
            case BaseException baseEx:
                statusCode = (HttpStatusCode)baseEx.StatusCode;
                message = baseEx.Message;
                break;

            // Tratamento para Exceptions Nativas do .NET (Fallback)
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Acesso negado: você precisa estar autenticado.";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "O recurso solicitado não foi encontrado.";
                break;

            case ArgumentException:
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case TimeoutException:
                statusCode = HttpStatusCode.RequestTimeout;
                message = "O tempo de resposta do servidor expirou.";
                break;

            // Tratamento para Exceptions do FluentValidator
            case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                message = string.Join(" | ", valEx.Errors.Select(x => x.ErrorMessage));
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = message,
            Detailed = env.IsDevelopment() ? exception.StackTrace : null
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(result);
    }
}