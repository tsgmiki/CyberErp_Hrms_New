using FluentValidation.Results;

namespace CyberErp.Hrms.App.Common.Exceptions;

public class AppValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Errors { get; }

    public AppValidationException(IEnumerable<ValidationFailure> errors)
        : base("Validation failed.")
    {
        Errors = errors.ToList();
    }
}