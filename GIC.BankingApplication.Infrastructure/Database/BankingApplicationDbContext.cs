namespace GIC.BankingApplication.Infrastructure.Database;

public class BankingApplicationDbContext(IOptions<DatabaseSettings> dbSettings) : DbContext, IBankingApplicationDbContext
{
    private readonly DatabaseSettings _dbSettings = dbSettings.Value;

    public DbContext Instance { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(_dbSettings.Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingApplicationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test")
            {
                optionsBuilder.UseInMemoryDatabase("BankAppInMemoryTest");
            }
            else
            {
                optionsBuilder.UseNpgsql(_dbSettings.ConnectionString, npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(BankingApplicationDbContext).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, _dbSettings.Schema);
                }).UseSnakeCaseNamingConvention();
            }
        }
    }

    public async Task<EntityEntry<TEntity>> AddEntityAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return await AddAsync(entity);
    }

    public DbSet<T> DbSet<T>() where T : class
    {
        return Set<T>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
