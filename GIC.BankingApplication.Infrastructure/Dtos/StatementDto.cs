using System.Transactions;

namespace GIC.BankingApplication.Infrastructure.Dtos;

public class StatementDto
{
    public string AccountNumber { get; set; }
    public List<StatementTransactionDto> Transactions { get; set; }
}
