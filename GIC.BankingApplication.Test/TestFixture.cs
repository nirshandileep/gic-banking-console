using GIC.BankingApplication.Application.Config;
using GIC.BankingApplication.Application.Infrastructure.MediatR;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.Infrastructure.Config;
using GIC.BankingApplication.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GIC.BankingApplication.Test;

public class TestFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; private set; }

    public TestFixture()
    {
        var services = new ServiceCollection();

        var configValues = new Dictionary<string, string>
        {
            ["Database:Host"] = "localhost",
            ["Database:Port"] = "5432",
            ["Database:Username"] = "postgres",
            ["Database:Password"] = "1234",
            ["Database:Database"] = "bankingapp",
            ["Database:Schema"] = "public"
        };

        var testConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        services.AddDbContext<BankingApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("BankAppInMemoryTest");
        });

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Application.Config.IoC));
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
        });

        services.RegisterApplication();
        services.RegisterInfrastructure(testConfig);

        services.AddTransient<ITransactionService, TransactionService>();
        services.AddTransient<IInterestRuleService, InterestRuleService>();
        services.AddTransient<IStatementService, StatementService>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        ServiceProvider?.Dispose();
    }
}