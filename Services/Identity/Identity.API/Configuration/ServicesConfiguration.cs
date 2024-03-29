﻿using Identity.API.DataAccess;
using Identity.API.Models.Entities;
using Identity.API.Services;
using Identity.API.Settings;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace Identity.API.Configuration;

static class ServicesConfiguration
{
    private static Assembly CurrentAssembly => typeof(Startup).Assembly;

    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentTime, CurrentTime>();
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "MicroShop - Identity.API",
                Version = "v1",
            });
        });
    }

    public static void AddCustomIdentity(this IServiceCollection services, 
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(CurrentAssembly.FullName);
            });
        });

        services.AddIdentity<User, Role>(setup =>
        {
            if (environment.IsDevelopment())
            {
                setup.Password.RequireDigit = false;
                setup.Password.RequireLowercase = false;
                setup.Password.RequireUppercase = false;
                setup.Password.RequireNonAlphanumeric = false;
                setup.Password.RequiredLength = 1;
            }
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
    }

    public static void AddCustomIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection");

        AuthSettings settings = new();

        configuration.GetSection("AuthSettings").Bind(settings);

        services.AddIdentityServer(x =>
        {
            x.IssuerUri = settings.IssuerUri;
            x.Authentication.CookieLifetime = TimeSpan.FromSeconds(settings.CookieLifetimeSec);
            x.Authentication.CookieSlidingExpiration = settings.CookieSlidingExpiration;
        })
        .AddDeveloperSigningCredential()
        .AddAspNetIdentity<User>()
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = builder => builder.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(CurrentAssembly.FullName));
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = builder => builder.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(CurrentAssembly.FullName));
        });
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddLocalApiAuthentication();
    }

    public static void AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }

    public static void AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck(
                name: "Self",
                check: () => HealthCheckResult.Healthy(),
                tags: new[] { "api" })
            .AddSqlServer(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                name: "Identity Db",
                tags: new[] { "database", "sql" });
    }
}