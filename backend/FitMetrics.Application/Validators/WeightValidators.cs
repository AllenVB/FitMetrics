using FitMetrics.Application.DTOs.Weight;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class CreateWeightEntryRequestValidator : AbstractValidator<CreateWeightEntryRequest>
{
    public CreateWeightEntryRequestValidator()
    {
        RuleFor(x => x.WeightKg).InclusiveBetween(20, 400);
        RuleFor(x => x.BodyFatPercentage)
            .InclusiveBetween(2, 70)
            .When(x => x.BodyFatPercentage.HasValue);
    }
}
