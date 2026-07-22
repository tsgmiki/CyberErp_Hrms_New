using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("hrmsAuditLog", "dbo");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            builder.Property(a => a.EntityName).HasMaxLength(300);
            builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(a => a.Changes).HasColumnType("nvarchar(max)");
            builder.Property(a => a.PerformedBy).HasMaxLength(200);

            builder.HasIndex(a => new { a.EntityType, a.EntityId });
            builder.HasIndex(a => a.Action);
            builder.HasIndex(a => a.BranchId);
            builder.HasIndex(a => a.CreatedAt);
        }
    }
}
