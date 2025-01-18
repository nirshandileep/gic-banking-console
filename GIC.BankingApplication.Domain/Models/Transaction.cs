using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Domain.Models;

public class Transaction
{
    public int Id { get; set; }
    public DateTime Date { get; private set; }
    public string TransactionId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public int AccountId { get; private set; }
    public Account Account { get; private set; }

    public static Transaction CreateNewTransaction(DateTime date, string transactionId, int accountId, decimal amount)
    {
        return new Transaction
        {
            Date = date,
            TransactionId = transactionId,
            AccountId = accountId,
            Amount = amount,
        };
    }

    public Transaction SetTransactionType(TransactionType type)
    {
        Type = type;
        return this;
    }
}
