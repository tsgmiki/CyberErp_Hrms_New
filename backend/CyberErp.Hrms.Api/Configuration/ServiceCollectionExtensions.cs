using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Reflection;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using CyberErp.Hrms.Inf;
using CyberErp.Hrms.Inf.Common;
using CyberErp.Hrms.Inf.Models;
using CyberErp.Hrms.Inf.MultiTenant;
using CyberErp.Hrms.App;
using CyberErp.Hrms.App.Common.DTOs;

namespace CyberErp.Hrms.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHrmsForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IServiceCollection AddHrmsSession(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddMemoryCache();   // per-user permission-link cache (EndpointPermissionService)
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".CyberErp.Hrms.Session";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        return services;
    }

    public static IServiceCollection AddHrmsControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                // Server-side per-operation permission enforcement (opt-in via [RequirePermission]).
                options.Filters.Add<PermissionAuthorizationFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                // Form-driven UI posts numeric fields as strings; accept them for int/decimal.
                options.JsonSerializerOptions.NumberHandling =
                    System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            });

        return services;
    }

    public static IServiceCollection AddHrmsCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(corsOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddHrmsValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GetAllRequest>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddHrmsDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<HrmsDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly("CyberErp.Hrms.Inf");
                // Safety net so a cold cache / stats refresh can't hard-fail with the 30s ADO default;
                // real speed comes from the indexes, not this ceiling.
                b.CommandTimeout(60);
            });
            // Audit-trail interceptor (resolved per-scope so it sees the current user/tenant).
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<HrmsDbContext>());

        return services;
    }

    public static IServiceCollection AddHrmsMultiTenancy(this IServiceCollection services)
    {
        services.AddMultiTenant<AppTenantInfo>()
            .WithStrategy<HybridTenantStrategy>(ServiceLifetime.Scoped)
            .WithStore<DatabaseTenantStore>(ServiceLifetime.Scoped);

        return services;
    }

    public static IServiceCollection AddHrmsSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddHrmsApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
