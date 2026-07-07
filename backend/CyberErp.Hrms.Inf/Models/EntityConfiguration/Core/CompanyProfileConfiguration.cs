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

            // One profile row per tenant.
            builder.HasIndex(p => p.TenantId).IsUnique();
        }
    }
}
