using FluentValidation;

namespace SpaceTruckersInc.Application.DTOs.Validators;

public sealed class RouteDtoValidator : AbstractValidator<RouteDto>
{
    public RouteDtoValidator()
    {
        _ = RuleFor(r => r.Id)
            .NotEmpty()
            .WithMessage("Id is required for update operations.");

        _ = RuleFor(r => r.Origin)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Origin is required and must be at most 200 characters.");

        _ = RuleFor(r => r.Destination)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Destination is required and must be at most 200 characters.");

        _ = RuleFor(r => r.EstimatedDuration)
            .Must(d => d.TotalSeconds >= 0)
            .WithMessage("EstimatedDuration must be non-negative.");

        _ = RuleForEach(r => r.Checkpoints)
            .Must(cp => !string.IsNullOrWhiteSpace(cp))
            .WithMessage("Each checkpoint must be non-empty and not whitespace.")
            .MaximumLength(200)
            .WithMessage("Each checkpoint must be at most 200 characters.")
            .When(r => r.Checkpoints is not null && r.Checkpoints.Count > 0);

        _ = RuleFor(r => r.RowVersion)
            .NotNull()
            .WithMessage("RowVersion is required for concurrency control.");
    }
}