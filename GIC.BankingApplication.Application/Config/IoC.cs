using GIC.BankingApplication.Application.Commands.InterestRule;

namespace GIC.BankingApplication.Application.Config;

public static class IoC
{
    public static void RegisterApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<CreateAccountCommand>, CreateAccountCommandValidator>();
        services.AddSingleton<IValidator<UpdateAccountCommand>, UpdateAccountCommandValidator>();
        services.AddSingleton<IValidator<CreateInterestRuleCommand>, CreateInterestRuleCommandValidator>();
        services.AddSingleton<IValidator<UpdateInterestRuleCommand>, UpdateInterestRuleCommandValidator>();
        services.AddSingleton<IValidator<CreateTransactionCommand>, CreateTransactionCommandValidator>();
    }
}
