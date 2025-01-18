using GIC.BankingApplication.Application.Commands.Account;
using GIC.BankingApplication.Application.Extensions;
using GIC.BankingApplication.Application.Queries.Account;

namespace GIC.BankingApplication.Application.Services;

public class TransactionService(IMediator mediator) : ITransactionService
{
    private readonly IMediator _mediator = mediator;

    public async Task InputTransaction(CreateTransactionRequestDto transaction)
    {
        var account = await _mediator.Send(new GetAccountByNumberQuery(transaction.AccountNumber));

        if (account == null)
        {
            await _mediator.Send(new CreateAccountCommand(
                new CreateAccountRequestDto
                {
                    AccountNumber = transaction.AccountNumber,
                    Balance = transaction.Amount
                }));

            account = await _mediator.Send(new GetAccountByNumberQuery(transaction.AccountNumber));
        }

        var nextTransactionId = account.Transactions.GenerateNextTransactionId(transaction.Date);

        transaction.AccountId = account.Id;
        transaction.TransactionId = nextTransactionId;

        await _mediator.Send(new CreateTransactionCommand(transaction));

        switch (transaction.Type)
        {
            case Domain.Enums.TransactionType.Deposit:
                await _mediator.Send(new UpdateAccountCommand(new UpdateAccountRequestDto
                {
                    AccountId = account.Id,
                    Balance = (account.Balance + transaction.Amount)
                }));
                break;
            case Domain.Enums.TransactionType.Withdrawal:
                await _mediator.Send(new UpdateAccountCommand(new UpdateAccountRequestDto
                {
                    AccountId = account.Id,
                    Balance = (account.Balance - transaction.Amount)
                }));
                break;
            case Domain.Enums.TransactionType.Interest:
                await _mediator.Send(new UpdateAccountCommand(new UpdateAccountRequestDto
                {
                    AccountId = account.Id,
                    Balance = (account.Balance + transaction.Amount)
                }));
                break;
            default:
                throw new InvalidOperationException("Invalid transaction type.");
        }
    }
}