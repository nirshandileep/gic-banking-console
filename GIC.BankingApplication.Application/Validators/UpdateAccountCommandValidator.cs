namespace GIC.BankingApplication.Application.Validators;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Request.Balance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Balance cannot be negative");
    }
}
