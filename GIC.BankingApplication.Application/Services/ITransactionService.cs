namespace GIC.BankingApplication.Application.Services;

public interface ITransactionService
{
    Task InputTransaction(CreateTransactionRequestDto transaction);
}