using GIC.BankingApplication.Application.Config;
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

        // 2. Build an in-memory configuration (mock config)
        var testConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                // Adjust as needed for your environment
                ["Database:Host"] = "localhost",
                ["Database:Port"] = "5432",
                ["Database:Username"] = "postgres",
                ["Database:Password"] = "1234",
                ["Database:Database"] = "bankingapp",
                ["Database:Schema"] = "public"
            })
            .Build();

        // 3. Register the in-memory database for EF Core
        services.AddDbContext<BankingApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("BankAppInMemoryTest");
        });
        
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        // 4. Register MediatR, so it can discover command/query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Application.Config.IoC));
        });

        // 5. Register your application and infrastructure
        services.RegisterApplication();
        services.RegisterInfrastructure(testConfig);
        //services.AddScoped<IBankingApplicationDbContext, BankingApplicationDbContext>();

        ServiceProvider = services.BuildServiceProvider();

    }

    public void Dispose()
    {
        ServiceProvider?.Dispose();
    }
}