using FitMetrics.Application.DTOs.Profile;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Age).InclusiveBetween(10, 120);
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.HeightCm).InclusiveBetween(50, 260);
        RuleFor(x => x.CurrentWeightKg).InclusiveBetween(20, 400);
        RuleFor(x => x.ActivityLevel).IsInEnum();
        RuleFor(x => x.GoalType).IsInEnum();

        RuleFor(x => x.TargetWeightKg).InclusiveBetween(20, 400).When(x => x.TargetWeightKg.HasValue);
        RuleFor(x => x.DailyCalorieGoal).InclusiveBetween(800, 8000).When(x => x.DailyCalorieGoal.HasValue);
        RuleFor(x => x.DailyProteinGoal).InclusiveBetween(20, 500).When(x => x.DailyProteinGoal.HasValue);
        RuleFor(x => x.DailyWaterGoalMl).InclusiveBetween(500, 8000).When(x => x.DailyWaterGoalMl.HasValue);
    }
}
