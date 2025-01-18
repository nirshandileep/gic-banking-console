using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Queries.InterestRule
{
    public record GetInterestRuleByDateQuery(DateTime Date) : IRequest<InterestRuleDto?>;

    public class GetInterestRuleByDateQueryHandler(IBankingApplicationDbContext dbContext) : IRequestHandler<GetInterestRuleByDateQuery,
        InterestRuleDto?>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;

        public async Task<InterestRuleDto?> Handle(GetInterestRuleByDateQuery request, CancellationToken cancellationToken)
        {
            var rule = await _dbContext.DbSet<Domain.Models.InterestRule>()
                .Select(e => new InterestRuleDto
                {
                    Id = e.Id,
                    Date = e.Date,
                    Rate = e.Rate,
                    RuleId = e.RuleId,
                }).SingleOrDefaultAsync(e => e.Date == request.Date, cancellationToken);

            return rule;
        }
    }
}
