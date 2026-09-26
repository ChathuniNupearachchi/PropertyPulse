using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyPulse.Domain.Entities;

namespace PropertyPulse.Infrastructure.Data.Configurations;

public class SiteVisitConfiguration : IEntityTypeConfiguration<SiteVisit>
{
    public void Configure(EntityTypeBuilder<SiteVisit> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Notes).HasMaxLength(2000);

        builder.HasOne(v => v.Lead)
            .WithMany()
            .HasForeignKey(v => v.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Property)
            .WithMany()
            .HasForeignKey(v => v.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Agent)
            .WithMany()
            .HasForeignKey(v => v.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
