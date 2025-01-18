using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Commands.InterestRule;

public record UpdateInterestRuleCommand(UpdateInterestRuleRequestDto Request) : CommandRequest;

public class UpdateInterestRuleCommandHandler(IBankingApplicationDbContext dbContext) : BaseCommandHandler<UpdateInterestRuleCommand>
{
    private readonly IBankingApplicationDbContext _dbContext = dbContext;
    private Domain.Models.InterestRule _rule;

    protected override async Task<Response> DoHandle()
    {
        _rule.SetRule(Command.Request.RuleId).SetRate(Command.Request.Rate);

        await _dbContext.SaveChangesAsync();

        return Response.Ok();
    }

    protected override async Task Validate(ValidationContext validationContext)
    {
        _rule = await _dbContext.DbSet<Domain.Models.InterestRule>().SingleOrDefaultAsync(x => x.Id == Command.Request.Id);

        if (_rule == null)
        {
            validationContext.AddError("Rule not found.");
        }
    }
}
