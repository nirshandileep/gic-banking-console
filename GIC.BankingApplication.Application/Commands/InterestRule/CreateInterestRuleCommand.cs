
namespace GIC.BankingApplication.Application.Commands.InterestRule
{
    public record CreateInterestRuleCommand(CreateInterestRuleRequestDto Request) : CommandRequest;

    public class CreateInterestRuleCommandHandler(IBankingApplicationDbContext dbContext) : BaseCommandHandler<CreateInterestRuleCommand>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;

        protected override async Task<Response> DoHandle()
        {
            var account = Domain.Models.InterestRule.CreateNewInterestRule(Command.Request.Date, Command.Request.RuleId, Command.Request.Rate);

            await _dbContext.AddEntityAsync(account);

            await _dbContext.SaveChangesAsync();

            return Response.Ok();
        }

        protected override Task Validate(ValidationContext validationContext)
        {
            return Task.CompletedTask;
        }
    }
}
