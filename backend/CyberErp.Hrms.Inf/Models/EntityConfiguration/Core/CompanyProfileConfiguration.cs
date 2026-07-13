using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
    {
        public void Configure(EntityTypeBuilder<CompanyProfile> builder)
        {
            builder.ToTable("hrms_CompanyProfile", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.LogoContent);                       // varbinary(max)
            builder.Property(p => p.LogoContentType).HasMaxLength(100);
            builder.Property(p => p.CompanyName).HasMaxLength(200);
            builder.Property(p => p.ContactAddress).HasMaxLength(500);
            builder.Property(p => p.ContactPhone).HasMaxLength(50);
            builder.Property(p => p.ContactEmail).HasMaxLength(200);

            // One profile row per tenant.
            builder.HasIndex(p => p.TenantId).IsUnique();
        }
    }

    public class OfferLetterTemplateConfiguration : IEntityTypeConfiguration<OfferLetterTemplate>
    {
        public void Configure(EntityTypeBuilder<OfferLetterTemplate> builder)
        {
            builder.ToTable("hrms_OfferLetterTemplate", "Core");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Body).IsRequired().HasMaxLength(8000);
            builder.Property(t => t.SignatoryName).HasMaxLength(200);
            builder.Property(t => t.SignatoryTitle).HasMaxLength(200);

            // One template row per tenant.
            builder.HasIndex(t => t.TenantId).IsUnique();
        }
    }
}
