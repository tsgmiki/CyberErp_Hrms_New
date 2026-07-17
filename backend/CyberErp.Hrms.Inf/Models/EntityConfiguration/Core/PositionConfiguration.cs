using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("hrmsPosition", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
            builder.Property(p => p.IsVacant).IsRequired().HasDefaultValue(true);

            builder.HasOne(p => p.PositionClass)
                .WithMany()
                .HasForeignKey(p => p.PositionClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.OrganizationUnit)
                .WithMany()
                .HasForeignKey(p => p.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Branch (multi-branch isolation).
            builder.HasOne(p => p.Branch)
                .WithMany()
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(p => p.PositionClass).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(p => p.OrganizationUnit).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(p => p.Branch).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(p => p.PositionClassId);
            builder.HasIndex(p => p.OrganizationUnitId);
            builder.HasIndex(p => p.BranchId);
            // Codes are unique within a branch.
            builder.HasIndex(p => new { p.TenantId, p.BranchId, p.Code }).IsUnique();
        }
    }
}
