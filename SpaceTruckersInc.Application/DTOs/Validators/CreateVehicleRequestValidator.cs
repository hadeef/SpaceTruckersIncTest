using FluentValidation;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        _ = RuleFor(r => r.Model)
            .NotEmpty()
            .WithMessage("Model is required.")
            .Must(name => VehicleModel.GetNames()
                .Contains(name!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(r => !string.IsNullOrWhiteSpace(r.Model))
            .WithMessage("Invalid vehicle model.");

        _ = RuleFor(r => r.CargoCapacity).GreaterThanOrEqualTo(0);
    }
}