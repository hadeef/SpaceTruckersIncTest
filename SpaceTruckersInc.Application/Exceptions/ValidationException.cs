using FluentValidation.Results;

namespace SpaceTruckersInc.Application.Exceptions;

public class ValidationException : ApplicationException
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Validation failed.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }

    public ValidationException() : base()
    {
    }

    public ValidationException(string? message) : base(message)
    {
    }

    public ValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public IDictionary<string, string[]> Errors { get; }
}