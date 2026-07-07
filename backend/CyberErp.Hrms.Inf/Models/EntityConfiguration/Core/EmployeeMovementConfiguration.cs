using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class EmployeeMovementConfiguration : IEntityTypeConfiguration<EmployeeMovement>
    {
        public void Configure(EntityTypeBuilder<EmployeeMovement> builder)
        {
            builder.ToTable("hrms_EmployeeMovement", "Core");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.MovementType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(m => m.FromSalary).HasPrecision(18, 2);
            builder.Property(m => m.ToSalary).HasPrecision(18, 2);
            builder.Property(m => m.Reason).HasMaxLength(1000);
            builder.Property(m => m.Remark).HasMaxLength(1000);

            // Movement history cascades with its employee; no FKs to positions/grades so past
            // actions survive master-data cleanup (the snapshot is historical, not relational).
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(m => m.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => m.EmployeeId);
            builder.HasIndex(m => new { m.Status, m.EffectiveDate });
        }
    }
}
