using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Application.Commands.Transaction
{
    public record CreateTransactionCommand(CreateTransactionRequestDto Request) : CommandRequest;

    public class CreateTransactionCommandHandler(IBankingApplicationDbContext dbContext) : BaseCommandHandler<CreateTransactionCommand>
    {
        private readonly IBankingApplicationDbContext _dbContext = dbContext;

        protected override async Task<Response> DoHandle()
        {
            var transaction = Domain.Models.Transaction.CreateNewTransaction(Command.Request.Date, Command.Request.TransactionId,
                Command.Request.AccountId, Command.Request.Amount).SetTransactionType(Command.Request.Type);

            await _dbContext.AddEntityAsync(transaction);

            await _dbContext.SaveChangesAsync();

            return Response.Ok();
        }

        protected override async Task Validate(ValidationContext validationContext)
        {
            if (await _dbContext.DbSet<Domain.Models.Transaction>()
                .AnyAsync(e => e.AccountId == Command.Request.AccountId
                    && e.TransactionId == Command.Request.TransactionId))
            {
                validationContext.AddError($"TransactionId {Command.Request.TransactionId} already exist for this account.");
            }
        }
    }
}
