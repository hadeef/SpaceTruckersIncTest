using FluentValidation;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class DriverDtoValidator : AbstractValidator<DriverDto>
{
    public DriverDtoValidator()
    {
        _ = RuleFor(d => d.Id)
            .NotEmpty()
            .WithMessage("Id is required for update operations.");

        _ = RuleFor(d => d.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Name is required and must be at most 200 characters.");

        _ = RuleFor(d => d.LicenseLevel)
            .NotEmpty()
            .WithMessage("LicenseLevel is required.")
            .Must(name => LicenseLevel.GetNames().Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(d => !string.IsNullOrWhiteSpace(d.LicenseLevel))
            .WithMessage("Invalid LicenseLevel.");

        _ = RuleFor(d => d.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(name => DriverStatus.GetNames().Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(d => !string.IsNullOrWhiteSpace(d.Status))
            .WithMessage("Invalid Status.");

        _ = RuleFor(d => d.RowVersion)
            .NotNull()
            .WithMessage("RowVersion is required for concurrency control.");
    }
}