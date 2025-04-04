﻿using Api.Common.Abstractions;
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
using Serilog;
using Api.Core.Services;
using System.Configuration;
using Microsoft.OpenApi.Models;
using Application.UserDevices.Abstractions;
using DataAccess.UserDevices.Repositories;
using Application.Notifications.Abstractions;
using DataAccess.Notifications.Repositories;
using DataAccess.DeliveryLogs.Repositories;
using Application.DeliveryLogs.Abstractions;
using Application.EventLogs.Abstractions;
using DataAccess.EventLogs.Repositories;
using Application.MessageDeliveries.Abstractions;
using DataAccess.MessageDeliveries.Repositories;
using Application.MessageQueue.Abstractions;
using DataAccess.MessageQueue.Repositories;
using DataAccess.NotificationRequests.Repositories;
using Application.NotificationRequests.Abstractions;
using DataAccess.NotificationTemplates.Repositories;
using Application.NotificationTemplates.Abstractions;
using Application.RecipientGroups.Abstractions;
using Application.Recipients.Abstractions;
using Application.Tenants.Abtstractions;




namespace Api.Core.Extensions
{
    public static class Extensions
    {

        public static void RegisterServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SignalHub API", Version = "v1" });

                // Configure Swagger to use the Authorization header
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
            });
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IDbConnectionProvider, DbConnection>();
            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(config =>
                    config.AddSqlServer()
                    .WithGlobalConnectionString(configuration.GetConnectionString("sqlServerConnectionString"))
                    .ScanIn(Assembly.GetExecutingAssembly()).For.All()
                    .ScanIn(Assembly.Load("DataAccess")).For.Migrations())

                    .AddLogging(config => config.AddFluentMigratorConsole());
            builder.Services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();
           
            builder.Services.AddTransient<IUserDevicesRepository, UserDevicesRepository>();
            builder.Services.AddTransient<INotificationsRepository, NotificationsRepository>();
            builder.Services.AddTransient<IDeliveryLogsRepository, DeliveryLogsRepository>();
            builder.Services.AddTransient<IEventLogsRepository, EventLogsRepository>();
            builder.Services.AddTransient<IMessageDeliveriesRepository, MessageDeliveriesRepository>();
            builder.Services.AddTransient<IMessageQueueRepository, MessageQueueRepository>();
            builder.Services.AddTransient<INotificationRequestsRepository, NotificationRequestsRepository>();
            builder.Services.AddTransient<INotificationTemplatesRepository, NotificationTemplatesRepository>();
            builder.Services.AddTransient<IRecipientGroupMembersRepository, RecipientGroupMembersRepository>();
            builder.Services.AddTransient<IRecipientGroupsRepository, RecipientGroupsRepository>();
            builder.Services.AddTransient<IRecipientsRepository, RecipientsRepository>();
            builder.Services.AddTransient<ITemplateChannelsRepository, TemplateChannelsRepository>();
            builder.Services.AddTransient<ITenantsRepository, TenantsRepository>(); 




            builder.Services.AddHttpClient<SmsService>();

            // Register SmsService
            builder.Services.AddScoped<SmsService>();

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddScoped(typeof(ValidationFilter<>));

            builder.Services.AddScoped<NotificationUtilityService>();

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

    // Customize JWT Bearer Events
    options.Events = new JwtBearerEvents
    {
    

        // Handle missing token
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.HandleResponse(); // Suppress the default response
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    StatusCode = 401,
                    Message = "Token is required to access this resource."
                });

                return context.Response.WriteAsync(result);
            }

            return Task.CompletedTask;
        },

        // Handle forbidden responses
        OnForbidden = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    StatusCode = 403,
                    Message = "You do not have permission to access this resource."
                });

                return context.Response.WriteAsync(result);
            }

            return Task.CompletedTask;
        }
    };
});


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserManagement", policy => policy.RequireClaim("Permission", "UserManagement"));
                options.AddPolicy("RoleManagement", policy => policy.RequireClaim("Permission", "RoleManagement"));
                options.AddPolicy("ClaimManagement", policy => policy.RequireClaim("Permission", "ClaimManagement"));
                options.AddPolicy("UserRoleManagement", policy => policy.RequireClaim("Permission", "UserRoleManagement"));
                options.AddPolicy("RoleClaimManagement", policy => policy.RequireClaim("Permission", "RoleClaimManagement"));
                options.AddPolicy("UserClaimManagement", policy => policy.RequireClaim("Permission", "UserClaimManagement"));
            });



            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            builder.Services.AddScoped(provider =>
                new TokenService(secretKey, issuer, audience, provider.GetRequiredService<IAuthenticationRepository>()));
            
            builder.Services.AddSerilog();

            
            var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();


            // Add EmailService to the dependency injection container
            builder.Services.AddSingleton<IEmailService, EmailService>();





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
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
