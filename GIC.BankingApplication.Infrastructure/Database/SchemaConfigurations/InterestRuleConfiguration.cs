using GIC.BankingApplication.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GIC.BankingApplication.Infrastructure.Database.SchemaConfigurations;

public class InterestRuleConfiguration : IEntityTypeConfiguration<InterestRule>
{
    public void Configure(EntityTypeBuilder<InterestRule> builder)
    {
        builder.ToTable("interest_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.RuleId).IsRequired();
        builder.Property(x => x.Rate).IsRequired();
    }
}