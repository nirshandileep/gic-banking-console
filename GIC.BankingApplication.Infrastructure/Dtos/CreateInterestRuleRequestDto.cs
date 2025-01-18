namespace GIC.BankingApplication.Infrastructure.Dtos;

public class CreateInterestRuleRequestDto
{
    public DateTime Date { get; set; }
    public string RuleId { get; set; }
    public decimal Rate { get; set; }
}
