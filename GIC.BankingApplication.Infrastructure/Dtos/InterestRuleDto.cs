namespace GIC.BankingApplication.Infrastructure.Dtos;

public class InterestRuleDto
{
    public DateTime EffectiveDate { get; set; }
    public string RuleId { get; set; }
    public decimal Rate { get; set; }
}
