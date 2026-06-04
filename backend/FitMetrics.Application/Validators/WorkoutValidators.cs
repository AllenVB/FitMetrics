using FitMetrics.Application.DTOs.Workouts;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class CreateWorkoutLogRequestValidator : AbstractValidator<CreateWorkoutLogRequest>
{
    public CreateWorkoutLogRequestValidator()
    {
        RuleFor(x => x.ExerciseId).GreaterThan(0);

        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 600).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.Sets).InclusiveBetween(1, 50).When(x => x.Sets.HasValue);
        RuleFor(x => x.Reps).InclusiveBetween(1, 1000).When(x => x.Reps.HasValue);
        RuleFor(x => x.WeightKg).InclusiveBetween(0, 1000).When(x => x.WeightKg.HasValue);

        RuleFor(x => x)
            .Must(x => x.DurationMinutes.HasValue || x.Sets.HasValue)
            .WithMessage("Süre (kardiyo) veya set sayısı (kuvvet) alanlarından en az biri girilmelidir.");
    }
}
