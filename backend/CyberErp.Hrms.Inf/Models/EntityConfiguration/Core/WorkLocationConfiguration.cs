using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class WorkLocationConfiguration : IEntityTypeConfiguration<WorkLocation>
    {
        public void Configure(EntityTypeBuilder<WorkLocation> builder)
        {
            builder.ToTable("hrms_WorkLocation", "Core");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Code).IsRequired().HasMaxLength(50);
            builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
            builder.Property(l => l.LocationType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(l => l.Address).HasMaxLength(500);
            builder.Property(l => l.Description).HasMaxLength(1000);

            // Self-referencing hierarchy — never cascade a tree on SQL Server
            builder.HasOne(l => l.Parent)
                .WithMany(l => l.Children)
                .HasForeignKey(l => l.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(l => l.Parent).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(l => l.Children).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(l => l.ParentId);
            builder.HasIndex(l => new { l.TenantId, l.Code }).IsUnique();
        }
    }
}
