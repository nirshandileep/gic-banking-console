namespace GIC.BankingApplication.Domain.Models;

public class InterestRule
{
    public int Id { get; private set; }
    public DateTime Date { get; private set; }
    public string RuleId { get; private set; }
    public decimal Rate { get; private set; }

    public static InterestRule CreateNewInterestRule(DateTime date, string ruleId, decimal rate)
    {
        return new InterestRule
        {
            Date = date,
            RuleId = ruleId,
            Rate = rate
        };
    }
}