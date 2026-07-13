using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
    {
        public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
        {
            builder.ToTable("hrms_EmployeeDocument", "Core");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.OwnerType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(d => d.OwnerField).HasMaxLength(100);   // sub-scope (dynamic-form field name)
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(300);
            builder.Property(d => d.ContentType).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Content).IsRequired();          // varbinary(max)

            builder.HasIndex(d => new { d.OwnerType, d.OwnerId });
            builder.HasIndex(d => d.EmployeeId);
        }
    }
}
