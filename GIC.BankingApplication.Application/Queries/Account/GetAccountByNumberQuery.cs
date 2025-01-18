using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Queries.Account
{
    public record GetAccountByNumberQuery(string AccountNumber) : IRequest<AccountDto?>;

    public class GetAccountByIdQueryHandler(IBankingApplicationDbContext dbContext) : IRequestHandler<GetAccountByNumberQuery, AccountDto?>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;

        public async Task<AccountDto?> Handle(GetAccountByNumberQuery request, CancellationToken cancellationToken)
        {
            var account = await _dbContext.DbSet<Domain.Models.Account>()
                .Where(e => e.AccountNumber == request.AccountNumber)
                .Include(e => e.Transactions)
                .SingleOrDefaultAsync(cancellationToken);

            if (account == null)
                return null;

            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Transactions = account.Transactions?.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    AccountId = t.AccountId,
                    Amount = t.Amount,
                    Date = t.Date,
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                })
            };
        }
    }
}
