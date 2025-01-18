using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Application.Extensions;

public static class TransactionTypeExtensions
{
    public static TransactionType ToTransactionType(this string input)
    {
        switch (input?.Trim().ToUpper())
        {
            case "D":
                return TransactionType.Deposit;
            case "W":
                return TransactionType.Withdrawal;
            case "I":
                return TransactionType.Interest;
            default:
                throw new ArgumentException($"Invalid transaction type: {input}");
        }
    }

    public static string ToDisplayChar(this TransactionType type)
    {
        return type switch
        {
            TransactionType.Deposit => "D",
            TransactionType.Withdrawal => "W",
            TransactionType.Interest => "I",
            _ => throw new ArgumentOutOfRangeException(nameof(type),
                   $"Unexpected transaction type: {type}")
        };
    }


}
