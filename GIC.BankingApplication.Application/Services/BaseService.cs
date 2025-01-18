namespace GIC.BankingApplication.Application.Services
{
    public class BaseService
    {
        protected IMediator Mediator { get; }

        public BaseService(IMediator mediator)
        {
            Mediator = mediator;
        }
    }
}
