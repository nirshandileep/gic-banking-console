namespace GIC.BankingApplication.Infrastructure.Dtos;

public class AccountDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public IEnumerable<TransactionDto> Transactions { get; set; }
}
