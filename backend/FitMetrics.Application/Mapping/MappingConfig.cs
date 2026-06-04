using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Nutrition;
using FitMetrics.Application.DTOs.Weight;
using FitMetrics.Application.DTOs.Workouts;
using FitMetrics.Domain.Entities;
using Mapster;

namespace FitMetrics.Application.Mapping;

/// <summary>
/// Mapster eşleme kuralları. Assembly taranarak otomatik kayıt edilir (DependencyInjection).
/// </summary>
public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Bmi, src => HealthCalculator.CalculateBmi(src.CurrentWeightKg, src.HeightCm));

        config.NewConfig<Food, FoodDto>();
        config.NewConfig<Exercise, ExerciseDto>();
        config.NewConfig<WeightEntry, WeightEntryDto>();
    }
}
