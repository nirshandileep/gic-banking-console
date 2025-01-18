namespace GIC.BankingApplication.Application.Services;

public interface IInterestRuleService
{
    void DefineInterestRule(CreateInterestRuleRequestDto interestRule);
    IEnumerable<InterestRuleDto> GetAllInterestRules();
}