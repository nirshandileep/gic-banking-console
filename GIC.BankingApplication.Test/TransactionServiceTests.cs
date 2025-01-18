using FluentAssertions;
using FluentValidation;
using GIC.BankingApplication.Application.Queries.Account;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Domain.Enums;
using GIC.BankingApplication.Infrastructure.Dtos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GIC.BankingApplication.Test;

public class TransactionServiceTests : IClassFixture<TestFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public TransactionServiceTests(TestFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public async Task InputTransaction_NewAccount_CreatesAccountAndTransactionInDb()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var newAccountNumber = "AC999";
        var request = new CreateTransactionRequestDto
        {
            AccountNumber = newAccountNumber,
            Amount = 100.00m,
            Date = System.DateTime.UtcNow,
            Type = TransactionType.Deposit
        };

        await transactionService.InputTransaction(request);

        var account = await mediator.Send(new GetAccountByNumberQuery(newAccountNumber));

        // Assert
        account.Should().NotBeNull("the account should have been created by the InputTransaction method");
        account!.Balance.Should().Be(100.00m, "the balance should equal the deposit amount for a new account");
        account.Transactions.Should().HaveCount(1, "there should be one transaction created for this deposit");

        var txn = account.Transactions.First();
        txn.TransactionId.Should().NotBeNullOrEmpty();
        txn.Amount.Should().Be(100.00m);
        txn.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task AddingTwoTransactionsOnSameDate_ShouldIncrementTransactionId()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var accountNumber = "AC123";
        var transactionDate = new DateTime(2023, 10, 24, 0, 0, 0, DateTimeKind.Utc);

        var firstRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = transactionDate,
            Type = TransactionType.Deposit,
            Amount = 100m
        };

        var secondRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = transactionDate,
            Type = TransactionType.Deposit,
            Amount = 200m
        };

        // Act
        await transactionService.InputTransaction(firstRequest);
        await transactionService.InputTransaction(secondRequest);

        var accountDto = await mediator.Send(new GetAccountByNumberQuery(accountNumber));

        // Assert
        accountDto.Should().NotBeNull("account should have been created or retrieved");
        accountDto!.Transactions.Should().HaveCount(2, "we inserted two transactions");

        var sortedTxns = accountDto.Transactions.OrderBy(t => t.TransactionId).ToList();
        sortedTxns[0].TransactionId.Should().Be("20231024-01");
        sortedTxns[1].TransactionId.Should().Be("20231024-02");
    }

    [Fact]
    public async Task AddingFirstTransactionAsWithdrawal_ShouldFail()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var newAccountNumber = "ACW001";
        var transactionDateUtc = new DateTime(2025, 01, 25, 0, 0, 0, DateTimeKind.Utc);

        // Create withdrawal request
        var request = new CreateTransactionRequestDto
        {
            AccountNumber = newAccountNumber,
            Date = transactionDateUtc,
            Type = TransactionType.Withdrawal,
            Amount = 50.00m
        };

        // Act & Assert
        Func<Task> act = () => transactionService.InputTransaction(request);

        await act.Should().ThrowAsync<ValidationException>();

        var account = await mediator.Send(new GetAccountByNumberQuery(newAccountNumber));
        account.Should().BeNull("no account should be created when the first transaction is a withdrawal");
    }

    [Fact]
    public async Task WithdrawalBeyondCurrentBalance_ShouldFailAndNotCreateTransaction()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var accountNumber = "ACOverdraft01";
        var txnDateUtc = new DateTime(2025, 02, 10, 0, 0, 0, DateTimeKind.Utc);

        var depositRequest1 = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 100m
        };

        var depositRequest2 = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 200m
        };

        var withdrawalRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Withdrawal,
            Amount = 400m
        };

        // Act
        await transactionService.InputTransaction(depositRequest1);
        await transactionService.InputTransaction(depositRequest2);

        Func<Task> act = async () =>
            await transactionService.InputTransaction(withdrawalRequest);

        await act.Should().ThrowAsync<ValidationException>();

        var accountDto = await mediator.Send(new GetAccountByNumberQuery(accountNumber));
        accountDto.Should().NotBeNull();

        accountDto!.Transactions.Should().HaveCount(2,
            "the failed withdrawal should not create a transaction record");

        accountDto.Balance.Should().Be(300m,
            "failed withdrawal should not affect the account balance");
    }

    [Theory]
    [InlineData(-50)]
    [InlineData(0)]
    public async Task DepositWithNonPositiveAmount_ShouldFail(decimal invalidAmount)
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var accountNumber = "ACZeroNeg01";
        var txnDateUtc = new DateTime(2025, 03, 10, 0, 0, 0, DateTimeKind.Utc);

        var depositRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = invalidAmount
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.InputTransaction(depositRequest);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task WithdrawNonPositiveAmount_ShouldFail_WhenAccountAlreadyHasTransactions(decimal invalidWithdrawal)
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var accountNumber = $"ACWithdrawZeroNeg{invalidWithdrawal}";
        var txnDateUtc = new DateTime(2025, 03, 15, 0, 0, 0, DateTimeKind.Utc);

        var initialDeposit = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 100m
        };

        await transactionService.InputTransaction(initialDeposit);

        var invalidWithdrawalRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Withdrawal,
            Amount = invalidWithdrawal
        };

        // Act
        Func<Task> act = async () =>
            await transactionService.InputTransaction(invalidWithdrawalRequest);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        var accountDto = await mediator.Send(new GetAccountByNumberQuery(accountNumber));
        accountDto.Should().NotBeNull("the account was previously created by the deposit");
        accountDto!.Transactions.Should().HaveCount(1, "the invalid withdrawal should not create a transaction record");
        accountDto.Balance.Should().Be(100m, "failed withdrawal should not affect the balance");
    }

    [Fact]
    public async Task WithdrawExactBalance_ShouldSucceed_AndResultInZeroBalance()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var accountNumber = "ACExactBalance";
        var txnDateUtc = new DateTime(2025, 04, 10, 0, 0, 0, DateTimeKind.Utc);

        var depositRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 300m
        };

        await transactionService.InputTransaction(depositRequest);

        var withdrawRequest = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Withdrawal,
            Amount = 300m
        };

        // Act
        await transactionService.InputTransaction(withdrawRequest);

        // Assert
        var accountDto = await mediator.Send(new GetAccountByNumberQuery(accountNumber));
        accountDto.Should().NotBeNull();
        accountDto!.Balance.Should().Be(0m, "withdrawing the entire balance should leave 0");

        accountDto.Transactions.Should().HaveCount(2, "one deposit and one withdrawal should be recorded");

        var lastTxn = accountDto.Transactions.Last();
        lastTxn.Type.Should().Be(TransactionType.Withdrawal);
        lastTxn.Amount.Should().Be(300m);
    }

    [Fact]
    public async Task ConcurrentDepositsAndWithdrawals_ShouldHandleConcurrencyCorrectly()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();

        var accountNumber = "ACConcurrent01";
        var txnDateUtc = new DateTime(2025, 04, 15, 0, 0, 0, DateTimeKind.Utc);

        var initialDeposit = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 500m 
        };

        await transactionService.InputTransaction(initialDeposit);

        var deposit1 = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 200m
        };

        var deposit2 = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Deposit,
            Amount = 300m
        };

        var withdrawal1 = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Withdrawal,
            Amount = 150m
        };

        var withdrawal2 = new CreateTransactionRequestDto
        {
            AccountNumber = accountNumber,
            Date = txnDateUtc,
            Type = TransactionType.Withdrawal,
            Amount = 200m
        };

        // Act
        await Task.WhenAll(
            transactionService.InputTransaction(deposit1),
            transactionService.InputTransaction(deposit2),
            transactionService.InputTransaction(withdrawal1),
            transactionService.InputTransaction(withdrawal2)
        );

        // Assert
        var accountDto = await mediator.Send(new GetAccountByNumberQuery(accountNumber));
        accountDto.Should().NotBeNull();

        accountDto!.Balance.Should().Be(650m, "concurrent transactions should not corrupt the balance");
        accountDto.Transactions.Should().HaveCount(5, "all concurrent transactions should be recorded");
    }
}
