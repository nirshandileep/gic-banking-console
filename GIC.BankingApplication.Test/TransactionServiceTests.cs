using GIC.BankingApplication.Application.Queries.Account;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Domain.Enums;
using GIC.BankingApplication.Infrastructure.Database;
using GIC.BankingApplication.Infrastructure.Dtos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;

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
        var transactionService = new TransactionService(mediator);

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
}
