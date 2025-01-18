namespace GIC.BankingApplication.Infrastructure.Dtos;

public class CreateAccountRequestDto
{
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
}