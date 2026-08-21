using Gimnasio.Application.Atlas;
using Gimnasio.Infrastructure.Atlas;
using Gimnasio.Infrastructure.Identity;
using Gimnasio.Infrastructure.Persistence;
using Gimnasio.Application.Training;
using Gimnasio.Infrastructure.Training;
using Gimnasio.Application.Insights;
using Gimnasio.Infrastructure.Insights;
using Gimnasio.Application.Measurements;
using Gimnasio.Infrastructure.Measurements;
using Gimnasio.Application.Health;
using Gimnasio.Infrastructure.Health;
using Gimnasio.Application.Planning;
using Gimnasio.Infrastructure.Planning;
using Gimnasio.Application.Learning;
using Gimnasio.Infrastructure.Learning;
using Gimnasio.Application.Reports;
using Gimnasio.Infrastructure.Reports;
using Gimnasio.Application.DataExport;
using Gimnasio.Infrastructure.DataExport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gimnasio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GimnasioDatabase")
            ?? throw new InvalidOperationException("Falta la conexión de datos de Proyecto Atlas.");

        services.AddDbContext<GimnasioDbContext>(options => options.UseSqlServer(connectionString));

        services.AddAuthorization();
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<GimnasioDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Atlas.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddScoped<IAtlasService, AtlasService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IInsightsService, InsightsService>();
        services.AddScoped<IMeasurementService, MeasurementService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IPlanningService, PlanningService>();
        services.AddScoped<ILearningService, LearningService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDataExportService, DataExportService>();
        return services;
    }
}
