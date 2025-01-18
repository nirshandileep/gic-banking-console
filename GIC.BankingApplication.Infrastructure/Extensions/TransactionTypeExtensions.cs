using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Infrastructure.Extensions;

public static class TransactionTypeExtensions
{
    public static string ToTransactionTypeChar(this TransactionType type)
    {
        return type switch
        {
            TransactionType.Deposit => "D",
            TransactionType.Withdrawal => "W",
            TransactionType.Interest => "I",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown transaction type {type}")
        };
    }
}
