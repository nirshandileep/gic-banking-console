namespace GIC.BankingApplication.Application.Commands;

public record CreateAccountCommand(CreateAccountRequestDto Request) : CommandRequest;

public class CreateAccountCommandHandler(IBankingApplicationDbContext dbContext) : BaseCommandHandler<CreateAccountCommand>
{
    private readonly IBankingApplicationDbContext _dbContext = dbContext;

    protected override async Task<Response> DoHandle()
    {
        var account = Domain.Models.Account.CreateNewAccount(Command.Request.AccountNumber, Command.Request.Balance);

        await _dbContext.AddEntityAsync(account);

        await _dbContext.SaveChangesAsync();

        return Response.Ok();
    }

    protected override Task Validate(ValidationContext validationContext)
    {
        return Task.CompletedTask;
    }
}