using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /// <summary>Maps <see cref="SalaryScale"/> onto the <c>coreSalaryScale</c> table.</summary>
    public class SalaryScaleConfiguration : IEntityTypeConfiguration<SalaryScale>
    {
        public void Configure(EntityTypeBuilder<SalaryScale> builder)
        {
            builder.ToTable("coreSalaryScale", "Core");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Salary).HasPrecision(18, 2).IsRequired();

            builder.HasOne(s => s.JobGrade)
                .WithMany()
                .HasForeignKey(s => s.JobGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Step)
                .WithMany()
                .HasForeignKey(s => s.StepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.JobGradeId);
            // One salary per (grade, step) within a tenant.
            builder.HasIndex(s => new { s.TenantId, s.JobGradeId, s.StepId }).IsUnique();
        }
    }
}
