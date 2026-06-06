using FitMetrics.Application.DTOs.Knowledge;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class CreateKnowledgeEntryRequestValidator : AbstractValidator<CreateKnowledgeEntryRequest>
{
    public CreateKnowledgeEntryRequestValidator()
    {
        RuleFor(x => x.Question).NotEmpty().WithMessage("Soru gereklidir.").MaximumLength(500);
        RuleFor(x => x.Answer).NotEmpty().WithMessage("Cevap gereklidir.").MaximumLength(4000);
    }
}
