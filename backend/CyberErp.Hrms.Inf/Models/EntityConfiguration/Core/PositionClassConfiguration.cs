using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class PositionClassConfiguration : IEntityTypeConfiguration<PositionClass>
    {
        public void Configure(EntityTypeBuilder<PositionClass> builder)
        {
            builder.ToTable("hrms_PositionClass", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
            builder.Property(p => p.MinQualifications).HasMaxLength(1000);
            builder.Property(p => p.Skills).HasMaxLength(1000);
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.WeeklyWorkingHours).HasPrecision(5, 2);

            builder.HasOne(p => p.SalaryScale)
                .WithMany()
                .HasForeignKey(p => p.SalaryScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.JobCategory)
                .WithMany()
                .HasForeignKey(p => p.JobCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.WorkLocation)
                .WithMany()
                .HasForeignKey(p => p.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing reporting line (class → class)
            builder.HasOne(p => p.ReportsToPositionClass)
                .WithMany()
                .HasForeignKey(p => p.ReportsToPositionClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(p => p.SalaryScale).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(p => p.JobCategory).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(p => p.WorkLocation).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(p => p.ReportsToPositionClass).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(p => p.SalaryScaleId);
            builder.HasIndex(p => p.JobCategoryId);
            builder.HasIndex(p => p.ReportsToPositionClassId);
            builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
        }
    }
}
