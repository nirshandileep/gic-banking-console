using FluentValidation.Results;

namespace GIC.BankingApplication.Application.Infrastructure.Validation;

public class ValidationContext
{
    public bool HasErrors => ValidationErrors != null && ValidationErrors.Any();
    public IList<ValidationFailure> ValidationErrors { get; private set; }

    public void AddError(string errorMessage)
    {
        ValidationErrors ??= [];

        ValidationErrors.Add(new ValidationFailure(null, errorMessage));
    }

    public void Clear()
    {
        ValidationErrors = [];
    }
}
