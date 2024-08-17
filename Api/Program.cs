using Api.Extensions;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterServices(builder.Configuration);


var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    // Ensure Swagger is aware of all registered endpoints
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register all endpoint definitions before Swagger setup
app.RegisterEndpointdefinitions();


using var scope = app.Services.CreateScope();
try
{
    var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    //migrator.ListMigrations();
    migrator.MigrateUp();
}
catch (Exception ex)
{
    // Log the exception or handle it appropriately
    Console.WriteLine($"An error occurred while listing migrations");
}

app.Run();

public partial class Program { }
