using FitMetrics.Application.DTOs.Ai;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class GenerateMealPlanRequestValidator : AbstractValidator<GenerateMealPlanRequest>
{
    public GenerateMealPlanRequestValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Bir hedef/istek metni giriniz.")
            .MaximumLength(500);
        RuleFor(x => x.TargetCalories)
            .InclusiveBetween(800, 8000)
            .When(x => x.TargetCalories.HasValue);
    }
}

public class AnalyzeMealPhotoRequestValidator : AbstractValidator<AnalyzeMealPhotoRequest>
{
    private static readonly string[] AllowedMediaTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public AnalyzeMealPhotoRequestValidator()
    {
        RuleFor(x => x.ImageBase64)
            .NotEmpty().WithMessage("Görüntü verisi gereklidir.")
            .MaximumLength(8_000_000).WithMessage("Görüntü çok büyük.");
        RuleFor(x => x.MediaType)
            .Must(m => AllowedMediaTypes.Contains(m))
            .WithMessage("Desteklenen formatlar: JPEG, PNG, WebP, GIF.");
    }
}

public class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Messages).NotEmpty().WithMessage("En az bir mesaj gereklidir.");
        RuleForEach(x => x.Messages).ChildRules(m =>
        {
            m.RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
            m.RuleFor(x => x.Role).Must(r => r is "user" or "assistant").WithMessage("Rol 'user' veya 'assistant' olmalıdır.");
        });
    }
}
