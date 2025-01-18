using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Infrastructure.Dtos;

public class CreateTransactionRequestDto
{
    public DateTime Date { get; set; }
    public string TransactionId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public int AccountId { get; set; }
    public string AccountNumber { get; set; }
}
