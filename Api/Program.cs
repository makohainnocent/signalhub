using Api.Common.Extensions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterServices(builder.Configuration);


var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.RegisterEndpointdefinitions();

app.UseAuthentication();
app.UseAuthorization();


using var scope = app.Services.CreateScope();
try
{
    var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
   
    migrator.MigrateUp();
    //migrator.MigrateDown(2024081602);
}
catch (Exception ex)
{
    
    Console.WriteLine($"An error occurred while listing migrations");
}

app.Run();

public partial class Program { }
