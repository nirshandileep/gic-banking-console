namespace GIC.BankingApplication.Application.Services;

public interface IInterestRuleService
{
    Task DefineInterestRule(CreateInterestRuleRequestDto interestRule);
    Task<IEnumerable<InterestRuleDto>> GetAllInterestRules();
}