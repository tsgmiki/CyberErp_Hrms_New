using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            // ---- SRMS platform alignment (2026-08-14, logic.md §12.13) --------------
            // ⚠️ OrganizationId is a REAL foreign key to the owning legal entity. It is not
            // BaseEntity.TenantId, which is the Finbuckle discriminator string.
            builder.Property(t => t.OrganizationId).IsRequired();
            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(t => t.OrganizationId);

            builder.Property(t => t.TenantTypeId);
            builder.Property(t => t.CurrencyOverride);
            builder.Property(t => t.LocaleOverride);
            builder.Property(t => t.TimezoneOverride);

            builder.Property(t => t.Identifier)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.ConnectionString)
                .HasMaxLength(500);

            builder.Property(t => t.Theme)
                .HasMaxLength(100);

            builder.Property(t => t.Address)
                .HasMaxLength(500);

            builder.Property(t => t.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(t => t.Email)
                .HasMaxLength(200);

            builder.Property(t => t.IsActive)
                .IsRequired();

            builder.Property(t => t.SubscriptionStartDate)
                .HasColumnType("datetime2(7)");

            builder.Property(t => t.SubscriptionEndDate)
                .HasColumnType("datetime2(7)");

            // Configure unique index on Identifier
            builder.HasIndex(t => t.Identifier)
                .IsUnique();

            // DateTime conversion for CreatedAt
            builder.Property(t => t.CreatedAt)
                .HasColumnType("datetime2(7)")
                .IsRequired();

            // DateTime conversion for UpdatedAt
            builder.Property(t => t.UpdatedAt)
                .HasColumnType("datetime2(7)");

            builder.Property(t => t.RowVersion)
                ;
        }
    }
}
