namespace GIC.BankingApplication.Application.Services;

public class TransactionService(IMediator mediator) : ITransactionService
{
    private readonly IMediator _mediator = mediator;

    public async Task InputTransaction(CreateTransactionRequestDto transaction)
    {
        var account = await _mediator.Send(new GetAccountByNumberQuery(transaction.AccountNumber));
        var newAccount = false;

        if (account == null)
        {
            if (transaction.Type != TransactionType.Deposit)
            {
                throw new ValidationException("First transaction of the account should be a Deposit.");
            }

            await _mediator.Send(new CreateAccountCommand(
                new CreateAccountRequestDto
                {
                    AccountNumber = transaction.AccountNumber,
                    Balance = transaction.Amount
                }));

            newAccount = true;
            account = await _mediator.Send(new GetAccountByNumberQuery(transaction.AccountNumber));
        }

        var nextTransactionId = account.Transactions.GenerateNextTransactionId(transaction.Date);

        transaction.AccountId = account.Id;
        transaction.TransactionId = nextTransactionId;

        if (account.Balance.ApplyTransaction(new TransactionDto { Amount = transaction.Amount, Type = transaction.Type }) < 0)
        {
            throw new ValidationException($"Insufficient Account Balance to withdraw {transaction.Amount:F2}.");
        }

        await _mediator.Send(new CreateTransactionCommand(transaction));

        if (newAccount)
            return;

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