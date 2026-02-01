using FluentValidation;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class VehicleDtoValidator : AbstractValidator<VehicleDto>
{
    public VehicleDtoValidator()
    {
        _ = RuleFor(v => v.Id)
            .NotEmpty()
            .WithMessage("Id is required for update operations.");

        _ = RuleFor(v => v.Model)
            .NotEmpty()
            .WithMessage("Model is required.")
            .Must(name => VehicleModel.GetNames().Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(v => !string.IsNullOrWhiteSpace(v.Model))
            .WithMessage("Invalid vehicle model.");

        _ = RuleFor(v => v.CargoCapacity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CargoCapacity must be non-negative.");

        _ = RuleFor(v => v.Condition)
            .NotEmpty()
            .WithMessage("Condition is required.")
            .Must(name => VehicleCondition.GetNames().Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(v => !string.IsNullOrWhiteSpace(v.Condition))
            .WithMessage("Invalid Condition.");

        _ = RuleFor(v => v.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(name => VehicleStatus.GetNames().Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(v => !string.IsNullOrWhiteSpace(v.Status))
            .WithMessage("Invalid Status.");

        _ = RuleFor(v => v.RowVersion)
            .NotNull()
            .WithMessage("RowVersion is required for concurrency control.");
    }
}