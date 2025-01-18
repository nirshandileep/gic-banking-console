using Microsoft.EntityFrameworkCore;

namespace GIC.BankingApplication.Infrastructure.Database;

public interface IDbContext
{
    DbContext Instance { get; }
}
