
using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Commands.Account
{
    public record UpdateAccountCommand(UpdateAccountRequestDto Request) : CommandRequest;

    public class UpdateAccountCommandHandler(IBankingApplicationDbContext dbContext) : BaseCommandHandler<UpdateAccountCommand>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;
        private Domain.Models.Account _account;

        protected override async Task<Response> DoHandle()
        {
            _account.SetBalance(Command.Request.Balance);

            await _dbContext.SaveChangesAsync();

            return Response.Ok();
        }

        protected override async Task Validate(ValidationContext validationContext)
        {
            _account = await _dbContext.DbSet<Domain.Models.Account>().FirstOrDefaultAsync(x => x.Id == Command.Request.AccountId);

            if (_account == null)
            {
                validationContext.AddError("Account not found.");
            }
        }
    }
}
