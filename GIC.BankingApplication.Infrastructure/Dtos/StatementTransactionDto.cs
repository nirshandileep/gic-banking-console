using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Infrastructure.Dtos;

public class StatementTransactionDto
{
    public DateTime Date { get; set; }
    public string TxnId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
}
