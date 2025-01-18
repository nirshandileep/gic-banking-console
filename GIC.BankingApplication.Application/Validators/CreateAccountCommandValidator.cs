namespace GIC.BankingApplication.Application.Validators;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Request.AccountNumber)
            .NotEmpty()
            .WithMessage("Account Number is required");

        RuleFor(x => x.Request.Balance)
            .GreaterThan(0)
            .WithMessage("Balance should be greater than 0");
    }
}
