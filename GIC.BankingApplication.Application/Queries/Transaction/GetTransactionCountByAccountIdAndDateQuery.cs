using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Queries.Transaction
{
    public record GetTransactionCountByAccountIdAndDateQuery(int AccountId, DateTime Date) : IRequest<int>;

    public class GetTransactionCountByAccountIdAndDateQueryHandler(IBankingApplicationDbContext dbContext)
        : IRequestHandler<GetTransactionCountByAccountIdAndDateQuery, int>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;

        public async Task<int> Handle(GetTransactionCountByAccountIdAndDateQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.DbSet<Domain.Models.Transaction>()
                .CountAsync(e => e.AccountId == request.AccountId
                    && e.Date == request.Date, cancellationToken);
        }
    }
}
