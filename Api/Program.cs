using Api.Core.Extensions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FluentMigrator.Runner;
using Api.Core.MiddleWares;
using Serilog;



Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting up");

try
{
    
    var builder = WebApplication.CreateBuilder(args);

    
    builder.RegisterServices(builder.Configuration);

    
    var app = builder.Build();


    
    if (app.Environment.IsDevelopment())
    {
        
        app.UseSwagger();
        app.UseSwaggerUI();

        using var scope = app.Services.CreateScope();
        try
        {
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            migrator.MigrateUp();
            // migrator.MigrateDown(2024081602);
        }
        catch (Exception ex)
        {
            
            Log.Error(ex, "An error occurred while listing migrations");
        }
    }
    else
    {
        app.UseExceptionHandlingMiddleware();
    }

   
    app.RegisterEndpointdefinitions();

    
    app.UseAuthentication();
    app.UseAuthorization();

    
    app.UseSerilogRequestLogging();

    
    app.Run();
}
catch (Exception ex)
{
    
    Log.Fatal(ex, "An error occurred during application startup");
}
finally
{
    
    Log.CloseAndFlush();
}
