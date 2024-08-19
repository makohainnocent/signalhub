using Api.Core.Extensions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FluentMigrator.Runner;
using Api.Core.MiddleWares;

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
        //migrator.MigrateDown(2024081602);
    }
    catch (Exception ex)
    {

        Console.WriteLine($"An error occurred while listing migrations");
    }

}
else
{
    app.UseExceptionHandlingMiddleware();
}


app.RegisterEndpointdefinitions();

app.UseAuthentication();
app.UseAuthorization();




app.Run();

public partial class Program { }
