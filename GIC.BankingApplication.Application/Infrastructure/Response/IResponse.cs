using FluentValidation.Results;

namespace GIC.BankingApplication.Application.Infrastructure.Response;

public interface IResponse
{
    IList<ValidationFailure> ValidationResults { get; set; }
    bool Success { get; }
    TResult Result<TResult>() where TResult : class;
    void SetResult<TResult>(TResult result) where TResult : class;
}
