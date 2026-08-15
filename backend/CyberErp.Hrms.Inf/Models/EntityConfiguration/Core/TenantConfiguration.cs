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

            // ⚠️ TenantId is GONE (2026-08-15). A tenant row carrying a tenant DISCRIMINATOR was
            // always meaningless — the row IS the tenant, and Core.Tenant has been in
            // Repository.IsGlobalEntity from the start, so nothing ever stamped or filtered it. All
            // three rows held the empty Guid. SRMS has no such column.
            builder.Ignore(t => t.TenantId);

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

            /*
             * TenantTypeId references Core.LookUpCategoryList — the PLATFORM lookup table, the one
             * SRMS constrains with FK_Tenant_LookUpCategoryList.
             *
             * ⚠️ NOT mapped through EF, and that is deliberate. CERP has TWO lookup systems:
             * Core.LookUpCategory/List mirrors the SRMS platform schema, and Hrms.LookUpCategory/List
             * is the HRMS domain one the LookupCategoryList ENTITY maps (education levels, fields of
             * study). A tenant TYPE is platform data, so the constraint has to point at the Core
             * table — which EF cannot express while the entity maps the Hrms one. Added in raw SQL by
             * the TenantTypeIdForeignKey migration instead.
             */
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
