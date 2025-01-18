using GIC.BankingApplication.Application.Commands.InterestRule;
using GIC.BankingApplication.Application.Queries.InterestRule;
using MediatR;

namespace GIC.BankingApplication.Application.Services;

public class InterestRuleService(IMediator mediator) : IInterestRuleService
{
    private readonly IMediator _mediator = mediator;

    public void DefineInterestRule(CreateInterestRuleRequestDto interestRule)
    {
        _mediator.Send(new CreateInterestRuleCommand(interestRule));
    }

    public IEnumerable<InterestRuleDto> GetAllInterestRules()
    {
        var interestRules = _mediator.Send(new GetAllInterestRulesQuery());
        return interestRules.Result;
    }
}
