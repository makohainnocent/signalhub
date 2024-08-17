using Api.Abstractions;
using Api.Filters;
//using Api.Middlewares;
using Application.Abstractions;
using DataAccess.DbConnection;
using FluentMigrator.Runner;
using System.Reflection;
//using DataAccess.Repositories;

namespace Api.Extensions
{
    public static class Extensions
    {

        public static void RegisterServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IDbConnectionProvider, DbConnection>();
            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(config =>
                    config.AddSqlServer()
                    .WithGlobalConnectionString(configuration.GetConnectionString("sqlServerConnectionString"))
                    .ScanIn(Assembly.GetExecutingAssembly()).For.All()
                    .ScanIn(Assembly.Load("DataAccess")).For.Migrations())
                    
                    .AddLogging(config=>config.AddFluentMigratorConsole());
            //builder.Services.AddTransient<ICandidateRepository, CandidateRepository>();

            //builder.Services.AddScoped<CandidateValidationFilter>();
        }

        public static void RegisterEndpointdefinitions(this WebApplication app)
        {
            var endpointdefs = typeof(Program).Assembly
                .GetTypes()
                .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(Activator.CreateInstance)
                .Cast<IEndpointDefinition>();

            foreach(var endpoints in endpointdefs)
            {
                endpoints.RegisterEndpoints(app);
            }
        }

        /*public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }*/
    }
}
