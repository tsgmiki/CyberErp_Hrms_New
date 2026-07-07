using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
    {
        public void Configure(EntityTypeBuilder<JobGrade> builder)
        {
            builder.ToTable("hrms_JobGrade", "Core");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
            builder.Property(g => g.NameA).HasMaxLength(200);
            builder.Property(g => g.Code).IsRequired().HasMaxLength(50);

            builder.HasIndex(g => new { g.TenantId, g.Code }).IsUnique();
        }
    }
}
