namespace GIC.BankingApplication.Application.Validators;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Request.Balance)
            .GreaterThan(0)
            .WithMessage("Balance should be greater than 0");
    }
}
