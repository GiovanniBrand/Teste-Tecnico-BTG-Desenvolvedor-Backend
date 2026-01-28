using KrtBank.Api.Configurations;
using KrtBank.Application;
using KrtBank.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

SerilogConfig.AddSerilog(builder);

try
{
    // 2. Configurações da API
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerConfiguration();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // 4. Pipeline de Execução
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerConfiguration();
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("Development");
    app.UseAuthorization();

    app.MapControllers();

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