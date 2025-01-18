using GIC.BankingApplication.Application.Commands.InterestRule;

namespace GIC.BankingApplication.Application.Services;

public class InterestRuleService(IMediator mediator) : IInterestRuleService
{
    private readonly IMediator _mediator = mediator;

    public async Task DefineInterestRule(CreateInterestRuleRequestDto interestRule)
    {
        var existingRule = await _mediator.Send(new GetInterestRuleByDateQuery(interestRule.Date));

        if (existingRule != null)
        {
            var updateRuleCommand = new UpdateInterestRuleCommand(
                new UpdateInterestRuleRequestDto
                {
                    Id = existingRule.Id,
                    Date = interestRule.Date,
                    RuleId = interestRule.RuleId,
                    Rate = interestRule.Rate
                });
            await _mediator.Send(updateRuleCommand);
        }
        else
        {
            await _mediator.Send(new CreateInterestRuleCommand(interestRule));
        }
    }

    public async Task<IEnumerable<InterestRuleDto>> GetAllInterestRules()
    {
        return await _mediator.Send(new GetAllInterestRulesQuery());
    }
}
