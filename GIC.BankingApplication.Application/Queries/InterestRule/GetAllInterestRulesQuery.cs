using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Queries.InterestRule
{
    public record GetAllInterestRulesQuery : IRequest<IEnumerable<InterestRuleDto>>;

    public class GetAllInterestRulesQueryHandler(IBankingApplicationDbContext dbContext) : IRequestHandler<GetAllInterestRulesQuery, 
        IEnumerable<InterestRuleDto>>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;

        public async Task<IEnumerable<InterestRuleDto>> Handle(GetAllInterestRulesQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.DbSet<Domain.Models.InterestRule>()
                .AsNoTracking()
                .AsQueryable();

            return await query.Select(e => new InterestRuleDto
            {
                EffectiveDate = e.Date,
                Rate = e.Rate,
                RuleId = e.RuleId,
            }).ToListAsync();
        }
    }
}
