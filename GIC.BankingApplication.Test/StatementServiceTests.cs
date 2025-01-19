using FluentAssertions;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Domain.Enums;
using GIC.BankingApplication.Infrastructure.Dtos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GIC.BankingApplication.Test;

public class StatementServiceTests : IClassFixture<TestFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public StatementServiceTests(TestFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public async Task GenerateStatement_ForAccountWithoutTransactions_ShouldReturnEmptyStatement()
    {
        // Arrange
        var statementService = _serviceProvider.GetRequiredService<IStatementService>();
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        var accountNumber = "ACEmptyAccount";

        // Act
        var statement = await statementService.GetAccountStatement(accountNumber);

        // Assert
        statement.Should().NotBeNull("the service should return a valid statement object");
        statement.AccountNumber.Should().Be(accountNumber, "the statement should belong to the requested account");
        statement.Transactions.Should().BeNull("there are no transactions for this account");
    }

    [Fact]
    public async Task GenerateStatement_WithMultipleTransactionsWithNoInterestRule_ShouldIncludeAllTransactionsAndCalculateBalancesCorrectly()
    {
        // Arrange
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        var statementService = _serviceProvider.GetRequiredService<IStatementService>();

        var accountNumber = "ACMultiTxn";
        var txnDateUtc = new DateTime(2025, 04, 01, 0, 0, 0, DateTimeKind.Utc);

        // Add multiple transactions
        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 500m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc.AddDays(1),
            Type = TransactionType.Deposit,
            Amount = 200m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc.AddDays(2),
            Type = TransactionType.Withdrawal,
            Amount = 300m
        });

        // Act
        var statement = await statementService.GetAccountStatement(accountNumber);

        // Assert
        statement.Should().NotBeNull("the statement should be generated successfully");
        statement.AccountNumber.Should().Be(accountNumber, "the statement should belong to the requested account");
        statement.Transactions.Should().HaveCount(3, "all transactions should be included in the statement");

        // Validate transaction details and balances
        var transactions = statement.Transactions.ToList();

        transactions[0].Type.Should().Be(TransactionType.Deposit);
        transactions[0].Amount.Should().Be(500m);

        transactions[1].Type.Should().Be(TransactionType.Deposit);
        transactions[1].Amount.Should().Be(200m);

        transactions[2].Type.Should().Be(TransactionType.Withdrawal);
        transactions[2].Amount.Should().Be(300m);
    }

    [Fact]
    public async Task GenerateMonthlyStatement_ShouldIncludeOnlyCurrentMonthTransactions_AndConsiderPreviousBalance()
    {
        // Arrange
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        var statementService = _serviceProvider.GetRequiredService<IStatementService>();
        var interestRuleService = _serviceProvider.GetRequiredService<IInterestRuleService>();

        var accountNumber = "ACMonthlyTest";

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2025, 03, 15, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 500m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2025, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 200m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2025, 04, 10, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Withdrawal,
            Amount = 100m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2025, 05, 01, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 300m
        });

        var rulePercentage = 2.5m;
        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2025, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE01",
            Rate = rulePercentage
        });

        var balancePeriods = new[]
        {
            (500m + 200m, 9),  // Balance 700m for 9 days (April 1-9)
            (500m + 200m - 100m, 21) // Balance 600m for 21 days (April 10-30)
        };

        var ruleRate = rulePercentage / 100m; 
        var totalInterest = balancePeriods.Sum(period =>
            (period.Item1 * period.Item2 * ruleRate) / 365m);

        var expectedInterest = Math.Round(totalInterest, 2, MidpointRounding.AwayFromZero);

        // Act
        var statement = await statementService.GetMonthlyStatement(accountNumber, "202504");

        // Assert
        statement.Should().NotBeNull("a statement should be generated");
        statement.AccountNumber.Should().Be(accountNumber, "the statement should belong to the requested account");

        // Verify transactions in the statement (only current month)
        var transactions = statement.Transactions.ToList();
        transactions.Should().HaveCount(3, "only transactions from the current month should be included");

        transactions[0].Type.Should().Be(TransactionType.Deposit);
        transactions[0].Amount.Should().Be(200m);
        transactions[0].Balance.Should().Be(700m, "starting balance from March plus April deposit");

        transactions[1].Type.Should().Be(TransactionType.Withdrawal);
        transactions[1].Amount.Should().Be(100m);
        transactions[1].Balance.Should().Be(600m, "previous balance minus withdrawal");

        transactions[2].Type.Should().Be(TransactionType.Interest);
        transactions[2].Amount.Should().Be(expectedInterest, "interest should be calculated for April based on daily balances");
        transactions[2].Balance.Should().Be(600m + expectedInterest, "interest is added to the final balance");

        // Verify transactions outside the current month are excluded
        transactions.Should().NotContain(t => t.Date.Month == 3 || t.Date.Month == 5,
            "transactions from March or May should not be included in the April statement");
    }

    [Fact]
    public async Task GenerateMonthlyStatement_NoTransactionsInMonth_ShouldCalculateInterestFromPreviousBalanceAndRule()
    {
        // Arrange
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        var statementService = _serviceProvider.GetRequiredService<IStatementService>();
        var interestRuleService = _serviceProvider.GetRequiredService<IInterestRuleService>();

        var accountNumber = "ACNoTxnMonth";

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2025, 03, 15, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 1000m
        });

        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2025, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE01",
            Rate = 3.0m
        });

        // Act
        var statement = await statementService.GetMonthlyStatement(accountNumber, "202504");

        // Assert
        statement.Should().NotBeNull("a statement should be generated");
        statement.AccountNumber.Should().Be(accountNumber, "the statement should belong to the requested account");

        var transactions = statement.Transactions.ToList();
        transactions.Should().HaveCount(1, "only the interest transaction should be included for this month");

        // Manually calculate interest for April
        var openingBalance = 1000m; 
        var daysInApril = 30;
        var ruleRate = 3.0m / 100m; 
        var totalInterest = (openingBalance * daysInApril * ruleRate) / 365m;

        var expectedInterest = Math.Round(totalInterest, 2, MidpointRounding.AwayFromZero);

        transactions[0].Type.Should().Be(TransactionType.Interest);
        transactions[0].Amount.Should().Be(expectedInterest, "interest should match the manually calculated value");
        transactions[0].Balance.Should().Be(openingBalance + expectedInterest, "interest should be added to the previous balance");

        transactions.Should().NotContain(t => t.Type != TransactionType.Interest,
            "only interest transactions should be included for a month with no other transactions");
    }
}
