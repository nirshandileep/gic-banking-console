namespace GIC.BankingApplication.Application.Infrastructure.Validation;

public class ValidationError
{
    public string ErrorMessage { get; }
    public string MemberName { get; }

    public ValidationError(string memberName, string errorMessage)
    {
        ErrorMessage = errorMessage;
        MemberName = memberName;
    }
}
