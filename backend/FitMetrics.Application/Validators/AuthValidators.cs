using FitMetrics.Application.DTOs.Auth;
using FluentValidation;

namespace FitMetrics.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta giriniz.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola zorunludur.")
            .MinimumLength(6).WithMessage("Parola en az 6 karakter olmalıdır.")
            .MaximumLength(100);

        RuleFor(x => x.Age).InclusiveBetween(10, 120);
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.HeightCm).InclusiveBetween(50, 260);
        RuleFor(x => x.CurrentWeightKg).InclusiveBetween(20, 400);
        RuleFor(x => x.ActivityLevel).IsInEnum();
        RuleFor(x => x.GoalType).IsInEnum();
        RuleFor(x => x.TargetWeightKg)
            .InclusiveBetween(20, 400)
            .When(x => x.TargetWeightKg.HasValue);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
