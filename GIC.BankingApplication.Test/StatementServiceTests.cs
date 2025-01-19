using FluentAssertions;
using GIC.BankingApplication.Application.Queries.Account;
using GIC.BankingApplication.Application.Queries.InterestRule;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Domain.Enums;
using GIC.BankingApplication.Infrastructure.Dtos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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
        var txnDateUtc = new DateTime(2020, 04, 01, 0, 0, 0, DateTimeKind.Utc);

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
            Date = new DateTime(2021, 03, 15, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 500m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2021, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 200m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2021, 04, 10, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Withdrawal,
            Amount = 100m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2021, 05, 01, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 300m
        });

        var rulePercentage = 2.5m;
        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2021, 04, 01, 0, 0, 0, DateTimeKind.Utc),
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
        var statement = await statementService.GetMonthlyStatement(accountNumber, "202104");

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
            Date = new DateTime(2022, 03, 15, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 1000m
        });

        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2022, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE01",
            Rate = 3.0m
        });

        // Act
        var statement = await statementService.GetMonthlyStatement(accountNumber, "202204");

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


    [Fact]
    public async Task GenerateMonthlyStatement_MultipleRulesBeforeMonth_ShouldUseLastRuleForCalculation()
    {
        // Arrange
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        var statementService = _serviceProvider.GetRequiredService<IStatementService>();
        var interestRuleService = _serviceProvider.GetRequiredService<IInterestRuleService>();

        var accountNumber = "ACMultipleRules";

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = new DateTime(2023, 03, 15, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 2000m
        });

        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2023, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE01",
            Rate = 2.0m
        });

        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2023, 03, 02, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE02",
            Rate = 2.5m
        });

        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2023, 03, 03, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE03",
            Rate = 3.0m
        });

        // Act
        var statement = await statementService.GetMonthlyStatement(accountNumber, "202304");

        // Assert
        statement.Should().NotBeNull("a statement should be generated");
        statement.AccountNumber.Should().Be(accountNumber, "the statement should belong to the requested account");

        var transactions = statement.Transactions.ToList();
        transactions.Should().HaveCount(1, "only the interest transaction should be included for this month");

        var openingBalance = 2000m;
        var daysInApril = 30;
        var ruleRate = 3.0m / 100m;
        var totalInterest = (openingBalance * daysInApril * ruleRate) / 365m;

        var expectedInterest = Math.Round(totalInterest, 2, MidpointRounding.AwayFromZero);

        transactions[0].Type.Should().Be(TransactionType.Interest);
        transactions[0].Amount.Should().Be(expectedInterest, "interest should match the manually calculated value using the last rule");
        transactions[0].Balance.Should().Be(openingBalance + expectedInterest, "interest should be added to the previous balance");

        transactions.Should().NotContain(t => t.Type != TransactionType.Interest,
            "only interest transactions should be included for a month with no other transactions");
    }

    [Fact]
    public async Task GetMonthlyStatement_ShouldReturnCorrectStatement_WhenTransactionsAndRulesAreProvided()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();

        var accountInput = "AC001";
        var yearMonthInput = "202504";
        var account = new AccountDto
        {
            Id = 1,
            AccountNumber = accountInput,
            Balance = 1000m,
            Transactions = new List<TransactionDto>
            {
                new TransactionDto { Date = new DateTime(2025, 03, 15, 0, 0, 0, DateTimeKind.Utc), TransactionId = "20250315-01", Type = TransactionType.Deposit, Amount = 1000m },
                new TransactionDto { Date = new DateTime(2025, 04, 01, 0, 0, 0, DateTimeKind.Utc), TransactionId = "20250401-01", Type = TransactionType.Deposit, Amount = 200m },
                new TransactionDto { Date = new DateTime(2025, 04, 10, 0, 0, 0, DateTimeKind.Utc), TransactionId = "20250410-01", Type = TransactionType.Withdrawal, Amount = 100m }
            }
        };

        var interestRules = new List<InterestRuleDto>
        {
            new InterestRuleDto { Date = new DateTime(2025, 03, 01, 0, 0, 0, DateTimeKind.Utc), RuleId = "RULE01", Rate = 3.0m }
        };

        // Mock the Mediator responses
        mockMediator.Setup(m => m.Send(It.IsAny<GetAccountByNumberQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(account);

        mockMediator.Setup(m => m.Send(It.IsAny<GetAllInterestRulesQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(interestRules);

        var statementService = new StatementService(mockMediator.Object);

        // Act
        var statement = await statementService.GetMonthlyStatement(accountInput, yearMonthInput);

        // Assert
        statement.Should().NotBeNull();
        statement.AccountNumber.Should().Be(accountInput);

        var transactions = statement.Transactions.ToList();
        transactions.Should().HaveCount(3, "two transactions and one interest transaction should be included");

        // Verify Transactions
        transactions[0].Type.Should().Be(TransactionType.Deposit);
        transactions[0].Amount.Should().Be(200m);

        transactions[1].Type.Should().Be(TransactionType.Withdrawal);
        transactions[1].Amount.Should().Be(100m);

        // Manually calculate interest
        var balancePeriods = new[]
        {
            (1000m + 200m, 9), // Balance 1200m for 9 days (April 1-9)
            (1000m + 200m - 100m, 21) // Balance 1100m for 21 days (April 10-30)
        };

        var ruleRate = 3.0m / 100m; // 3.0% annual interest
        var totalInterest = balancePeriods.Sum(period =>
            (period.Item1 * period.Item2 * ruleRate) / 365m);

        var expectedInterest = Math.Round(totalInterest, 2, MidpointRounding.AwayFromZero);

        transactions[2].Type.Should().Be(TransactionType.Interest);
        transactions[2].Amount.Should().Be(expectedInterest, "interest should match the manually calculated value");
        transactions[2].Balance.Should().Be(1100m + expectedInterest, "final balance should include interest");

        // Verify no transactions outside April
        transactions.Should().NotContain(t => t.Date.Month == 3 || t.Date.Month == 5,
            "transactions outside the statement month should not be included");
    }

    [Fact]
    public async Task GetMonthlyStatement_ShouldCalculateOpeningBalanceFromPreviousMonth_WhenNoTransactionsOnFirstDay()
    {
        // Arrange
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        var statementService = _serviceProvider.GetRequiredService<IStatementService>();
        var interestRuleService = _serviceProvider.GetRequiredService<IInterestRuleService>();

        var account1 = "AC001"; // Relevant account
        var account2 = "AC002"; // Irrelevant account
        var yearMonthInput = "202406"; // June 2024

        // Transactions for Account 1 (Relevant Account)
        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = account1,
            Date = new DateTime(2024, 05, 15, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 1000m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = account1,
            Date = new DateTime(2024, 06, 07, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 500m
        });

        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = account1,
            Date = new DateTime(2024, 06, 20, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Withdrawal,
            Amount = 200m
        });

        // Transactions for Account 2 (Irrelevant Account)
        await transactionService.InputTransaction(new CreateTransactionRequestDto
        {
            AccountNumber = account2,
            Date = new DateTime(2024, 06, 10, 0, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Deposit,
            Amount = 1000m
        });

        // Interest rules
        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2024, 05, 01, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE01",
            Rate = 2.0m // 2.0% annual interest
        });

        await interestRuleService.DefineInterestRule(new CreateInterestRuleRequestDto
        {
            Date = new DateTime(2024, 06, 15, 0, 0, 0, DateTimeKind.Utc),
            RuleId = "RULE02",
            Rate = 3.0m // 3.0% annual interest
        });

        // Act
        var statement = await statementService.GetMonthlyStatement(account1, yearMonthInput);

        // Assert
        statement.Should().NotBeNull();
        statement.AccountNumber.Should().Be(account1, "the statement should belong to the requested account");

        var transactions = statement.Transactions.ToList();
        transactions.Should().HaveCount(3, "two transactions and one interest transaction should be included for the relevant account");

        // Verify transactions for Account 1
        transactions[0].Type.Should().Be(TransactionType.Deposit);
        transactions[0].Amount.Should().Be(500m);

        transactions[1].Type.Should().Be(TransactionType.Withdrawal);
        transactions[1].Amount.Should().Be(200m);

        // Manual interest calculation
        var balancePeriods = new[]
        {
            (1000m, 6, 2.0m), // Opening balance 1000m for 6 days (June 1-6) at 2.0%
            (1000m + 500m, 8, 2.0m), // Balance 1500m for 8 days (June 7-14) at 2.0%
            (1000m + 500m, 5, 3.0m), // Balance 1300m for 6 days (June 15-19) at 3.0%
            (1000m + 500m - 200m, 11, 3.0m) // Balance 1300m for 10 days (June 20-30) at 3.0%
        };

        var totalInterest = balancePeriods.Sum(period =>
            (period.Item1 * period.Item2 * (period.Item3 / 100m)) / 365m);

        var expectedInterest = Math.Round(totalInterest, 2, MidpointRounding.AwayFromZero);

        transactions[2].Type.Should().Be(TransactionType.Interest);
        transactions[2].Amount.Should().Be(expectedInterest, "interest should match the manually calculated value");
        transactions[2].Balance.Should().Be(1300m + expectedInterest, "final balance should include interest");

        // Verify that no transactions from Account 2 are included
        transactions.Should().NotContain(t => t.TxnId.StartsWith("AC002"), "transactions from other accounts should not be included");
    }

}
