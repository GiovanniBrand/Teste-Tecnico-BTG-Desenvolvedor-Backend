using System.Diagnostics;
using Serilog;

namespace KrtBank.Api.Middlewares;

public class PerformanceLogMiddleware
{
    private readonly RequestDelegate _next;

    public PerformanceLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            // Continua o fluxo da requisição
            await _next(context);
        }
        finally
        {
            sw.Stop();

            Log.Information(
                "HTTP {Method} {Path} respondeu {StatusCode} em {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds
            );
        }
    }
}