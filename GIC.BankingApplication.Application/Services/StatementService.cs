namespace GIC.BankingApplication.Application.Services;

public class StatementService(IMediator mediator) : IStatementService
{
    private readonly IMediator _mediator = mediator;

    public async Task<StatementDto> GetAccountStatement(string accountNumber)
    {
        var account = await _mediator.Send(new GetAccountByNumberQuery(accountNumber));

        var statement = new StatementDto
        {
            AccountNumber = accountNumber,
            Transactions = account?.Transactions.Select(e => new StatementTransactionDto
            {
                Date = e.Date,
                TxnId = e.TransactionId,
                Type = e.Type,
                Amount = e.Amount,
            }).ToList()
        };

        return statement;
    }

    public async Task<StatementDto> GetMonthlyStatement(string accountInput, string yearMonthInput)
    {
        DateTime startDate, endDate;
        (startDate, endDate) = GetStatementDateRange(yearMonthInput);

        var account = await _mediator.Send(new GetAccountByNumberQuery(accountInput));
        if (account == null)
            throw new InvalidOperationException($"Account '{accountInput}' not found.");

        (var statementTransactions, var runningBalance, var totalMonthInterest) = 
            await BuildTransactionsLineItems(startDate, endDate, account);

        AddInteresetTransactionLineItem(endDate, runningBalance, statementTransactions, totalMonthInterest);

        var statementMonthTransactions = statementTransactions
            .Where(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date)
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Type)
            .ToList();

        var statementDto = new StatementDto
        {
            AccountNumber = accountInput,
            Transactions = statementMonthTransactions
        };

        return statementDto;
    }

    private async Task<(List<StatementTransactionDto> statementTransactions, decimal runningBalance,  
        decimal totalMonthInterest)> BuildTransactionsLineItems(DateTime startDate, DateTime endDate, AccountDto account)
    {
        var allTransactions = account.Transactions
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id)
                    .ToList();

        var allRules = await _mediator.Send(new GetAllInterestRulesQuery());
        var sortedRules = allRules.OrderBy(r => r.Date).ThenBy(e => e.Id).ToList();

        var runningBalance = 0m;
        var statementTransactions = new List<StatementTransactionDto>();
        DateTime dayBeforeStart = startDate.AddDays(-1);
        runningBalance = CalculateBalanceUpToDate(allTransactions, dayBeforeStart);

        var totalMonthInterest = 0m;
        for (DateTime currentDay = startDate; currentDay <= endDate; currentDay = currentDay.AddDays(1))
        {
            var currentDayTransactions = allTransactions
                .Where(t => t.Date.Date == currentDay.Date)
                .ToList();

            foreach (var dt in currentDayTransactions)
            {
                runningBalance = runningBalance.ApplyTransaction(dt);

                statementTransactions.Add(new StatementTransactionDto
                {
                    Date = dt.Date,
                    TxnId = dt.TransactionId,
                    Type = dt.Type,
                    Amount = dt.Amount,
                    Balance = runningBalance
                });
            }

            var rule = GetActiveInterestRule(sortedRules, currentDay);
            if (rule != null)
            {
                decimal dailyInterest = runningBalance * (rule.Rate / 100m) / 365m;
                totalMonthInterest += dailyInterest;
            }
        }

        return (statementTransactions, runningBalance, totalMonthInterest);
    }

    private static void AddInteresetTransactionLineItem(DateTime endDate, decimal runningBalance, List<StatementTransactionDto> statementTransactions, 
        decimal totalMonthInterest)
    {
        decimal monthlyInterest = Math.Round(totalMonthInterest, 2, MidpointRounding.AwayFromZero);
        if (monthlyInterest != 0)
        {
            runningBalance += monthlyInterest;

            statementTransactions.Add(new StatementTransactionDto
            {
                Date = endDate,
                TxnId = "",
                Type = TransactionType.Interest,
                Amount = monthlyInterest,
                Balance = runningBalance
            });
        }
    }

    private (DateTime, DateTime) GetStatementDateRange(string yearMonthInput)
    {
        if (string.IsNullOrWhiteSpace(yearMonthInput) || yearMonthInput.Length != 6)
            throw new ArgumentException("Invalid yearMonthInput format. Must be YYYYMM.", nameof(yearMonthInput));

        int year = int.Parse(yearMonthInput[..4], CultureInfo.InvariantCulture);
        int month = int.Parse(yearMonthInput[4..], CultureInfo.InvariantCulture);

        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return (startDate, endDate);
    }

    private static decimal CalculateBalanceUpToDate(List<TransactionDto> allTxns, DateTime upToDate)
    {
        decimal balance = 0m;
        var relevantTxns = allTxns
            .Where(t => t.Date.Date <= upToDate.Date)
            .ToList();

        relevantTxns.ForEach(tx => balance = balance.ApplyTransaction(tx));

        return balance;
    }

    private static InterestRuleDto GetActiveInterestRule(List<InterestRuleDto> sortedRules, DateTime currentDay)
    {
        InterestRuleDto activeRule = null;

        foreach (var rule in sortedRules)
        {
            if (rule.Date.Date <= currentDay.Date)
            {
                activeRule = rule;
            }
            else
            {
                break;
            }
        }

        return activeRule;
    }
}
