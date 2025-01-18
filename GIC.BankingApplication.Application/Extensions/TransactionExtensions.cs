using GIC.BankingApplication.Domain.Enums;

namespace GIC.BankingApplication.Application.Extensions
{
    public static class TransactionExtensions
    {
        public static string GenerateNextTransactionId(
            this IEnumerable<TransactionDto> existingTransactions,
            DateTime transactionDate)
        {
            int maxNumber = 0;

            if (existingTransactions != null && existingTransactions.Any())
            {
                var sameDayForAccountTxns = existingTransactions
                    .Where(t => t.Date.Date == transactionDate.Date)
                    .ToList();

                foreach (var txn in sameDayForAccountTxns)
                {
                    var parts = txn.TransactionId?.Split('-');
                    if (parts?.Length == 2 && int.TryParse(parts[1], out int runningNum))
                    {
                        if (runningNum > maxNumber)
                            maxNumber = runningNum;
                    }
                }
            }

            int nextNumber = maxNumber + 1;
            string datePrefix = transactionDate.ToString("yyyyMMdd");
            return $"{datePrefix}-{nextNumber:D2}";
        }

        public static decimal ApplyTransaction(this decimal balance, TransactionDto transaction)
        {
            switch (transaction.Type)
            {
                case TransactionType.Deposit:
                    balance += transaction.Amount;
                    break;
                case TransactionType.Withdrawal:
                    balance -= transaction.Amount;
                    break;
                case TransactionType.Interest:
                    balance += transaction.Amount;
                    break;
                default:
                    throw new InvalidOperationException($"Calculation logic for transaction type {transaction.Type} is not defined.");
            }

            return balance;
        }

    }
}
