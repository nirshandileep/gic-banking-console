using FluentValidation.Results;

namespace GIC.BankingApplication.Application.Infrastructure.Response;

public class Response : IResponse
{
    public IList<ValidationFailure> ValidationResults { get; set; }

    public bool Success => ValidationResults == null || !ValidationResults.Any();

    protected object ResponseResult { get; private set; }

    public Response()
    {
        ValidationResults = [];
    }

    public Response(IList<ValidationFailure> validationResults)
    {
        ValidationResults = validationResults;
    }

    public TResult Result<TResult>() where TResult : class
    {
        return ResponseResult as TResult;
    }

    public void SetResult<TResult>(TResult result) where TResult : class
    {
        ResponseResult = result;
    }

    public static Response Ok<TResult>(TResult result) where TResult : class
    {
        var response = new Response();

        response.SetResult(result);

        return response;
    }

    public static Response Ok()
    {
        return new Response();
    }
}
