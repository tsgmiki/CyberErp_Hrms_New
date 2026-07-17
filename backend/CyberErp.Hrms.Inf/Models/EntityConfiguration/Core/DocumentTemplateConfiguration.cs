using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
    {
        public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
        {
            builder.ToTable("hrmsDocumentTemplate", "dbo");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.DocumentType).IsRequired().HasConversion<string>().HasMaxLength(50);
            builder.Property(t => t.HeaderHtml);                      // nvarchar(max) optional letterhead
            builder.Property(t => t.Body).IsRequired();               // nvarchar(max) HTML body
            builder.Property(t => t.FooterHtml);                     // nvarchar(max) optional footer
            builder.Property(t => t.Description).HasMaxLength(1000);

            builder.HasIndex(t => new { t.TenantId, t.Name }).IsUnique();
        }
    }
}
