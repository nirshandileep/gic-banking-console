using GIC.BankingApplication.Application.Commands.InterestRule;

namespace GIC.BankingApplication.Application.Validators;

public class CreateInterestRuleCommandValidator : AbstractValidator<CreateInterestRuleCommand>
{
    public CreateInterestRuleCommandValidator()
    {
        RuleFor(x => x.Request.RuleId)
            .NotEmpty()
            .WithMessage("RuleId is required");

        RuleFor(x => x.Request.Date)
            .NotEmpty()
            .WithMessage("Date is required");

        RuleFor(x => x.Request.Rate)
            .NotEmpty()
            .WithMessage("Rate is required");

        RuleFor(x => x.Request.Rate)
            .GreaterThan(0)
            .LessThan(100)
            .WithMessage("Rate should be between 0 and 100");
    }
}
