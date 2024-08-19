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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DataAccess.Authentication.Utilities;
using Api.Core.MiddleWares;
using Serilog;
using Api.Core.Services;
using System.Configuration;



namespace Api.Core.Extensions
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

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped(typeof(ValidationFilter<>));

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();


            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
                    };
                });

            builder.Services.AddAuthorization();


            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            builder.Services.AddScoped(provider =>
                new TokenService(secretKey, issuer, audience, provider.GetRequiredService<IAuthenticationRepository>()));
            
            builder.Services.AddSerilog();

            
            var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

            
            builder.Services.AddSingleton<IEmailService>(new EmailService(
                smtpSettings.Host,
                smtpSettings.Port,
                smtpSettings.Username,
                smtpSettings.Password,
                smtpSettings.FromEmail
            ));

            


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

        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
