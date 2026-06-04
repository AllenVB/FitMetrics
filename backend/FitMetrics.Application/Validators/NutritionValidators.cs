using FitMetrics.Application.DTOs.Nutrition;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class CreateNutritionLogRequestValidator : AbstractValidator<CreateNutritionLogRequest>
{
    public CreateNutritionLogRequestValidator()
    {
        RuleFor(x => x.FoodId).GreaterThan(0);
        RuleFor(x => x.AmountGrams)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(5000);
        RuleFor(x => x.MealType).IsInEnum();
    }
}

public class CreateFoodRequestValidator : AbstractValidator<CreateFoodRequest>
{
    public CreateFoodRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Category).MaximumLength(60);
        RuleFor(x => x.CaloriesPer100g).InclusiveBetween(0, 1000);
        RuleFor(x => x.ProteinPer100g).InclusiveBetween(0, 100);
        RuleFor(x => x.CarbsPer100g).InclusiveBetween(0, 100);
        RuleFor(x => x.FatPer100g).InclusiveBetween(0, 100);
    }
}
