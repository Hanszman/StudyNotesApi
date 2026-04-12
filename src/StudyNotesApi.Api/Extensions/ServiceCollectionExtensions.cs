using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StudyNotesApi.Api.HealthChecks;
using StudyNotesApi.Api.Security.CurrentUser;
using StudyNotesApi.Api.Security.Jwt;
using StudyNotesApi.Api.Security.PasswordHasher;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Infrastructure.DependencyInjection;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Services;

namespace StudyNotesApi.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        ValidateJwtOptions(jwtOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authorizationHeader = context.Request.Headers.Authorization.ToString();
                        if (string.IsNullOrWhiteSpace(authorizationHeader))
                        {
                            return Task.CompletedTask;
                        }

                        const string bearerPrefix = "Bearer ";
                        context.Token = authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
                            ? authorizationHeader[bearerPrefix.Length..].Trim()
                            : authorizationHeader.Trim();

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
        services
            .AddHealthChecks()
            .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is online."))
            .AddCheck<DatabaseHealthCheck>("database");
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StudyNotes API",
                Version = "v1",
                Description = "REST API for managing study notes."
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Enter the full Authorization header value, for example: Bearer eyJ..."
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document, null)] = []
            });
        });

        return services;
    }

    private static void ValidateJwtOptions(JwtOptions jwtOptions)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must contain at least 32 characters.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer was not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience was not configured.");
        }

        if (jwtOptions.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpirationMinutes must be greater than zero.");
        }
    }
}
