using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("hrms_Branch", "Core");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Code).IsRequired().HasMaxLength(50);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Description).HasMaxLength(1000);
            builder.Property(b => b.Address).HasMaxLength(500);

            // Self-referencing branch hierarchy — never cascade a tree on SQL Server.
            builder.HasOne(b => b.Parent)
                .WithMany(b => b.Children)
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(b => b.Parent).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(b => b.Children).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(b => b.ParentId);
            builder.HasIndex(b => new { b.TenantId, b.Code }).IsUnique();
        }
    }
}
