namespace GIC.BankingApplication.Infrastructure.Dtos;

public class UpdateAccountRequestDto
{
    public int AccountId { get; set; }
    public decimal Balance { get; set; }
}
