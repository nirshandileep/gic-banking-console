using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Infrastructure.Database;

public class BankingApplicationDbContextSeed
{
    public async Task SeedAsync(BankingApplicationDbContext context)
    {
        using (context)
        {
            await context.Database.MigrateAsync();
        }
    }
}
