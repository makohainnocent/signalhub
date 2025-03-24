using Api.Core.Extensions;
using Api.Core.MiddleWares;
using FluentMigrator.Runner;
using Serilog;

// Configure the logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Log to the console
    .WriteTo.Seq("http://localhost:5341") // Log to Seq (replace with your Seq server URL)
    .CreateLogger();

// Log an information message
Log.Information("Starting up");



try
{
    var builder = WebApplication.CreateBuilder(args);
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()    // Allow any origin
                  .AllowAnyMethod()    // Allow any HTTP method
                  .AllowAnyHeader();   // Allow any header
        });
    });

    // Register other services
    builder.RegisterServices(builder.Configuration);

    var app = builder.Build();

    // Make sure CORS is applied before other middleware
    app.UseCors();

   
    app.UseMiddleware<JwtTokenExpirationMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        using var scope = app.Services.CreateScope();
        try
        {
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            migrator.MigrateUp();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while listing migrations");
        }
    }
    else
    {
        app.UseSwagger();
        app.UseSwaggerUI();
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
