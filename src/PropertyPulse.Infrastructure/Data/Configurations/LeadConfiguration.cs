using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyPulse.Domain.Entities;

namespace PropertyPulse.Infrastructure.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.FullName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Phone).IsRequired().HasMaxLength(30);
        builder.Property(l => l.Email).HasMaxLength(256);
        builder.Property(l => l.Stage).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.BudgetLkr).HasColumnType("numeric(18,2)");

        builder.HasOne(l => l.Agent)
            .WithMany()
            .HasForeignKey(l => l.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Property)
            .WithMany()
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Notes)
            .WithOne(n => n.Lead)
            .HasForeignKey(n => n.LeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
