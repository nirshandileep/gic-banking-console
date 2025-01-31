﻿namespace GIC.BankingApplication.Infrastructure.Dtos;

public class InterestRuleDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string RuleId { get; set; }
    public decimal Rate { get; set; }
}
