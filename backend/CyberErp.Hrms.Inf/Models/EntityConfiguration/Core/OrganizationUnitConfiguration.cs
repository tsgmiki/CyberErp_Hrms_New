using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class OrganizationUnitConfiguration : IEntityTypeConfiguration<OrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            builder.ToTable("hrms_OrganizationUnit", "Core");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Code).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
            builder.Property(o => o.UnitType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            builder.Property(o => o.Description).HasMaxLength(1000);

            // Self-referencing hierarchy — never cascade a tree on SQL Server
            builder.HasOne(o => o.Parent)
                .WithMany(o => o.Children)
                .HasForeignKey(o => o.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(o => o.Parent).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(o => o.Children).UsePropertyAccessMode(PropertyAccessMode.Field);

            // Optional work location
            builder.HasOne(o => o.WorkLocation)
                .WithMany()
                .HasForeignKey(o => o.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(o => o.WorkLocation).UsePropertyAccessMode(PropertyAccessMode.Field);

            // Branch (multi-branch isolation).
            builder.HasOne(o => o.Branch)
                .WithMany()
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(o => o.Branch).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(o => o.ParentId);
            builder.HasIndex(o => o.WorkLocationId);
            builder.HasIndex(o => o.BranchId);
            // Codes are unique within a branch (each branch may reuse the same unit codes).
            builder.HasIndex(o => new { o.TenantId, o.BranchId, o.Code }).IsUnique();
        }
    }
}
