namespace GIC.BankingApplication.Application.Services;

public interface IStatementService
{
    Task<StatementDto> GetAccountStatement(string accountNumber);
    Task<StatementDto> GetMonthlyStatement(string accountInput, string yearMonthInput);
}
