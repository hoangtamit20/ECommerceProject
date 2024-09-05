using System.Reflection;
using System.Text;
using Core.Domain;
using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Core.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = RuntimeContext.AppSettings.JwtSetting.Issuer,
                ValidAudience = RuntimeContext.AppSettings.JwtSetting.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RuntimeContext.AppSettings.JwtSetting.SecretKey))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var jwt = context.SecurityToken as JsonWebToken;
                    if (jwt == null)
                    {
                        context.Fail("Invalid token");
                    }
                    return Task.CompletedTask;
                }
            };
        });
        // .AddGoogle(googleOptions =>
        // {
        //     googleOptions.ClientId = builder.Configuration[AppSettingsConfig.GOOGLE_CLIENTID_WEB]!;
        //     googleOptions.ClientSecret = builder.Configuration[AppSettingsConfig.GOOLE_CLIENTSECRET]!;
        // });

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
                Type = SecuritySchemeType.ApiKey,
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();

            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Ecommerce api",
                Description = "Sample .NET api by ",
                Contact = new OpenApiContact()
                {
                    Name = "Hoang Trong Tam",
                    Url = new Uri("https://www.youtube.com")
                }
            });

            options.EnableAnnotations();

            // var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var path = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            // options.IncludeXmlComments(path);
        });

        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpContextAccessor();
        return services;
    }
    public static IServiceCollection AddCETCoreService(this IServiceCollection services)
    {
        services.AddCETDbContext();
        return services;
    }

    public static IServiceCollection AddCustomerCoreService(this IServiceCollection services)
    {
        services.AddCustomerDbContext();
        return services;
    }

    public static IApplicationBuilder MapJwtRevocation(this IApplicationBuilder app)
    {
        app.UseMiddleware<JwtRevocationMiddleware>();
        return app;
    }
}
