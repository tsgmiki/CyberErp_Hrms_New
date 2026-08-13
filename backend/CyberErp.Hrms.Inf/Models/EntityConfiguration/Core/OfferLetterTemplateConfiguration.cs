using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /*
     * Was CompanyProfileConfiguration.cs. CompanyProfile was consolidated into Core.Organization on
     * 2026-08-13 (logic.md §12.11) and its configuration went with it; the offer-letter template
     * shared the file and is unrelated, so it moved here under its own name.
     */
    public class OfferLetterTemplateConfiguration : IEntityTypeConfiguration<OfferLetterTemplate>
    {
        public void Configure(EntityTypeBuilder<OfferLetterTemplate> builder)
        {
            builder.ToTable("OfferLetterTemplate", "Hrms");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Body).IsRequired().HasMaxLength(8000);
            builder.Property(t => t.SignatoryName).HasMaxLength(200);
            builder.Property(t => t.SignatoryTitle).HasMaxLength(200);

            // One template row per tenant.
            builder.HasIndex(t => t.TenantId).IsUnique();
        }
    }
}
