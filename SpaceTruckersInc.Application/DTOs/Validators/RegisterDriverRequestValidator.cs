using FluentValidation;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class RegisterDriverRequestValidator : AbstractValidator<RegisterDriverRequest>
{
    public RegisterDriverRequestValidator()
    {
        _ = RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(200);

        _ = RuleFor(r => r.LicenseLevel)
            .NotEmpty()
            .WithMessage("LicenseLevel is required.")
            .Must(name => LicenseLevel.GetNames().Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(r => !string.IsNullOrWhiteSpace(r.LicenseLevel))
            .WithMessage("Invalid license level.");
    }
}