using GIC.BankingApplication.Application.Commands.InterestRule;

namespace GIC.BankingApplication.Application.Services;

public class InterestRuleService(IMediator mediator) : IInterestRuleService
{
    private readonly IMediator _mediator = mediator;

    public async Task DefineInterestRule(CreateInterestRuleRequestDto interestRule)
    {
        await _mediator.Send(new CreateInterestRuleCommand(interestRule));
    }

    public async Task<IEnumerable<InterestRuleDto>> GetAllInterestRules()
    {
        return await _mediator.Send(new GetAllInterestRulesQuery());
    }
}
