using System.Reflection;
using FitMetrics.Application.Services;
using FitMetrics.Application.Services.Interfaces;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace FitMetrics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Mapster eşleme yapılandırması
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // FluentValidation kuralları
        services.AddValidatorsFromAssembly(assembly);

        // İş servisleri
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<INutritionService, NutritionService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IWeightService, WeightService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IInsightService, InsightService>();
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDietitianService, DietitianService>();

        return services;
    }
}
