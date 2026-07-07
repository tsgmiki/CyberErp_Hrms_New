using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class DisciplinaryMeasureConfiguration : IEntityTypeConfiguration<DisciplinaryMeasure>
    {
        public void Configure(EntityTypeBuilder<DisciplinaryMeasure> builder)
        {
            builder.ToTable("hrms_DisciplinaryMeasure", "Core");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.ViolationType).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Description).HasMaxLength(2000);
            builder.Property(d => d.MeasureType).IsRequired().HasConversion<string>().HasMaxLength(40);
            builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(d => d.Resolution).HasMaxLength(2000);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(d => d.EmployeeId);
            builder.HasIndex(d => d.Status);
        }
    }
}
