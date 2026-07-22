using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class DisciplinaryMeasureConfiguration : IEntityTypeConfiguration<DisciplinaryMeasure>
    {
        public void Configure(EntityTypeBuilder<DisciplinaryMeasure> builder)
        {
            builder.ToTable("hrmsDisciplinaryMeasure", "dbo");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.ViolationType).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Description).HasMaxLength(2000);
            builder.Property(d => d.MeasureType).IsRequired().HasConversion<string>().HasMaxLength(40);
            builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(d => d.Resolution).HasMaxLength(2000);
            builder.Property(d => d.AffectsPromotion).HasDefaultValue(false);
            builder.Property(d => d.AffectsReward).HasDefaultValue(false);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite supports both the per-employee history read and the eligibility filter
            // (EmployeeId + Status); it also covers EmployeeId-only lookups (leftmost prefix).
            builder.HasIndex(d => new { d.EmployeeId, d.Status });
            builder.HasIndex(d => d.Status);
        }
    }
}
