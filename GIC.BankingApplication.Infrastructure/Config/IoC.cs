using GIC.BankingApplication.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GIC.BankingApplication.Infrastructure.Config;

public static class IoC
{
    public static void RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        services.AddScoped<IBankingApplicationDbContext, BankingApplicationDbContext>();
    }
}
