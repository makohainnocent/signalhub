using Api.Common.Abstractions;
using Application.Common.Abstractions;
using DataAccess.Common.DbConnection;
using FluentMigrator.Runner;
using System.Reflection;
using FluentValidation;
using Application.Authentication.Abstractions;
using DataAccess.Authentication.Repositories;
using Api.Common.Filters;
using Api.Authentication.Validators;
using Domain.Core.Models;
using Microsoft.AspNetCore.Identity;
using Domain.Authentication.Requests;
using Asp.Versioning;



namespace Api.Common.Extensions
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

                    .AddLogging(config => config.AddFluentMigratorConsole());
            builder.Services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();





            //builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();
            //builder.Services.AddValidatorsFromAssembly(Assembly.Load("Api"));
            //builder.Services.AddValidatorsFromAssembly(Assembly.Load("Domain"));
            //builder.Services.AddValidatorsFromAssembly(Assembly.Load("Application"));
            //builder.Services.AddValidatorsFromAssembly(Assembly.Load("DataAccess"));
            //builder.Services.AddScoped<IValidator<UserRegistrationRequest>, UserRegistrationValidator>();

            // Register all validators in the current assembly
            //builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped<IValidator<UserRegistrationRequest>, UserRegistrationValidator>();
            builder.Services.AddScoped(typeof(ValidationFilter<>));

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();


            }).AddApiExplorer(options=>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl= true;
            });
        }

        public static void RegisterEndpointdefinitions(this WebApplication app)
        {
            var endpointdefs = typeof(Program).Assembly
                .GetTypes()
                .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(Activator.CreateInstance)
                .Cast<IEndpointDefinition>();

            foreach (var endpoints in endpointdefs)
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
