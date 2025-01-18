using GIC.BankingApplication.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIC.BankingApplication.Infrastructure.Database.SchemaConfigurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("accounts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AccountNumber).IsRequired();
            builder.HasMany(x => x.Transactions).WithOne(x => x.Account).HasForeignKey(x => x.AccountId);
        }
    }
}
