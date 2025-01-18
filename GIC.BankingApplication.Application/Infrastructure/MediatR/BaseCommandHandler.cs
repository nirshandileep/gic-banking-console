using GIC.BankingApplication.Application.Infrastructure.Response;
using GIC.BankingApplication.Application.Infrastructure.Validation;
using GIC.BankingApplication.Infrastructure.Database;
using MediatR;
using System.Transactions;

namespace GIC.BankingApplication.Application.Infrastructure.MediatR;

public abstract class BaseCommandHandler<TCommand>
    : IRequestHandler<TCommand, IResponse>
    where TCommand : CommandRequest
{
    protected TCommand Command { get; private set; }
    public IBankingApplicationDbContext BankingApplicationDbContext { get; set; }

    public virtual async Task<IResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        Command = request;

        var validationContext = new ValidationContext();

        await Validate(validationContext);
        if (validationContext.HasErrors)
            return new Response.Response(validationContext.ValidationErrors);

        using var transaction = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
        var result = await DoHandle();
        transaction.Complete();

        return !result.Success ? new Response.Response(result.ValidationResults) : result;
    }

    protected abstract Task Validate(ValidationContext validationContext);

    protected abstract Task<Response.Response> DoHandle();
}
