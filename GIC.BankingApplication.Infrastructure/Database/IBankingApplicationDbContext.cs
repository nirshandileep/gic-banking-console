using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GIC.BankingApplication.Infrastructure.Database;

public interface IBankingApplicationDbContext
{
    DbSet<TEntity> DbSet<TEntity>() where TEntity : class;
    Task<EntityEntry<TEntity>> AddEntityAsync<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
}
