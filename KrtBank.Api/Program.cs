using KrtBank.Api.Configurations;
using KrtBank.Application;
using KrtBank.Infrastructure;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

SerilogConfig.AddSerilog(builder);
var sw = Stopwatch.StartNew();

try
{
    //Configurações de Serviços
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCorsConfiguration();
    builder.Services.AddHealthCheckConfiguration(builder.Configuration);

    builder.Services.AddJwtConfiguration(builder.Configuration);
    builder.Services.AddSwaggerConfiguration();

    var app = builder.Build();

    // Pipeline de Execução
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerConfiguration();
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseCors("Development");

    app.UseAuthConfiguration();

    // Mapeamento de Endpoints
    app.MapControllers();
    app.MapHealthChecks("/health");

    sw.Stop();
    Log.Information("KrtBank API subiu com sucesso em {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal no startup da aplicação.");
}
finally
{
    Log.CloseAndFlush();
}